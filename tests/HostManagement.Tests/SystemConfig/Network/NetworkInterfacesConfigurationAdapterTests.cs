using System.Net;
using AwesomeAssertions;
using HostManagement.Scripts;
using HostManagement.Shared.Communication.Enums;
using HostManagement.SystemConfig.Contracts;
using HostManagement.SystemConfig.Network;
using NSubstitute;
using Xunit;

namespace HostManagement.Tests.SystemConfig.Network;

public class NetworkInterfacesConfigurationAdapterTests
{
    private const string Lan1 = "lan1";

    private static NetworkInterfacesConfigurationAdapter CreateSut(
        IScriptHandler scriptHandler,
        bool bypassHasChanges = false,
        bool bypassApply = false)
            => new(scriptHandler, bypassHasChanges, bypassApply);

    private static NetworkInterfaceCommonInformation CreateCommonInformation(string name, bool enabled = true)
        => new() { Name = name, Enabled = enabled };

    public sealed class GetConfiguration
    {
        private const string RawInterfacesString =
            $"{Lan1}            UP             00:50:56:36:6d:33 <BROADCAST,MULTICAST,UP,LOWER_UP>";

        [Fact]
        public async Task ShouldReturnError_WhenGetInterfacesScriptFails()
        {
            // Arrange
            var scriptHandler = Substitute.For<IScriptHandler>();

            scriptHandler
                .ExecuteGetNetworkInterfacesScript()
                .Returns((Result.ErrorResult, ""));

            var sut = CreateSut(scriptHandler);

            // Act + Assert
            var result = await sut.GetConfiguration(new(), CancellationToken.None);
            result.OperationStatus.Should().Be(OperationStatus.Error);
            result.Messages.Should().Contain(Result.ErrorMessage);
        }

        [Fact]
        public async Task ShouldReturnError_WhenCommonInfoScriptFails()
        {
            // Arrange
            var scriptHandler = Substitute.For<IScriptHandler>();

            scriptHandler
                .ExecuteGetNetworkInterfacesScript()
                .Returns((Result.SuccessResult, ""));

            scriptHandler
                .ExecuteGetNetworkInterfacesCommonInformationScript(Arg.Any<ScriptOptions>())
                .Returns((Result.ErrorResult, []));

            var sut = CreateSut(scriptHandler);

            // Act + Assert
            var result = await sut.GetConfiguration(new(), CancellationToken.None);
            result.OperationStatus.Should().Be(OperationStatus.Error);
            result.Messages.Should().Contain(Result.ErrorMessage);
        }

        [Fact]
        public async Task ShouldReturnError_WhenInterfaceSettingsScriptFails()
        {
            // Arrange
            var scriptHandler = Substitute.For<IScriptHandler>();

            scriptHandler
                .ExecuteGetNetworkInterfacesScript()
                .Returns((Result.SuccessResult, RawInterfacesString));

            scriptHandler
                .ExecuteGetNetworkInterfacesCommonInformationScript(Arg.Any<ScriptOptions>())
                .Returns((Result.SuccessResult, [CreateCommonInformation(Lan1)]));

            scriptHandler
                .ExecuteGetInterfaceSettingsScript(Lan1)
                .Returns((Result.ErrorResult, new(), new()));

            var sut = CreateSut(scriptHandler);

            // Act + Assert
            var result = await sut.GetConfiguration(new(), CancellationToken.None);
            result.OperationStatus.Should().Be(OperationStatus.Error);
            result.Messages.Should().Contain(Result.ErrorMessage);
        }

        [Theory]
        [InlineData(OperationStatus.Warning, false)]
        [InlineData(OperationStatus.Success, false)]
        [InlineData(OperationStatus.Warning, true)]
        [InlineData(OperationStatus.Success, true)]
        public async Task ShouldPopulateInterfaces_WhenSuccessOrWarning(
            OperationStatus operationStatusFromCommonInfoScript,
            bool dhcpEnabled)
        {
            // Arrange
            var scriptHandler = Substitute.For<IScriptHandler>();

            scriptHandler
                .ExecuteGetNetworkInterfacesScript()
                .Returns((Result.SuccessResult, RawInterfacesString));

            var expectedResult = operationStatusFromCommonInfoScript is OperationStatus.Warning
                ? Result.WarningResult
                : Result.SuccessResult;

            scriptHandler
                .ExecuteGetNetworkInterfacesCommonInformationScript(Arg.Any<ScriptOptions>())
                .Returns((expectedResult, [CreateCommonInformation(Lan1),]));

            scriptHandler
                .ExecuteGetInterfaceSettingsScript(Lan1)
                .Returns((Result.SuccessResult, new() { DHCPEnabled = dhcpEnabled }, new()));

            var sut = CreateSut(scriptHandler);
            var config = new SystemConfiguration();

            // Act
            var result = await sut.GetConfiguration(config, CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(operationStatusFromCommonInfoScript);

            if (operationStatusFromCommonInfoScript is OperationStatus.Warning)
                result.Messages.Should().Contain(Result.WarningMessage);
            else
                result.Messages.Should().BeEmpty();

            config.NetworkInterfaces.Should().NotBeNull();

            var networkInterface = config.NetworkInterfaces.Should().ContainSingle().Subject;
            networkInterface.CommonInformation.Should().NotBeNull();
            networkInterface.CommonInformation.Name.Should().Be(Lan1);
            networkInterface.IPv4.Should().NotBeNull();

            _ = await scriptHandler.DidNotReceive().ExecuteGetInterfaceIPv4DHCPLeaseScript(Lan1);
        }
    }

    public sealed class HasChanges
    {
        [Fact]
        public void ShouldBeFalse_WhenEqual()
        {
            var scriptHandler = Substitute.For<IScriptHandler>();
            var sut = CreateSut(scriptHandler);
            var current = new SystemConfiguration();
            var proposed = new SystemConfiguration();

            sut.HasChanges(current, proposed).Should().BeFalse();
        }

        [Fact]
        public void ShouldBeTrue_WhenInterfacesDifferent()
        {
            var scriptHandler = Substitute.For<IScriptHandler>();
            var sut = CreateSut(scriptHandler);
            var current = new SystemConfiguration();
            var proposed = new SystemConfiguration()
            {
                NetworkInterfaces =
                [
                    new()
                    {
                        CommonInformation = CreateCommonInformation(Lan1),
                        IPv4 = new(),
                        VLAN = new()
                    }
                ]
            };

            sut.HasChanges(current, proposed).Should().BeTrue();
        }

        [Fact]
        public void ShouldBeTrue_WhenDNSDifferent()
        {
            var scriptHandler = Substitute.For<IScriptHandler>();
            var sut = CreateSut(scriptHandler);
            var current = new SystemConfiguration();
            var proposed = new SystemConfiguration()
            {
                NetworkDNSSettings = new()
                {
                    Hostname = "new"
                }
            };

            sut.HasChanges(current, proposed).Should().BeTrue();
        }

        [Fact]
        public void ShouldBeTrue_WhenBypassHasChangesFlagTrue()
        {
            var scriptHandler = Substitute.For<IScriptHandler>();
            var sut = CreateSut(scriptHandler, bypassHasChanges: true);
            var current = new SystemConfiguration();
            var proposed = new SystemConfiguration();

            sut.HasChanges(current, proposed).Should().BeTrue();
        }
    }

    public sealed class SetConfiguration
    {
        [Theory]
        [InlineData(OperationStatus.Warning)]
        [InlineData(OperationStatus.Error)]
        public async Task ShouldReturnError_WhenNetworkInterfaceAvailableScriptReturnsWarningOrError(
            OperationStatus operationStatusFromInterfaceAvailableScript)
        {
            // Arrange
            var scriptHandler = Substitute.For<IScriptHandler>();

            var (expectedResult, expectedMessage) = operationStatusFromInterfaceAvailableScript is OperationStatus.Warning
                ? (Result.WarningResult, Result.WarningMessage)
                : (Result.ErrorResult, Result.ErrorMessage);

            scriptHandler
                .ExecuteIsNetworkInterfaceAvailableScript(Arg.Any<string>())
                .Returns(expectedResult);

            var sut = CreateSut(scriptHandler, bypassHasChanges: true);

            var config = new SystemConfiguration()
            {
                NetworkInterfaces =
                [
                    new()
                    {
                        CommonInformation = CreateCommonInformation(Lan1),
                        IPv4 = new(),
                        VLAN = new()
                    }
                ]
            };

            // Act + Assert
            var result = await sut.SetConfiguration(config, CancellationToken.None);
            result.OperationStatus.Should().Be(OperationStatus.Error);
            result.Messages.Should().Contain(expectedMessage);

            _ = await scriptHandler.DidNotReceive().ExecuteSetInterfaceSettingsScript(Arg.Any<ScriptOptions>());
        }

        [Fact]
        public async Task ShouldReturnError_WhenSetInterfaceSettingsScriptFails()
        {
            // Arrange
            var scriptHandler = Substitute.For<IScriptHandler>();

            scriptHandler
                .ExecuteIsNetworkInterfaceAvailableScript(Arg.Any<string>())
                .Returns(Result.SuccessResult);

            scriptHandler
                .ExecuteSetInterfaceSettingsScript(Arg.Any<ScriptOptions>())
                .Returns(Result.ErrorResult);

            var sut = CreateSut(scriptHandler, bypassHasChanges: true);

            var config = new SystemConfiguration()
            {
                NetworkInterfaces =
                [
                    new()
                    {
                        CommonInformation = CreateCommonInformation(Lan1),
                        IPv4 = new(),
                        VLAN = new()
                    }
                ]
            };

            // Act + Assert
            var result = await sut.SetConfiguration(config, CancellationToken.None);
            result.OperationStatus.Should().Be(OperationStatus.Error);
            result.Messages.Should().Contain(Result.ErrorMessage);
        }

        [Fact]
        public async Task ShouldSetInterfaceSettings_WhenCalled()
        {
            // Arrange
            ScriptOptions? captured = default;
            var scriptHandler = Substitute.For<IScriptHandler>();

            scriptHandler
                .ExecuteIsNetworkInterfaceAvailableScript(Arg.Any<string>())
                .Returns(Result.SuccessResult);

            scriptHandler
                .ExecuteSetInterfaceSettingsScript(Arg.Do<ScriptOptions>(so => captured = so))
                .Returns(Result.SuccessResult);

            var sut = CreateSut(scriptHandler, bypassHasChanges: true);

            var config = new SystemConfiguration()
            {
                NetworkInterfaces =
                [
                    new()
                    {
                        CommonInformation = new()
                        {
                            Enabled = false,
                            Name = Lan1,
                            PhysicalAddress = "00:11:22:33:44:55"
                        },
                        IPv4 = new()
                        {
                            DHCPEnabled = true,
                            Gateway = IPAddress.Parse("192.168.0.1"),
                            IPv4Details =
                            [
                                new()
                                {
                                    IPAddress = IPAddress.Parse("192.168.0.10"),
                                    Netmask = IPAddress.Parse("255.255.255.0")
                                }
                            ]
                        },
                        VLAN = new()
                        {
                            Enabled = true,
                            ID = 100
                        }
                    }
                ]
            };

            // Act
            var result = await sut.SetConfiguration(config, CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Success);
            result.Messages.Should().BeEmpty();

            await scriptHandler.Received(1).ExecuteSetInterfaceSettingsScript(Arg.Any<ScriptOptions>());

            captured.Should().NotBeNull();
            captured!.Options.Should().ContainInOrder(
                ScriptOptionNames.Network.VLANID, "100",
                ScriptOptionNames.Network.InterfaceName, Lan1,
                ScriptOptionNames.Network.MACAddress, "00:11:22:33:44:55",
                ScriptOptionNames.Network.Enabled, "false",
                ScriptOptionNames.Network.DHCPEnabled, "true",
                ScriptOptionNames.Network.IPAddress, "192.168.0.10/24",
                ScriptOptionNames.Network.Gateway, "192.168.0.1");
        }
    }
}
