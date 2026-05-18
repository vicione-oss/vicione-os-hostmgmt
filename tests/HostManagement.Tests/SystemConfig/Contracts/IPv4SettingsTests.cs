using System.Net;
using AwesomeAssertions;
using HostManagement.SystemConfig.Contracts;
using Xunit;

namespace HostManagement.Tests.SystemConfig.Contracts;

public sealed class IPv4SettingsTests
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
        public void ShouldReturnTrue_WhenEmptySettings()
        {
            var settings1 = new IPv4Settings();
            var settings2 = new IPv4Settings();

            var result = settings1.Equals(settings2);

            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDifferentGateway()
        {
            var settings1 = CreateSampleSettings();
            var settings2 = CreateSampleSettings();
            settings2.Gateway = IPAddress.Parse("10.0.0.1");

            var result = settings1.Equals(settings2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenNullVsNonNullGateway()
        {
            var settings1 = new IPv4Settings { Gateway = null };
            var settings2 = new IPv4Settings { Gateway = IPAddress.Parse("192.168.1.1") };

            var result = settings1.Equals(settings2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDifferentDHCPEnabled()
        {
            var settings1 = CreateSampleSettings();
            var settings2 = CreateSampleSettings();
            settings2.DHCPEnabled = !settings1.DHCPEnabled;

            var result = settings1.Equals(settings2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenNullVsNonNullDHCPEnabled()
        {
            var settings1 = new IPv4Settings { DHCPEnabled = null };
            var settings2 = new IPv4Settings { DHCPEnabled = true };

            var result = settings1.Equals(settings2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDifferentIPv4Details()
        {
            var settings1 = CreateSampleSettings();
            var settings2 = CreateSampleSettings();
            settings2.IPv4Details!.Add(new IPv4Detail
            {
                IPAddress = IPAddress.Parse("192.168.1.200"),
                Netmask = IPAddress.Parse("255.255.255.0")
            });

            var result = settings1.Equals(settings2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenNullVsNonNullIPv4Details()
        {
            var settings1 = new IPv4Settings { IPv4Details = null };
            var settings2 = new IPv4Settings
            {
                IPv4Details =
                [
                    new IPv4Detail
                    {
                        IPAddress = IPAddress.Parse("192.168.1.100"),
                        Netmask = IPAddress.Parse("255.255.255.0")
                    }
                ]
            };

            var result = settings1.Equals(settings2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDifferentIPv4DetailOrder()
        {
            var detail1 = new IPv4Detail { IPAddress = IPAddress.Parse("192.168.1.100"), Netmask = IPAddress.Parse("255.255.255.0") };
            var detail2 = new IPv4Detail { IPAddress = IPAddress.Parse("192.168.1.101"), Netmask = IPAddress.Parse("255.255.255.0") };

            var settings1 = new IPv4Settings { IPv4Details = [detail1, detail2] };
            var settings2 = new IPv4Settings { IPv4Details = [detail2, detail1] };

            var result = settings1.Equals(settings2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDifferentVLANEnabled()
        {
            var settings1 = CreateSampleSettings();
            var settings2 = CreateSampleSettings();
            settings2.VLANEnabled = !settings1.VLANEnabled;

            var result = settings1.Equals(settings2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenNullVsNonNullVLANEnabled()
        {
            var settings1 = new IPv4Settings { VLANEnabled = null };
            var settings2 = new IPv4Settings { VLANEnabled = true };

            var result = settings1.Equals(settings2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDifferentVLANID()
        {
            var settings1 = CreateSampleSettings();
            var settings2 = CreateSampleSettings();
            settings2.VLANID = 200;

            var result = settings1.Equals(settings2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenNullVsNonNullVLANID()
        {
            var settings1 = new IPv4Settings { VLANID = null };
            var settings2 = new IPv4Settings { VLANID = 100 };

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
        public void ShouldReturnFalse_WhenNestedIPv4DetailDifferences()
        {
            var settings1 = CreateSampleSettings();
            var settings2 = CreateSampleSettings();
            settings2.IPv4Details![0].IPAddress = IPAddress.Parse("192.168.1.200");

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
        public void ShouldReturnDifferentHashCode_WhenDifferentSettings()
        {
            var settings1 = CreateSampleSettings();
            var settings2 = CreateSampleSettings();
            settings2.Gateway = IPAddress.Parse("10.0.0.1");

            var hash1 = settings1.GetHashCode();
            var hash2 = settings2.GetHashCode();

            hash1.Should().NotBe(hash2);
        }
    }

    private static IPv4Settings CreateSampleSettings()
    {
        return new IPv4Settings
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
            ],
            VLANEnabled = true,
            VLANID = 100
        };
    }
}
