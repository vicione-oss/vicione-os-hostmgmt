using System.Net;
using FluentAssertions;
using HostManagement.Scripts.ScriptOptionNames;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;
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
            var networkInterface = config.NetworkInterfacesSettings.NetworkInterfaces.Should().ContainSingle().Subject;
            networkInterface.CommonInformation.Name.Should().Be("eth0");
            networkInterface.IPv4.DHCPLease!.LeaseExpires.Should().NotBe(default);
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
            SystemConfiguration proposed = new();
            proposed.NetworkInterfacesSettings.NetworkInterfaces.Add(new()
            {
                CommonInformation = Common("eth0"),
                IPv4 = new(),
            });

            sut.HasChanges(current, proposed).Should().BeTrue();
        }

        [Fact]
        public void ShouldBeTrue_WhenDNSDifferent()
        {
            var handler = Substitute.For<IScriptHandler>();
            var sut = CreateSut(handler);
            SystemConfiguration current = new();
            SystemConfiguration proposed = new();
            proposed.NetworkDNSSettings.Hostname = "new";

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
            SystemConfiguration config = new();
            config.NetworkInterfacesSettings.NetworkInterfaces.Add(new NetworkInterfaceDetail
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
            });

            var status = await sut.SetConfiguration(config);

            status.Should().Be(OperationStatus.Success);
            await handler.Received(1).ExecuteSetInterfaceIPv4SettingsScript(Arg.Any<ScriptOptions>());
            captured.Should().NotBeNull();
            captured!.Options.Should().ContainInOrder(
                NetworkScriptOptionName.VLANID,
                "100",
                NetworkScriptOptionName.InterfaceName,
                "eth0",
                NetworkScriptOptionName.MACAddress,
                "00:11:22:33:44:55",
                NetworkScriptOptionName.Enabled,
                "false",
                NetworkScriptOptionName.DHCPEnabled,
                "true",
                NetworkScriptOptionName.IPAddress,
                "192.168.0.10/24",
                NetworkScriptOptionName.Gateway,
                "192.168.0.1");
        }

        [Fact]
        public async Task ShouldReturnError_WhenScriptFails()
        {
            var handler = Substitute.For<IScriptHandler>();
            handler.ExecuteSetInterfaceIPv4SettingsScript(Arg.Any<ScriptOptions>())
                .Returns(Task.FromResult(OperationStatus.Error));
            var sut = CreateSut(handler, bypassHasChanges: true);
            SystemConfiguration config = new();
            config.NetworkInterfacesSettings.NetworkInterfaces.Add(new NetworkInterfaceDetail
            {
                CommonInformation = Common("eth0"),
                IPv4 = new(),
            });

            var status = await sut.SetConfiguration(config);

            status.Should().Be(OperationStatus.Error);
        }
    }
}
