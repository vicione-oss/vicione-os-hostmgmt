using System.Net;
using System.Net.NetworkInformation;
using System.Text.Json;
using AwesomeAssertions;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Syslog;
using HostManagement.Shared.Enums;
using HostManagement.Shared.Serialization;
using Xunit;

namespace HostManagement.Shared.Tests.Contracts;

public class SystemConfigurationSerializationTests
{
    private static readonly IPAddress s_ipAddress = IPAddress.Parse("120.86.52.18");
    private static readonly SystemConfiguration s_sampleSystemConfiguration = new()
    {
        Version = 2,
        NetworkInterfaces =
        [
            new()
            {
                CommonInformation = new()
                {
                    Enabled = true,
                    Name = "lan1",
                    UserDefinedMACAddress = new()
                    {
                        Enabled = true,
                        Address = PhysicalAddress.Parse("01:23:45:67:89:AB")
                    }
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
                            Netmask = s_ipAddress
                        }
                    ]
                },
                VLAN = new()
                {
                    Enabled = true,
                    ID = 1
                }
            }
        ],
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
                        IPAddress = s_ipAddress
                    }
                ]
            },
            NameServers = new()
            {
                Enabled = true,
                Addresses = [s_ipAddress]
            },
            PrimaryDNSSuffix = new()
            {
                Enabled = true,
                Suffix = "dnsSuffix"

            },
            SearchDomains = new()
            {
                Enabled = true,
                Domains = ["searchDomain"]
            }
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
            NoProxy = new()
            {
                Enabled = true,
                Entries = ["InternalProxy"]
            }
        },
        NetworkNTPSettings = new()
        {
            Enabled = true,
            Servers = ["NTPServer"]
        },
        Syslog = new()
        {
            Enabled = true,
            Address = "239.0.0.1:6000",
            TransportProtocol = SyslogTransportProtocol.Udp,
            LogFormat = SyslogLogFormat.Rfc5424,
            ConnectionRetrySeconds = 30,
            ExcludeSyslogFacilities =
            [
                SyslogFacility.Auth,
                SyslogFacility.Authpriv,
                SyslogFacility.SolarisCron
            ],
            ExcludeSyslogLevels =
            [
                SyslogLevel.Debug,
                SyslogLevel.Info
            ]
        },
        Services =
        [
            new()
            {
                Name = "TestService",
                State = ServiceState.Enabled
            }
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
          "NetworkInterfaces": [
            {
              "CommonInformation": {
                "Enabled": true,
                "Name": "lan1",
                "UserDefinedMACAddress": {
                    "Enabled": true,
                    "Address": "01:23:45:67:89:AB"
                }
              },
              "IPv4": {
                "Gateway": "120.86.52.18",
                "DHCPEnabled": true,
                "IPv4Details": [
                  {
                    "IPAddress": "120.86.52.18",
                    "Netmask": "120.86.52.18"
                  }
                ]
              },
              "VLAN": {
                "Enabled": true,
                "ID": 1
              }
            }
          ],
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
            "PrimaryDNSSuffix": {
              "Enabled": true,
              "Suffix": "dnsSuffix"
            },
            "SearchDomains": {
              "Enabled": true,
              "Domains": [
                "searchDomain"
              ]
            }
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
          "Syslog": {
            "Enabled": true,
            "Address": "239.0.0.1:6000",
            "TransportProtocol": "udp",
            "LogFormat": "rfc5424",
            "ConnectionRetrySeconds": 30,
            "ExcludeSyslogFacilities": [
              "auth",
              "authpriv",
              "solaris-cron"
            ],
            "ExcludeSyslogLevels": [
              "debug",
              "info"
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
            var result = () => MessageSerializer.Deserialize<SystemConfiguration>(literalNullJson);

            // Assert
            result.Should()
                .Throw<JsonException>()
                .WithMessage(
                    "Message content could not be deserialized to 'SystemConfiguration'." +
                    Environment.NewLine +
                    $"Message content was: '{literalNullJson}'");
        }

        [Fact]
        public void ShouldReturnSystemConfiguration_WhenJsonValid()
        {
            // Act
            var result = MessageSerializer.Deserialize<SystemConfiguration>(s_sampleNonIndentedSystemConfigurationJson);

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
            var result = MessageSerializer.Serialize(s_sampleSystemConfiguration);

            // Assert
            result.Should().Be(s_sampleNonIndentedSystemConfigurationJson);
        }
    }
}
