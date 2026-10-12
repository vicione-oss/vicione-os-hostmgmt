using System.Net;
using System.Text.Json;
using AwesomeAssertions;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Contracts.Syslog;
using HostManagement.Shared.Enums;
using HostManagement.SystemConfig;
using Xunit;
using SharedConfiguration = HostManagement.Shared.Contracts.SystemConfiguration;

namespace HostManagement.Tests.SystemConfig;

public sealed class SharedSystemConfigurationMergerTests
{
    private static readonly int s_currentVersion = new SharedConfiguration().Version;

    public sealed class Merge
    {
        [Fact]
        public void ShouldThrow_WhenCurrentConfigurationNull()
        {
            // Arrange
            using var document = JsonDocument.Parse("{}");

            // Act
            var act = () => SharedSystemConfigurationMerger.Merge(null!, document.RootElement);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Theory]
        [InlineData("null")]
        [InlineData("[]")]
        [InlineData("true")]
        [InlineData("\"invalid\"")]
        public void ShouldThrow_WhenProposedConfigIsNotAnObject(string json)
        {
            // Arrange
            using var document = JsonDocument.Parse(json);

            // Act
            var act = () => SharedSystemConfigurationMerger.Merge(new SharedConfiguration(), document.RootElement);

            // Assert
            act.Should().Throw<JsonException>().WithMessage("Proposed configuration must be a JSON object.");
        }

        [Fact]
        public void ShouldThrow_WhenVersionMissing()
        {
            // Arrange
            using var document = JsonDocument.Parse("{\"NetworkDNSSettings\":{\"Hostname\":\"server-new\"}}");

            // Act
            var act = () => SharedSystemConfigurationMerger.Merge(CreateCurrentConfiguration(), document.RootElement);

            // Assert
            act.Should().Throw<JsonException>().WithMessage("Configuration version is required.");
        }

        [Theory]
        [InlineData("null")]
        [InlineData("true")]
        [InlineData("\"2\"")]
        [InlineData("\"invalid\"")]
        [InlineData("2.5")]
        [InlineData("99999999999")]
        [InlineData("{}")]
        [InlineData("[]")]
        public void ShouldThrow_WhenVersionNotAnInteger(string version)
        {
            // Arrange
            using var document = JsonDocument.Parse($"{{\"Version\":{version}}}");

            // Act
            var act = () => SharedSystemConfigurationMerger.Merge(CreateCurrentConfiguration(), document.RootElement);

            // Assert
            act.Should().Throw<JsonException>().WithMessage("Configuration version must be an integer.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(-2)]
        [InlineData(int.MaxValue)]
        public void ShouldThrow_WhenVersionDiffersFromCurrentVersion(int version)
        {
            // Arrange
            using var document = JsonDocument.Parse(
                $"{{\"Version\":{version},\"NetworkDNSSettings\":{{\"Hostname\":\"server-new\"}}}}");

            // Act
            var act = () => SharedSystemConfigurationMerger.Merge(CreateCurrentConfiguration(), document.RootElement);

            // Assert
            act.Should()
                .Throw<JsonException>()
                .WithMessage($"Unsupported configuration version {version} (expected {s_currentVersion}).");
        }

        [Fact]
        public void ShouldMerge_WhenVersionMatchesCurrentVersion()
        {
            // Arrange
            var current = CreateCurrentConfiguration();
            using var document = JsonDocument.Parse(
                $"{{\"Version\":{s_currentVersion},\"NetworkDNSSettings\":{{\"Hostname\":\"server-new\"}}}}");

            // Act
            var merged = SharedSystemConfigurationMerger.Merge(current, document.RootElement);

            // Assert
            merged.Version.Should().Be(s_currentVersion);
            merged.NetworkDNSSettings.Hostname.Should().Be("server-new");
        }

        [Fact]
        public void ShouldThrow_WhenProposedConfigHasDuplicateVersion()
        {
            // Arrange
            var json = "{\"Version\":2,\"Version\":2}";
            using var document = JsonDocument.Parse(json);

            // Act
            var act = () => SharedSystemConfigurationMerger.Merge(new SharedConfiguration(), document.RootElement);

            // Assert
            act.Should().Throw<JsonException>().WithMessage("Duplicate configuration property*");
        }

        [Theory]
        [InlineData("{\"NetworkInterfaces\":[{\"CommonInformation\":{\"Name\":\"lan1\",\"Name\":\"lan2\"},\"IPv4\":{\"DHCPEnabled\":true}}]}")]
        [InlineData("{\"NetworkDNSSettings\":{\"Hostname\":\"first\",\"Hostname\":\"second\"}}")]
        [InlineData("{\"Services\":[{\"Name\":\"ssh.service\",\"Name\":\"cron.service\",\"State\":\"Enabled\"}]}")]
        public void ShouldThrow_WhenProposedConfigHasDuplicatePropertyName(string json)
        {
            // Arrange
            using var document = ParseWithCurrentVersion(json);

            // Act
            var act = () => SharedSystemConfigurationMerger.Merge(new SharedConfiguration(), document.RootElement);

            // Assert
            act.Should().Throw<JsonException>().WithMessage("Duplicate configuration property*");
        }

        [Theory]
        [InlineData("{\"UnknownSetting\":true}")]
        [InlineData("{\"NetworkInterfaces\":[{\"CommonInformation\":{\"Enabled\":true,\"Name\":\"lan1\",\"UnknownSetting\":{}}}]}")]
        [InlineData("{\"NetworkInterfaces\":[{\"CommonInformation\":{\"UnknownSetting\":true,\"Name\":\"lan1\"},\"IPv4\":{}}]}")]
        [InlineData("{\"NetworkInterfaces\":[{\"CommonInformation\":{\"Enabled\":true,\"Name\":\"lan1\"},\"VLAN\":{\"UnknownSetting\":true}}]}")]
        [InlineData("{\"NetworkDNSSettings\":{\"UnknownSetting\":true}}")]
        [InlineData("{\"Services\":[{\"Name\":\"new.service\",\"State\":\"Enabled\",\"UnknownSetting\":true}]}")]
        [InlineData("{\"NetworkDNSSettings\":{\"StaticHosts\":{\"Hosts\":[{\"IPAddress\":\"192.0.2.2\",\"Hostname\":\"a\",\"UnknownSetting\":true}]}}}")]
        public void ShouldThrow_WhenProposedConfigHasUnknownProperty(string json)
        {
            // Arrange
            using var document = ParseWithCurrentVersion(json);

            // Act
            var act = () => SharedSystemConfigurationMerger.Merge(CreateCurrentConfiguration(), document.RootElement);

            // Assert
            act.Should().Throw<JsonException>().WithMessage("Unknown configuration property*");
        }

        [Theory]
        [InlineData("{\"NetworkDNSSettings\":{\"MulticastDNSEnabled\":\"invalidBool\"}}")]
        [InlineData("{\"Syslog\":{\"ConnectionRetrySeconds\":\"invalidInt\"}}")]
        [InlineData("{\"Syslog\":{\"TransportProtocol\":\"invalidEnum\"}}")]
        public void ShouldThrow_WhenKnownValueInvalid(string json)
        {
            // Arrange
            using var document = ParseWithCurrentVersion(json);

            // Act
            var act = () => SharedSystemConfigurationMerger.Merge(new SharedConfiguration(), document.RootElement);

            // Assert
            act.Should().Throw<JsonException>().WithMessage("*JSON*could not be converted*");
        }

        [Fact]
        public void ShouldThrow_WhenObjectCollectionIsUnkeyed()
        {
            // Arrange
            using var document = ParseWithCurrentVersion("{\"NetworkNTPSettings\":{\"Servers\":[{}]}}");

            // Act
            var act = () => SharedSystemConfigurationMerger.Merge(CreateCurrentConfiguration(), document.RootElement);

            // Assert
            act.Should()
                .Throw<NotSupportedException>()
                .WithMessage("Object collection * has no defined identity strategy.");
        }

        [Theory]
        [InlineData("{\"NetworkInterfaces\":[null]}")]
        [InlineData("{\"NetworkDNSSettings\":{\"StaticHosts\":{\"Hosts\":[null]}}}")]
        [InlineData("{\"Services\":[true]}")]
        public void ShouldThrow_WhenCollectionExpectedObject(string json)
        {
            // Arrange
            using var document = ParseWithCurrentVersion(json);

            // Act
            var act = () => SharedSystemConfigurationMerger.Merge(CreateCurrentConfiguration(), document.RootElement);

            // Assert
            act.Should().Throw<JsonException>().WithMessage("Expected an object at*");
        }

        [Theory]
        [InlineData("{\"NetworkInterfaces\":[{}]}")]
        [InlineData("{\"NetworkInterfaces\":[{\"CommonInformation\":{\"Name\":\" \"}}]}")]
        [InlineData("{\"Services\":[{\"State\":\"Enabled\"}]}")]
        public void ShouldThrow_WhenCollectionIdentityMissingOrInvalid(string json)
        {
            // Arrange
            using var document = ParseWithCurrentVersion(json);

            // Act
            var act = () => SharedSystemConfigurationMerger.Merge(CreateCurrentConfiguration(), document.RootElement);

            // Assert
            act.Should().Throw<JsonException>().WithMessage("Missing or invalid*");
        }

        [Theory]
        [InlineData("{\"NetworkInterfaces\":[{\"CommonInformation\":{\"Name\":\"lan1\"}},{\"CommonInformation\":{\"Name\":\"lan1\"}}]}")]
        [InlineData("{\"Services\":[{\"Name\":\"ssh.service\"},{\"Name\":\"ssh.service\"}]}")]
        public void ShouldThrow_WhenCollectionIdentitiesDuplicated(string json)
        {
            // Arrange
            using var document = ParseWithCurrentVersion(json);

            // Act
            var act = () => SharedSystemConfigurationMerger.Merge(CreateCurrentConfiguration(), document.RootElement);

            // Assert
            act.Should().Throw<JsonException>();
        }

        [Theory]
        [InlineData("{\"Services\":[{\"Name\":\"ssh.service\",\"State\":\"invalid\"}]}")]
        [InlineData("{\"NetworkInterfaces\":[{\"CommonInformation\":{\"Name\":\"lan1\",\"Enabled\":\"invalid\"}}]}")]
        [InlineData("{\"NetworkDNSSettings\":{\"StaticHosts\":{\"Hosts\":[{\"IPAddress\":\"invalid\",\"Hostname\":\"a\"}]}}}")]
        [InlineData("{\"NetworkNTPSettings\":{\"Servers\":[42]}}")]
        [InlineData("{\"NetworkDNSSettings\":{\"NameServers\":{\"Addresses\":[\"invalid\"]}}}")]
        public void ShouldThrow_WhenCollectionContainsInvalidKnownValue(string json)
        {
            // Arrange
            using var document = ParseWithCurrentVersion(json);

            // Act
            var act = () => SharedSystemConfigurationMerger.Merge(CreateCurrentConfiguration(), document.RootElement);

            // Assert
            act.Should().Throw<JsonException>();
        }

        [Theory]
        [InlineData("{\"NetworkInterfaces\":[{\"CommonInformation\":{\"Name\":\"lan1\"},\"IPv4\":{\"IPv4Details\":[{\"IPAddress\":\"192.0.2.10\"}]}}]}")]
        [InlineData("{\"NetworkDNSSettings\":{\"StaticHosts\":{\"Hosts\":[{\"IPAddress\":\"192.0.2.1\"}]}}}")]
        public void ShouldThrow_WhenReplacementListEntryMissingRequiredProperty(string json)
        {
            // Arrange
            using var document = ParseWithCurrentVersion(json);

            // Act
            var act = () => SharedSystemConfigurationMerger.Merge(CreateCurrentConfiguration(), document.RootElement);

            // Assert
            act.Should().Throw<JsonException>().WithMessage("*missing required properties*");
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ShouldThrow_WhenCurrentCollectionContainsDuplicateIdentities(bool duplicateServices)
        {
            // Arrange
            var current = CreateCurrentConfiguration();
            string proposedJson;

            if (duplicateServices)
            {
                current.Services.Add(new() { Name = "ssh.service", State = ServiceState.Disabled });
                proposedJson = "{\"Services\":[]}";
            }
            else
            {
                current.NetworkInterfaces.Add(new()
                {
                    CommonInformation = new() { Name = "lan1", Enabled = false },
                    IPv4 = new() { DHCPEnabled = true }
                });
                proposedJson = "{\"NetworkInterfaces\":[]}";
            }

            using var document = ParseWithCurrentVersion(proposedJson);

            // Act
            var act = () => SharedSystemConfigurationMerger.Merge(current, document.RootElement);

            // Assert
            act.Should().Throw<JsonException>().WithMessage("Duplicate collection identity*");
        }

        [Fact]
        public void ShouldThrow_WhenNewServiceMissingRequiredState()
        {
            // Arrange
            using var document = ParseWithCurrentVersion("{\"Services\":[{\"Name\":\"new.service\"}]}");

            // Act
            var act = () => SharedSystemConfigurationMerger.Merge(CreateCurrentConfiguration(), document.RootElement);

            // Assert
            act.Should().Throw<JsonException>().WithMessage("*State*");
        }

        [Fact]
        public void ShouldThrow_WhenUnknownNetworkInterfaceContainsInvalidKnownValue()
        {
            // Arrange
            using var document = ParseWithCurrentVersion(
                "{\"NetworkInterfaces\":[{\"CommonInformation\":{\"Name\":\"lan99\",\"Enabled\":\"invalid\"}}]}");

            // Act
            var act = () => SharedSystemConfigurationMerger.Merge(CreateCurrentConfiguration(), document.RootElement);

            // Assert
            act.Should().Throw<JsonException>();
        }

        [Fact]
        public void ShouldPreserveConfiguration_WhenEntireCurrentConfigurationSubmitted()
        {
            // Arrange
            var current = CreateCurrentConfiguration();
            current.NetworkInterfaces[0].IPv4.IPv4Details.Add(new()
            {
                IPAddress = IPAddress.Parse("192.0.2.20"),
                Netmask = IPAddress.Parse("255.255.255.0")
            });
            current.NetworkDNSSettings.NameServers.Addresses.Add(IPAddress.Parse("192.0.2.53"));
            current.NetworkProxySettings.NoProxy.Entries.Add("example.org");
            current.Syslog.ExcludeSyslogFacilities.Add(SyslogFacility.Auth);
            current.Syslog.ExcludeSyslogLevels.Add(SyslogLevel.Debug);

            var proposedJson = JsonSerializer.SerializeToElement(
                current,
                SourceGenerationContext.Default.SystemConfiguration);

            // Act
            var merged = SharedSystemConfigurationMerger.Merge(current, proposedJson);

            // Assert
            merged.Should().NotBeSameAs(current);
            merged.Should().BeEquivalentTo(current);
        }

        [Fact]
        public void ShouldRetainCurrentValues_WhenProposedConfigIsEmpty()
        {
            // Arrange
            var current = CreateCurrentConfiguration();
            using var document = ParseWithCurrentVersion("{}");

            // Act
            var merged = SharedSystemConfigurationMerger.Merge(current, document.RootElement);

            // Assert

            // Merged and current configs are not reference equal but value equal after merge
            merged.Should().NotBeSameAs(current);
            merged.Should().BeEquivalentTo(current);
            merged.NetworkDNSSettings.Should().NotBeSameAs(current.NetworkDNSSettings);
        }

        [Fact]
        public void ShouldPreserveOmittedNestedSettings_WhenKnownSettingsUpdated()
        {
            // Arrange
            var current = CreateCurrentConfiguration();
            using var document = JsonDocument.Parse(
                """
                {
                  "Version": 2,
                  "NetworkDNSSettings": {
                    "Hostname": "server-new",
                    "StaticHosts": { "Enabled": false }
                  },
                  "NetworkProxySettings": {
                    "HTTP": { "Port": 3128 }
                  },
                  "Syslog": { "Enabled": false }
                }
                """);

            // Act
            var merged = SharedSystemConfigurationMerger.Merge(current, document.RootElement);

            // Assert
            current.NetworkDNSSettings.Hostname.Should().Be("server-old");
            current.NetworkDNSSettings.StaticHosts.Enabled.Should().BeTrue();
            current.NetworkProxySettings.HTTP.Port.Should().Be(8080);
            current.Syslog.Enabled.Should().BeTrue();

            merged.Version.Should().Be(current.Version);
            merged.NetworkDNSSettings.Hostname.Should().Be("server-new");
            merged.NetworkDNSSettings.StaticHosts.Enabled.Should().BeFalse();
            merged.NetworkProxySettings.HTTP.Server.Should().Be("proxy.example.org");
            merged.NetworkProxySettings.HTTP.Port.Should().Be(3128);
            merged.Syslog.Enabled.Should().BeFalse();
            merged.Syslog.Address.Should().Be("192.0.2.10:514");
            merged.Syslog.ConnectionRetrySeconds.Should().Be(60);
        }

        [Fact]
        public void ShouldUseExplicitDefaults_WhenSubmitted()
        {
            // Arrange
            var current = CreateCurrentConfiguration();
            using var document = ParseWithCurrentVersion(
                """
                {
                  "NetworkDNSSettings": { "Hostname": "", "MulticastDNSEnabled": false },
                  "NetworkProxySettings": { "HTTP": { "Port": 0 } },
                  "Syslog": { "Enabled": false, "Address": "", "TransportProtocol": "udp", "LogFormat": "rfc5424", "ConnectionRetrySeconds": 30, "ExcludeSyslogFacilities": [], "ExcludeSyslogLevels": [] }
                }
                """);

            // Act
            var merged = SharedSystemConfigurationMerger.Merge(current, document.RootElement);

            // Assert
            merged.NetworkDNSSettings.Hostname.Should().BeEmpty();
            merged.NetworkDNSSettings.MulticastDNSEnabled.Should().BeFalse();
            merged.NetworkProxySettings.HTTP.Server.Should().Be("proxy.example.org");
            merged.NetworkProxySettings.HTTP.Port.Should().Be(0);
            merged.Syslog.Equals(new SyslogSettings()).Should().BeTrue();
        }

        [Fact]
        public void ShouldAllowNull_WhenKnownPropertyIsNullable()
        {
            // Arrange
            var current = CreateCurrentConfiguration();
            using var document = ParseWithCurrentVersion("{\"NetworkProxySettings\":{\"HTTP\":{\"Port\":null}}}");

            // Act
            var merged = SharedSystemConfigurationMerger.Merge(current, document.RootElement);

            // Assert
            current.NetworkProxySettings.HTTP.Port.Should().Be(8080);

            merged.NetworkProxySettings.HTTP.Server.Should().Be("proxy.example.org");
            merged.NetworkProxySettings.HTTP.Port.Should().BeNull();
        }

        [Fact]
        public void ShouldPreserveOmittedCollections_WhenScalarPropertyUpdated()
        {
            // Arrange
            var current = CreateCurrentConfiguration();
            using var document = ParseWithCurrentVersion("{\"NetworkDNSSettings\":{\"Hostname\":\"server-new\"}}");

            // Act
            var merged = SharedSystemConfigurationMerger.Merge(current, document.RootElement);

            // Assert
            merged.NetworkInterfaces.Should().NotBeSameAs(current.NetworkInterfaces);
            merged.NetworkInterfaces.Should().BeEquivalentTo(current.NetworkInterfaces);

            merged.NetworkDNSSettings.SearchDomains.Domains
                .Should().BeEquivalentTo(current.NetworkDNSSettings.SearchDomains.Domains);
            merged.NetworkNTPSettings.Servers.Should().BeEquivalentTo(current.NetworkNTPSettings.Servers);
            merged.Services.Should().BeEquivalentTo(current.Services);
        }

        [Fact]
        public void ShouldReplaceSubmittedValueCollections_WhilePreservingOtherFields()
        {
            // Arrange
            var current = CreateCurrentConfiguration();
            using var document = ParseWithCurrentVersion(
                """
                {
                  "NetworkNTPSettings": { "Servers": ["time1.example.org", "time2.example.org"] },
                  "NetworkDNSSettings": { "SearchDomains": { "Domains": [] } },
                  "Syslog": { "ExcludeSyslogLevels": ["debug"] }
                }
                """);

            // Act
            var merged = SharedSystemConfigurationMerger.Merge(current, document.RootElement);

            // Assert
            current.NetworkDNSSettings.SearchDomains.Domains.Should().ContainSingle();
            current.NetworkNTPSettings.Servers.Should().ContainSingle().Which.Should().Be("ntp.example.org");

            merged.NetworkDNSSettings.SearchDomains.Enabled.Should().BeTrue();
            merged.NetworkDNSSettings.SearchDomains.Domains.Should().BeEmpty();

            merged.NetworkNTPSettings.Enabled.Should().BeTrue();
            merged.NetworkNTPSettings.Servers.Should().Equal("time1.example.org", "time2.example.org");
            merged.Syslog.ExcludeSyslogLevels.Should().ContainSingle();
        }

        [Fact]
        public void ShouldMergeExistingNetworkInterfaces_WithoutAddingUnknownInterfaces()
        {
            // Arrange
            var current = CreateCurrentConfiguration();
            current.NetworkInterfaces[0].CommonInformation.UserDefinedMACAddress.Enabled = true;
            current.NetworkInterfaces[0].VLAN = new() { Enabled = true, ID = 20 };
            current.NetworkInterfaces.Add(new()
            {
                CommonInformation = new() { Name = "lan2", Enabled = true },
                IPv4 = new() { DHCPEnabled = true }
            });

            using var document = ParseWithCurrentVersion(
                """
                {
                  "NetworkInterfaces": [
                    {
                      "CommonInformation": { "Name": "lan1", "Enabled": false },
                      "IPv4": { "DHCPEnabled": false }
                    },
                    {
                      "CommonInformation": { "Name": "lan99", "Enabled": true },
                      "IPv4": { "DHCPEnabled": true }
                    }
                  ]
                }
                """);

            // Act
            var merged = SharedSystemConfigurationMerger.Merge(current, document.RootElement);

            // Assert
            current.NetworkInterfaces.Select(x => x.CommonInformation.Name).Should().Equal("lan1", "lan2");
            current.NetworkInterfaces[0].IPv4.DHCPEnabled.Should().BeTrue();

            merged.NetworkInterfaces.Select(x => x.CommonInformation.Name).Should().Equal("lan1", "lan2");
            merged.NetworkInterfaces[0].CommonInformation.Enabled.Should().BeFalse();
            merged.NetworkInterfaces[0].CommonInformation.UserDefinedMACAddress.Enabled.Should().BeTrue();
            merged.NetworkInterfaces[0].IPv4.DHCPEnabled.Should().BeFalse();
            merged.NetworkInterfaces[0].VLAN.ID.Should().Be(20);
            merged.NetworkInterfaces[1].CommonInformation.Enabled.Should().BeTrue();
        }

        [Fact]
        public void ShouldPreserveNamedCollectionEntries_WhenSubmittedArraysEmpty()
        {
            // Arrange
            var current = CreateCurrentConfiguration();
            using var document = ParseWithCurrentVersion("{\"NetworkInterfaces\":[],\"Services\":[]}");

            // Act
            var merged = SharedSystemConfigurationMerger.Merge(current, document.RootElement);

            // Assert
            merged.NetworkInterfaces.Should().BeEquivalentTo(current.NetworkInterfaces);
            merged.Services.Should().BeEquivalentTo(current.Services);
        }

        [Fact]
        public void ShouldMergeServicesByName_WithoutRemovingOmittedServices()
        {
            // Arrange
            var current = CreateCurrentConfiguration();
            current.Services.Add(new() { Name = "cron.service", State = ServiceState.Enabled });

            using var document = ParseWithCurrentVersion(
                """
                {
                  "Services": [
                    { "Name": "ssh.service", "State": "Disabled" },
                    { "Name": "cups.service", "State": "Enabled" }
                  ]
                }
                """);

            // Act
            var merged = SharedSystemConfigurationMerger.Merge(current, document.RootElement);

            // Assert
            current.Services[0].State.Should().Be(ServiceState.Enabled);

            merged.Services.Select(x => x.Name).Should().Equal("ssh.service", "cron.service", "cups.service");
            merged.Services[0].State.Should().Be(ServiceState.Disabled);
            merged.Services[1].State.Should().Be(ServiceState.Enabled);
        }

        [Fact]
        public void ShouldReplaceIPv4Details_WhenExplicitlySubmitted()
        {
            // Arrange
            var current = CreateCurrentConfiguration();
            current.NetworkInterfaces[0].IPv4 = new()
            {
                DHCPEnabled = false,
                IPv4Details =
                [
                    new() { IPAddress = IPAddress.Parse("192.0.2.10"), Netmask = IPAddress.Parse("255.255.255.0") },
                    new() { IPAddress = IPAddress.Parse("192.0.2.11"), Netmask = IPAddress.Parse("255.255.255.0") }
                ]
            };

            using var document = ParseWithCurrentVersion(
                """
                {
                  "NetworkInterfaces": [
                    {
                      "CommonInformation": { "Name": "lan1" },
                      "IPv4": {
                        "DHCPEnabled": false,
                        "IPv4Details": [
                          { "IPAddress": "192.0.2.10", "Netmask": "255.255.255.128" },
                          { "IPAddress": "192.0.2.12", "Netmask": "255.255.255.0" }
                        ]
                      }
                    }
                  ]
                }
                """);

            // Act
            var merged = SharedSystemConfigurationMerger.Merge(current, document.RootElement);

            // Assert
            current.NetworkInterfaces[0].IPv4.IPv4Details.Select(x => x.IPAddress.ToString())
                .Should().Equal("192.0.2.10", "192.0.2.11");
            current.NetworkInterfaces[0].IPv4.IPv4Details[0].Netmask
                .Should().Be(IPAddress.Parse("255.255.255.0"));

            merged.NetworkInterfaces[0].IPv4.IPv4Details.Select(x => x.IPAddress.ToString())
                .Should().Equal("192.0.2.10", "192.0.2.12");
            merged.NetworkInterfaces[0].IPv4.IPv4Details[0].Netmask
                .Should().Be(IPAddress.Parse("255.255.255.128"));
        }

        [Fact]
        public void ShouldReplaceStaticHosts_WhenHostsExplicitlySubmitted()
        {
            // Arrange
            var current = CreateCurrentConfiguration();

            using var document = ParseWithCurrentVersion(
                """
                {
                  "NetworkDNSSettings": {
                    "StaticHosts": {
                      "Hosts": [
                        { "IPAddress": "192.0.2.11", "Hostname": "new.example.org" }
                      ]
                    }
                  }
                }
                """);

            // Act
            var merged = SharedSystemConfigurationMerger.Merge(current, document.RootElement);

            // Assert
            current.NetworkDNSSettings.StaticHosts.Hosts.Should().ContainSingle()
                .Which.Hostname.Should().Be("old.example.org");

            merged.NetworkDNSSettings.StaticHosts.Hosts.Should().ContainSingle()
                .Which.Hostname.Should().Be("new.example.org");
        }

        [Fact]
        public void ShouldIgnoreUnknownNetworkInterface_WhenRequiredFieldsAreMissing()
        {
            // Arrange
            using var document = ParseWithCurrentVersion(
                "{\"NetworkInterfaces\":[{\"CommonInformation\":{\"Name\":\"lan99\"}}]}");

            // Act
            var merged = SharedSystemConfigurationMerger.Merge(CreateCurrentConfiguration(), document.RootElement);

            // Assert
            merged.NetworkInterfaces.Select(x => x.CommonInformation.Name).Should().Equal("lan1");
        }

        [Fact]
        public void ShouldReplaceObjectLists_WhenSubmittedAsEmptyArrays()
        {
            // Arrange
            var current = CreateCurrentConfiguration();

            current.NetworkInterfaces[0].IPv4 = new()
            {
                DHCPEnabled = false,
                IPv4Details =
                [
                    new() { IPAddress = IPAddress.Parse("192.0.2.10"), Netmask = IPAddress.Parse("255.255.255.0") }
                ]
            };

            using var document = ParseWithCurrentVersion(
                """
                {
                  "NetworkInterfaces": [
                    { "CommonInformation": { "Name": "lan1" }, "IPv4": { "DHCPEnabled": true, "IPv4Details": [] } }
                  ],
                  "NetworkDNSSettings": { "StaticHosts": { "Enabled": false, "Hosts": [] } }
                }
                """);

            // Act
            var merged = SharedSystemConfigurationMerger.Merge(current, document.RootElement);

            // Assert
            current.NetworkInterfaces[0].IPv4.IPv4Details.Should().ContainSingle();
            current.NetworkDNSSettings.StaticHosts.Hosts.Should().ContainSingle();

            merged.NetworkInterfaces[0].IPv4.IPv4Details.Should().BeEmpty();
            merged.NetworkDNSSettings.StaticHosts.Hosts.Should().BeEmpty();
        }

        [Fact]
        public void ShouldPreserveOmittedValues_WhenNestedObjectIsEmpty()
        {
            // Arrange
            var current = CreateCurrentConfiguration();
            using var document = ParseWithCurrentVersion("{\"Syslog\":{},\"NetworkDNSSettings\":{\"StaticHosts\":{}}}");

            // Act
            var merged = SharedSystemConfigurationMerger.Merge(current, document.RootElement);

            // Assert
            merged.Should().BeEquivalentTo(current);
        }
    }

    private static SharedConfiguration CreateCurrentConfiguration() => new()
    {
        Version = 2,
        NetworkInterfaces =
        [
            new()
            {
                CommonInformation = new()
                {
                    Enabled = true,
                    Name = "lan1"
                },
                IPv4 = new() { DHCPEnabled = true }
            }
        ],
        NetworkDNSSettings = new NetworkDNSSettings
        {
            Hostname = "server-old",
            MulticastDNSEnabled = true,
            StaticHosts = new()
            {
                Enabled = true,
                Hosts =
                [
                    new() { IPAddress = IPAddress.Parse("192.0.2.10"), Hostname = "old.example.org" }
                ]
            },
            SearchDomains = new() { Enabled = true, Domains = ["corp.example.org"] },
            PrimaryDNSSuffix = new() { Enabled = true, Suffix = "corp.example.org" }
        },
        NetworkProxySettings = new()
        {
            HTTP = new() { Enabled = true, Server = "proxy.example.org", Port = 8080 }
        },
        NetworkNTPSettings = new() { Enabled = true, Servers = ["ntp.example.org"] },
        Syslog = new()
        {
            Enabled = true,
            Address = "192.0.2.10:514",
            ConnectionRetrySeconds = 60
        },
        Services = [new() { Name = "ssh.service", State = ServiceState.Enabled }]
    };

    private static JsonDocument ParseWithCurrentVersion(string json)
    {
        var body = json.Trim()[1..^1].Trim();

        return JsonDocument.Parse(body.Length == 0
            ? $"{{\"Version\":{s_currentVersion}}}"
            : $"{{\"Version\":{s_currentVersion},{body}}}");
    }
}
