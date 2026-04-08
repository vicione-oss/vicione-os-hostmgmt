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
                .Returns(Task.FromResult((OperationStatus.Error, "")));

            var sut = CreateSut(scriptHandler);

            // Act + Assert
            var status = await sut.GetConfiguration(new(), CancellationToken.None);
            status.Should().Be(OperationStatus.Error);
        }

        [Fact]
        public async Task ShouldReturnError_WhenCommonInfoScriptFails()
        {
            // Arrange
            var scriptHandler = Substitute.For<IScriptHandler>();

            scriptHandler
                .ExecuteGetNetworkInterfacesScript()
                .Returns(Task.FromResult((OperationStatus.Success, "")));

            scriptHandler
                .ExecuteGetNetworkInterfacesCommonInformationScript(Arg.Any<ScriptOptions>())
                .Returns((OperationStatus.Error, []));

            var sut = CreateSut(scriptHandler);

            // Act + Assert
            var status = await sut.GetConfiguration(new(), CancellationToken.None);
            status.Should().Be(OperationStatus.Error);
        }

        [Fact]
        public async Task ShouldReturnError_WhenIPv4SettingsScriptFails()
        {
            // Arrange
            var scriptHandler = Substitute.For<IScriptHandler>();

            scriptHandler
                .ExecuteGetNetworkInterfacesScript()
                .Returns(Task.FromResult((OperationStatus.Success, RawInterfacesString)));

            scriptHandler
                .ExecuteGetNetworkInterfacesCommonInformationScript(Arg.Any<ScriptOptions>())
                .Returns((OperationStatus.Success, [CreateCommonInformation(Lan1)]));

            scriptHandler
                .ExecuteGetInterfaceIPv4SettingsScript(Lan1)
                .Returns((OperationStatus.Error, new()));

            var sut = CreateSut(scriptHandler);

            // Act + Assert
            var status = await sut.GetConfiguration(new(), CancellationToken.None);
            status.Should().Be(OperationStatus.Error);
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
                .Returns(Task.FromResult((OperationStatus.Success, RawInterfacesString)));

            scriptHandler
                .ExecuteGetNetworkInterfacesCommonInformationScript(Arg.Any<ScriptOptions>())
                .Returns((operationStatusFromCommonInfoScript, [CreateCommonInformation(Lan1),]));

            scriptHandler
                .ExecuteGetInterfaceIPv4SettingsScript(Lan1)
                .Returns((OperationStatus.Success, new() { DHCPEnabled = dhcpEnabled }));

            if (dhcpEnabled)
            {
                scriptHandler
                    .ExecuteGetInterfaceIPv4DHCPLeaseScript(Lan1)
                    .Returns((OperationStatus.Success, new() { LeaseExpires = DateTimeOffset.UtcNow }));
            }

            var sut = CreateSut(scriptHandler);
            var config = new SystemConfiguration();

            // Act
            var status = await sut.GetConfiguration(config, CancellationToken.None);

            // Assert
            status.Should().Be(operationStatusFromCommonInfoScript);
            config.NetworkInterfacesSettings.Should().NotBeNull();

            var networkInterface = config.NetworkInterfacesSettings.NetworkInterfaces.Should().ContainSingle().Subject;
            networkInterface.CommonInformation.Should().NotBeNull();
            networkInterface.CommonInformation.Name.Should().Be(Lan1);
            networkInterface.IPv4.Should().NotBeNull();

            if (dhcpEnabled)
            {
                _ = await scriptHandler.Received(1).ExecuteGetInterfaceIPv4DHCPLeaseScript(Lan1);
                networkInterface.IPv4.DHCPLease.Should().NotBeNull();
                networkInterface.IPv4.DHCPLease.LeaseExpires.Should().NotBe(default);
            }
            else
            {
                _ = await scriptHandler.DidNotReceive().ExecuteGetInterfaceIPv4DHCPLeaseScript(Lan1);
                networkInterface.IPv4.DHCPLease.Should().BeNull();
            }
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
                NetworkInterfacesSettings = new()
                {
                    NetworkInterfaces =
                    [
                        new()
                        {
                            CommonInformation = CreateCommonInformation(Lan1),
                            IPv4 = new()
                        }
                    ]
                }
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

            scriptHandler
                .ExecuteIsNetworkInterfaceAvailableScript(Arg.Any<string>())
                .Returns(Task.FromResult(operationStatusFromInterfaceAvailableScript));

            var sut = CreateSut(scriptHandler, bypassHasChanges: true);

            var config = new SystemConfiguration()
            {
                NetworkInterfacesSettings = new()
                {
                    NetworkInterfaces =
                    [
                        new()
                        {
                            CommonInformation = CreateCommonInformation(Lan1),
                            IPv4 = new()
                        }
                    ]
                }
            };

            // Act + Assert
            var status = await sut.SetConfiguration(config, CancellationToken.None);
            status.Should().Be(OperationStatus.Error);
            _ = await scriptHandler.DidNotReceive().ExecuteSetInterfaceIPv4SettingsScript(Arg.Any<ScriptOptions>());
        }

        [Fact]
        public async Task ShouldReturnError_WhenSetInterfaceIPv4ScriptFails()
        {
            // Arrange
            var scriptHandler = Substitute.For<IScriptHandler>();

            scriptHandler
                .ExecuteIsNetworkInterfaceAvailableScript(Arg.Any<string>())
                .Returns(Task.FromResult(OperationStatus.Success));

            scriptHandler
                .ExecuteSetInterfaceIPv4SettingsScript(Arg.Any<ScriptOptions>())
                .Returns(Task.FromResult(OperationStatus.Error));

            var sut = CreateSut(scriptHandler, bypassHasChanges: true);

            var config = new SystemConfiguration()
            {
                NetworkInterfacesSettings = new()
                {
                    NetworkInterfaces =
                    [
                        new()
                        {
                            CommonInformation = CreateCommonInformation(Lan1),
                            IPv4 = new()
                        }
                    ]
                }
            };

            // Act + Assert
            var status = await sut.SetConfiguration(config, CancellationToken.None);
            status.Should().Be(OperationStatus.Error);
        }

        [Fact]
        public async Task ShouldSetIPv4Settings_WhenCalled()
        {
            // Arrange
            ScriptOptions? captured = default;
            var scriptHandler = Substitute.For<IScriptHandler>();

            scriptHandler
                .ExecuteIsNetworkInterfaceAvailableScript(Arg.Any<string>())
                .Returns(Task.FromResult(OperationStatus.Success));

            scriptHandler
                .ExecuteSetInterfaceIPv4SettingsScript(Arg.Do<ScriptOptions>(so => captured = so))
                .Returns(Task.FromResult(OperationStatus.Success));

            var sut = CreateSut(scriptHandler, bypassHasChanges: true);

            var config = new SystemConfiguration()
            {
                NetworkInterfacesSettings = new()
                {
                    NetworkInterfaces =
                    [
                        new()
                        {
                            CommonInformation = new()
                            {
                                Enabled = false,
                                Name = Lan1,
                                PhysicalAddress = "00:11:22:33:44:55",
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
                                        Netmask = IPAddress.Parse("255.255.255.0"),
                                    },
                                ],
                                VLANEnabled = true,
                                VLANID = 100
                            }
                        }
                    ]
                }
            };

            // Act
            var status = await sut.SetConfiguration(config, CancellationToken.None);

            // Assert
            status.Should().Be(OperationStatus.Success);

            await scriptHandler.Received(1).ExecuteSetInterfaceIPv4SettingsScript(Arg.Any<ScriptOptions>());

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
