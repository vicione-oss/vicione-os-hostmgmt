using AwesomeAssertions;
using HostManagement.SystemConfig.Contracts;
using Xunit;

namespace HostManagement.Tests.SystemConfig.Contracts;

public class NetworkNTPSettingsTests
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
            var settings1 = new NetworkNTPSettings();
            var settings2 = new NetworkNTPSettings();

            var result = settings1.Equals(settings2);

            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDifferentNTPServersEnabled()
        {
            var settings1 = CreateSampleSettings();
            var settings2 = CreateSampleSettings();
            settings2.NTPServersEnabled = !settings1.NTPServersEnabled;

            var result = settings1.Equals(settings2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenNullVsNonNullNTPServersEnabled()
        {
            var settings1 = new NetworkNTPSettings { NTPServersEnabled = null };
            var settings2 = new NetworkNTPSettings { NTPServersEnabled = true };

            var result = settings1.Equals(settings2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDifferentNTPServers()
        {
            var settings1 = CreateSampleSettings();
            var settings2 = CreateSampleSettings();
            settings2.NTPServers!.Add("extra-ntp.example.com");

            var result = settings1.Equals(settings2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenNullVsNonNullNTPServers()
        {
            var settings1 = new NetworkNTPSettings { NTPServers = null };
            var settings2 = new NetworkNTPSettings { NTPServers = ["ntp.example.com"] };

            var result = settings1.Equals(settings2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDifferentNTPServerOrder()
        {
            var settings1 = new NetworkNTPSettings
            {
                NTPServers = ["ntp1.example.com", "ntp2.example.com"]
            };
            var settings2 = new NetworkNTPSettings
            {
                NTPServers = ["ntp2.example.com", "ntp1.example.com"]
            };

            var result = settings1.Equals(settings2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDifferentFallbackNTPServers()
        {
            var settings1 = CreateSampleSettings();
            var settings2 = CreateSampleSettings();
            settings2.FallbackNTPServers = ["different-fallback.example.com"];

            var result = settings1.Equals(settings2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenNullVsEmptyFallbackNTPServers()
        {
            var settings1 = new NetworkNTPSettings { FallbackNTPServers = null };
            var settings2 = new NetworkNTPSettings { FallbackNTPServers = [] };

            var result = settings1.Equals(settings2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnTrue_WhenBothNullFallbackNTPServers()
        {
            var settings1 = new NetworkNTPSettings { FallbackNTPServers = null };
            var settings2 = new NetworkNTPSettings { FallbackNTPServers = null };

            var result = settings1.Equals(settings2);

            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldReturnFalse_WhenNullVsNonNullFallbackNTPServers()
        {
            var settings1 = new NetworkNTPSettings { FallbackNTPServers = null };
            var settings2 = new NetworkNTPSettings { FallbackNTPServers = ["fallback.example.com"] };

            var result = settings1.Equals(settings2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDifferentFallbackNTPServerOrder()
        {
            var settings1 = new NetworkNTPSettings
            {
                FallbackNTPServers = ["fallback1.example.com", "fallback2.example.com"]
            };
            var settings2 = new NetworkNTPSettings
            {
                FallbackNTPServers = ["fallback2.example.com", "fallback1.example.com"]
            };

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
        public void ShouldNotThrow_WhenNullFallbackServers()
        {
            var settings = new NetworkNTPSettings { FallbackNTPServers = null };

            var exception = Record.Exception(() => settings.GetHashCode());

            exception.Should().BeNull();
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
            settings2.NTPServersEnabled = !settings1.NTPServersEnabled;

            var hash1 = settings1.GetHashCode();
            var hash2 = settings2.GetHashCode();

            hash1.Should().NotBe(hash2);
        }
    }

    private static NetworkNTPSettings CreateSampleSettings()
    {
        return new NetworkNTPSettings
        {
            FallbackNTPServers = ["fallback1.ntp.org", "fallback2.ntp.org"],
            NTPServersEnabled = true,
            NTPServers = ["ntp1.example.com", "ntp2.example.com"]
        };
    }
}
