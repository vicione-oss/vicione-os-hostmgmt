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

public class NetworkDNSConfigurationAdapterTests
{
    private static NetworkDNSConfigurationAdapter CreateSut(SystemConfiguration current, IScriptHandler handler, bool bypassHasChanges = false, bool bypassApply = false)
        => new(current, handler, bypassHasChanges, bypassApply);

    public sealed class GetConfiguration
    {
        [Fact]
        public async Task ShouldReturnError_WhenScriptFails()
        {
            var handler = Substitute.For<IScriptHandler>();
            handler.ExecuteGetNetworkDNSSettingsScript()
                .Returns((OperationStatus.Error, NetworkDNSSettings.Empty));
            SystemConfiguration current = new();
            SystemConfiguration config = new();
            var dnsSettings = config.NetworkDNSSettings;
            var sut = CreateSut(current, handler);

            var status = await sut.GetConfiguration(config);

            status.Should().Be(OperationStatus.Error);
            config.NetworkDNSSettings.Should().BeSameAs(dnsSettings);
        }

        [Fact]
        public async Task ShouldPopulate_WhenSuccess()
        {
            var handler = Substitute.For<IScriptHandler>();
            var scriptResult = new NetworkDNSSettings(nameServers: [IPAddress.Parse("1.1.1.1")])
            {
                Hostname = "host",
                NameServersEnabled = true,
            };
            handler.ExecuteGetNetworkDNSSettingsScript()
                .Returns((OperationStatus.Success, scriptResult));
            SystemConfiguration current = new();
            SystemConfiguration config = new();
            var sut = CreateSut(current, handler);

            var status = await sut.GetConfiguration(config);

            status.Should().Be(OperationStatus.Success);
            config.NetworkDNSSettings.Should().Be(scriptResult);
        }
    }

    public sealed class HasChanges
    {
        [Fact]
        public void ShouldBeFalse_WhenEqual()
        {
            var handler = Substitute.For<IScriptHandler>();
            SystemConfiguration current = new();
            SystemConfiguration proposed = new();
            var sut = CreateSut(current, handler);

            sut.HasChanges(current, proposed).Should().BeFalse();
        }

        [Fact]
        public void ShouldBeTrue_WhenInterfacesDifferent()
        {
            var handler = Substitute.For<IScriptHandler>();
            SystemConfiguration current = new();
            SystemConfiguration proposed = new();
            proposed.NetworkInterfacesSettings.NetworkInterfaces.Add(new()
            {
                CommonInformation = new() { Enabled = true, Name = "eth0" },
                IPv4 = new(),
            });
            var sut = CreateSut(current, handler);

            sut.HasChanges(current, proposed).Should().BeTrue();
        }

        [Fact]
        public void ShouldBeTrue_WhenDNSDifferent()
        {
            var handler = Substitute.For<IScriptHandler>();
            SystemConfiguration current = new();
            SystemConfiguration proposed = new();
            proposed.NetworkDNSSettings.Hostname = "new";
            var sut = CreateSut(current, handler);

            sut.HasChanges(current, proposed).Should().BeTrue();
        }

        [Fact]
        public void ShouldBeTrue_WhenBypassFlagEnabled()
        {
            var handler = Substitute.For<IScriptHandler>();
            SystemConfiguration current = new();
            SystemConfiguration proposed = new();
            var sut = CreateSut(current, handler, bypassHasChanges: true);

            sut.HasChanges(current, proposed).Should().BeTrue();
        }
    }

    public sealed class SetConfiguration
    {
        [Fact]
        public async Task ShouldCallScripts_ForChangedSettings()
        {
            var handler = Substitute.For<IScriptHandler>();
            List<ScriptOptions> captured = [];
            string? hostnameCaptured = default;
            handler.ExecuteSetNetworkMulticastDNSSettingsScript(Arg.Do<ScriptOptions>(captured.Add))
                .Returns(Task.FromResult(OperationStatus.Success));
            handler.ExecuteSetStaticHostsScript(Arg.Do<ScriptOptions>(captured.Add))
                .Returns(Task.FromResult(OperationStatus.Success));
            handler.ExecuteSetNetworkDNSSettingsScript(Arg.Do<ScriptOptions>(captured.Add))
                .Returns(Task.FromResult(OperationStatus.Success));
            handler.ExecuteSetHostnameScript(Arg.Do<string>(so => hostnameCaptured = so))
                .Returns(Task.FromResult(OperationStatus.Success));
            SystemConfiguration proposed = new()
            {
                NetworkDNSSettings = new()
                {
                    DNSSuffixEnabled = true,
                    DNSSuffix = "intranet",
                    Hostname = "newhost",
                    MulticastDNSEnabled = true,
                    NameServersEnabled = true,
                    NameServers = [IPAddress.Parse("1.1.1.1")],
                    SearchDomainsEnabled = true,
                    SearchDomains = ["searchengine"],
                    StaticHostsEnabled = true,
                    StaticHosts = [new StaticHostDetail { Hostname = "local", IPAddress = IPAddress.Parse("10.0.0.1"), }],
                }
            };
            var sut = CreateSut(new(), handler, bypassHasChanges: true);

            var status = await sut.SetConfiguration(proposed);

            status.Should().Be(OperationStatus.Success);
            captured.Should().HaveCount(3).And.SatisfyRespectively(
                so => so.Options.Should().ContainInOrder(
                    NetworkMulticastDNSScriptOptionName.MulticastDNSEnabled,
                    "true"),
                so => so.Options.Should().ContainInOrder(
                    NetworkDNSStaticHostsScriptOptionName.Enabled,
                    "true",
                    NetworkDNSStaticHostsScriptOptionName.IPAddress,
                    "10.0.0.1",
                    NetworkDNSStaticHostsScriptOptionName.Hostname,
                    "local"),
                so => so.Options.Should().ContainInOrder(
                    NetworkDNSScriptOptionName.NameServersEnabled,
                    "true",
                    NetworkDNSScriptOptionName.NameServers,
                    "1.1.1.1",
                    NetworkDNSScriptOptionName.DNSSuffixEnabled,
                    "true",
                    NetworkDNSScriptOptionName.DNSSuffix,
                    "intranet",
                    NetworkDNSScriptOptionName.SearchDomainsEnabled,
                    "true",
                    NetworkDNSScriptOptionName.SearchDomains,
                    "searchengine"));
            hostnameCaptured.Should().NotBeNull();
            hostnameCaptured!.Should().Be("newhost");
        }
    }
}
