using AwesomeAssertions;
using Xunit;

namespace HostManagement.Configuration.Tests;

public sealed class ConfigurableServiceNamesNormalizerTests
{
    public sealed class Normalize
    {
        [Fact]
        public void ShouldReturnEmptyCollection_WhenNamesAreEmpty()
        {
            // Arrange
            IReadOnlyCollection<string> names = [];

            // Act
            var result = ConfigurableServiceNamesNormalizer.Normalize(names);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void ShouldAppendServiceSuffix_WhenNameHasNoKnownUnitTypeSuffix()
        {
            // Arrange
            IReadOnlyCollection<string> names = ["example"];

            // Act
            var result = ConfigurableServiceNamesNormalizer.Normalize(names);

            // Assert
            result.Should().Equal("example.service");
        }

        [Theory]
        [InlineData("example.service")]
        [InlineData("example.socket")]
        [InlineData("example.timer")]
        [InlineData("example.mount")]
        [InlineData("example.automount")]
        [InlineData("example.target")]
        [InlineData("example.path")]
        [InlineData("example.slice")]
        [InlineData("example.scope")]
        public void ShouldKeepNameUnchanged_WhenNameHasKnownUnitTypeSuffix(string name)
        {
            // Arrange
            IReadOnlyCollection<string> names = [name];

            // Act
            var result = ConfigurableServiceNamesNormalizer.Normalize(names);

            // Assert
            result.Should().Equal(name);
        }

        [Fact]
        public void ShouldNormalizeAllNames_WhenMultipleNamesAreProvided()
        {
            // Arrange
            IReadOnlyCollection<string> names =
            [
                "first",
                "second.socket",
                "third"
            ];

            // Act
            var result = ConfigurableServiceNamesNormalizer.Normalize(names);

            // Assert
            result.Should().Equal("first.service", "second.socket", "third.service");
        }

        [Fact]
        public void ShouldRemoveDuplicateNames()
        {
            // Arrange
            IReadOnlyCollection<string> names =
            [
                "example",
                "example"
            ];

            // Act
            var result = ConfigurableServiceNamesNormalizer.Normalize(names);

            // Assert
            result.Should().Equal("example.service");
        }

        [Fact]
        public void ShouldRemoveDuplicatesCreatedByNormalization()
        {
            // Arrange
            IReadOnlyCollection<string> names =
            [
                "example",
                "example.service"
            ];

            // Act
            var result = ConfigurableServiceNamesNormalizer.Normalize(names);

            // Assert
            result.Should().Equal("example.service");
        }

        [Fact]
        public void ShouldTreatNamesAsCaseSensitive()
        {
            // Arrange
            IReadOnlyCollection<string> names =
            [
                "example",
                "Example"
            ];

            // Act
            var result = ConfigurableServiceNamesNormalizer.Normalize(names);

            // Assert
            result.Should().Equal("example.service", "Example.service");
        }

        [Fact]
        public void ShouldPreserveFirstOccurrenceOrder_WhenDuplicatesAreRemoved()
        {
            // Arrange
            IReadOnlyCollection<string> names =
            [
                "first",
                "second",
                "first",
                "third",
                "second.service"
            ];

            // Act
            var result = ConfigurableServiceNamesNormalizer.Normalize(names);

            // Assert
            result.Should().Equal("first.service", "second.service", "third.service");
        }
    }
}
