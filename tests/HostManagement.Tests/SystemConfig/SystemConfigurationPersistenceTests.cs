using System.IO.Abstractions.TestingHelpers;
using System.Net;
using System.Text.Json;
using AwesomeAssertions;
using HostManagement.Configuration;
using HostManagement.SystemConfig;
using HostManagement.SystemConfig.Contracts;
using Xunit;

namespace HostManagement.Tests.SystemConfig;

public class SystemConfigurationPersistenceTests
{
    private readonly MockFileSystem _fileSystem = new();
    private readonly SystemConfigurationPersistence _persistence;

    public SystemConfigurationPersistenceTests() => _persistence = new(_fileSystem);

    public sealed class LoadPreparationConfiguration : SystemConfigurationPersistenceTests
    {
        // DO NOT modify!
        private static readonly string s_jsonConfigV1_1_1 =
            """
            {
              "Version": 1,
              "NetworkInterfacesSettings": {
                "NetworkInterfaces": [
                  {
                    "CommonInformation": {
                      "Enabled": true,
                      "Name": "lan1",
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
                      "VLANEnabled": false,
                      "VLANID": 0
                    }
                  },
                  {
                    "CommonInformation": {
                      "Enabled": true,
                      "Name": "lan2",
                      "PhysicalAddress": "00:02:01:ac:52:34"
                    },
                    "IPv4": {
                      "DHCPEnabled": false,
                      "IPv4Details": [
                        {
                          "IPAddress": "192.168.0.100",
                          "Netmask": "255.255.255.0"
                        }
                      ],
                      "VLANEnabled": false,
                      "VLANID": 0
                    }
                  }
                ]
              },
              "NetworkDNSSettings": {
                "Hostname": "AE1400-Host-Management",
                "MulticastDNSEnabled": false,
                "StaticHostsEnabled": false,
                "StaticHosts": [],
                "NameServersEnabled": true,
                "NameServers": [
                  "192.168.10.10"
                ],
                "DNSSuffixEnabled": false,
                "DNSSuffix": "",
                "SearchDomainsEnabled": false,
                "SearchDomains": []
              },
              "NetworkProxySettings": {
                "FTP": {
                  "Enabled": false
                },
                "HTTP": {
                  "Enabled": false
                },
                "HTTPS": {
                  "Enabled": false
                },
                "SFTP": {
                  "Enabled": false
                },
                "SOCKS": {
                  "Enabled": false
                },
                "DoNotProxyListEnabled": false,
                "DoNotProxyList": []
              },
              "NetworkNTPSettings": {
                "NTPServersEnabled": false,
                "NTPServers": []
              },
              "Services": [
                {
                  "Name": "ssh.service",
                  "State": "Enabled"
                },
                {
                  "Name": "remoteconnect.service",
                  "State": "Disabled"
                },
                {
                  "Name": "vicione-suite.service",
                  "State": "Enabled"
                }
              ]
            }
            """
            .UseEnvironmentNewLine();

        // Modify if current config changed, e.g. 'Version' should be the current version (currently 2)
        private static readonly SystemConfiguration s_configV1_1_1_AfterMigration = new()
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
                        PhysicalAddress = "00:02:01:ac:52:33"
                    },
                    IPv4 = new()
                    {
                        Gateway = IPAddress.Parse("192.168.1.1"),
                        DHCPEnabled = false,
                        IPv4Details =
                        [
                            new()
                            {
                                IPAddress = IPAddress.Parse("192.168.60.3"),
                                Netmask = IPAddress.Parse("255.255.0.0")
                            }
                        ],
                    },
                    VLAN = new()
                    {
                        Enabled = false,
                        ID = 0
                    }
                },
                new()
                {
                    CommonInformation = new()
                    {
                        Enabled = true,
                        Name = "lan2",
                        PhysicalAddress = "00:02:01:ac:52:34"
                    },
                    IPv4 = new()
                    {
                        DHCPEnabled = false,
                        IPv4Details =
                        [
                            new()
                            {
                                IPAddress = IPAddress.Parse("192.168.0.100"),
                                Netmask = IPAddress.Parse("255.255.255.0")
                            }
                        ],
                    },
                    VLAN = new()
                    {
                        Enabled = false,
                        ID = 0
                    }
                }
            ],
            NetworkDNSSettings = new()
            {
                Hostname = "AE1400-Host-Management",
                MulticastDNSEnabled = false,
                StaticHosts = new()
                {
                    Enabled = false,
                    Hosts = []
                },
                NameServers = new()
                {
                    Enabled = true,
                    Addresses = [IPAddress.Parse("192.168.10.10")]
                },
                PrimaryDNSSuffix = new()
                {
                    Enabled = false,
                    Suffix = ""
                },
                SearchDomains = new()
                {
                    Enabled = false,
                    Domains = []
                }
            },
            NetworkProxySettings = new()
            {
                FTP = new()
                {
                    Enabled = false
                },
                HTTP = new()
                {
                    Enabled = false
                },
                HTTPS = new()
                {
                    Enabled = false
                },
                SFTP = new()
                {
                    Enabled = false
                },
                NoProxy = new()
                {
                    Enabled = false,
                    Entries = []
                }
            },
            NetworkNTPSettings = new()
            {
                Enabled = false,
                Servers = []
            },
            Services =
            [
                new()
                {
                    Name = "ssh.service",
                    State = ServiceState.Enabled
                },
                new()
                {
                    Name = "remoteconnect.service",
                    State = ServiceState.Disabled
                },
                new()
                {
                    Name = "vicione-suite.service",
                    State = ServiceState.Enabled
                }
            ]
        };

        // Config of v1.3.0 is identical to v1.1.1
        private static readonly string s_jsonConfigV1_3_0 = s_jsonConfigV1_1_1;
        private static readonly SystemConfiguration s_configV1_3_0_AfterMigration = s_configV1_1_1_AfterMigration;

        [Fact]
        public async Task ShouldThrow_WhenLoadingEmptyFile()
        {
            _fileSystem.AddFile(AppPaths.DefaultSystemConfigurationFilePath, new(string.Empty));

            await _persistence.Invoking(x => x.LoadPreparationConfiguration(CancellationToken.None))
                .Should().ThrowAsync<InvalidSystemConfigurationException>()
                .WithMessage("UTF-8 JSON stream could not be deserialized to 'SystemConfiguration'.")
                .WithInnerException(typeof(JsonException))
                .WithMessage("*expected*valid*JSON*");
        }

        [Fact]
        public async Task ShouldThrow_WhenLoadingEmptyJsonFileWithoutVersion()
        {
            _fileSystem.AddFile(AppPaths.DefaultSystemConfigurationFilePath, new("{}"));

            await _persistence.Invoking(x => x.LoadPreparationConfiguration(CancellationToken.None))
                .Should().ThrowAsync<InvalidSystemConfigurationException>()
                .WithMessage("UTF-8 JSON stream could not be deserialized to 'SystemConfiguration'.")
                .WithInnerException(typeof(JsonException))
                .WithMessage("*missing*required*'Version'*");
        }

        [Fact]
        public async Task ShouldThrow_WhenVersionIsUnsupported()
        {
            _fileSystem.AddFile(AppPaths.DefaultSystemConfigurationFilePath, new("""{"Version":0}"""));

            await _persistence.Invoking(x => x.LoadPreparationConfiguration(CancellationToken.None))
                .Should()
                .ThrowAsync<NotSupportedException>()
                .WithMessage("*version '0' is not supported*");
        }

        [Fact]
        public async Task ShouldThrow_WhenVersionIsNotANumber()
        {
            _fileSystem.AddFile(AppPaths.DefaultSystemConfigurationFilePath, new("""{"Version":"2"}"""));

            await _persistence.Invoking(x => x.LoadPreparationConfiguration(CancellationToken.None))
                .Should().ThrowAsync<InvalidSystemConfigurationException>()
                .WithMessage("UTF-8 JSON stream could not be deserialized to 'SystemConfiguration'.")
                .WithInnerException(typeof(JsonException))
                .WithMessage("*'Version' must be an integer*");
        }

        [Fact]
        public async Task ShouldLoadMinimalJsonFile_WhenVersionPresent()
        {
            _fileSystem.AddFile(AppPaths.DefaultSystemConfigurationFilePath, new("""{"Version":2}"""));

            await _persistence.Invoking(x => x.LoadPreparationConfiguration(CancellationToken.None))
                .Should().NotThrowAsync();
        }

        [Fact]
        public async Task ShouldLoadPartiallyDefinedDefaultSystemConfiguration()
        {
            var config =
                """
                {
                    "Version": 2,
                    "NetworkInterfaces": [
                        {
                            "CommonInformation": {
                                "Enabled": true,
                                "Name": "lan1"
                            },
                            "IPv4": {
                                "DHCPEnabled": true
                            }
                        },
                        {
                            "CommonInformation": {
                                "Enabled": true,
                                "Name": "lan2"
                            },
                            "IPv4": {
                                "DHCPEnabled": false,
                                "IPv4Details": [
                                    {
                                        "IPAddress": "192.168.0.100",
                                        "Netmask": "255.255.255.0"
                                    }
                                ]
                            }
                        }
                    ],
                    "NetworkDNSSettings": {
                        "Hostname": "%%hostname%%"
                    }
                }
                """
                .UseEnvironmentNewLine();

            _fileSystem.AddFile(AppPaths.DefaultSystemConfigurationFilePath, new(config));

            await _persistence.Invoking(x => x.LoadPreparationConfiguration(CancellationToken.None))
                .Should().NotThrowAsync();
        }

        [Fact]
        public async Task ShouldLoadSystemConfiguration_WhenVersionIsV1_1_1()
        {
            _fileSystem.AddFile(AppPaths.DefaultSystemConfigurationFilePath, new(s_jsonConfigV1_1_1));
            var result = await _persistence.LoadPreparationConfiguration(CancellationToken.None);
            result.Should().BeEquivalentTo(s_configV1_1_1_AfterMigration);
        }

        [Fact]
        public async Task ShouldLoadSystemConfiguration_WhenVersionIsV1_3_0()
        {
            _fileSystem.AddFile(AppPaths.DefaultSystemConfigurationFilePath, new(s_jsonConfigV1_3_0));
            var result = await _persistence.LoadPreparationConfiguration(CancellationToken.None);
            result.Should().BeEquivalentTo(s_configV1_3_0_AfterMigration);
        }
    }

    public sealed class PreparationConfigurationExists : SystemConfigurationPersistenceTests
    {
        [Fact]
        public void ShouldReturnFalse_WhenFileDoesNotExist()
            => _persistence.ExistsPreparationConfiguration().Should().BeFalse();

        [Fact]
        public void ShouldReturnTrue_WhenFileExists()
        {
            _fileSystem.AddFile(AppPaths.DefaultSystemConfigurationFilePath, new(string.Empty));

            _persistence.ExistsPreparationConfiguration().Should().BeTrue();
        }
    }

    public sealed class UserConfigurationExists : SystemConfigurationPersistenceTests
    {
        [Fact]
        public void ShouldReturnFalse_WhenFileDoesNotExist()
            => _persistence.ExistsUserConfiguration().Should().BeFalse();

        [Fact]
        public void ShouldReturnTrue_WhenFileExists()
        {
            _fileSystem.AddFile(AppPaths.SystemConfigurationFilePath, new(string.Empty));

            _persistence.ExistsUserConfiguration().Should().BeTrue();
        }
    }

    public sealed class GetPreparationConfigurationPath : SystemConfigurationPersistenceTests
    {
        [Fact]
        public void ShouldReturnDefaultSystemConfigurationFilePath()
            => _persistence.GetPreparationConfigurationPath().Should().Be(AppPaths.DefaultSystemConfigurationFilePath);
    }

    public sealed class GetUserConfigurationPath : SystemConfigurationPersistenceTests
    {
        [Fact]
        public void ShouldReturnSystemConfigurationFilePath()
            => _persistence.GetUserConfigurationPath().Should().Be(AppPaths.SystemConfigurationFilePath);
    }

    public sealed class SaveUserConfiguration : SystemConfigurationPersistenceTests
    {
        [Fact]
        public async Task ShouldNotStoreOptionalInformation_WhenNewConfigurationIsSaved()
        {
            var expectedJson =
                """
                {
                  "Version": 2
                }
                """
                .UseEnvironmentNewLine();

            await _persistence.SaveUserConfiguration(new(), CancellationToken.None);

            var json = await _fileSystem.File
                .ReadAllTextAsync(AppPaths.SystemConfigurationFilePath, CancellationToken.None);

            json.Should().Be(expectedJson);
        }

        [Fact]
        public async Task ShouldNotStoreOptionalInformation_WhenFullyPopulatedConfigurationIsSaved()
        {
            var ipAddress = IPAddress.Parse("120.86.52.18");

            SystemConfiguration systemConfiguration = new()
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
                            PhysicalAddress = "01:23:45:67:89:AB"
                        },
                        IPv4 = new()
                        {
                            DHCPEnabled = true,
                            Gateway = ipAddress,
                            IPv4Details =
                            [
                                new()
                                {
                                    IPAddress = ipAddress,
                                    Netmask = ipAddress
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
                                IPAddress = ipAddress
                            }
                        ]
                    },
                    NameServers = new()
                    {
                        Enabled = true,
                        Addresses = [ipAddress]
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
                    FTP = new()
                    {
                        Enabled = true,
                        Password = "ftpPassword",
                        Port = 21,
                        Server = "ftpServer",
                        Username = "ftpUser"
                    },
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
                Services =
                [
                    new()
                    {
                        Name = "TestService",
                        State = ServiceState.Enabled
                    }
                ]
            };

            var expectedJson =
            """
            {
              "Version": 2,
              "NetworkInterfaces": [
                {
                  "CommonInformation": {
                    "Name": "lan1",
                    "Enabled": true,
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
              "Services": [
                {
                  "Name": "TestService",
                  "State": "Enabled"
                }
              ]
            }
            """
            .UseEnvironmentNewLine();

            await _persistence.SaveUserConfiguration(systemConfiguration, CancellationToken.None);

            var json = await _fileSystem.File
                .ReadAllTextAsync(AppPaths.SystemConfigurationFilePath, CancellationToken.None);

            json.Should().Be(expectedJson);
        }
    }
}
