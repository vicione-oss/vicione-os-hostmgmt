using FluentAssertions;
using HostManagement.Common.Wrappers;
using Xunit;

namespace HostManagement.Common.Tests.Wrappers;

public class StringWrapperTests
{
    public class ConcatenateStrings
    {
        [Theory]
        [InlineData("ConcatenatedString", "Concatenated", "String")]
        [InlineData("AConcatenatedString", "A", "Concatenated", "String")]
        [InlineData("A long concatenated string", "A ", "long ", "concatenated ", "string")]
        public void ShouldConcatenateStrings_WhenValuesProvided(
            string expectedString,
            params string[] values)
        {
            // Arrange
            var sut = new StringWrapper();

            // Act
            var result = sut.ConcatenateStrings(values);

            // Assert
            result.Should().Be(expectedString);
        }
    }
}
