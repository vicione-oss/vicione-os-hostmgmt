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
            // Arrange
            var settings1 = CreateSampleSettings();
            var settings2 = CreateSampleSettings();

            // Act
            var result = settings1.Equals(settings2);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldReturnTrue_WhenEmptySettings()
        {
            // Arrange
            var settings1 = new IPv4Settings();
            var settings2 = new IPv4Settings();

            // Act
            var result = settings1.Equals(settings2);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDifferentGateway()
        {
            // Arrange
            var settings1 = CreateSampleSettings();
            var settings2 = CreateSampleSettings();
            settings2.Gateway = IPAddress.Parse("10.0.0.1");

            // Act
            var result = settings1.Equals(settings2);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenNullVsNonNullGateway()
        {
            // Arrange
            var settings1 = new IPv4Settings { Gateway = null };
            var settings2 = new IPv4Settings { Gateway = IPAddress.Parse("192.168.1.1") };

            // Act
            var result = settings1.Equals(settings2);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDifferentDHCPEnabled()
        {
            // Arrange
            var settings1 = CreateSampleSettings();
            var settings2 = CreateSampleSettings();
            settings2.DHCPEnabled = !settings1.DHCPEnabled;

            // Act
            var result = settings1.Equals(settings2);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenNullVsNonNullDHCPEnabled()
        {
            // Arrange
            var settings1 = new IPv4Settings { DHCPEnabled = null };
            var settings2 = new IPv4Settings { DHCPEnabled = true };

            // Act
            var result = settings1.Equals(settings2);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDifferentIPv4Details()
        {
            // Arrange
            var settings1 = CreateSampleSettings();
            var settings2 = CreateSampleSettings();
            settings2.IPv4Details!.Add(new IPv4Detail
            {
                IPAddress = IPAddress.Parse("192.168.1.200"),
                Netmask = IPAddress.Parse("255.255.255.0")
            });

            // Act
            var result = settings1.Equals(settings2);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenNullVsNonNullIPv4Details()
        {
            // Arrange
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

            // Act
            var result = settings1.Equals(settings2);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDifferentIPv4DetailOrder()
        {
            // Arrange
            var detail1 = new IPv4Detail { IPAddress = IPAddress.Parse("192.168.1.100"), Netmask = IPAddress.Parse("255.255.255.0") };
            var detail2 = new IPv4Detail { IPAddress = IPAddress.Parse("192.168.1.101"), Netmask = IPAddress.Parse("255.255.255.0") };

            var settings1 = new IPv4Settings { IPv4Details = [detail1, detail2] };
            var settings2 = new IPv4Settings { IPv4Details = [detail2, detail1] };

            // Act
            var result = settings1.Equals(settings2);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenNull()
        {
            // Arrange
            var settings = CreateSampleSettings();

            // Act
            var result = settings.Equals(null);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenNestedIPv4DetailDifferences()
        {
            // Arrange
            var settings1 = CreateSampleSettings();
            var settings2 = CreateSampleSettings();
            settings2.IPv4Details![0].IPAddress = IPAddress.Parse("192.168.1.200");

            // Act
            var result = settings1.Equals(settings2);

            // Assert
            result.Should().BeFalse();
        }
    }

    public sealed class GetHashCodeMethod
    {
        [Fact]
        public void ShouldReturnSameHashCode_WhenIdenticalSettings()
        {
            // Arrange
            var settings1 = CreateSampleSettings();
            var settings2 = CreateSampleSettings();

            // Act
            var hash1 = settings1.GetHashCode();
            var hash2 = settings2.GetHashCode();

            // Assert
            hash1.Should().Be(hash2);
        }

        [Fact]
        public void ShouldReturnDifferentHashCode_WhenDifferentSettings()
        {
            // Arrange
            var settings1 = CreateSampleSettings();
            var settings2 = CreateSampleSettings();
            settings2.Gateway = IPAddress.Parse("10.0.0.1");

            // Act
            var hash1 = settings1.GetHashCode();
            var hash2 = settings2.GetHashCode();

            // Assert
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
            ]
        };
    }
}
