using AwesomeAssertions;
using HostManagement.SystemConfig.Contracts;
using Xunit;

namespace HostManagement.Tests.SystemConfig.Contracts;

public sealed class VLANSettingsTests
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
            var settings1 = new VLANSettings();
            var settings2 = new VLANSettings();

            // Act
            var result = settings1.Equals(settings2);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDifferentEnabled()
        {
            // Arrange
            var settings1 = CreateSampleSettings();
            var settings2 = CreateSampleSettings();
            settings2.Enabled = !settings1.Enabled;

            // Act
            var result = settings1.Equals(settings2);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenNullVsNonNullEnabled()
        {
            // Arrange
            var settings1 = new VLANSettings { Enabled = null };
            var settings2 = new VLANSettings { Enabled = true };

            // Act
            var result = settings1.Equals(settings2);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDifferentID()
        {
            // Arrange
            var settings1 = CreateSampleSettings();
            var settings2 = CreateSampleSettings();
            settings2.ID = 200;

            // Act
            var result = settings1.Equals(settings2);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenNullVsNonNullID()
        {
            // Arrange
            var settings1 = new VLANSettings { ID = null };
            var settings2 = new VLANSettings { ID = 100 };

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

    private static VLANSettings CreateSampleSettings()
    {
        return new VLANSettings
        {
            Enabled = true,
            ID = 42
        };
    }
}
