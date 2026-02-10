using System.Net;
using AwesomeAssertions;
using HostManagement.Scripts;
using HostManagement.Shared.Communication.Enums;
using HostManagement.SystemConfig.Contracts;
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
                .Returns((OperationStatus.Error, new()));
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
            NetworkDNSSettings scriptResult = new()
            {
                NameServers =
                [
                    IPAddress.Parse("1.1.1.1"),
                ],
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
            SystemConfiguration proposed = new()
            {
                NetworkInterfacesSettings = new()
                {
                    NetworkInterfaces =
                    [
                        new()
                        {
                            CommonInformation = new() { Enabled = true, Name = "eth0", },
                            IPv4 = new(),
                        },
                    ],
                },
            };
            var sut = CreateSut(current, handler);

            sut.HasChanges(current, proposed).Should().BeTrue();
        }

        [Fact]
        public void ShouldBeTrue_WhenDNSDifferent()
        {
            var handler = Substitute.For<IScriptHandler>();
            SystemConfiguration current = new();
            SystemConfiguration proposed = new()
            {
                NetworkDNSSettings = new()
                {
                    Hostname = "new",
                },
            };
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
            // Arrange
            List<ScriptOptions> captured = [];
            string? hostnameCaptured = default;
            var rawInterfacesString = "ens33            UP             00:50:56:36:6d:33 <BROADCAST,MULTICAST,UP,LOWER_UP>";

            var handler = Substitute.For<IScriptHandler>();
            handler.ExecuteGetNetworkInterfacesScript()
                .Returns(Task.FromResult((OperationStatus.Success, rawInterfacesString)));
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

            // Act
            var status = await sut.SetConfiguration(proposed);

            // Assert
            status.Should().Be(OperationStatus.Success);
            hostnameCaptured.Should().NotBeNull();
            hostnameCaptured!.Should().Be("newhost");

            captured.Should().HaveCount(3).And.SatisfyRespectively(
                so => so.Options.Should().ContainInOrder(
                    ScriptOptionNames.NetworkMulticastDNS.MulticastDNSEnabled, "true",
                    ScriptOptionNames.NetworkMulticastDNS.Interfaces, rawInterfacesString),
                so => so.Options.Should().ContainInOrder(
                    ScriptOptionNames.NetworkDNSStaticHosts.Enabled, "true",
                    ScriptOptionNames.NetworkDNSStaticHosts.IPAddress, "10.0.0.1",
                    ScriptOptionNames.NetworkDNSStaticHosts.Hostname, "local"),
                so => so.Options.Should().ContainInOrder(
                    ScriptOptionNames.NetworkDNS.NameServersEnabled, "true",
                    ScriptOptionNames.NetworkDNS.NameServers, "1.1.1.1",
                    ScriptOptionNames.NetworkDNS.DNSSuffixEnabled, "true",
                    ScriptOptionNames.NetworkDNS.DNSSuffix, "intranet",
                    ScriptOptionNames.NetworkDNS.SearchDomainsEnabled, "true",
                    ScriptOptionNames.NetworkDNS.SearchDomains, "searchengine"));
        }
    }
}
