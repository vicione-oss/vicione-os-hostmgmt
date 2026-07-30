using System.Net;
using AwesomeAssertions;
using HostManagement.SystemConfig.Contracts;
using Xunit;

namespace HostManagement.Tests.SystemConfig.Contracts;

public sealed class NameServersSettingsTests
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
            var settings1 = new NameServersSettings();
            var settings2 = new NameServersSettings();

            // Act
            var result = settings1.Equals(settings2);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldReturnFalse_WhenNullVsNonNullEnabled()
        {
            // Arrange
            var settings1 = new NameServersSettings { Enabled = null };
            var settings2 = new NameServersSettings { Enabled = true };

            // Act
            var result = settings1.Equals(settings2);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDifferentAddresses()
        {
            // Arrange
            var settings1 = CreateSampleSettings();
            var settings2 = CreateSampleSettings();
            settings2.Addresses!.Add(IPAddress.Parse("1.1.1.1"));

            // Act
            var result = settings1.Equals(settings2);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenNullVsNonNullAddresses()
        {
            // Arrange
            var settings1 = new NameServersSettings { Addresses = null };
            var settings2 = new NameServersSettings { Addresses = [IPAddress.Parse("8.8.8.8")] };

            // Act
            var result = settings1.Equals(settings2);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDifferentAddressesOrder()
        {
            // Arrange
            var settings1 = new NameServersSettings
            {
                Addresses = [IPAddress.Parse("8.8.8.8"), IPAddress.Parse("8.8.4.4")]
            };
            var settings2 = new NameServersSettings
            {
                Addresses = [IPAddress.Parse("8.8.4.4"), IPAddress.Parse("8.8.8.8")]
            };

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
            settings2.Enabled = !settings1.Enabled;

            var hash1 = settings1.GetHashCode();
            var hash2 = settings2.GetHashCode();

            hash1.Should().NotBe(hash2);
        }
    }

    private static NameServersSettings CreateSampleSettings()
    {
        return new NameServersSettings
        {
            Enabled = true,
            Addresses = [IPAddress.Parse("8.8.8.8"), IPAddress.Parse("8.8.4.4")]
        };
    }
}
