using System.Net;
using AwesomeAssertions;
using HostManagement.Scripts;
using HostManagement.Shared.Communication.Enums;
using HostManagement.SystemConfig.Contracts;
using HostManagement.SystemConfig.Network;
using NSubstitute;
using Xunit;

namespace HostManagement.Tests.SystemConfig.Network;

public sealed class NetworkInterfacesConfigurationAdapterTests
{
    private const string Lan1 = "lan1";

    private static NetworkInterfacesConfigurationAdapter CreateSut(
        IScriptHandler handler,
        bool bypassHasChanges = false,
        bool bypassApply = false)
           => new(handler, bypassHasChanges, bypassApply);

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
            var handler = Substitute.For<IScriptHandler>();
            handler.ExecuteGetNetworkInterfacesScript().Returns((Result.ErrorResult, ""));

            var sut = CreateSut(handler);

            // Act
            var result = await sut.GetConfiguration(new(), CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Error);
            result.Messages.Should().Contain(Result.ErrorMessage);
        }

        [Fact]
        public async Task ShouldReturnError_WhenCommonInfoScriptFails()
        {
            // Arrange
            var handler = Substitute.For<IScriptHandler>();

            handler
                .ExecuteGetNetworkInterfacesScript()
                .Returns((Result.SuccessResult, ""));

            handler
                .ExecuteGetNetworkInterfacesCommonInformationScript(Arg.Any<ScriptOptions>())
                .Returns((Result.ErrorResult, []));

            var sut = CreateSut(handler);

            // Act
            var result = await sut.GetConfiguration(new(), CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Error);
            result.Messages.Should().Contain(Result.ErrorMessage);
        }

        [Fact]
        public async Task ShouldReturnError_WhenInterfaceSettingsScriptFails()
        {
            // Arrange
            var handler = Substitute.For<IScriptHandler>();

            handler
                .ExecuteGetNetworkInterfacesScript()
                .Returns((Result.SuccessResult, RawInterfacesString));

            handler
                .ExecuteGetNetworkInterfacesCommonInformationScript(Arg.Any<ScriptOptions>())
                .Returns((Result.SuccessResult, [CreateCommonInformation(Lan1)]));

            handler
                .ExecuteGetInterfaceSettingsScript(Lan1)
                .Returns((Result.ErrorResult, new(), new()));

            var sut = CreateSut(handler);

            // Act
            var result = await sut.GetConfiguration(new(), CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Error);
            result.Messages.Should().Contain(Result.ErrorMessage);
        }

        [Theory]
        [InlineData(OperationStatus.Warning, false)]
        [InlineData(OperationStatus.Success, false)]
        [InlineData(OperationStatus.Warning, true)]
        [InlineData(OperationStatus.Success, true)]
        public async Task ShouldPopulateInterfaces_WhenScriptSucceedsOrWarns(
            OperationStatus operationStatusFromCommonInfoScript,
            bool dhcpEnabled)
        {
            // Arrange
            var handler = Substitute.For<IScriptHandler>();

            handler
                .ExecuteGetNetworkInterfacesScript()
                .Returns((Result.SuccessResult, RawInterfacesString));

            var expectedResult = operationStatusFromCommonInfoScript is OperationStatus.Warning
                ? Result.WarningResult
                : Result.SuccessResult;

            handler
                .ExecuteGetNetworkInterfacesCommonInformationScript(Arg.Any<ScriptOptions>())
                .Returns((expectedResult, [CreateCommonInformation(Lan1),]));

            handler
                .ExecuteGetInterfaceSettingsScript(Lan1)
                .Returns((Result.SuccessResult, new() { DHCPEnabled = dhcpEnabled }, new()));

            var config = new SystemConfiguration();
            var sut = CreateSut(handler);

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

            _ = await handler.DidNotReceive().ExecuteGetInterfaceIPv4DHCPLeaseScript(Lan1);
        }
    }

    public sealed class HasChanges
    {
        [Fact]
        public void ShouldReturnFalse_WhenConfigurationsEqual()
        {
            // Arrange
            var handler = Substitute.For<IScriptHandler>();
            var current = new SystemConfiguration();
            var proposed = new SystemConfiguration();
            var sut = CreateSut(handler);

            // Act
            var result = sut.HasChanges(current, proposed);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnTrue_WhenNetworkInterfacesDiffer()
        {
            // Arrange
            var handler = Substitute.For<IScriptHandler>();
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

            var sut = CreateSut(handler);

            // Act
            var result = sut.HasChanges(current, proposed);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldReturnTrue_WhenNetworkDNSSettingsDiffer()
        {
            // Arrange
            var handler = Substitute.For<IScriptHandler>();
            var current = new SystemConfiguration();
            var proposed = new SystemConfiguration()
            {
                NetworkDNSSettings = new()
                {
                    Hostname = "new"
                }
            };

            var sut = CreateSut(handler);

            // Act
            var result = sut.HasChanges(current, proposed);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldReturnTrue_WhenChangeCheckBypassed()
        {
            // Arrange
            var handler = Substitute.For<IScriptHandler>();
            var current = new SystemConfiguration();
            var proposed = new SystemConfiguration();
            var sut = CreateSut(handler, bypassHasChanges: true);

            // Act
            var result = sut.HasChanges(current, proposed);

            // Assert
            result.Should().BeTrue();
        }
    }

    public sealed class SetConfiguration
    {
        private static readonly SystemConfiguration s_proposedConfig = new()
        {
            NetworkInterfaces =
            [
                new()
                {
                    CommonInformation = new()
                    {
                        Enabled = false,
                        Name = Lan1,
                        UserDefinedMACAddress = new()
                        {
                            Enabled = true,
                            Address = "00:11:22:33:44:55"
                        }
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

        [Theory]
        [InlineData(OperationStatus.Warning)]
        [InlineData(OperationStatus.Error)]
        public async Task ShouldReturnError_WhenNetworkInterfaceAvailableScriptReturnsWarningOrError(
            OperationStatus operationStatusFromInterfaceAvailableScript)
        {
            // Arrange
            var handler = Substitute.For<IScriptHandler>();

            var (expectedResult, expectedMessage) = operationStatusFromInterfaceAvailableScript is OperationStatus.Warning
                ? (Result.WarningResult, Result.WarningMessage)
                : (Result.ErrorResult, Result.ErrorMessage);

            handler
                .ExecuteIsNetworkInterfaceAvailableScript(Arg.Any<string>())
                .Returns(expectedResult);

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

            var sut = CreateSut(handler, bypassHasChanges: true);

            // Act
            var result = await sut.SetConfiguration(config, CancellationToken.None);
            result.OperationStatus.Should().Be(OperationStatus.Error);
            result.Messages.Should().Contain(expectedMessage);

            // Assert
            _ = await handler.DidNotReceive().ExecuteSetInterfaceSettingsScript(Arg.Any<ScriptOptions>());
        }

        [Fact]
        public async Task ShouldReturnError_WhenSetInterfaceSettingsScriptFails()
        {
            // Arrange
            var handler = Substitute.For<IScriptHandler>();
            handler.ExecuteIsNetworkInterfaceAvailableScript(Arg.Any<string>()).Returns(Result.SuccessResult);
            handler.ExecuteSetInterfaceSettingsScript(Arg.Any<ScriptOptions>()).Returns(Result.ErrorResult);

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

            var sut = CreateSut(handler, bypassHasChanges: true);

            // Act
            var result = await sut.SetConfiguration(config, CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Error);
            result.Messages.Should().Contain(Result.ErrorMessage);
        }

        [Fact]
        public async Task ShouldSetInterfaceSettings_WhenConfigurationValid()
        {
            // Arrange
            ScriptOptions? capturedOptions = default;
            var handler = Substitute.For<IScriptHandler>();
            handler.ExecuteIsNetworkInterfaceAvailableScript(Arg.Any<string>()).Returns(Result.SuccessResult);
            handler.ExecuteSetInterfaceSettingsScript(Arg.Do<ScriptOptions>(so => capturedOptions = so))
                .Returns(Result.SuccessResult);

            var sut = CreateSut(handler, bypassHasChanges: true);

            // Act
            var result = await sut.SetConfiguration(s_proposedConfig, CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Success);
            result.Messages.Should().BeEmpty();

            await handler.Received(1).ExecuteSetInterfaceSettingsScript(Arg.Any<ScriptOptions>());

            capturedOptions.Should().NotBeNull();
            capturedOptions!.Options.Should().ContainInOrder(
                ScriptOptionNames.Network.VLANID, "100",
                ScriptOptionNames.Network.InterfaceName, Lan1,
                ScriptOptionNames.Network.UserDefinedMACAddressEnabled, "true",
                ScriptOptionNames.Network.UserDefinedMACAddress, "00:11:22:33:44:55",
                ScriptOptionNames.Network.Enabled, "false",
                ScriptOptionNames.Network.DHCPEnabled, "true",
                ScriptOptionNames.Network.IPAddress, "192.168.0.10/24",
                ScriptOptionNames.Network.Gateway, "192.168.0.1");
        }
    }
}
