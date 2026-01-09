using AwesomeAssertions;
using Xunit;

namespace HostManagement.Scripts.Tests;

public class ScriptResponseTests
{
    public class HasErrors
    {
        [Fact]
        public void ShouldReturnFalse_WhenExitCodeZero()
        {
            // Arrange
            var scriptResponse = new ScriptResponse(0, "StdOut", "");

            // Act
            var result = scriptResponse.HasErrors;

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData(1, "", "StdErr")]
        [InlineData(42, "", "StdErr")]
        [InlineData(125, "", "StdErr")]
        public void ShouldReturnTrue_WhenExitCodeGreaterThanZero(
            int exitCode,
            string standardOutput,
            string standardError)
        {
            // Arrange
            var scriptResponse = new ScriptResponse(exitCode, standardOutput, standardError);

            // Act
            var result = scriptResponse.HasErrors;

            // Assert
            result.Should().BeTrue();
        }
    }

    public class HasWarnings
    {
        [Theory]
        [InlineData(0, "", "")]
        [InlineData(1, "", "")]
        [InlineData(1, "", "StdErr")]
        public void ShouldReturnFalse_WhenExitCodeNotZeroAndOrNoStandardErrorPresent(
            int exitCode,
            string standardOutput,
            string standardError)
        {
            // Arrange
            var scriptResponse = new ScriptResponse(exitCode, standardOutput, standardError);

            // Act
            var result = scriptResponse.HasWarnings;

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData(0, "", "StdErr")]
        [InlineData(0, "", "Another error message")]
        public void ShouldReturnTrue_WhenExitCodeZeroAndStandardErrorPresent(
            int exitCode,
            string standardOutput,
            string standardError)
        {
            // Arrange
            var scriptResponse = new ScriptResponse(exitCode, standardOutput, standardError);

            // Act
            var result = scriptResponse.HasWarnings;

            // Assert
            result.Should().BeTrue();
        }
    }
}
