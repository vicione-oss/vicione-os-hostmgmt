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
            scriptHandler.ExecuteGetNetworkDNSSettingsScript().Returns((Result.ErrorResult, new()));

            var current = new SystemConfiguration();
            var config = new SystemConfiguration();
            var dnsSettings = config.NetworkDNSSettings;
            var sut = CreateSut(current, scriptHandler);

            // Act + Assert
            var result = await sut.GetConfiguration(config, CancellationToken.None);

            result.OperationStatus.Should().Be(OperationStatus.Error);
            result.Messages.Should().Contain(Result.ErrorMessage);
            config.NetworkDNSSettings.Should().BeSameAs(dnsSettings);
        }

        [Fact]
        public async Task ShouldPopulate_WhenSuccess()
        {
            // Arrange
            var dnsSettings = new NetworkDNSSettings()
            {
                NameServers = new NameServersSettings()
                {
                    Enabled = true,
                    Addresses = [IPAddress.Parse("1.1.1.1")]
                },
                Hostname = "host"
            };

            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler.ExecuteGetNetworkDNSSettingsScript().Returns((Result.SuccessResult, dnsSettings));

            var current = new SystemConfiguration();
            var config = new SystemConfiguration();
            var sut = CreateSut(current, scriptHandler);

            // Act + Assert
            var result = await sut.GetConfiguration(config, CancellationToken.None);

            result.OperationStatus.Should().Be(OperationStatus.Success);
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
            StaticHosts = new StaticHostsSettings()
            {
                Enabled = true,
                Hosts =
                [
                    new StaticHostDetail { Hostname = "local1", IPAddress = IPAddress.Parse("10.0.0.1") },
                    new StaticHostDetail { Hostname = "local2", IPAddress = IPAddress.Parse("10.0.0.2") }
                ]
            },
            NameServers = new NameServersSettings()
            {
                Enabled = true,
                Addresses = [IPAddress.Parse("1.1.1.1"), IPAddress.Parse("8.8.8.8")]
            },
            PrimaryDNSSuffix = new PrimaryDNSSuffixSettings()
            {
                Enabled = true,
                Suffix = "intranet"
            },
            SearchDomainsEnabled = true,
            SearchDomains = ["searchengine", "search.company.com"]
        };

        private static readonly SystemConfiguration s_proposed = new() { NetworkDNSSettings = s_proposedDNS };

        [Fact]
        public async Task ShouldReturnError_WhenSetMulticastReturnsErrorFromGetInterfacesCall()
        {
            // Arrange
            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler.ExecuteGetNetworkInterfacesScript().Returns((Result.ErrorResult, ""));

            var current = new SystemConfiguration();
            var sut = CreateSut(current, scriptHandler, bypassHasChanges: true);

            // Act + Assert
            var result = await sut.SetConfiguration(s_proposed, CancellationToken.None);

            result.OperationStatus.Should().Be(OperationStatus.Error);
            result.Messages.Should().Contain(Result.ErrorMessage);

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
                .Returns((Result.SuccessResult, RawInterfacesString));

            scriptHandler
                .ExecuteSetNetworkMulticastDNSSettingsScript(Arg.Any<ScriptOptions>())
                .Returns(Result.ErrorResult);

            var current = new SystemConfiguration();
            var sut = CreateSut(current, scriptHandler, bypassHasChanges: true);

            // Act + Assert
            var result = await sut.SetConfiguration(s_proposed, CancellationToken.None);

            result.OperationStatus.Should().Be(OperationStatus.Error);
            result.Messages.Should().Contain(Result.ErrorMessage);

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
                .Returns((Result.SuccessResult, RawInterfacesString));

            scriptHandler
                .ExecuteSetNetworkMulticastDNSSettingsScript(Arg.Any<ScriptOptions>())
                .Returns(Result.SuccessResult);

            scriptHandler
                .ExecuteSetStaticHostsScript(Arg.Any<ScriptOptions>())
                .Returns(Result.ErrorResult);

            var current = new SystemConfiguration();
            var sut = CreateSut(current, scriptHandler, bypassHasChanges: true);

            // Act + Assert
            var result = await sut.SetConfiguration(s_proposed, CancellationToken.None);

            result.OperationStatus.Should().Be(OperationStatus.Error);
            result.Messages.Should().Contain(Result.ErrorMessage);

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
                .Returns((Result.SuccessResult, RawInterfacesString));

            scriptHandler
                .ExecuteSetNetworkMulticastDNSSettingsScript(Arg.Any<ScriptOptions>())
                .Returns(Result.SuccessResult);

            scriptHandler
                .ExecuteSetStaticHostsScript(Arg.Any<ScriptOptions>())
                .Returns(Result.SuccessResult);

            scriptHandler
                .ExecuteSetNetworkDNSSettingsScript(Arg.Any<ScriptOptions>())
                .Returns(Result.ErrorResult);

            var current = new SystemConfiguration();
            var sut = CreateSut(current, scriptHandler, bypassHasChanges: true);

            // Act + Assert
            var result = await sut.SetConfiguration(s_proposed, CancellationToken.None);

            result.OperationStatus.Should().Be(OperationStatus.Error);
            result.Messages.Should().Contain(Result.ErrorMessage);

            _ = await scriptHandler.DidNotReceive().ExecuteSetHostnameScript(Arg.Any<string>());
        }

        [Fact]
        public async Task ShouldReturnError_WhenSetHostnameReturnsError()
        {
            // Arrange
            var scriptHandler = Substitute.For<IScriptHandler>();

            scriptHandler
                .ExecuteGetNetworkInterfacesScript()
                .Returns((Result.SuccessResult, RawInterfacesString));

            scriptHandler
                .ExecuteSetNetworkMulticastDNSSettingsScript(Arg.Any<ScriptOptions>())
                .Returns(Result.SuccessResult);

            scriptHandler
                .ExecuteSetStaticHostsScript(Arg.Any<ScriptOptions>())
                .Returns(Result.SuccessResult);

            scriptHandler
                .ExecuteSetNetworkDNSSettingsScript(Arg.Any<ScriptOptions>())
                .Returns(Result.SuccessResult);

            scriptHandler
                .ExecuteSetHostnameScript(Arg.Any<string>())
                .Returns(Result.ErrorResult);

            var current = new SystemConfiguration();
            var sut = CreateSut(current, scriptHandler, bypassHasChanges: true);

            // Act + Assert
            var result = await sut.SetConfiguration(s_proposed, CancellationToken.None);

            result.OperationStatus.Should().Be(OperationStatus.Error);
            result.Messages.Should().Contain(Result.ErrorMessage);
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
            var result = await sut.SetConfiguration(proposed, CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Success);
            result.Messages.Should().BeEmpty();

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
                .Returns((Result.SuccessResult, RawInterfacesString));

            scriptHandler
                .ExecuteSetNetworkMulticastDNSSettingsScript(Arg.Do<ScriptOptions>(captured.Add))
                .Returns(Result.SuccessResult);

            scriptHandler
                .ExecuteSetStaticHostsScript(Arg.Do<ScriptOptions>(captured.Add))
                .Returns(Result.SuccessResult);

            scriptHandler
                .ExecuteSetNetworkDNSSettingsScript(Arg.Do<ScriptOptions>(captured.Add))
                .Returns(Result.SuccessResult);

            scriptHandler
                .ExecuteSetHostnameScript(Arg.Do<string>(so => hostnameCaptured = so))
                .Returns(Result.SuccessResult);

            var current = new SystemConfiguration();
            var sut = CreateSut(current, scriptHandler, bypassHasChanges: true);

            // Act
            var result = await sut.SetConfiguration(s_proposed, CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Success);
            result.Messages.Should().BeEmpty();

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
                var result = await sut.SetConfiguration(proposed, CancellationToken.None);

                result.OperationStatus.Should().Be(OperationStatus.Success);
                result.Messages.Should().BeEmpty();

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
                var result = await sut.SetConfiguration(proposed, CancellationToken.None);

                result.OperationStatus.Should().Be(OperationStatus.Success);
                result.Messages.Should().BeEmpty();

                _ = await scriptHandler.DidNotReceive().ExecuteGetNetworkInterfacesScript();
                _ = await scriptHandler.DidNotReceive().ExecuteSetNetworkMulticastDNSSettingsScript(Arg.Any<ScriptOptions>());
            }
        }

        public sealed class SetStaticHostsConfiguration
        {
            [Fact]
            public async Task ShouldNotCallStaticHostsScript_WhenEnabledNull()
            {
                // Arrange
                var scriptHandler = Substitute.For<IScriptHandler>();
                var current = new SystemConfiguration();
                var proposed = new SystemConfiguration()
                {
                    NetworkDNSSettings = new()
                    {
                        StaticHosts = new()
                        {
                            Enabled = null,
                            Hosts = s_proposedDNS.StaticHosts!.Hosts
                        }
                    }
                };

                var sut = CreateSut(current, scriptHandler, bypassHasChanges: true);

                // Act + Assert
                var result = await sut.SetConfiguration(proposed, CancellationToken.None);

                result.OperationStatus.Should().Be(OperationStatus.Success);
                result.Messages.Should().BeEmpty();

                _ = await scriptHandler.DidNotReceive().ExecuteSetStaticHostsScript(Arg.Any<ScriptOptions>());
            }

            [Fact]
            public async Task ShouldCallStaticHostsScript_WhenHostsNull()
            {
                // Arrange
                ScriptOptions? staticHostsOptionsCaptured = default;
                var scriptHandler = Substitute.For<IScriptHandler>();

                scriptHandler
                    .ExecuteSetStaticHostsScript(Arg.Do<ScriptOptions>(so => staticHostsOptionsCaptured = so))
                    .Returns(Result.SuccessResult);

                var current = new SystemConfiguration();
                var proposed = new SystemConfiguration()
                {
                    NetworkDNSSettings = new()
                    {
                        StaticHosts = new()
                        {
                            Enabled = s_proposedDNS.StaticHosts!.Enabled,
                            Hosts = null
                        }
                    }
                };

                var sut = CreateSut(current, scriptHandler, bypassHasChanges: true);

                // Act
                var result = await sut.SetConfiguration(proposed, CancellationToken.None);

                // Assert
                result.OperationStatus.Should().Be(OperationStatus.Success);
                result.Messages.Should().BeEmpty();

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
                        NameServers = new()
                        {
                            Enabled = enabled,
                            Addresses = value is null
                                ? null
                                : string.IsNullOrWhiteSpace(value)
                                    ? []
                                    : [IPAddress.Parse(value)]
                        }
                    }
                };

                var scriptHandler = Substitute.For<IScriptHandler>();
                scriptHandler.ExecuteSetHostnameScript("changed").Returns(Result.SuccessResult);

                var sut = CreateSut(current, scriptHandler, bypassHasChanges: true);

                // Act + Assert
                var result = await sut.SetConfiguration(proposed, CancellationToken.None);

                result.OperationStatus.Should().Be(OperationStatus.Success);
                result.Messages.Should().BeEmpty();

                _ = await scriptHandler.DidNotReceive().ExecuteSetNetworkDNSSettingsScript(Arg.Any<ScriptOptions>());
                _ = await scriptHandler.Received(1).ExecuteSetHostnameScript("changed");
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
                        PrimaryDNSSuffix = new()
                        {
                            Enabled = enabled,
                            Suffix = value
                        }
                    }
                };

                var scriptHandler = Substitute.For<IScriptHandler>();
                scriptHandler.ExecuteSetHostnameScript("changed").Returns(Result.SuccessResult);

                var sut = CreateSut(current, scriptHandler, bypassHasChanges: true);

                // Act + Assert
                var result = await sut.SetConfiguration(proposed, CancellationToken.None);

                result.OperationStatus.Should().Be(OperationStatus.Success);
                result.Messages.Should().BeEmpty();

                _ = await scriptHandler.DidNotReceive().ExecuteSetNetworkDNSSettingsScript(Arg.Any<ScriptOptions>());
                _ = await scriptHandler.Received(1).ExecuteSetHostnameScript("changed");
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
                scriptHandler.ExecuteSetHostnameScript("changed").Returns(Result.SuccessResult);

                var sut = CreateSut(current, scriptHandler, bypassHasChanges: true);

                // Act + Assert
                var result = await sut.SetConfiguration(proposed, CancellationToken.None);

                result.OperationStatus.Should().Be(OperationStatus.Success);
                result.Messages.Should().BeEmpty();

                _ = await scriptHandler.DidNotReceive().ExecuteSetNetworkDNSSettingsScript(Arg.Any<ScriptOptions>());
                _ = await scriptHandler.Received(1).ExecuteSetHostnameScript("changed");
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
                var result = await sut.SetConfiguration(proposed, CancellationToken.None);

                result.OperationStatus.Should().Be(OperationStatus.Success);
                result.Messages.Should().BeEmpty();

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
                var result = await sut.SetConfiguration(proposed, CancellationToken.None);

                result.OperationStatus.Should().Be(OperationStatus.Success);
                result.Messages.Should().BeEmpty();

                _ = await scriptHandler.DidNotReceive().ExecuteSetHostnameScript(Arg.Any<string>());
            }
        }
    }
}
