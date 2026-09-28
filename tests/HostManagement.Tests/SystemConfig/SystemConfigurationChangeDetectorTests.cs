using AwesomeAssertions;
using HostManagement.SystemConfig;
using Xunit;

namespace HostManagement.Tests.SystemConfig;

public sealed class SystemConfigurationChangeDetectorTests
{
    public sealed class HasChanges
    {
        [Fact]
        public void ShouldReturnFalse_WhenBothValuesNull()
            => SystemConfigurationChangeDetector.HasChanges<string>(null, null).Should().BeFalse();

        [Fact]
        public void ShouldReturnFalse_WhenValuesEqual()
            => SystemConfigurationChangeDetector.HasChanges("current", "current").Should().BeFalse();

        [Fact]
        public void ShouldReturnTrue_WhenValuesDiffer()
            => SystemConfigurationChangeDetector.HasChanges("current", "proposed").Should().BeTrue();

        [Fact]
        public void ShouldReturnTrue_WhenCurrentValueNull()
            => SystemConfigurationChangeDetector.HasChanges(null, "proposed").Should().BeTrue();

        [Fact]
        public void ShouldReturnTrue_WhenProposedValueNull()
            => SystemConfigurationChangeDetector.HasChanges("current", null).Should().BeTrue();
    }

    public sealed class HasSequenceChanges
    {
        [Fact]
        public void ShouldReturnFalse_WhenSequencesReferenceSameInstance()
        {
            // Arrange
            int[] sequence = [1, 2, 3];

            // Act
            var result = SystemConfigurationChangeDetector.HasSequenceChanges(sequence, sequence);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenSequencesEqual()
        {
            // Arrange
            int[] current = [1, 2, 3];
            int[] proposed = [1, 2, 3];

            // Act
            var result = SystemConfigurationChangeDetector.HasSequenceChanges(current, proposed);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnTrue_WhenCurrentSequenceNull()
        {
            // Arrange
            int[]? current = null;
            int[] proposed = [1, 2, 3];

            // Act
            var result = SystemConfigurationChangeDetector.HasSequenceChanges(current, proposed);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldReturnTrue_WhenProposedSequenceNull()
        {
            // Arrange
            int[] current = [1, 2, 3];
            int[]? proposed = null;

            // Act
            var result = SystemConfigurationChangeDetector.HasSequenceChanges(current, proposed);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldReturnTrue_WhenSequenceContentDiffers()
        {
            // Arrange
            int[] current = [1, 2, 3];
            int[] proposed = [1, 2, 4];

            // Act
            var result = SystemConfigurationChangeDetector.HasSequenceChanges(current, proposed);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldReturnTrue_WhenSequenceOrderDiffers()
        {
            // Arrange
            int[] current = [1, 2, 3];
            int[] proposed = [3, 2, 1];

            // Act
            var result = SystemConfigurationChangeDetector.HasSequenceChanges(current, proposed);

            // Assert
            result.Should().BeTrue();
        }
    }

    public sealed class HasSequenceChangesWithOrder
    {
        [Fact]
        public void ShouldReturnFalse_WhenSequencesReferenceSameInstance()
        {
            // Arrange
            int[] sequence = [1, 2, 3];

            // Act
            var result = SystemConfigurationChangeDetector.HasSequenceChanges(
                sequence,
                sequence,
                value => value);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnTrue_WhenCurrentSequenceNull()
        {
            // Arrange
            int[]? current = null;
            int[] proposed = [1, 2, 3];

            // Act
            var result = SystemConfigurationChangeDetector.HasSequenceChanges(
                current,
                proposed,
                value => value);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldReturnTrue_WhenProposedSequenceNull()
        {
            // Arrange
            int[] current = [1, 2, 3];
            int[]? proposed = null;

            // Act
            var result = SystemConfigurationChangeDetector.HasSequenceChanges(
                current,
                proposed,
                value => value);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldReturnTrue_WhenOrderSelectorNullAndSequenceOrderDiffers()
        {
            // Arrange
            int[] current = [1, 2, 3];
            int[] proposed = [3, 2, 1];

            // Act
            var result = SystemConfigurationChangeDetector.HasSequenceChanges<int, int>(
                current,
                proposed,
                null);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldReturnFalse_WhenSequenceOrderDiffersAndOrderSelectorProvided()
        {
            // Arrange
            int[] current = [1, 2, 3];
            int[] proposed = [3, 2, 1];

            // Act
            var result = SystemConfigurationChangeDetector.HasSequenceChanges(
                current,
                proposed,
                value => value);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnTrue_WhenSequenceContentDiffersAfterOrdering()
        {
            // Arrange
            int[] current = [1, 2, 3];
            int[] proposed = [3, 2, 4];

            // Act
            var result = SystemConfigurationChangeDetector.HasSequenceChanges(
                current,
                proposed,
                value => value);

            // Assert
            result.Should().BeTrue();
        }
    }
}
