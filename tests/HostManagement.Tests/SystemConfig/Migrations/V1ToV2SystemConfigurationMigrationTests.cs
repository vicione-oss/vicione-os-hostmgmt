using System.Net;
using System.Text.Json;
using AwesomeAssertions;
using HostManagement.SystemConfig.Migrations;
using Xunit;
// Version 2 is not frozen yet. Until it is frozen, the migration target intentionally
// uses the current system configuration contracts. When version 3 is introduced,
// replace this alias with HostManagement.SystemConfig.Migrations.Contracts.V2.
using TargetContracts = HostManagement.SystemConfig.Contracts;

namespace HostManagement.Tests.SystemConfig.Migrations;

public class V1ToV2SystemConfigurationMigrationTests
{
    public sealed class Migrate
    {
        [Fact]
        public void ShouldUseCurrentContractsOnlyWhileVersionTwoIsCurrent()
        {
            TargetContracts.SystemConfiguration.CurrentVersion.Should().Be(
                2,
                "V1 to V2 migration tests must be switched to V2 contracts when a newer current version is introduced");
        }

        [Fact]
        public void ShouldMigrateMinimalConfiguration_WhenVersionIsV1()
        {
            // Arrange
            using var sourceDocument = JsonDocument.Parse(
                """
                {
                  "Version": 1
                }
                """);

            var sut = new V1ToV2SystemConfigurationMigration();

            // Act
            var result = MigrateSourceDocument(sut, sourceDocument);

            // Assert
            result.Should().NotBeNull();
            result.Version.Should().Be(2);
            result.NetworkInterfacesSettings.Should().BeNull();
            result.NetworkDNSSettings.Should().BeNull();
            result.NetworkProxySettings.Should().BeNull();
            result.NetworkNTPSettings.Should().BeNull();
            result.Services.Should().BeNull();
        }

        [Fact]
        public void ShouldMigrateNetworkInterfacesSettings_WhenSourceContainsInterface()
        {
            // Arrange
            using var sourceDocument = JsonDocument.Parse(
                """
                {
                  "Version": 1,
                  "NetworkInterfacesSettings": {
                    "NetworkInterfaces": [
                      {
                        "CommonInformation": {
                          "Name": "lan1",
                          "Enabled": true,
                          "PhysicalAddress": "00:02:01:ac:52:33"
                        },
                        "IPv4": {
                          "Gateway": "192.168.1.1",
                          "DHCPEnabled": false,
                          "IPv4Details": [
                            {
                              "IPAddress": "192.168.60.3",
                              "Netmask": "255.255.0.0"
                            }
                          ],
                          "VLANEnabled": true,
                          "VLANID": 100
                        }
                      }
                    ]
                  }
                }
                """);

            var sut = new V1ToV2SystemConfigurationMigration();

            // Act
            var result = MigrateSourceDocument(sut, sourceDocument);

            // Assert
            result.Should().NotBeNull();
            result.NetworkInterfacesSettings.Should().NotBeNull();

            var networkInterface = result.NetworkInterfacesSettings.NetworkInterfaces.Should().ContainSingle().Subject;
            networkInterface.CommonInformation.Should().NotBeNull();
            networkInterface.CommonInformation.Name.Should().Be("lan1");
            networkInterface.CommonInformation.Enabled.Should().BeTrue();
            networkInterface.CommonInformation.PhysicalAddress.Should().Be("00:02:01:ac:52:33");

            networkInterface.IPv4.Should().NotBeNull();
            networkInterface.IPv4.Gateway.Should().Be(IPAddress.Parse("192.168.1.1"));
            networkInterface.IPv4.DHCPEnabled.Should().BeFalse();

            var ipv4Details = networkInterface.IPv4.IPv4Details.Should().ContainSingle().Subject;
            ipv4Details.IPAddress.Should().Be(IPAddress.Parse("192.168.60.3"));
            ipv4Details.Netmask.Should().Be(IPAddress.Parse("255.255.0.0"));

            networkInterface.IPv4.VLANEnabled.Should().BeTrue();
            networkInterface.IPv4.VLANID.Should().Be(100);
        }

        [Fact]
        public void ShouldMigrateNetworkDnsSettings_WhenSourceContainsDnsSettings()
        {
            // Arrange
            using var sourceDocument = JsonDocument.Parse(
                """
                {
                  "Version": 1,
                  "NetworkDNSSettings": {
                    "Hostname": "test-host",
                    "MulticastDNSEnabled": true,
                    "StaticHostsEnabled": true,
                    "StaticHosts": [
                      {
                        "IPAddress": "192.168.1.10",
                        "Hostname": "local-device"
                      }
                    ],
                    "NameServersEnabled": true,
                    "NameServers": [
                      "192.168.1.1"
                    ],
                    "DNSSuffixEnabled": true,
                    "DNSSuffix": "example.local",
                    "SearchDomainsEnabled": true,
                    "SearchDomains": [
                      "example.local"
                    ]
                  }
                }
                """);

            var sut = new V1ToV2SystemConfigurationMigration();

            // Act
            var result = MigrateSourceDocument(sut, sourceDocument);

            // Assert
            result.Should().NotBeNull();
            result.NetworkDNSSettings.Should().NotBeNull();

            var dnsSettings = result.NetworkDNSSettings;
            dnsSettings.Hostname.Should().Be("test-host");
            dnsSettings.MulticastDNSEnabled.Should().BeTrue();

            dnsSettings.StaticHosts.Should().NotBeNull();
            dnsSettings.StaticHosts.Enabled.Should().BeTrue();

            var host = dnsSettings.StaticHosts.Hosts.Should().ContainSingle().Subject;
            host.IPAddress.Should().Be(IPAddress.Parse("192.168.1.10"));
            host.Hostname.Should().Be("local-device");

            dnsSettings.NameServers.Should().NotBeNull();
            dnsSettings.NameServers.Enabled.Should().BeTrue();
            dnsSettings.NameServers.Addresses.Should().ContainSingle().Which.Should().Be(IPAddress.Parse("192.168.1.1"));

            dnsSettings.DNSSuffixEnabled.Should().BeTrue();
            dnsSettings.DNSSuffix.Should().Be("example.local");

            dnsSettings.SearchDomainsEnabled.Should().BeTrue();
            dnsSettings.SearchDomains.Should().ContainSingle().Which.Should().Be("example.local");
        }

        [Fact]
        public void ShouldMigrateNetworkProxySettings_WhenSourceContainsProxySettings()
        {
            // Arrange
            using var sourceDocument = JsonDocument.Parse(
                """
                {
                  "Version": 1,
                  "NetworkProxySettings": {
                    "HTTP": {
                      "Enabled": true,
                      "Server": "http://proxy.example.com",
                      "Port": 8080,
                      "Username": "proxy-user",
                      "Password": "proxy-password"
                    },
                    "DoNotProxyListEnabled": true,
                    "DoNotProxyList": [
                      "localhost",
                      "127.0.0.1"
                    ]
                  }
                }
                """);

            var sut = new V1ToV2SystemConfigurationMigration();

            // Act
            var result = MigrateSourceDocument(sut, sourceDocument);

            // Assert
            result.Should().NotBeNull();
            result.NetworkProxySettings.Should().NotBeNull();

            var proxySettings = result.NetworkProxySettings;
            proxySettings.HTTP.Should().NotBeNull();
            proxySettings.HTTP.Enabled.Should().BeTrue();
            proxySettings.HTTP.Server.Should().Be("http://proxy.example.com");
            proxySettings.HTTP.Port.Should().Be(8080);
            proxySettings.HTTP.Username.Should().Be("proxy-user");
            proxySettings.HTTP.Password.Should().Be("proxy-password");

            proxySettings.FTP.Should().BeNull();
            proxySettings.HTTPS.Should().BeNull();
            proxySettings.SFTP.Should().BeNull();
            proxySettings.SOCKS.Should().BeNull();

            proxySettings.NoProxy.Should().NotBeNull();
            proxySettings.NoProxy.Enabled.Should().BeTrue();
            proxySettings.NoProxy.Entries.Should().Equal("localhost", "127.0.0.1");
        }

        [Fact]
        public void ShouldMigrateNetworkNtpSettings_WhenSourceContainsNtpSettings()
        {
            // Arrange
            using var sourceDocument = JsonDocument.Parse(
                """
                {
                  "Version": 1,
                  "NetworkNTPSettings": {
                    "FallbackNTPServers": [
                      "fallback.example.local"
                    ],
                    "NTPServersEnabled": true,
                    "NTPServers": [
                      "time.example.local",
                      "192.168.1.20"
                    ]
                  }
                }
                """);

            var sut = new V1ToV2SystemConfigurationMigration();

            // Act
            var result = MigrateSourceDocument(sut, sourceDocument);

            // Assert
            result.Should().NotBeNull();
            result.NetworkNTPSettings.Should().NotBeNull();

            result.NetworkNTPSettings.Enabled.Should().BeTrue();
            result.NetworkNTPSettings.Servers.Should().Equal("time.example.local", "192.168.1.20");
        }

        [Fact]
        public void ShouldMigrateServices_WhenSourceContainsService()
        {
            // Arrange
            using var sourceDocument = JsonDocument.Parse(
                """
                {
                  "Version": 1,
                  "Services": [
                    {
                      "Name": "custom.service",
                      "State": "Enabled"
                    }
                  ]
                }
                """);

            var sut = new V1ToV2SystemConfigurationMigration();

            // Act
            var result = MigrateSourceDocument(sut, sourceDocument);

            // Assert
            result.Should().NotBeNull();
            var service = result.Services.Should().ContainSingle().Subject;
            service.Name.Should().Be("custom.service");
            service.State.Should().Be(TargetContracts.ServiceState.Enabled);
        }

        [Fact]
        public void ShouldMigrateServiceStateToUnknown_WhenSourceStateUnsupported()
        {
            // Arrange
            using var sourceDocument = JsonDocument.Parse(
                """
                {
                  "Version": 1,
                  "Services": [
                    {
                      "Name": "custom.service",
                      "State": 42
                    }
                  ]
                }
                """);

            var sut = new V1ToV2SystemConfigurationMigration();

            // Act
            var result = MigrateSourceDocument(sut, sourceDocument);

            // Assert
            result.Should().NotBeNull();
            result.Services.Should().ContainSingle();
            result.Services[0].Name.Should().Be("custom.service");
            result.Services[0].State.Should().Be(TargetContracts.ServiceState.Unknown);
        }

        [Theory]
        [InlineData("Unknown", TargetContracts.ServiceState.Unknown)]
        [InlineData("Enabled", TargetContracts.ServiceState.Enabled)]
        [InlineData("Disabled", TargetContracts.ServiceState.Disabled)]
        internal void ShouldMigrateServiceState_WhenSourceStateSupported(
            string sourceState,
            TargetContracts.ServiceState expectedState)
        {
            // Arrange
            using var sourceDocument = JsonDocument.Parse(
                $$"""
                {
                  "Version": 1,
                  "Services": [
                    {
                      "Name": "custom.service",
                      "State": "{{sourceState}}"
                    }
                  ]
                }
                """);

            var sut = new V1ToV2SystemConfigurationMigration();

            // Act
            var result = MigrateSourceDocument(sut, sourceDocument);

            // Assert
            result.Should().NotBeNull();
            result.Services.Should().ContainSingle();
            result.Services[0].State.Should().Be(expectedState);
        }

        private static TargetContracts.SystemConfiguration MigrateSourceDocument(
            V1ToV2SystemConfigurationMigration migration,
            JsonDocument sourceDocument)
        {
            var sourceConfiguration = new V1SystemConfigurationContractDeserializer()
                .Deserialize(sourceDocument.RootElement);

            var result = migration.Migrate(sourceConfiguration);

            return result.Should().BeOfType<TargetContracts.SystemConfiguration>().Subject;
        }
    }
}
