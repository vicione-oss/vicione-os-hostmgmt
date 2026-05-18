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
    private const string RawInterfacesString =
        "lan1            UP             00:50:56:36:6d:33 <BROADCAST,MULTICAST,UP,LOWER_UP>";

    private static NetworkDNSConfigurationAdapter CreateSut(
        SystemConfiguration current,
        IScriptHandler scriptHandler,
        bool bypassHasChanges = false,
        bool bypassApply = false)
        => new(current, scriptHandler, bypassHasChanges, bypassApply);

    public sealed class GetConfiguration
    {
        [Fact]
        public async Task ShouldReturnError_WhenScriptFails()
        {
            // Arrange
            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler.ExecuteGetNetworkDNSSettingsScript().Returns((OperationStatus.Error, new()));

            var current = new SystemConfiguration();
            var config = new SystemConfiguration();
            var dnsSettings = config.NetworkDNSSettings;
            var sut = CreateSut(current, scriptHandler);

            // Act + Assert
            var status = await sut.GetConfiguration(config, CancellationToken.None);

            status.Should().Be(OperationStatus.Error);
            config.NetworkDNSSettings.Should().BeSameAs(dnsSettings);
        }

        [Fact]
        public async Task ShouldPopulate_WhenSuccess()
        {
            // Arrange
            var dnsSettings = new NetworkDNSSettings()
            {
                NameServersEnabled = true,
                NameServers = [IPAddress.Parse("1.1.1.1")],
                Hostname = "host"
            };

            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler.ExecuteGetNetworkDNSSettingsScript().Returns((OperationStatus.Success, dnsSettings));

            var current = new SystemConfiguration();
            var config = new SystemConfiguration();
            var sut = CreateSut(current, scriptHandler);

            // Act + Assert
            var status = await sut.GetConfiguration(config, CancellationToken.None);

            status.Should().Be(OperationStatus.Success);
            config.NetworkDNSSettings.Should().Be(dnsSettings);
        }
    }

    public sealed class HasChanges
    {
        [Fact]
        public void ShouldBeFalse_WhenEqual()
        {
            var scriptHandler = Substitute.For<IScriptHandler>();
            var current = new SystemConfiguration();
            var proposed = new SystemConfiguration();
            var sut = CreateSut(current, scriptHandler);

            sut.HasChanges(current, proposed).Should().BeFalse();
        }

        [Fact]
        public void ShouldBeTrue_WhenInterfacesDifferent()
        {
            // Arrange
            var scriptHandler = Substitute.For<IScriptHandler>();
            var current = new SystemConfiguration();
            var proposed = new SystemConfiguration()
            {
                NetworkInterfacesSettings = new()
                {
                    NetworkInterfaces =
                    [
                        new()
                        {
                            CommonInformation = new() { Enabled = true, Name = "lan1" },
                            IPv4 = new()
                        }
                    ]
                }
            };

            // Act + Assert
            var sut = CreateSut(current, scriptHandler);
            sut.HasChanges(current, proposed).Should().BeTrue();
        }

        [Fact]
        public void ShouldBeTrue_WhenDNSDifferent()
        {
            // Arrange
            var scriptHandler = Substitute.For<IScriptHandler>();
            var current = new SystemConfiguration();
            var proposed = new SystemConfiguration() { NetworkDNSSettings = new() { Hostname = "new" } };

            // Act + Assert
            var sut = CreateSut(current, scriptHandler);
            sut.HasChanges(current, proposed).Should().BeTrue();
        }

        [Fact]
        public void ShouldBeTrue_WhenBypassHasChangesFlagTrue()
        {
            var scriptHandler = Substitute.For<IScriptHandler>();
            var current = new SystemConfiguration();
            var proposed = new SystemConfiguration();
            var sut = CreateSut(current, scriptHandler, bypassHasChanges: true);

            sut.HasChanges(current, proposed).Should().BeTrue();
        }
    }

    public sealed class SetConfiguration
    {
        private static readonly NetworkDNSSettings s_proposedDNS = new()
        {
            Hostname = "newhost",
            MulticastDNSEnabled = true,
            StaticHostsEnabled = true,
            StaticHosts =
            [
                new StaticHostDetail { Hostname = "local1", IPAddress = IPAddress.Parse("10.0.0.1") },
                new StaticHostDetail { Hostname = "local2", IPAddress = IPAddress.Parse("10.0.0.2") }
            ],
            NameServersEnabled = true,
            NameServers = [IPAddress.Parse("1.1.1.1"), IPAddress.Parse("8.8.8.8")],
            DNSSuffixEnabled = true,
            DNSSuffix = "intranet",
            SearchDomainsEnabled = true,
            SearchDomains = ["searchengine", "search.company.com"]
        };

        private static readonly SystemConfiguration s_proposed = new() { NetworkDNSSettings = s_proposedDNS };

        [Fact]
        public async Task ShouldReturnError_WhenSetMulticastReturnsErrorFromGetInterfacesCall()
        {
            // Arrange
            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler.ExecuteGetNetworkInterfacesScript().Returns((OperationStatus.Error, ""));

            var current = new SystemConfiguration();
            var sut = CreateSut(current, scriptHandler, bypassHasChanges: true);

            // Act + Assert
            var status = await sut.SetConfiguration(s_proposed, CancellationToken.None);

            status.Should().Be(OperationStatus.Error);
            _ = await scriptHandler.DidNotReceive().ExecuteSetNetworkMulticastDNSSettingsScript(Arg.Any<ScriptOptions>());
            _ = await scriptHandler.DidNotReceive().ExecuteSetStaticHostsScript(Arg.Any<ScriptOptions>());
            _ = await scriptHandler.DidNotReceive().ExecuteSetNetworkDNSSettingsScript(Arg.Any<ScriptOptions>());
            _ = await scriptHandler.DidNotReceive().ExecuteSetHostnameScript(Arg.Any<string>());
        }

        [Fact]
        public async Task ShouldReturnError_WhenSetMulticastReturnsError()
        {
            // Arrange
            var scriptHandler = Substitute.For<IScriptHandler>();

            scriptHandler
                .ExecuteGetNetworkInterfacesScript()
                .Returns((OperationStatus.Success, RawInterfacesString));

            scriptHandler
                .ExecuteSetNetworkMulticastDNSSettingsScript(Arg.Any<ScriptOptions>())
                .Returns(OperationStatus.Error);

            var current = new SystemConfiguration();
            var sut = CreateSut(current, scriptHandler, bypassHasChanges: true);

            // Act + Assert
            var status = await sut.SetConfiguration(s_proposed, CancellationToken.None);

            status.Should().Be(OperationStatus.Error);
            _ = await scriptHandler.DidNotReceive().ExecuteSetStaticHostsScript(Arg.Any<ScriptOptions>());
            _ = await scriptHandler.DidNotReceive().ExecuteSetNetworkDNSSettingsScript(Arg.Any<ScriptOptions>());
            _ = await scriptHandler.DidNotReceive().ExecuteSetHostnameScript(Arg.Any<string>());
        }

        [Fact]
        public async Task ShouldReturnError_WhenSetStaticHostsReturnsError()
        {
            // Arrange
            var scriptHandler = Substitute.For<IScriptHandler>();

            scriptHandler
                .ExecuteGetNetworkInterfacesScript()
                .Returns((OperationStatus.Success, RawInterfacesString));

            scriptHandler
                .ExecuteSetNetworkMulticastDNSSettingsScript(Arg.Any<ScriptOptions>())
                .Returns(OperationStatus.Success);

            scriptHandler
                .ExecuteSetStaticHostsScript(Arg.Any<ScriptOptions>())
                .Returns(OperationStatus.Error);

            var current = new SystemConfiguration();
            var sut = CreateSut(current, scriptHandler, bypassHasChanges: true);

            // Act + Assert
            var status = await sut.SetConfiguration(s_proposed, CancellationToken.None);

            status.Should().Be(OperationStatus.Error);
            _ = await scriptHandler.DidNotReceive().ExecuteSetNetworkDNSSettingsScript(Arg.Any<ScriptOptions>());
            _ = await scriptHandler.DidNotReceive().ExecuteSetHostnameScript(Arg.Any<string>());
        }

        [Fact]
        public async Task ShouldReturnError_WhenSetDNSReturnsError()
        {
            // Arrange
            var scriptHandler = Substitute.For<IScriptHandler>();

            scriptHandler
                .ExecuteGetNetworkInterfacesScript()
                .Returns((OperationStatus.Success, RawInterfacesString));

            scriptHandler
                .ExecuteSetNetworkMulticastDNSSettingsScript(Arg.Any<ScriptOptions>())
                .Returns(OperationStatus.Success);

            scriptHandler
                .ExecuteSetStaticHostsScript(Arg.Any<ScriptOptions>())
                .Returns(OperationStatus.Success);

            scriptHandler
                .ExecuteSetNetworkDNSSettingsScript(Arg.Any<ScriptOptions>())
                .Returns(OperationStatus.Error);

            var current = new SystemConfiguration();
            var sut = CreateSut(current, scriptHandler, bypassHasChanges: true);

            // Act + Assert
            var status = await sut.SetConfiguration(s_proposed, CancellationToken.None);

            status.Should().Be(OperationStatus.Error);
            _ = await scriptHandler.DidNotReceive().ExecuteSetHostnameScript(Arg.Any<string>());
        }

        [Fact]
        public async Task ShouldReturnError_WhenSetHostnameReturnsError()
        {
            // Arrange
            var scriptHandler = Substitute.For<IScriptHandler>();

            scriptHandler
                .ExecuteGetNetworkInterfacesScript()
                .Returns((OperationStatus.Success, RawInterfacesString));

            scriptHandler
                .ExecuteSetNetworkMulticastDNSSettingsScript(Arg.Any<ScriptOptions>())
                .Returns(OperationStatus.Success);

            scriptHandler
                .ExecuteSetStaticHostsScript(Arg.Any<ScriptOptions>())
                .Returns(OperationStatus.Success);

            scriptHandler
                .ExecuteSetNetworkDNSSettingsScript(Arg.Any<ScriptOptions>())
                .Returns(OperationStatus.Success);

            scriptHandler
                .ExecuteSetHostnameScript(Arg.Any<string>())
                .Returns(OperationStatus.Error);

            var current = new SystemConfiguration();
            var sut = CreateSut(current, scriptHandler, bypassHasChanges: true);

            // Act + Assert
            var status = await sut.SetConfiguration(s_proposed, CancellationToken.None);

            status.Should().Be(OperationStatus.Error);
        }

        [Fact]
        public async Task ShouldReturnSuccess_WhenNetworkDNSSettingsNull()
        {
            // Arrange
            var scriptHandler = Substitute.For<IScriptHandler>();
            var current = new SystemConfiguration();
            var proposed = new SystemConfiguration() { NetworkDNSSettings = null };
            var sut = CreateSut(current, scriptHandler, bypassHasChanges: true);

            // Act
            var status = await sut.SetConfiguration(proposed, CancellationToken.None);

            // Assert
            status.Should().Be(OperationStatus.Success);

            _ = await scriptHandler.DidNotReceive().ExecuteGetNetworkInterfacesScript();
            _ = await scriptHandler.DidNotReceive().ExecuteSetNetworkMulticastDNSSettingsScript(Arg.Any<ScriptOptions>());
            _ = await scriptHandler.DidNotReceive().ExecuteSetStaticHostsScript(Arg.Any<ScriptOptions>());
            _ = await scriptHandler.DidNotReceive().ExecuteSetNetworkDNSSettingsScript(Arg.Any<ScriptOptions>());
            _ = await scriptHandler.DidNotReceive().ExecuteSetHostnameScript(Arg.Any<string>());
        }

        [Fact]
        public async Task ShouldReturnSuccess_WhenDNSSettingsConfiguredSuccessfully()
        {
            // Arrange
            List<ScriptOptions> captured = [];
            string? hostnameCaptured = default;

            var scriptHandler = Substitute.For<IScriptHandler>();

            scriptHandler
                .ExecuteGetNetworkInterfacesScript()
                .Returns((OperationStatus.Success, RawInterfacesString));

            scriptHandler
                .ExecuteSetNetworkMulticastDNSSettingsScript(Arg.Do<ScriptOptions>(captured.Add))
                .Returns(OperationStatus.Success);

            scriptHandler
                .ExecuteSetStaticHostsScript(Arg.Do<ScriptOptions>(captured.Add))
                .Returns(OperationStatus.Success);

            scriptHandler
                .ExecuteSetNetworkDNSSettingsScript(Arg.Do<ScriptOptions>(captured.Add))
                .Returns(OperationStatus.Success);

            scriptHandler
                .ExecuteSetHostnameScript(Arg.Do<string>(so => hostnameCaptured = so))
                .Returns(OperationStatus.Success);

            var current = new SystemConfiguration();
            var sut = CreateSut(current, scriptHandler, bypassHasChanges: true);

            // Act
            var status = await sut.SetConfiguration(s_proposed, CancellationToken.None);

            // Assert
            status.Should().Be(OperationStatus.Success);
            hostnameCaptured.Should().NotBeNull();
            hostnameCaptured.Should().Be("newhost");

            captured.Should().HaveCount(3).And.SatisfyRespectively(
                so => so.Options.Should().ContainInOrder(
                    ScriptOptionNames.NetworkMulticastDNS.MulticastDNSEnabled, "true",
                    ScriptOptionNames.NetworkMulticastDNS.Interfaces, RawInterfacesString),
                so => so.Options.Should().ContainInOrder(
                    ScriptOptionNames.NetworkDNSStaticHosts.Enabled, "true",
                    ScriptOptionNames.NetworkDNSStaticHosts.IPAddress, "10.0.0.1",
                    ScriptOptionNames.NetworkDNSStaticHosts.Hostname, "local1",
                    ScriptOptionNames.NetworkDNSStaticHosts.IPAddress, "10.0.0.2",
                    ScriptOptionNames.NetworkDNSStaticHosts.Hostname, "local2"),
                so => so.Options.Should().ContainInOrder(
                    ScriptOptionNames.NetworkDNS.NameServersEnabled, "true",
                    ScriptOptionNames.NetworkDNS.NameServers, "1.1.1.1", "8.8.8.8",
                    ScriptOptionNames.NetworkDNS.DNSSuffixEnabled, "true",
                    ScriptOptionNames.NetworkDNS.DNSSuffix, "intranet",
                    ScriptOptionNames.NetworkDNS.SearchDomainsEnabled, "true",
                    ScriptOptionNames.NetworkDNS.SearchDomains, "searchengine", "search.company.com"));
        }

        public sealed class SetMulticastConfiguration
        {
            [Fact]
            public async Task ShouldNotCallMulticastScript_WhenMulticastEnabledIsNull()
            {
                // Arrange
                var scriptHandler = Substitute.For<IScriptHandler>();
                var current = new SystemConfiguration();
                var proposed = new SystemConfiguration() { NetworkDNSSettings = new() { MulticastDNSEnabled = null } };
                var sut = CreateSut(current, scriptHandler, bypassHasChanges: true);

                // Act + Assert
                var status = await sut.SetConfiguration(proposed, CancellationToken.None);

                status.Should().Be(OperationStatus.Success);
                _ = await scriptHandler.DidNotReceive().ExecuteGetNetworkInterfacesScript();
                _ = await scriptHandler.DidNotReceive().ExecuteSetNetworkMulticastDNSSettingsScript(Arg.Any<ScriptOptions>());
            }

            [Theory]
            [InlineData(false)]
            [InlineData(true)]
            public async Task ShouldNotCallMulticastScript_WhenMulticastEnabledUnchanged(bool multicastEnabled)
            {
                // Arrange
                var scriptHandler = Substitute.For<IScriptHandler>();

                var current = new SystemConfiguration()
                {
                    NetworkDNSSettings = new() { MulticastDNSEnabled = multicastEnabled }
                };
                var proposed = new SystemConfiguration()
                {
                    NetworkDNSSettings = new() { MulticastDNSEnabled = multicastEnabled }
                };

                var sut = CreateSut(current, scriptHandler, bypassHasChanges: true);

                // Act + Assert
                var status = await sut.SetConfiguration(proposed, CancellationToken.None);

                status.Should().Be(OperationStatus.Success);
                _ = await scriptHandler.DidNotReceive().ExecuteGetNetworkInterfacesScript();
                _ = await scriptHandler.DidNotReceive().ExecuteSetNetworkMulticastDNSSettingsScript(Arg.Any<ScriptOptions>());
            }
        }

        public sealed class SetStaticHostsConfiguration
        {
            [Fact]
            public async Task ShouldNotCallStaticHostsScript_WhenStaticHostsEnabledNull()
            {
                // Arrange
                var scriptHandler = Substitute.For<IScriptHandler>();
                var current = new SystemConfiguration();
                var proposed = new SystemConfiguration()
                {
                    NetworkDNSSettings = new()
                    {
                        StaticHostsEnabled = null,
                        StaticHosts = s_proposedDNS.StaticHosts
                    }
                };

                var sut = CreateSut(current, scriptHandler, bypassHasChanges: true);

                // Act + Assert
                var status = await sut.SetConfiguration(proposed, CancellationToken.None);

                status.Should().Be(OperationStatus.Success);
                _ = await scriptHandler.DidNotReceive().ExecuteSetStaticHostsScript(Arg.Any<ScriptOptions>());
            }

            [Fact]
            public async Task ShouldCallStaticHostsScript_WhenStaticHostsListNull()
            {
                // Arrange
                ScriptOptions? staticHostsOptionsCaptured = default;
                var scriptHandler = Substitute.For<IScriptHandler>();

                scriptHandler
                    .ExecuteSetStaticHostsScript(Arg.Do<ScriptOptions>(so => staticHostsOptionsCaptured = so))
                    .Returns(OperationStatus.Success);

                var current = new SystemConfiguration();
                var proposed = new SystemConfiguration()
                {
                    NetworkDNSSettings = new()
                    {
                        StaticHostsEnabled = s_proposedDNS.StaticHostsEnabled,
                        StaticHosts = null
                    }
                };

                var sut = CreateSut(current, scriptHandler, bypassHasChanges: true);

                // Act
                var status = await sut.SetConfiguration(proposed, CancellationToken.None);

                // Assert
                status.Should().Be(OperationStatus.Success);
                _ = await scriptHandler.Received().ExecuteSetStaticHostsScript(Arg.Any<ScriptOptions>());

                staticHostsOptionsCaptured.Should().NotBeNull();
                staticHostsOptionsCaptured.Options.Should().ContainInOrder(ScriptOptionNames.NetworkDNSStaticHosts.Enabled, "true");
                staticHostsOptionsCaptured.Options.Should().NotContain(ScriptOptionNames.NetworkDNSStaticHosts.IPAddress);
                staticHostsOptionsCaptured.Options.Should().NotContain(ScriptOptionNames.NetworkDNSStaticHosts.Hostname);
            }
        }

        public sealed class SetDNSConfiguration
        {
            [Theory]
            [InlineData(null, null)]
            [InlineData(null, "")]
            [InlineData(null, "1.1.1.1")]
            [InlineData(false, null)]
            [InlineData(false, "")]
            [InlineData(true, null)]
            [InlineData(true, "")]
            public async Task ShouldNotAddNameServerOptions_WhenNoNameServerProvided(bool? enabled, string? value)
            {
                // Arrange
                var current = new SystemConfiguration();
                var proposed = new SystemConfiguration()
                {
                    NetworkDNSSettings = new()
                    {
                        Hostname = "changed",
                        NameServersEnabled = enabled,
                        NameServers = value is null
                            ? null
                            : string.IsNullOrWhiteSpace(value)
                                ? []
                                : [IPAddress.Parse(value)]
                    }
                };

                var scriptHandler = Substitute.For<IScriptHandler>();
                var sut = CreateSut(current, scriptHandler, bypassHasChanges: true);

                // Act + Assert
                var status = await sut.SetConfiguration(proposed, CancellationToken.None);

                status.Should().Be(OperationStatus.Success);
                _ = await scriptHandler.DidNotReceive().ExecuteSetNetworkDNSSettingsScript(Arg.Any<ScriptOptions>());
                _ = await scriptHandler.Received().ExecuteSetHostnameScript(Arg.Any<string>());
            }

            [Theory]
            [InlineData(null, null)]
            [InlineData(null, "search.company.com")]
            [InlineData(false, null)]
            [InlineData(true, null)]
            public async Task ShouldNotAddSuffixOptions_WhenNoSuffixProvided(bool? enabled, string? value)
            {
                // Arrange
                var current = new SystemConfiguration();
                var proposed = new SystemConfiguration()
                {
                    NetworkDNSSettings = new()
                    {
                        Hostname = "changed",
                        DNSSuffixEnabled = enabled,
                        DNSSuffix = value
                    }
                };

                var scriptHandler = Substitute.For<IScriptHandler>();
                var sut = CreateSut(current, scriptHandler, bypassHasChanges: true);

                // Act + Assert
                var status = await sut.SetConfiguration(proposed, CancellationToken.None);

                status.Should().Be(OperationStatus.Success);
                _ = await scriptHandler.DidNotReceive().ExecuteSetNetworkDNSSettingsScript(Arg.Any<ScriptOptions>());
                _ = await scriptHandler.Received().ExecuteSetHostnameScript(Arg.Any<string>());
            }

            [Theory]
            [InlineData(null, null)]
            [InlineData(null, "")]
            [InlineData(null, "search.company.com")]
            [InlineData(false, null)]
            [InlineData(false, "")]
            [InlineData(true, null)]
            [InlineData(true, "")]
            public async Task ShouldNotAddSearchDomainOptions_WhenNoSearchDomainsProvided(bool? enabled, string? value)
            {
                // Arrange
                var current = new SystemConfiguration();
                var proposed = new SystemConfiguration()
                {
                    NetworkDNSSettings = new()
                    {
                        Hostname = "changed",
                        SearchDomainsEnabled = enabled,
                        SearchDomains = value is null
                            ? null
                            : string.IsNullOrWhiteSpace(value)
                                ? []
                                : [value]
                    }
                };

                var scriptHandler = Substitute.For<IScriptHandler>();
                var sut = CreateSut(current, scriptHandler, bypassHasChanges: true);

                // Act + Assert
                var status = await sut.SetConfiguration(proposed, CancellationToken.None);

                status.Should().Be(OperationStatus.Success);
                _ = await scriptHandler.DidNotReceive().ExecuteSetNetworkDNSSettingsScript(Arg.Any<ScriptOptions>());
                _ = await scriptHandler.Received().ExecuteSetHostnameScript(Arg.Any<string>());
            }
        }

        public sealed class SetHostname
        {
            [Fact]
            public async Task ShouldNotCallHostnameScript_WhenHostnameNull()
            {
                // Arrange
                var current = new SystemConfiguration()
                {
                    NetworkDNSSettings = new() { Hostname = s_proposedDNS.Hostname }
                };

                var proposed = new SystemConfiguration()
                {
                    NetworkDNSSettings = new() { Hostname = null }
                };

                var scriptHandler = Substitute.For<IScriptHandler>();
                var sut = CreateSut(current, scriptHandler, bypassHasChanges: true);

                // Act + Assert
                var status = await sut.SetConfiguration(proposed, CancellationToken.None);

                status.Should().Be(OperationStatus.Success);
                _ = await scriptHandler.DidNotReceive().ExecuteSetHostnameScript(Arg.Any<string>());
            }

            [Fact]
            public async Task ShouldNotCallHostnameScript_WhenHostnameUnchanged()
            {
                // Arrange
                var current = new SystemConfiguration()
                {
                    NetworkDNSSettings = new() { Hostname = s_proposedDNS.Hostname }
                };

                var proposed = new SystemConfiguration()
                {
                    NetworkDNSSettings = new() { Hostname = s_proposedDNS.Hostname }
                };

                var scriptHandler = Substitute.For<IScriptHandler>();
                var sut = CreateSut(current, scriptHandler, bypassHasChanges: true);

                // Act + Assert
                var status = await sut.SetConfiguration(proposed, CancellationToken.None);

                status.Should().Be(OperationStatus.Success);
                _ = await scriptHandler.DidNotReceive().ExecuteSetHostnameScript(Arg.Any<string>());
            }
        }
    }
}
