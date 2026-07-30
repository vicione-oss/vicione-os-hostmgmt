using System.Net;
using AwesomeAssertions;
using HostManagement.SystemConfig.Contracts;
using Xunit;

namespace HostManagement.Tests.SystemConfig.Contracts;

public sealed class StaticHostsSettingsTests
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
            var settings1 = new StaticHostsSettings();
            var settings2 = new StaticHostsSettings();

            // Act
            var result = settings1.Equals(settings2);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldReturnFalse_WhenNullVsNonNullEnabled()
        {
            // Arrange
            var settings1 = new StaticHostsSettings { Enabled = null };
            var settings2 = new StaticHostsSettings { Enabled = true };

            // Act
            var result = settings1.Equals(settings2);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDifferentHosts()
        {
            // Arrange
            var settings1 = CreateSampleSettings();
            var settings2 = CreateSampleSettings();

            settings2.Hosts!.Add(new StaticHostDetail
            {
                IPAddress = IPAddress.Parse("192.168.1.200"),
                Hostname = "extra-host"
            });

            // Act
            var result = settings1.Equals(settings2);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenNullVsNonNullHosts()
        {
            // Arrange
            var settings1 = new StaticHostsSettings { Hosts = null };
            var settings2 = new StaticHostsSettings
            {
                Hosts =
                [
                    new StaticHostDetail
                    {
                        IPAddress = IPAddress.Parse("192.168.1.100"),
                        Hostname = "host1"
                    }
                ]
            };

            // Act
            var result = settings1.Equals(settings2);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDifferentHostsOrder()
        {
            // Arrange
            var host1 = new StaticHostDetail { IPAddress = IPAddress.Parse("192.168.1.100"), Hostname = "host1" };
            var host2 = new StaticHostDetail { IPAddress = IPAddress.Parse("192.168.1.101"), Hostname = "host2" };

            var settings1 = new StaticHostsSettings { Hosts = [host1, host2] };
            var settings2 = new StaticHostsSettings { Hosts = [host2, host1] };

            // Act
            var result = settings1.Equals(settings2);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenNullVsNonNull()
        {
            // Arrange
            var settings = CreateSampleSettings();

            // Act
            var result = settings.Equals(null);

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
            settings2.Enabled = !settings1.Enabled;

            // Act
            var hash1 = settings1.GetHashCode();
            var hash2 = settings2.GetHashCode();

            // Assert
            hash1.Should().NotBe(hash2);
        }
    }

    private static StaticHostsSettings CreateSampleSettings()
    {
        return new StaticHostsSettings
        {
            Enabled = true,
            Hosts =
            [
                new StaticHostDetail
                {
                    IPAddress = IPAddress.Parse("192.168.1.100"),
                    Hostname = "static-host1"
                }
            ]
        };
    }
}
