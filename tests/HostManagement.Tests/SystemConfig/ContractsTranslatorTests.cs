using System.Net;
using System.Net.NetworkInformation;
using AwesomeAssertions;
using HostManagement.SystemConfig;
using HostManagement.SystemConfig.Contracts;
using Xunit;

namespace HostManagement.Tests.SystemConfig;

public sealed class ContractsTranslatorTests
{
    public sealed class ToSharedContracts
    {
        [Fact]
        public void ShouldMapInternalConfigToSharedConfig()
        {
            // Arrange
            var internalConfig = new SystemConfiguration()
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
                                Address = PhysicalAddress.Parse("00:02:01:ac:52:33")
                            }
                        }
                    },
                    new()
                    {
                        CommonInformation = new()
                        {
                            Enabled = true,
                            Name = "lan2",
                            UserDefinedMACAddress = new()
                            {
                                Enabled = true,
                                Address = PhysicalAddress.Parse("00:02:01:ac:52:34")
                            }
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
                            Enabled = true,
                            ID = 42
                        }
                    }
                ],
                NetworkDNSSettings = new()
                {
                    Hostname = "debian",
                    MulticastDNSEnabled = false,
                    StaticHosts = new()
                    {
                        Enabled = true,
                        Hosts =
                        [
                            new()
                            {
                                Hostname = "statichost",
                                IPAddress = IPAddress.Parse("192.168.10.21")
                            }
                        ]
                    },
                    NameServers = new()
                    {
                        Enabled = true,
                        Addresses = [IPAddress.Parse("192.168.10.10")]
                    },
                    PrimaryDNSSuffix = new()
                    {
                        Enabled = false,
                        Suffix = "suffix"
                    },
                    SearchDomains = new()
                    {
                        Enabled = false,
                        Domains = ["google.com"]
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
                        Enabled = true,
                        Server = "httpServer",
                        Port = 8080,
                        Username = "httpUser",
                        Password = "httpPassword"
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
                        Entries = ["no-proxy-item"]
                    }
                },
                NetworkNTPSettings = new()
                {
                    Enabled = false,
                    Servers = ["time.google.com"]
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
                        Name = "vicione-suite.service",
                        State = ServiceState.Disabled
                    }
                ]
            };

            var expectedSharedConfig = new Shared.Contracts.SystemConfiguration()
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
                                Address = PhysicalAddress.Parse("00:02:01:ac:52:33")
                            }
                        },
                        IPv4 = new()
                        {
                            Gateway = null,
                            DHCPEnabled = false,
                            IPv4Details = [],
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
                            UserDefinedMACAddress = new()
                            {
                                Enabled = true,
                                Address = PhysicalAddress.Parse("00:02:01:ac:52:34")
                            }
                        },
                        IPv4 = new()
                        {
                            Gateway = null,
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
                            Enabled = true,
                            ID = 42
                        }
                    }
                ],
                NetworkDNSSettings = new()
                {
                    Hostname = "debian",
                    MulticastDNSEnabled = false,
                    StaticHosts = new()
                    {
                        Enabled = true,
                        Hosts =
                        [
                            new()
                            {
                                Hostname = "statichost",
                                IPAddress = IPAddress.Parse("192.168.10.21")
                            }
                        ]
                    },
                    NameServers = new()
                    {
                        Enabled = true,
                        Addresses = [IPAddress.Parse("192.168.10.10")]
                    },
                    PrimaryDNSSuffix = new()
                    {
                        Enabled = false,
                        Suffix = "suffix"
                    },
                    SearchDomains = new()
                    {
                        Enabled = false,
                        Domains = ["google.com"]
                    }
                },
                NetworkProxySettings = new()
                {
                    FTP = new()
                    {
                        Enabled = false,
                        Server = null,
                        Port = null,
                        Username = null,
                        Password = null
                    },
                    HTTP = new()
                    {
                        Enabled = true,
                        Server = "httpServer",
                        Port = 8080,
                        Username = "httpUser",
                        Password = "httpPassword"
                    },
                    HTTPS = new()
                    {
                        Enabled = false,
                        Server = null,
                        Port = null,
                        Username = null,
                        Password = null
                    },
                    SFTP = new()
                    {
                        Enabled = false,
                        Server = null,
                        Port = null,
                        Username = null,
                        Password = null
                    },
                    NoProxy = new()
                    {
                        Enabled = false,
                        Entries = ["no-proxy-item"]
                    }
                },
                NetworkNTPSettings = new()
                {
                    Enabled = false,
                    Servers = ["time.google.com"]
                },
                Services =
                [
                    new()
                    {
                        Name = "ssh.service",
                        State = Shared.Enums.ServiceState.Enabled
                    },
                    new()
                    {
                        Name = "vicione-suite.service",
                        State = Shared.Enums.ServiceState.Disabled
                    }
                ]
            };

            // Act
            var sharedConfig = internalConfig.ToSharedContracts();

            // Assert
            sharedConfig.Should().BeEquivalentTo(expectedSharedConfig);
        }

        [Fact]
        public void ShouldMapInternalDHCPLeaseToSharedDHCPLease()
        {
            // Arrange
            var sampleDateTimeOffset = new DateTimeOffset(2025, 06, 23, 12, 34, 56, offset: TimeSpan.FromHours(2));

            var internalLease = new DHCPLease
            {
                LeaseObtained = sampleDateTimeOffset,
                LeaseExpires = sampleDateTimeOffset,
                Gateway = IPAddress.Parse("192.168.0.1"),
                NetworkDNSSettings = new(),
                IPv4Detail = new()
                {
                    IPAddress = IPAddress.Parse("192.168.0.100"),
                    Netmask = IPAddress.Parse("255.255.255.0")
                }
            };

            var expectedSharedLease = new Shared.Contracts.Network.DHCPLease
            {
                LeaseObtained = sampleDateTimeOffset,
                LeaseExpires = sampleDateTimeOffset,
                Gateway = IPAddress.Parse("192.168.0.1"),
                NetworkDNSSettings = new(),
                IPv4Detail = new()
                {
                    IPAddress = IPAddress.Parse("192.168.0.100"),
                    Netmask = IPAddress.Parse("255.255.255.0")
                }
            };

            // Act
            var sharedLease = internalLease.ToSharedContracts();

            // Assert
            sharedLease.Should().BeEquivalentTo(expectedSharedLease);
        }
    }

    public sealed class ToInternalContracts
    {
        [Fact]
        public void ShouldMapSharedConfigToInternalConfig()
        {
            // Arrange
            var sharedConfig = new Shared.Contracts.SystemConfiguration()
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
                                Address = PhysicalAddress.Parse("00:02:01:ac:52:33")
                            }
                        },
                        IPv4 = new()
                        {
                            Gateway = null,
                            DHCPEnabled = false,
                            IPv4Details = [],
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
                            UserDefinedMACAddress = new()
                            {
                                Enabled = true,
                                Address = PhysicalAddress.Parse("00:02:01:ac:52:34")
                            }
                        },
                        IPv4 = new()
                        {
                            Gateway = null,
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
                            Enabled = true,
                            ID = 42
                        }
                    }
                ],
                NetworkDNSSettings = new()
                {
                    Hostname = "debian",
                    MulticastDNSEnabled = false,
                    StaticHosts = new()
                    {
                        Enabled = true,
                        Hosts =
                        [
                            new()
                            {
                                Hostname = "statichost",
                                IPAddress = IPAddress.Parse("192.168.10.21")
                            }
                        ]
                    },
                    NameServers = new()
                    {
                        Enabled = true,
                        Addresses = [IPAddress.Parse("192.168.10.10")]
                    },
                    PrimaryDNSSuffix = new()
                    {
                        Enabled = false,
                        Suffix = "suffix"
                    },
                    SearchDomains = new()
                    {
                        Enabled = false,
                        Domains = ["google.com"]
                    }
                },
                NetworkProxySettings = new()
                {
                    FTP = new()
                    {
                        Enabled = false,
                        Server = null,
                        Port = null,
                        Username = null,
                        Password = null
                    },
                    HTTP = new()
                    {
                        Enabled = true,
                        Server = "httpServer",
                        Port = 8080,
                        Username = "httpUser",
                        Password = "httpPassword"
                    },
                    HTTPS = new()
                    {
                        Enabled = false,
                        Server = null,
                        Port = null,
                        Username = null,
                        Password = null
                    },
                    SFTP = new()
                    {
                        Enabled = false,
                        Server = null,
                        Port = null,
                        Username = null,
                        Password = null
                    },
                    NoProxy = new()
                    {
                        Enabled = false,
                        Entries = ["no-proxy-item"]
                    }
                },
                NetworkNTPSettings = new()
                {
                    Enabled = false,
                    Servers = ["time.google.com"]
                },
                Syslog = new()
                {
                    Enabled = true,
                    Address = "239.0.0.1:6000",
                    TransportProtocol = Shared.Contracts.Syslog.SyslogTransportProtocol.Udp,
                    LogFormat = Shared.Contracts.Syslog.SyslogLogFormat.Rfc5424,
                    ConnectionRetrySeconds = 30,
                    ExcludeSyslogFacilities =
                    [
                        Shared.Contracts.Syslog.SyslogFacility.Auth,
                        Shared.Contracts.Syslog.SyslogFacility.Authpriv,
                        Shared.Contracts.Syslog.SyslogFacility.SolarisCron
                    ],
                    ExcludeSyslogLevels =
                    [
                        Shared.Contracts.Syslog.SyslogLevel.Debug,
                        Shared.Contracts.Syslog.SyslogLevel.Info
                    ]
                },
                Services =
                [
                    new()
                    {
                        Name = "ssh.service",
                        State = Shared.Enums.ServiceState.Enabled
                    },
                    new()
                    {
                        Name = "vicione-suite.service",
                        State = Shared.Enums.ServiceState.Disabled
                    }
                ]
            };

            var expectedInternalConfig = new SystemConfiguration()
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
                                Address = PhysicalAddress.Parse("00:02:01:ac:52:33")
                            }
                        },
                        IPv4 = new()
                        {
                            DHCPEnabled = false,
                            IPv4Details = [],
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
                            UserDefinedMACAddress = new()
                            {
                                Enabled = true,
                                Address = PhysicalAddress.Parse("00:02:01:ac:52:34")
                            }
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
                            Enabled = true,
                            ID = 42
                        }
                    }
                ],
                NetworkDNSSettings = new()
                {
                    Hostname = "debian",
                    MulticastDNSEnabled = false,
                    StaticHosts = new()
                    {
                        Enabled = true,
                        Hosts =
                        [
                            new()
                            {
                                Hostname = "statichost",
                                IPAddress = IPAddress.Parse("192.168.10.21")
                            }
                        ]
                    },
                    NameServers = new()
                    {
                        Enabled = true,
                        Addresses = [IPAddress.Parse("192.168.10.10")]
                    },
                    PrimaryDNSSuffix = new()
                    {
                        Enabled = false,
                        Suffix = "suffix"
                    },
                    SearchDomains = new()
                    {
                        Enabled = false,
                        Domains = ["google.com"]
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
                        Enabled = true,
                        Server = "httpServer",
                        Port = 8080,
                        Username = "httpUser",
                        Password = "httpPassword"
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
                        Entries = ["no-proxy-item"]
                    }
                },
                NetworkNTPSettings = new()
                {
                    Enabled = false,
                    Servers = ["time.google.com"]
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
                        Name = "ssh.service",
                        State = ServiceState.Enabled
                    },
                    new()
                    {
                        Name = "vicione-suite.service",
                        State = ServiceState.Disabled
                    }
                ]
            };

            // Act
            var internalConfig = sharedConfig.ToInternalContracts();

            // Assert
            internalConfig.Should().BeEquivalentTo(expectedInternalConfig);
        }
    }
}
