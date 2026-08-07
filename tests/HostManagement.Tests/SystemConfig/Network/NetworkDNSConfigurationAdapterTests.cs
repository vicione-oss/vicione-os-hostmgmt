using System.Net;
using AwesomeAssertions;
using HostManagement.Scripts;
using HostManagement.Shared.Communication.Enums;
using HostManagement.SystemConfig.Contracts;
using HostManagement.SystemConfig.Network;
using NSubstitute;
using Xunit;

namespace HostManagement.Tests.SystemConfig.Network;

public sealed class NetworkDNSConfigurationAdapterTests
{
    private const string RawInterfacesString =
        "lan1            UP             00:50:56:36:6d:33 <BROADCAST,MULTICAST,UP,LOWER_UP>";

    private static NetworkDNSConfigurationAdapter CreateSut(
        SystemConfiguration current,
        IScriptHandler handler,
        bool bypassHasChanges = false,
        bool bypassApply = false)
           => new(current, handler, bypassHasChanges, bypassApply);

    public sealed class GetConfiguration
    {
        [Fact]
        public async Task ShouldReturnError_WhenScriptFails()
        {
            // Arrange
            var handler = Substitute.For<IScriptHandler>();
            handler.ExecuteGetNetworkDNSSettingsScript().Returns((Result.ErrorResult, new()));

            var current = new SystemConfiguration();
            var config = new SystemConfiguration();
            var dnsSettings = config.NetworkDNSSettings;
            var sut = CreateSut(current, handler);

            // Act
            var result = await sut.GetConfiguration(config, CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Error);
            result.Messages.Should().Contain(Result.ErrorMessage);
            config.NetworkDNSSettings.Should().BeSameAs(dnsSettings);
        }

        [Fact]
        public async Task ShouldPopulateSettings_WhenScriptSucceeds()
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

            var handler = Substitute.For<IScriptHandler>();
            handler.ExecuteGetNetworkDNSSettingsScript().Returns((Result.SuccessResult, dnsSettings));

            var current = new SystemConfiguration();
            var config = new SystemConfiguration();
            var sut = CreateSut(current, handler);

            // Act
            var result = await sut.GetConfiguration(config, CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Success);
            config.NetworkDNSSettings.Should().Be(dnsSettings);
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
            var sut = CreateSut(current, handler);

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
                        CommonInformation = new() { Enabled = true, Name = "lan1" },
                        IPv4 = new()
                    }
                ]
            };

            var sut = CreateSut(current, handler);

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
            var proposed = new SystemConfiguration() { NetworkDNSSettings = new() { Hostname = "new" } };
            var sut = CreateSut(current, handler);

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
            var sut = CreateSut(current, handler, bypassHasChanges: true);

            // Act
            var result = sut.HasChanges(current, proposed);

            // Assert
            result.Should().BeTrue();
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
            SearchDomains = new SearchDomainsSettings()
            {
                Enabled = true,
                Domains = ["searchengine", "search.company.com"]
            }
        };

        private static readonly SystemConfiguration s_proposed = new() { NetworkDNSSettings = s_proposedDNS };

        [Fact]
        public async Task ShouldReturnError_WhenGettingInterfacesForMulticastDNSFails()
        {
            // Arrange
            var handler = Substitute.For<IScriptHandler>();
            handler.ExecuteGetNetworkInterfacesScript().Returns((Result.ErrorResult, ""));

            var current = new SystemConfiguration();
            var sut = CreateSut(current, handler, bypassHasChanges: true);

            // Act
            var result = await sut.SetConfiguration(s_proposed, CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Error);
            result.Messages.Should().Contain(Result.ErrorMessage);

            _ = await handler.DidNotReceive().ExecuteSetNetworkMulticastDNSSettingsScript(Arg.Any<ScriptOptions>());
            _ = await handler.DidNotReceive().ExecuteSetStaticHostsScript(Arg.Any<ScriptOptions>());
            _ = await handler.DidNotReceive().ExecuteSetNetworkDNSSettingsScript(Arg.Any<ScriptOptions>());
            _ = await handler.DidNotReceive().ExecuteSetHostnameScript(Arg.Any<string>());
        }

        [Fact]
        public async Task ShouldReturnError_WhenSettingMulticastDNSFails()
        {
            // Arrange
            var handler = Substitute.For<IScriptHandler>();

            handler
                .ExecuteGetNetworkInterfacesScript()
                .Returns((Result.SuccessResult, RawInterfacesString));

            handler
                .ExecuteSetNetworkMulticastDNSSettingsScript(Arg.Any<ScriptOptions>())
                .Returns(Result.ErrorResult);

            var current = new SystemConfiguration();
            var sut = CreateSut(current, handler, bypassHasChanges: true);

            // Act
            var result = await sut.SetConfiguration(s_proposed, CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Error);
            result.Messages.Should().Contain(Result.ErrorMessage);

            _ = await handler.DidNotReceive().ExecuteSetStaticHostsScript(Arg.Any<ScriptOptions>());
            _ = await handler.DidNotReceive().ExecuteSetNetworkDNSSettingsScript(Arg.Any<ScriptOptions>());
            _ = await handler.DidNotReceive().ExecuteSetHostnameScript(Arg.Any<string>());
        }

        [Fact]
        public async Task ShouldReturnError_WhenSettingStaticHostsFails()
        {
            // Arrange
            var handler = Substitute.For<IScriptHandler>();

            handler
                .ExecuteGetNetworkInterfacesScript()
                .Returns((Result.SuccessResult, RawInterfacesString));

            handler
                .ExecuteSetNetworkMulticastDNSSettingsScript(Arg.Any<ScriptOptions>())
                .Returns(Result.SuccessResult);

            handler
                .ExecuteSetStaticHostsScript(Arg.Any<ScriptOptions>())
                .Returns(Result.ErrorResult);

            var current = new SystemConfiguration();
            var sut = CreateSut(current, handler, bypassHasChanges: true);

            // Act
            var result = await sut.SetConfiguration(s_proposed, CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Error);
            result.Messages.Should().Contain(Result.ErrorMessage);

            _ = await handler.DidNotReceive().ExecuteSetNetworkDNSSettingsScript(Arg.Any<ScriptOptions>());
            _ = await handler.DidNotReceive().ExecuteSetHostnameScript(Arg.Any<string>());
        }

        [Fact]
        public async Task ShouldReturnError_WhenSettingDNSFails()
        {
            // Arrange
            var handler = Substitute.For<IScriptHandler>();

            handler
                .ExecuteGetNetworkInterfacesScript()
                .Returns((Result.SuccessResult, RawInterfacesString));

            handler
                .ExecuteSetNetworkMulticastDNSSettingsScript(Arg.Any<ScriptOptions>())
                .Returns(Result.SuccessResult);

            handler
                .ExecuteSetStaticHostsScript(Arg.Any<ScriptOptions>())
                .Returns(Result.SuccessResult);

            handler
                .ExecuteSetNetworkDNSSettingsScript(Arg.Any<ScriptOptions>())
                .Returns(Result.ErrorResult);

            var current = new SystemConfiguration();
            var sut = CreateSut(current, handler, bypassHasChanges: true);

            // Act
            var result = await sut.SetConfiguration(s_proposed, CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Error);
            result.Messages.Should().Contain(Result.ErrorMessage);

            _ = await handler.DidNotReceive().ExecuteSetHostnameScript(Arg.Any<string>());
        }

        [Fact]
        public async Task ShouldReturnError_WhenSettingHostnameFails()
        {
            // Arrange
            var handler = Substitute.For<IScriptHandler>();

            handler
                .ExecuteGetNetworkInterfacesScript()
                .Returns((Result.SuccessResult, RawInterfacesString));

            handler
                .ExecuteSetNetworkMulticastDNSSettingsScript(Arg.Any<ScriptOptions>())
                .Returns(Result.SuccessResult);

            handler
                .ExecuteSetStaticHostsScript(Arg.Any<ScriptOptions>())
                .Returns(Result.SuccessResult);

            handler
                .ExecuteSetNetworkDNSSettingsScript(Arg.Any<ScriptOptions>())
                .Returns(Result.SuccessResult);

            handler
                .ExecuteSetHostnameScript(Arg.Any<string>())
                .Returns(Result.ErrorResult);

            var current = new SystemConfiguration();
            var sut = CreateSut(current, handler, bypassHasChanges: true);

            // Act
            var result = await sut.SetConfiguration(s_proposed, CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Error);
            result.Messages.Should().Contain(Result.ErrorMessage);
        }

        [Fact]
        public async Task ShouldReturnSuccess_WhenNetworkDNSSettingsNull()
        {
            // Arrange
            var handler = Substitute.For<IScriptHandler>();
            var current = new SystemConfiguration();
            var proposed = new SystemConfiguration() { NetworkDNSSettings = null };
            var sut = CreateSut(current, handler, bypassHasChanges: true);

            // Act
            var result = await sut.SetConfiguration(proposed, CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Success);
            result.Messages.Should().BeEmpty();

            _ = await handler.DidNotReceive().ExecuteGetNetworkInterfacesScript();
            _ = await handler.DidNotReceive().ExecuteSetNetworkMulticastDNSSettingsScript(Arg.Any<ScriptOptions>());
            _ = await handler.DidNotReceive().ExecuteSetStaticHostsScript(Arg.Any<ScriptOptions>());
            _ = await handler.DidNotReceive().ExecuteSetNetworkDNSSettingsScript(Arg.Any<ScriptOptions>());
            _ = await handler.DidNotReceive().ExecuteSetHostnameScript(Arg.Any<string>());
        }

        [Fact]
        public async Task ShouldReturnSuccess_WhenAllDNSSettingsScriptsSucceed()
        {
            // Arrange
            List<ScriptOptions> capturedOptions = [];
            string? capturedHostname = default;

            var handler = Substitute.For<IScriptHandler>();

            handler
                .ExecuteGetNetworkInterfacesScript()
                .Returns((Result.SuccessResult, RawInterfacesString));

            handler
                .ExecuteSetNetworkMulticastDNSSettingsScript(Arg.Do<ScriptOptions>(capturedOptions.Add))
                .Returns(Result.SuccessResult);

            handler
                .ExecuteSetStaticHostsScript(Arg.Do<ScriptOptions>(capturedOptions.Add))
                .Returns(Result.SuccessResult);

            handler
                .ExecuteSetNetworkDNSSettingsScript(Arg.Do<ScriptOptions>(capturedOptions.Add))
                .Returns(Result.SuccessResult);

            handler
                .ExecuteSetHostnameScript(Arg.Do<string>(so => capturedHostname = so))
                .Returns(Result.SuccessResult);

            var current = new SystemConfiguration();
            var sut = CreateSut(current, handler, bypassHasChanges: true);

            // Act
            var result = await sut.SetConfiguration(s_proposed, CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Success);
            result.Messages.Should().BeEmpty();

            capturedHostname.Should().NotBeNull();
            capturedHostname.Should().Be("newhost");

            capturedOptions.Should().HaveCount(3).And.SatisfyRespectively(
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
                var handler = Substitute.For<IScriptHandler>();
                var current = new SystemConfiguration();
                var proposed = new SystemConfiguration() { NetworkDNSSettings = new() { MulticastDNSEnabled = null } };
                var sut = CreateSut(current, handler, bypassHasChanges: true);

                // Act
                var result = await sut.SetConfiguration(proposed, CancellationToken.None);

                // Assert
                result.OperationStatus.Should().Be(OperationStatus.Success);
                result.Messages.Should().BeEmpty();

                _ = await handler.DidNotReceive().ExecuteGetNetworkInterfacesScript();
                _ = await handler.DidNotReceive().ExecuteSetNetworkMulticastDNSSettingsScript(Arg.Any<ScriptOptions>());
            }

            [Theory]
            [InlineData(false)]
            [InlineData(true)]
            public async Task ShouldNotCallMulticastScript_WhenMulticastEnabledUnchanged(bool multicastEnabled)
            {
                // Arrange
                var handler = Substitute.For<IScriptHandler>();

                var current = new SystemConfiguration()
                {
                    NetworkDNSSettings = new() { MulticastDNSEnabled = multicastEnabled }
                };
                var proposed = new SystemConfiguration()
                {
                    NetworkDNSSettings = new() { MulticastDNSEnabled = multicastEnabled }
                };

                var sut = CreateSut(current, handler, bypassHasChanges: true);

                // Act
                var result = await sut.SetConfiguration(proposed, CancellationToken.None);

                // Assert
                result.OperationStatus.Should().Be(OperationStatus.Success);
                result.Messages.Should().BeEmpty();

                _ = await handler.DidNotReceive().ExecuteGetNetworkInterfacesScript();
                _ = await handler.DidNotReceive().ExecuteSetNetworkMulticastDNSSettingsScript(Arg.Any<ScriptOptions>());
            }
        }

        public sealed class SetStaticHostsConfiguration
        {
            [Fact]
            public async Task ShouldNotCallStaticHostsScript_WhenEnabledNull()
            {
                // Arrange
                var handler = Substitute.For<IScriptHandler>();
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

                var sut = CreateSut(current, handler, bypassHasChanges: true);

                // Act
                var result = await sut.SetConfiguration(proposed, CancellationToken.None);

                // Assert
                result.OperationStatus.Should().Be(OperationStatus.Success);
                result.Messages.Should().BeEmpty();

                _ = await handler.DidNotReceive().ExecuteSetStaticHostsScript(Arg.Any<ScriptOptions>());
            }

            [Fact]
            public async Task ShouldCallStaticHostsScript_WhenHostsNull()
            {
                // Arrange
                ScriptOptions? capturedOptions = default;
                var handler = Substitute.For<IScriptHandler>();

                handler
                    .ExecuteSetStaticHostsScript(Arg.Do<ScriptOptions>(so => capturedOptions = so))
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

                var sut = CreateSut(current, handler, bypassHasChanges: true);

                // Act
                var result = await sut.SetConfiguration(proposed, CancellationToken.None);

                // Assert
                result.OperationStatus.Should().Be(OperationStatus.Success);
                result.Messages.Should().BeEmpty();

                _ = await handler.Received().ExecuteSetStaticHostsScript(Arg.Any<ScriptOptions>());

                capturedOptions.Should().NotBeNull();
                capturedOptions.Options.Should().ContainInOrder(ScriptOptionNames.NetworkDNSStaticHosts.Enabled, "true");
                capturedOptions.Options.Should().NotContain(ScriptOptionNames.NetworkDNSStaticHosts.IPAddress);
                capturedOptions.Options.Should().NotContain(ScriptOptionNames.NetworkDNSStaticHosts.Hostname);
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
                var handler = Substitute.For<IScriptHandler>();
                handler.ExecuteSetHostnameScript("changed").Returns(Result.SuccessResult);

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

                var sut = CreateSut(current, handler, bypassHasChanges: true);

                // Act
                var result = await sut.SetConfiguration(proposed, CancellationToken.None);

                // Assert
                result.OperationStatus.Should().Be(OperationStatus.Success);
                result.Messages.Should().BeEmpty();

                _ = await handler.DidNotReceive().ExecuteSetNetworkDNSSettingsScript(Arg.Any<ScriptOptions>());
                _ = await handler.Received(1).ExecuteSetHostnameScript("changed");
            }

            [Theory]
            [InlineData(null, null)]
            [InlineData(null, "search.company.com")]
            [InlineData(false, null)]
            [InlineData(true, null)]
            public async Task ShouldNotAddSuffixOptions_WhenNoSuffixProvided(bool? enabled, string? value)
            {
                // Arrange
                var handler = Substitute.For<IScriptHandler>();
                handler.ExecuteSetHostnameScript("changed").Returns(Result.SuccessResult);

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

                var sut = CreateSut(current, handler, bypassHasChanges: true);

                // Act
                var result = await sut.SetConfiguration(proposed, CancellationToken.None);

                // Assert
                result.OperationStatus.Should().Be(OperationStatus.Success);
                result.Messages.Should().BeEmpty();

                _ = await handler.DidNotReceive().ExecuteSetNetworkDNSSettingsScript(Arg.Any<ScriptOptions>());
                _ = await handler.Received(1).ExecuteSetHostnameScript("changed");
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
                var handler = Substitute.For<IScriptHandler>();
                handler.ExecuteSetHostnameScript("changed").Returns(Result.SuccessResult);

                var current = new SystemConfiguration();
                var proposed = new SystemConfiguration()
                {
                    NetworkDNSSettings = new()
                    {
                        Hostname = "changed",
                        SearchDomains = new()
                        {
                            Enabled = enabled,
                            Domains = value is null
                                ? null
                                : string.IsNullOrWhiteSpace(value)
                                    ? []
                                    : [value]
                        }
                    }
                };

                var sut = CreateSut(current, handler, bypassHasChanges: true);

                // Act
                var result = await sut.SetConfiguration(proposed, CancellationToken.None);

                // Assert
                result.OperationStatus.Should().Be(OperationStatus.Success);
                result.Messages.Should().BeEmpty();

                _ = await handler.DidNotReceive().ExecuteSetNetworkDNSSettingsScript(Arg.Any<ScriptOptions>());
                _ = await handler.Received(1).ExecuteSetHostnameScript("changed");
            }
        }

        public sealed class SetHostname
        {
            [Fact]
            public async Task ShouldNotCallHostnameScript_WhenHostnameNull()
            {
                // Arrange
                var handler = Substitute.For<IScriptHandler>();

                var current = new SystemConfiguration()
                {
                    NetworkDNSSettings = new() { Hostname = s_proposedDNS.Hostname }
                };

                var proposed = new SystemConfiguration()
                {
                    NetworkDNSSettings = new() { Hostname = null }
                };

                var sut = CreateSut(current, handler, bypassHasChanges: true);

                // Act
                var result = await sut.SetConfiguration(proposed, CancellationToken.None);

                // Assert
                result.OperationStatus.Should().Be(OperationStatus.Success);
                result.Messages.Should().BeEmpty();

                _ = await handler.DidNotReceive().ExecuteSetHostnameScript(Arg.Any<string>());
            }

            [Fact]
            public async Task ShouldNotCallHostnameScript_WhenHostnameUnchanged()
            {
                // Arrange
                var handler = Substitute.For<IScriptHandler>();

                var current = new SystemConfiguration()
                {
                    NetworkDNSSettings = new() { Hostname = s_proposedDNS.Hostname }
                };

                var proposed = new SystemConfiguration()
                {
                    NetworkDNSSettings = new() { Hostname = s_proposedDNS.Hostname }
                };

                var sut = CreateSut(current, handler, bypassHasChanges: true);

                // Act
                var result = await sut.SetConfiguration(proposed, CancellationToken.None);

                // Assert
                result.OperationStatus.Should().Be(OperationStatus.Success);
                result.Messages.Should().BeEmpty();

                _ = await handler.DidNotReceive().ExecuteSetHostnameScript(Arg.Any<string>());
            }
        }
    }
}
