using System.Net;
using AwesomeAssertions;
using HostManagement.SystemConfig.Contracts;
using Xunit;

namespace HostManagement.Tests.SystemConfig.Contracts;

public sealed class NetworkDNSSettingsTests
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
            var settings1 = new NetworkDNSSettings();
            var settings2 = new NetworkDNSSettings();

            // Act
            var result = settings1.Equals(settings2);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDifferentHostname()
        {
            // Arrange
            var settings1 = CreateSampleSettings();
            var settings2 = CreateSampleSettings();
            settings2.Hostname = "different-hostname";

            // Act
            var result = settings1.Equals(settings2);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenNullVsNonNullHostname()
        {
            // Arrange
            var settings1 = new NetworkDNSSettings { Hostname = null };
            var settings2 = new NetworkDNSSettings { Hostname = "test-host" };

            // Act
            var result = settings1.Equals(settings2);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDifferentMulticastDNSEnabled()
        {
            // Arrange
            var settings1 = CreateSampleSettings();
            var settings2 = CreateSampleSettings();
            settings2.MulticastDNSEnabled = !settings1.MulticastDNSEnabled;

            // Act
            var result = settings1.Equals(settings2);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenNullVsNonNullMulticastDNSEnabled()
        {
            // Arrange
            var settings1 = new NetworkDNSSettings { MulticastDNSEnabled = null };
            var settings2 = new NetworkDNSSettings { MulticastDNSEnabled = true };

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

        [Fact]
        public void ShouldReturnFalse_WhenComplexNestedDifferences()
        {
            // Arrange
            var settings1 = CreateSampleSettings();
            var settings2 = CreateSampleSettings();
            settings2.StaticHosts!.Hosts![0].Hostname = "modified-hostname";

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
            settings2.Hostname = "different-hostname";

            // Act
            var hash1 = settings1.GetHashCode();
            var hash2 = settings2.GetHashCode();

            // Assert
            hash1.Should().NotBe(hash2);
        }
    }

    private static NetworkDNSSettings CreateSampleSettings()
    {
        return new NetworkDNSSettings
        {
            Hostname = "test-host",
            MulticastDNSEnabled = true,
            StaticHosts = new StaticHostsSettings
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
            },
            NameServers = new NameServersSettings
            {
                Enabled = true,
                Addresses = [IPAddress.Parse("8.8.8.8"), IPAddress.Parse("8.8.4.4")]
            },
            PrimaryDNSSuffix = new PrimaryDNSSuffixSettings
            {
                Enabled = true,
                Suffix = "example.local"
            },
            SearchDomains = new SearchDomainsSettings()
            {
                Enabled = true,
                Domains = ["search1.local", "search2.local"]
            }
        };
    }
}
