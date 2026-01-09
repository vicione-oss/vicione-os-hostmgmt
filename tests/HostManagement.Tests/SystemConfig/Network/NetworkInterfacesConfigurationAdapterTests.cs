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
    private static NetworkInterfacesConfigurationAdapter CreateSut(IScriptHandler handler, bool bypassHasChanges = false, bool bypassApply = false)
        => new(handler, bypassHasChanges, bypassApply);

    private static NetworkInterfaceCommonInformation Common(string name, bool enabled = true) => new() { Name = name, Enabled = enabled };

    public sealed class GetConfiguration
    {
        [Fact]
        public async Task ShouldReturnError_WhenCommonInfoScriptFails()
        {
            var handler = Substitute.For<IScriptHandler>();
            handler.ExecuteGetNetworkInterfacesCommonInformationScript().Returns((OperationStatus.Error, []));
            var sut = CreateSut(handler);

            var status = await sut.GetConfiguration(new());

            status.Should().Be(OperationStatus.Error);
        }

        [Fact]
        public async Task ShouldReturnError_WhenIPv4SettingsScriptFails()
        {
            var handler = Substitute.For<IScriptHandler>();
            handler.ExecuteGetNetworkInterfacesCommonInformationScript().Returns((OperationStatus.Success, [Common("eth0"),]));
            handler.ExecuteGetInterfaceIPv4SettingsScript("eth0").Returns((OperationStatus.Error, new()));
            var sut = CreateSut(handler);

            var status = await sut.GetConfiguration(new());

            status.Should().Be(OperationStatus.Error);
        }

        [Fact]
        public async Task ShouldPopulateInterfaces_WhenSuccess()
        {
            var handler = Substitute.For<IScriptHandler>();
            handler.ExecuteGetNetworkInterfacesCommonInformationScript().Returns((OperationStatus.Success, [Common("eth0"),]));
            handler.ExecuteGetInterfaceIPv4SettingsScript("eth0").Returns((OperationStatus.Success, new() { DHCPEnabled = true, }));
            handler.ExecuteGetInterfaceIPv4DHCPLeaseScript("eth0").Returns((OperationStatus.Success, new() { LeaseExpires = DateTimeOffset.UtcNow, }));
            var sut = CreateSut(handler);
            SystemConfiguration config = new();

            var status = await sut.GetConfiguration(config);

            status.Should().Be(OperationStatus.Success);
            config.NetworkInterfacesSettings.Should().NotBeNull();
            var networkInterface = config.NetworkInterfacesSettings.NetworkInterfaces.Should().ContainSingle().Subject;
            networkInterface.CommonInformation.Should().NotBeNull();
            networkInterface.CommonInformation.Name.Should().Be("eth0");
            networkInterface.IPv4.Should().NotBeNull();
            networkInterface.IPv4.DHCPLease.Should().NotBeNull();
            networkInterface.IPv4.DHCPLease.LeaseExpires.Should().NotBe(default);
        }
    }

    public sealed class HasChanges
    {
        [Fact]
        public void ShouldBeFalse_WhenEqual()
        {
            var handler = Substitute.For<IScriptHandler>();
            var sut = CreateSut(handler);
            SystemConfiguration current = new();
            SystemConfiguration proposed = new();

            sut.HasChanges(current, proposed).Should().BeFalse();
        }

        [Fact]
        public void ShouldBeTrue_WhenInterfacesDifferent()
        {
            var handler = Substitute.For<IScriptHandler>();
            var sut = CreateSut(handler);
            SystemConfiguration current = new();
            SystemConfiguration proposed = new()
            {
                NetworkInterfacesSettings = new()
                {
                    NetworkInterfaces =
                    [
                        new()
                        {
                            CommonInformation = Common("eth0"),
                            IPv4 = new(),
                        },
                    ],
                },
            };

            sut.HasChanges(current, proposed).Should().BeTrue();
        }

        [Fact]
        public void ShouldBeTrue_WhenDNSDifferent()
        {
            var handler = Substitute.For<IScriptHandler>();
            var sut = CreateSut(handler);
            SystemConfiguration current = new();
            SystemConfiguration proposed = new()
            {
                NetworkDNSSettings = new()
                {
                    Hostname = "new",
                },
            };

            sut.HasChanges(current, proposed).Should().BeTrue();
        }

        [Fact]
        public void ShouldBeTrue_WhenBypassFlagEnabled()
        {
            var handler = Substitute.For<IScriptHandler>();
            var sut = CreateSut(handler, bypassHasChanges: true);
            SystemConfiguration current = new();
            SystemConfiguration proposed = new();

            sut.HasChanges(current, proposed).Should().BeTrue();
        }
    }

    public sealed class SetConfiguration
    {
        [Fact]
        public async Task ShouldCallScripts()
        {
            var handler = Substitute.For<IScriptHandler>();
            ScriptOptions? captured = default;
            handler.ExecuteSetInterfaceIPv4SettingsScript(Arg.Do<ScriptOptions>(so => captured = so))
                .Returns(Task.FromResult(OperationStatus.Success));
            var sut = CreateSut(handler, bypassHasChanges: true);
            SystemConfiguration config = new()
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
                                Name = "eth0",
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
                                VLANID = 100,
                                VLANEnabled = true,
                            },
                        },
                    ],
                },
            };

            var status = await sut.SetConfiguration(config);

            status.Should().Be(OperationStatus.Success);
            await handler.Received(1).ExecuteSetInterfaceIPv4SettingsScript(Arg.Any<ScriptOptions>());
            captured.Should().NotBeNull();
            captured!.Options.Should().ContainInOrder(
                ScriptOptionNames.Network.VLANID,
                "100",
                ScriptOptionNames.Network.InterfaceName,
                "eth0",
                ScriptOptionNames.Network.MACAddress,
                "00:11:22:33:44:55",
                ScriptOptionNames.Network.Enabled,
                "false",
                ScriptOptionNames.Network.DHCPEnabled,
                "true",
                ScriptOptionNames.Network.IPAddress,
                "192.168.0.10/24",
                ScriptOptionNames.Network.Gateway,
                "192.168.0.1");
        }

        [Fact]
        public async Task ShouldReturnError_WhenScriptFails()
        {
            var handler = Substitute.For<IScriptHandler>();
            handler.ExecuteSetInterfaceIPv4SettingsScript(Arg.Any<ScriptOptions>())
                .Returns(Task.FromResult(OperationStatus.Error));
            var sut = CreateSut(handler, bypassHasChanges: true);
            SystemConfiguration config = new()
            {
                NetworkInterfacesSettings = new()
                {
                    NetworkInterfaces =
                    [
                        new()
                        {
                            CommonInformation = Common("eth0"),
                            IPv4 = new(),
                        },
                    ],
                },
            };

            var status = await sut.SetConfiguration(config);

            status.Should().Be(OperationStatus.Error);
        }
    }
}
