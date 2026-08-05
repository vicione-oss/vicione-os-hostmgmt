using System.Net;
using AwesomeAssertions;
using HostManagement.SystemConfig.Contracts;
using Xunit;

namespace HostManagement.Tests.SystemConfig.Contracts;

public class SystemConfigurationTests
{
    public sealed class EqualsMethod
    {
        [Fact]
        public void ShouldReturnTrue_WhenEqualConfigurationsComparedAsObjects()
        {
            // Arrange
            var config1 = CreateSampleConfiguration();
            var config2 = CreateSampleConfiguration();

            // Act
            var result = object.Equals(config1, config2);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldReturnTrue_WhenEqualConfigurationsComparedByDefaultComparer()
        {
            // Arrange
            var config1 = CreateSampleConfiguration();
            var config2 = CreateSampleConfiguration();

            // Act
            var result = EqualityComparer<SystemConfiguration>.Default.Equals(config1, config2);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldReturnTrue_WhenIdenticalObjects()
        {
            var config1 = CreateSampleConfiguration();
            var config2 = CreateSampleConfiguration();

            var result = config1.Equals(config2);

            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldReturnTrue_WhenEmptyConfiguration()
        {
            var config1 = new SystemConfiguration();
            var config2 = new SystemConfiguration();

            var result = config1.Equals(config2);

            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldReturnFalse_WhenNullVsNonNull()
        {
            var config = CreateSampleConfiguration();

            var result = config.Equals(null);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenNullVsNonNullNetworkInterfaces()
        {
            var config1 = new SystemConfiguration { NetworkInterfaces = null };
            var config2 = new SystemConfiguration { NetworkInterfaces = [] };

            var result = config1.Equals(config2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDifferentNetworkInterfaces()
        {
            var config1 = CreateSampleConfiguration();
            var config2 = CreateSampleConfiguration();
            config2.NetworkInterfaces!.Add(new NetworkInterfaceDetail
            {
                CommonInformation = new NetworkInterfaceCommonInformation { Enabled = true, Name = "eth2" },
                IPv4 = new IPv4Settings()
            });

            var result = config1.Equals(config2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDifferentNumberOfNetworkInterfaces()
        {
            var config1 = CreateSampleConfiguration();
            var config2 = CreateSampleConfiguration();
            config2.NetworkInterfaces!.Add(new NetworkInterfaceDetail
            {
                CommonInformation = new NetworkInterfaceCommonInformation { Enabled = true, Name = "eth1" },
                IPv4 = new IPv4Settings(),
                VLAN = new VLANSettings()
            });

            var result = config1.Equals(config2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDifferentNetworkInterfaceOrder()
        {
            var interface1 = new NetworkInterfaceDetail
            {
                CommonInformation = new NetworkInterfaceCommonInformation { Enabled = true, Name = "eth0" },
                IPv4 = new IPv4Settings(),
                VLAN = new VLANSettings()
            };
            var interface2 = new NetworkInterfaceDetail
            {
                CommonInformation = new NetworkInterfaceCommonInformation { Enabled = true, Name = "eth1" },
                IPv4 = new IPv4Settings(),
                VLAN = new VLANSettings()
            };

            var config1 = CreateSampleConfiguration();
            config1.NetworkInterfaces = [interface1, interface2];

            var config2 = CreateSampleConfiguration();
            config2.NetworkInterfaces = [interface2, interface1];

            var result = config1.Equals(config2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDifferentNetworkInterfaceProperties()
        {
            var config1 = CreateSampleConfiguration();
            var config2 = CreateSampleConfiguration();
            config2.NetworkInterfaces![0].CommonInformation!.Name = "different-name";

            var result = config1.Equals(config2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenNestedIPv4Differences()
        {
            var config1 = CreateSampleConfiguration();
            var config2 = CreateSampleConfiguration();
            config2.NetworkInterfaces![0].IPv4!.Gateway = IPAddress.Parse("10.0.0.1");

            var result = config1.Equals(config2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDifferentVLANEnabled()
        {
            var config1 = CreateSampleConfiguration();
            var config2 = CreateSampleConfiguration();
            config2.NetworkInterfaces![0].VLAN!.Enabled = !config1.NetworkInterfaces![0].VLAN!.Enabled;

            var result = config1.Equals(config2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenNullVsNonNullVLANEnabled()
        {
            var config1 = CreateSampleConfiguration();
            var config2 = CreateSampleConfiguration();
            config2.NetworkInterfaces![0].VLAN!.Enabled = null;

            var result = config1.Equals(config2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDifferentVLANID()
        {
            var config1 = CreateSampleConfiguration();
            var config2 = CreateSampleConfiguration();
            config2.NetworkInterfaces![0].VLAN!.ID = 200;

            var result = config1.Equals(config2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenNullVsNonNullVLANID()
        {
            var config1 = CreateSampleConfiguration();
            var config2 = CreateSampleConfiguration();
            config2.NetworkInterfaces![0].VLAN!.ID = null;

            var result = config1.Equals(config2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnTrue_WhenBothNetworkInterfacesNull()
        {
            var config1 = CreateSampleConfiguration();
            var config2 = CreateSampleConfiguration();

            config1.NetworkInterfaces = null;
            config2.NetworkInterfaces = null;

            var result = config1.Equals(config2);

            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldReturnFalse_WhenNullVsNonNullNetworkDNSSettings()
        {
            var config1 = new SystemConfiguration { NetworkDNSSettings = null };
            var config2 = new SystemConfiguration
            {
                NetworkDNSSettings = new NetworkDNSSettings { Hostname = "test-host" }
            };

            var result = config1.Equals(config2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDifferentDNSSettings()
        {
            var config1 = CreateSampleConfiguration();
            var config2 = CreateSampleConfiguration();
            config2.NetworkDNSSettings!.Hostname = "different-hostname";

            var result = config1.Equals(config2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenNullVsNonNullNetworkProxySettings()
        {
            var config1 = new SystemConfiguration { NetworkProxySettings = null };
            var config2 = new SystemConfiguration
            {
                NetworkProxySettings = new NetworkProxySettings
                {
                    HTTP = new NetworkProxyDetail { Enabled = false, Server = "proxy.com", Port = 8080 }
                }
            };

            var result = config1.Equals(config2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDifferentProxySettings()
        {
            var config1 = CreateSampleConfiguration();
            var config2 = CreateSampleConfiguration();
            config2.NetworkProxySettings!.HTTP!.Enabled = true;

            var result = config1.Equals(config2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenNullVsNonNullNetworkNTPSettings()
        {
            var config1 = new SystemConfiguration { NetworkNTPSettings = null };
            var config2 = new SystemConfiguration
            {
                NetworkNTPSettings = new NetworkNTPSettings { Enabled = true }
            };

            var result = config1.Equals(config2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDifferentNTPSettings()
        {
            var config1 = CreateSampleConfiguration();
            var config2 = CreateSampleConfiguration();
            config2.NetworkNTPSettings!.Enabled = true;

            var result = config1.Equals(config2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenNullVsNonNullServices()
        {
            var config1 = new SystemConfiguration { Services = null };
            var config2 = new SystemConfiguration
            {
                Services = [new ServiceDetail { Name = "apache2", State = ServiceState.Enabled }]
            };

            var result = config1.Equals(config2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDifferentServices()
        {
            var config1 = CreateSampleConfiguration();
            var config2 = CreateSampleConfiguration();
            config2.Services!.Add(new ServiceDetail { Name = "nginx", State = ServiceState.Enabled });

            var result = config1.Equals(config2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnTrue_WhenServicesInDifferentOrder()
        {
            var config1 = new SystemConfiguration
            {
                Services =
                [
                    new ServiceDetail { Name = "service1", State = ServiceState.Enabled },
                    new ServiceDetail { Name = "service2", State = ServiceState.Disabled },
                ]
            };

            var config2 = new SystemConfiguration
            {
                Services =
                [
                    new ServiceDetail { Name = "service2", State = ServiceState.Disabled },
                    new ServiceDetail { Name = "service1", State = ServiceState.Enabled },
                ]
            };

            var result = config1.Equals(config2);

            result.Should().BeTrue();
        }
    }

    public sealed class GetHashCodeMethod
    {
        [Fact]
        public void ShouldReturnSameHashCode_WhenIdenticalObjects()
        {
            var config1 = CreateSampleConfiguration();
            var config2 = CreateSampleConfiguration();

            var hash1 = config1.GetHashCode();
            var hash2 = config2.GetHashCode();

            hash1.Should().Be(hash2);
        }

        [Fact]
        public void ShouldReturnDifferentHashCode_WhenDifferentObjects()
        {
            var config1 = CreateSampleConfiguration();
            var config2 = CreateSampleConfiguration();
            config2.NetworkDNSSettings!.Hostname = "different-hostname";

            var hash1 = config1.GetHashCode();
            var hash2 = config2.GetHashCode();

            hash1.Should().NotBe(hash2);
        }
    }

    private static SystemConfiguration CreateSampleConfiguration()
    {
        return new SystemConfiguration
        {
            Version = 2,
            NetworkInterfaces =
            [
                new NetworkInterfaceDetail
                {
                    CommonInformation = new NetworkInterfaceCommonInformation
                    {
                        Enabled = true,
                        Name = "eth0",
                        PhysicalAddress = "00:11:22:33:44:55",
                    },
                    IPv4 = new IPv4Settings
                    {
                        Gateway = IPAddress.Parse("192.168.1.1"),
                        DHCPEnabled = false,
                        IPv4Details =
                        [
                            new IPv4Detail
                            {
                                IPAddress = IPAddress.Parse("192.168.1.100"),
                                Netmask = IPAddress.Parse("255.255.255.0"),
                            },
                        ],
                    },
                    VLAN = new VLANSettings
                    {
                        Enabled = true,
                        ID = 100
                    }
                },
            ],
            NetworkDNSSettings = new NetworkDNSSettings
            {
                Hostname = "test-host",
                MulticastDNSEnabled = true,
                NameServers = new NameServersSettings
                {
                    Enabled = true,
                    Addresses = [IPAddress.Parse("8.8.8.8"), IPAddress.Parse("8.8.4.4")]
                }
            },
            NetworkProxySettings = new NetworkProxySettings
            {
                HTTP = new NetworkProxyDetail { Enabled = false, Server = "http-proxy.example.com", Port = 8080, },
                HTTPS = new NetworkProxyDetail { Enabled = false, Server = "http-proxy.example.com", Port = 8081, },
            },
            NetworkNTPSettings = new NetworkNTPSettings
            {
                Enabled = false,
                Servers = ["pool.ntp.org",],
            },
            Services =
            [
                new ServiceDetail { Name = "apache2", State = ServiceState.Enabled, },
            ],
        };
    }
}
