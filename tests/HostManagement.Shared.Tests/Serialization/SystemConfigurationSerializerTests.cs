using System.Net;
using System.Text;
using System.Text.Json;
using AwesomeAssertions;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Enums;
using HostManagement.Shared.Serialization;
using Xunit;

namespace HostManagement.Shared.Tests.Serialization;

public class SystemConfigurationSerializerTests
{
    private static readonly IPAddress s_ipAddress = IPAddress.Parse("120.86.52.18");
    private static readonly SystemConfiguration s_sampleSystemConfiguration = new()
    {
        Version = 2,
        NetworkInterfacesSettings = new()
        {
            NetworkInterfaces =
            [
                new()
                {
                    CommonInformation = new()
                    {
                        Enabled = true,
                        Name = "lan1",
                        PhysicalAddress = "01:23:45:67:89:AB"
                    },
                    IPv4 = new()
                    {
                        Gateway = s_ipAddress,
                        DHCPEnabled = true,
                        IPv4Details =
                        [
                            new()
                            {
                                IPAddress = s_ipAddress,
                                Netmask = s_ipAddress,
                            }
                        ],
                        VLANEnabled = true,
                        VLANID = 1,
                    },
                },
            ],
        },
        NetworkDNSSettings = new()
        {
            Hostname = "hostname",
            MulticastDNSEnabled = true,
            StaticHosts = new()
            {
                Enabled = true,
                Hosts =
                [
                    new()
                    {
                        Hostname = "statichost",
                        IPAddress = s_ipAddress,
                    },
                ]
            },
            NameServers = new()
            {
                Enabled = true,
                Addresses = [s_ipAddress]
            },
            DNSSuffixEnabled = true,
            DNSSuffix = "dnsSuffix",
            SearchDomainsEnabled = true,
            SearchDomains = ["searchDomain"]
        },
        NetworkProxySettings = new()
        {
            HTTP = new()
            {
                Enabled = true,
                Password = "httpPassword",
                Port = 80,
                Server = "httpServer",
                Username = "httpUser"
            },
            HTTPS = new()
            {
                Enabled = true,
                Password = "httpsPassword",
                Port = 443,
                Server = "httpsServer",
                Username = "httpsUser"
            },
            FTP = new()
            {
                Enabled = true,
                Password = "ftpPassword",
                Port = 21,
                Server = "ftpServer",
                Username = "ftpUser"
            },
            SFTP = new()
            {
                Enabled = true,
                Password = "sftpPassword",
                Port = 22,
                Server = "sftpServer",
                Username = "sftpUser"
            },
            SOCKS = new()
            {
                Enabled = true,
                Password = "socksPassword",
                Port = 1080,
                Server = "socksServer",
                Username = "socksUser"
            },
            NoProxy = new()
            {
                Enabled = true,
                Entries = ["InternalProxy"]
            }
        },
        NetworkNTPSettings = new()
        {
            Enabled = true,
            Servers = ["NTPServer"],
        },
        Services =
        [
            new()
            {
                Name = "TestService",
                State = ServiceState.Enabled,
            },
        ]
    };

    private static readonly string s_sampleNonIndentedSystemConfigurationJson =
        GetSampleNonIndentedSystemConfigurationJson();

    private static string GetSampleNonIndentedSystemConfigurationJson()
    {
        var sampleJsonWithIndentation =
        """
        {
          "Version": 2,
          "NetworkInterfacesSettings": {
            "NetworkInterfaces": [
              {
                "CommonInformation": {
                  "Enabled": true,
                  "Name": "lan1",
                  "PhysicalAddress": "01:23:45:67:89:AB"
                },
                "IPv4": {
                  "Gateway": "120.86.52.18",
                  "DHCPEnabled": true,
                  "IPv4Details": [
                    {
                      "IPAddress": "120.86.52.18",
                      "Netmask": "120.86.52.18"
                    }
                  ],
                  "VLANEnabled": true,
                  "VLANID": 1
                }
              }
            ]
          },
          "NetworkDNSSettings": {
            "Hostname": "hostname",
            "MulticastDNSEnabled": true,
            "StaticHosts": {
              "Enabled": true,
              "Hosts": [
                {
                  "IPAddress": "120.86.52.18",
                  "Hostname": "statichost"
                }
              ]
            },
            "NameServers": {
              "Enabled": true,
              "Addresses": [
                "120.86.52.18"
              ]
            },
            "DNSSuffixEnabled": true,
            "DNSSuffix": "dnsSuffix",
            "SearchDomainsEnabled": true,
            "SearchDomains": [
              "searchDomain"
            ]
          },
          "NetworkProxySettings": {
            "FTP": {
              "Enabled": true,
              "Server": "ftpServer",
              "Port": 21,
              "Username": "ftpUser",
              "Password": "ftpPassword"
            },
            "HTTP": {
              "Enabled": true,
              "Server": "httpServer",
              "Port": 80,
              "Username": "httpUser",
              "Password": "httpPassword"
            },
            "HTTPS": {
              "Enabled": true,
              "Server": "httpsServer",
              "Port": 443,
              "Username": "httpsUser",
              "Password": "httpsPassword"
            },
            "SFTP": {
              "Enabled": true,
              "Server": "sftpServer",
              "Port": 22,
              "Username": "sftpUser",
              "Password": "sftpPassword"
            },
            "SOCKS": {
              "Enabled": true,
              "Server": "socksServer",
              "Port": 1080,
              "Username": "socksUser",
              "Password": "socksPassword"
            },
            "NoProxy": {
              "Enabled": true,
              "Entries": [
                "InternalProxy"
              ]
            }
          },
          "NetworkNTPSettings": {
            "Enabled": true,
            "Servers": [
              "NTPServer"
            ]
          },
          "Services": [
            {
              "Name": "TestService",
              "State": "Enabled"
            }
          ]
        }
        """;

        // Serialize JSON again to ensure it matches the expected format (non-indented).
        using var doc = JsonDocument.Parse(sampleJsonWithIndentation);
        return JsonSerializer.Serialize(doc.RootElement);
    }

    public class Deserialize
    {
        [Fact]
        public void ShouldThrow_WhenJsonContainsLiteralNull()
        {
            // Arrange
            var literalNullJson = "null";

            // Act
            var result = () => SystemConfigurationSerializer.Deserialize(literalNullJson);

            // Assert
            result.Should()
                .Throw<InvalidOperationException>()
                .WithMessage(
                    "JSON string could not be deserialized to 'SystemConfiguration'." +
                    Environment.NewLine +
                    $"JSON string was: '{literalNullJson}'");
        }

        [Fact]
        public void ShouldReturnSystemConfiguration_WhenJsonValid()
        {
            // Act
            var result = SystemConfigurationSerializer.Deserialize(s_sampleNonIndentedSystemConfigurationJson);

            // Assert
            result.Should().BeOfType<SystemConfiguration>();
        }
    }

    public class DeserializeAsync
    {
        [Fact]
        public void ShouldThrow_WhenJsonContainsLiteralNull()
        {
            // Arrange
            var literalNullJson = "null";

            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(literalNullJson));
            stream.Position = 0;

            // Act
            var result = async () => await SystemConfigurationSerializer.DeserializeAsync(stream);

            // Assert
            result.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("UTF-8 JSON stream could not be deserialized to 'SystemConfiguration'.");
        }

        [Fact]
        public async Task ShouldReturnSystemConfiguration_WhenJsonValid()
        {
            // Arrange
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(s_sampleNonIndentedSystemConfigurationJson));
            stream.Position = 0;

            // Act
            var result = await SystemConfigurationSerializer
                .DeserializeAsync(stream, cancellationToken: CancellationToken.None);

            // Assert
            result.Should().BeOfType<SystemConfiguration>();
        }
    }

    public class Serialize
    {
        [Fact]
        public void ShouldSerializeSystemConfiguration_WhenCalled()
        {
            // Act
            var result = SystemConfigurationSerializer.Serialize(s_sampleSystemConfiguration);

            // Assert
            result.Should().Be(s_sampleNonIndentedSystemConfigurationJson);
        }
    }

    public class SerializeAsync
    {
        [Fact]
        public async Task ShouldSerializeSystemConfiguration_WhenCalled()
        {
            // Arrange
            using var stream = new MemoryStream();

            // Act
            await SystemConfigurationSerializer
                .SerializeAsync(stream, s_sampleSystemConfiguration, cancellationToken: CancellationToken.None);

            var result = Encoding.UTF8.GetString(stream.ToArray());

            // Assert
            result.Should().Be(s_sampleNonIndentedSystemConfigurationJson);
        }
    }
}
