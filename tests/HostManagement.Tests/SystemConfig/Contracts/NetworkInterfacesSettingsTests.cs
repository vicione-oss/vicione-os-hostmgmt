using System.Net;
using AwesomeAssertions;
using HostManagement.SystemConfig.Contracts;
using Xunit;

namespace HostManagement.Tests.SystemConfig.Contracts;

public class NetworkInterfacesSettingsTests
{
    public sealed class EqualsMethod
    {
        [Fact]
        public void ShouldReturnTrue_WhenIdenticalSettings()
        {
            var settings1 = CreateSampleSettings();
            var settings2 = CreateSampleSettings();

            var result = settings1.Equals(settings2);

            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldReturnTrue_WhenEmptyLists()
        {
            var settings1 = new NetworkInterfacesSettings { NetworkInterfaces = [] };
            var settings2 = new NetworkInterfacesSettings { NetworkInterfaces = [] };

            var result = settings1.Equals(settings2);

            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldReturnTrue_WhenBothNull()
        {
            var settings1 = new NetworkInterfacesSettings { NetworkInterfaces = null };
            var settings2 = new NetworkInterfacesSettings { NetworkInterfaces = null };

            var result = settings1.Equals(settings2);

            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldReturnFalse_WhenNullVsNonNullNetworkInterfaces()
        {
            var settings1 = new NetworkInterfacesSettings { NetworkInterfaces = null };
            var settings2 = new NetworkInterfacesSettings
            {
                NetworkInterfaces =
                [
                    new NetworkInterfaceDetail
                    {
                        CommonInformation = new NetworkInterfaceCommonInformation { Enabled = true, Name = "eth0" },
                        IPv4 = new IPv4Settings(),
                        VLAN = new VLANSettings()
                    }
                ]
            };

            var result = settings1.Equals(settings2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDifferentNumberOfInterfaces()
        {
            var settings1 = CreateSampleSettings();
            var settings2 = CreateSampleSettings();
            settings2.NetworkInterfaces!.Add(new NetworkInterfaceDetail
            {
                CommonInformation = new NetworkInterfaceCommonInformation { Enabled = true, Name = "eth1" },
                IPv4 = new IPv4Settings(),
                VLAN = new VLANSettings()
            });

            var result = settings1.Equals(settings2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDifferentInterfaceOrder()
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

            var settings1 = new NetworkInterfacesSettings
            {
                NetworkInterfaces = [interface1, interface2]
            };
            var settings2 = new NetworkInterfacesSettings
            {
                NetworkInterfaces = [interface2, interface1]
            };

            var result = settings1.Equals(settings2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDifferentInterfaceProperties()
        {
            var settings1 = CreateSampleSettings();
            var settings2 = CreateSampleSettings();
            settings2.NetworkInterfaces![0].CommonInformation!.Name = "different-name";

            var result = settings1.Equals(settings2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenNull()
        {
            var settings = CreateSampleSettings();

            var result = settings.Equals(null);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenNestedIPv4Differences()
        {
            var settings1 = CreateSampleSettings();
            var settings2 = CreateSampleSettings();
            settings2.NetworkInterfaces![0].IPv4!.Gateway = IPAddress.Parse("10.0.0.1");

            var result = settings1.Equals(settings2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDifferentVLANEnabled()
        {
            var settings1 = CreateSampleSettings();
            var settings2 = CreateSampleSettings();
            settings2.NetworkInterfaces![0].VLAN!.Enabled = !settings1.NetworkInterfaces![0].VLAN!.Enabled;

            var result = settings1.Equals(settings2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenNullVsNonNullVLANEnabled()
        {
            var settings1 = CreateSampleSettings();
            var settings2 = CreateSampleSettings();
            settings2.NetworkInterfaces![0].VLAN!.Enabled = null;

            var result = settings1.Equals(settings2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDifferentVLANID()
        {
            var settings1 = CreateSampleSettings();
            var settings2 = CreateSampleSettings();
            settings2.NetworkInterfaces![0].VLAN!.ID = 200;

            var result = settings1.Equals(settings2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenNullVsNonNullVLANID()
        {
            var settings1 = CreateSampleSettings();
            var settings2 = CreateSampleSettings();
            settings2.NetworkInterfaces![0].VLAN!.ID = null;

            var result = settings1.Equals(settings2);

            result.Should().BeFalse();
        }
    }

    public sealed class GetHashCodeMethod
    {
        [Fact]
        public void ShouldReturnSameHashCode_WhenIdenticalSettings()
        {
            var settings1 = CreateSampleSettings();
            var settings2 = CreateSampleSettings();

            var hash1 = settings1.GetHashCode();
            var hash2 = settings2.GetHashCode();

            hash1.Should().Be(hash2);
        }

        [Fact]
        public void ShouldBeConsistentAcrossMultipleCalls()
        {
            var settings = CreateSampleSettings();

            var hash1 = settings.GetHashCode();
            var hash2 = settings.GetHashCode();
            var hash3 = settings.GetHashCode();

            hash1.Should().Be(hash2).And.Be(hash3);
        }
    }

    private static NetworkInterfacesSettings CreateSampleSettings()
    {
        return new NetworkInterfacesSettings
        {
            NetworkInterfaces =
            [
                new NetworkInterfaceDetail
                {
                    CommonInformation = new NetworkInterfaceCommonInformation
                    {
                        Enabled = true,
                        Name = "eth0",
                        PhysicalAddress = "00:11:22:33:44:55"
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
                                Netmask = IPAddress.Parse("255.255.255.0")
                            }
                        ]
                    },
                    VLAN = new VLANSettings
                    {
                        Enabled = true,
                        ID = 100
                    }
                }
            ]
        };
    }
}
