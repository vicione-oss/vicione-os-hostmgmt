using System.IO.Abstractions.TestingHelpers;
using System.Net;
using System.Text.Json;
using FluentAssertions;
using HostManagement.Configuration;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Enums;
using HostManagement.SystemConfig;
using Xunit;

namespace HostManagement.Tests.SystemConfig;

public class SystemConfigurationPersistenceTests
{
    private readonly MockFileSystem _fileSystem = new();
    private readonly SystemConfigurationPersistence _persistence;

    public SystemConfigurationPersistenceTests()
        => _persistence = new(_fileSystem);

    public sealed class LoadSystemConfiguration : SystemConfigurationPersistenceTests
    {
        [Fact]
        public async Task Cannot_load_empty_file_because_of_missing_version()
        {
            _fileSystem.AddFile(AppPaths.DefaultSystemConfigurationFilePath, new(string.Empty));

            await _persistence.Invoking(x => x.LoadSystemConfiguration(AppPaths.DefaultSystemConfigurationFilePath))
                .Should().ThrowAsync<JsonException>().WithMessage("*expected*valid*JSON*");
        }

        [Fact]
        public async Task Cannot_load_empty_json_file_because_of_missing_version()
        {
            _fileSystem.AddFile(AppPaths.DefaultSystemConfigurationFilePath, new("{}"));

            await _persistence.Invoking(x => x.LoadSystemConfiguration(AppPaths.DefaultSystemConfigurationFilePath))
                .Should().ThrowAsync<JsonException>().WithMessage("*missing*required*'Version'*");
        }

        [Fact]
        public async Task Can_load_minimal_json_file()
        {
            _fileSystem.AddFile(AppPaths.DefaultSystemConfigurationFilePath, new("""{"Version":1}"""));

            await _persistence.Invoking(x => x.LoadSystemConfiguration(AppPaths.DefaultSystemConfigurationFilePath))
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

            var result = await _persistence.Invoking(x => x.LoadSystemConfiguration(AppPaths.DefaultSystemConfigurationFilePath))
                .Should().NotThrowAsync();
        }
    }

    public sealed class SaveSystemConfiguration : SystemConfigurationPersistenceTests
    {
        [Fact]
        public async Task Does_not_store_optional_information_of_default_configuration()
        {
            var expectedJson =
                """
                {
                  "Version": 1,
                  "NetworkInterfacesSettings": {
                    "NetworkInterfaces": []
                  },
                  "NetworkDNSSettings": {
                    "Hostname": "",
                    "MulticastDNSEnabled": false,
                    "StaticHostsEnabled": false,
                    "StaticHosts": [],
                    "NameServersEnabled": false,
                    "NameServers": [],
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
                  "Services": []
                }
                """
                .UseEnvironmentNewLine();

            await _persistence.SaveSystemConfiguration(new(), AppPaths.DefaultSystemConfigurationFilePath);

            var json = await _fileSystem.File.ReadAllTextAsync(AppPaths.DefaultSystemConfigurationFilePath);

            json.Should().Be(expectedJson);
        }

        [Fact]
        public async Task Does_not_store_optional_information_of_fully_populated_configuration()
        {
            var ipAddress = IPAddress.Parse("120.86.52.18");

            var systemConfiguration = new SystemConfiguration()
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

            await _persistence.SaveSystemConfiguration(systemConfiguration, AppPaths.DefaultSystemConfigurationFilePath);

            var json = await _fileSystem.File.ReadAllTextAsync(AppPaths.DefaultSystemConfigurationFilePath);

            json.Should().Be(expectedJson);
        }
    }
}
