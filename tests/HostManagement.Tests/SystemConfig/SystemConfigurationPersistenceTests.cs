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

    public SystemConfigurationPersistenceTests()
        => _persistence = new(_fileSystem);

    public sealed class LoadPreparationConfiguration : SystemConfigurationPersistenceTests
    {
        [Fact]
        public async Task Cannot_load_empty_file_because_of_missing_version()
        {
            _fileSystem.AddFile(AppPaths.DefaultSystemConfigurationFilePath, new(string.Empty));

            await _persistence.Invoking(x => x.LoadPreparationConfiguration(CancellationToken.None))
                .Should().ThrowAsync<JsonException>().WithMessage("*expected*valid*JSON*");
        }

        [Fact]
        public async Task Cannot_load_empty_json_file_because_of_missing_version()
        {
            _fileSystem.AddFile(AppPaths.DefaultSystemConfigurationFilePath, new("{}"));

            await _persistence.Invoking(x => x.LoadPreparationConfiguration(CancellationToken.None))
                .Should().ThrowAsync<JsonException>().WithMessage("*missing*required*'Version'*");
        }

        [Fact]
        public async Task Can_load_minimal_json_file()
        {
            _fileSystem.AddFile(AppPaths.DefaultSystemConfigurationFilePath, new("""{"Version":1}"""));

            await _persistence.Invoking(x => x.LoadPreparationConfiguration(CancellationToken.None))
                .Should().NotThrowAsync();
        }

        [Fact]
        public async Task Can_load_partially_defined_usual_first_time_configuration()
        {
            var config =
                """
                {
                    "Version": 1,
                    "NetworkInterfacesSettings": {
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
                        ]
                    },
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
        public async Task Can_load_configuration_of_v_1_1_1()
        {
            var config =
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

            _fileSystem.AddFile(AppPaths.DefaultSystemConfigurationFilePath, new(config));

            var result = await _persistence.LoadPreparationConfiguration(CancellationToken.None);

            result.Should().BeEquivalentTo(new SystemConfiguration()
            {
                Version = 1,
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
                                PhysicalAddress = "00:02:01:ac:52:33",
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
                                        Netmask = IPAddress.Parse("255.255.0.0"),
                                    }
                                ],
                                VLANEnabled = false,
                                VLANID = 0,
                            },
                        },
                        new()
                        {
                            CommonInformation = new()
                            {
                                Enabled = true,
                                Name = "lan2",
                                PhysicalAddress = "00:02:01:ac:52:34",
                            },
                            IPv4 = new()
                            {
                                DHCPEnabled = false,
                                IPv4Details =
                                [
                                    new()
                                    {
                                        IPAddress = IPAddress.Parse("192.168.0.100"),
                                        Netmask = IPAddress.Parse("255.255.255.0"),
                                    },
                                ],
                                VLANEnabled = false,
                                VLANID = 0,
                            },
                        },
                    ],
                },
                NetworkDNSSettings = new()
                {
                    Hostname = "AE1400-Host-Management",
                    MulticastDNSEnabled = false,
                    StaticHostsEnabled = false,
                    StaticHosts = [],
                    NameServersEnabled = true,
                    NameServers = [IPAddress.Parse("192.168.10.10")],
                    DNSSuffixEnabled = false,
                    DNSSuffix = "",
                    SearchDomainsEnabled = false,
                    SearchDomains = [],
                },
                NetworkProxySettings = new()
                {
                    FTP = new()
                    {
                        Enabled = false,
                    },
                    HTTP = new()
                    {
                        Enabled = false,
                    },
                    HTTPS = new()
                    {
                        Enabled = false,
                    },
                    SFTP = new()
                    {
                        Enabled = false,
                    },
                    SOCKS = new()
                    {
                        Enabled = false,
                    },
                    DoNotProxyListEnabled = false,
                    DoNotProxyList = [],
                },
                NetworkNTPSettings = new()
                {
                    NTPServersEnabled = false,
                    NTPServers = [],
                },
                Services =
                [
                    new()
                    {
                        Name = "ssh.service",
                        State = ServiceState.Enabled,
                    },
                    new()
                    {
                        Name = "remoteconnect.service",
                        State = ServiceState.Disabled,
                    },
                    new()
                    {
                        Name = "vicione-suite.service",
                        State = ServiceState.Enabled,
                    },
                ],
            });
        }
    }

    public sealed class PreparationConfigurationExists : SystemConfigurationPersistenceTests
    {
        [Fact]
        public void ShouldReturnFalse_WhenFileDoesNotExist()
            => _persistence.PreparationConfigurationExists().Should().BeFalse();

        [Fact]
        public void ShouldReturnTrue_WhenFileExists()
        {
            _fileSystem.AddFile(AppPaths.DefaultSystemConfigurationFilePath, new(string.Empty));

            _persistence.PreparationConfigurationExists().Should().BeTrue();
        }
    }

    public sealed class UserConfigurationExists : SystemConfigurationPersistenceTests
    {
        [Fact]
        public void ShouldReturnFalse_WhenFileDoesNotExist()
            => _persistence.UserConfigurationExists().Should().BeFalse();

        [Fact]
        public void ShouldReturnTrue_WhenFileExists()
        {
            _fileSystem.AddFile(AppPaths.SystemConfigurationFilePath, new(string.Empty));

            _persistence.UserConfigurationExists().Should().BeTrue();
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
        public async Task Does_not_store_optional_information_of_default_configuration()
        {
            var expectedJson =
                """
                {
                  "Version": 1
                }
                """
                .UseEnvironmentNewLine();

            await _persistence.SaveUserConfiguration(new(), CancellationToken.None);

            var json = await _fileSystem.File
                .ReadAllTextAsync(AppPaths.SystemConfigurationFilePath, CancellationToken.None);

            json.Should().Be(expectedJson);
        }

        [Fact]
        public async Task Does_not_store_optional_information_of_fully_populated_configuration()
        {
            var ipAddress = IPAddress.Parse("120.86.52.18");

            SystemConfiguration systemConfiguration = new()
            {
                NetworkDNSSettings = new()
                {
                    DNSSuffix = "dnsSuffix",
                    DNSSuffixEnabled = true,
                    Hostname = "hostname",
                    MulticastDNSEnabled = true,
                    NameServers = [ipAddress],
                    NameServersEnabled = true,
                    SearchDomains = ["searchDomain"],
                    SearchDomainsEnabled = true,
                    StaticHosts =
                    [
                        new()
                        {
                            Hostname = "statichost",
                            IPAddress = ipAddress,
                        },
                    ],
                    StaticHostsEnabled = true,
                },
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
                                PhysicalAddress = "01:23:45:67:89:AB",
                            },
                            IPv4 = new()
                            {
                                DHCPEnabled = true,
                                DHCPLease = new()
                                {
                                    Gateway = ipAddress,
                                    IPv4Detail = new()
                                    {
                                        IPAddress = ipAddress,
                                        Netmask = ipAddress,
                                    },
                                    LeaseExpires = DateTimeOffset.UtcNow,
                                    LeaseObtained = DateTimeOffset.UtcNow,
                                    NetworkDNSSettings = new()
                                    {
                                        DNSSuffix = "dnsSuffix",
                                        DNSSuffixEnabled = true,
                                        Hostname = "hostname",
                                        MulticastDNSEnabled = true,
                                        NameServers = [ipAddress],
                                        NameServersEnabled = true,
                                        SearchDomains = ["searchDomain"],
                                        SearchDomainsEnabled = true,
                                        StaticHosts =
                                        [
                                            new()
                                            {
                                                Hostname = "statichost",
                                                IPAddress = ipAddress,
                                            },
                                        ],
                                        StaticHostsEnabled = true,
                                    },
                                },
                                Gateway = ipAddress,
                                IPv4Details =
                                [
                                    new()
                                    {
                                        IPAddress = ipAddress,
                                        Netmask = ipAddress,
                                    }
                                ],
                                VLANEnabled = true,
                                VLANID = 1,
                            },
                        },
                    ],
                },
                NetworkNTPSettings = new()
                {
                    FallbackNTPServers = ["fallbackNTPServer"],
                    NTPServersEnabled = true,
                    NTPServers = ["NTPServer"],
                },
                NetworkProxySettings = new()
                {
                    DoNotProxyList = ["InternalProxy"],
                    DoNotProxyListEnabled = true,
                    FTP = new()
                    {
                        Enabled = true,
                        Password = "ftpPassword",
                        Port = 21,
                        Server = "ftpServer",
                        Username = "ftpUser",
                    },
                    HTTP = new()
                    {
                        Enabled = true,
                        Password = "httpPassword",
                        Port = 80,
                        Server = "httpServer",
                        Username = "httpUser",
                    },
                    HTTPS = new()
                    {
                        Enabled = true,
                        Password = "httpsPassword",
                        Port = 443,
                        Server = "httpsServer",
                        Username = "httpsUser",
                    },
                    SFTP = new()
                    {
                        Enabled = true,
                        Password = "sftpPassword",
                        Port = 22,
                        Server = "sftpServer",
                        Username = "sftpUser",
                    },
                    SOCKS = new()
                    {
                        Enabled = true,
                        Password = "socksPassword",
                        Port = 1080,
                        Server = "socksServer",
                        Username = "socksUser",
                    },
                },
                Services =
                [
                    new()
                    {
                        Name = "TestService",
                        State = ServiceState.Enabled,
                    },
                ],
                Version = 1,
            };

            var expectedJson =
            """
            {
              "Version": 1,
              "NetworkInterfacesSettings": {
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
                "StaticHostsEnabled": true,
                "StaticHosts": [
                  {
                    "IPAddress": "120.86.52.18",
                    "Hostname": "statichost"
                  }
                ],
                "NameServersEnabled": true,
                "NameServers": [
                  "120.86.52.18"
                ],
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
                "DoNotProxyListEnabled": true,
                "DoNotProxyList": [
                  "InternalProxy"
                ]
              },
              "NetworkNTPSettings": {
                "NTPServersEnabled": true,
                "NTPServers": [
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
