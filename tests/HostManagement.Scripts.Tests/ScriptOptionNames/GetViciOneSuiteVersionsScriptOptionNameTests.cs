using FluentAssertions;
using Xunit;
using HostManagement.Scripts.ScriptOptionNames;

namespace HostManagement.Scripts.Tests.ScriptOptionNames;

public class GetViciOneSuiteVersionsScriptOptionNameTests
{
    [Fact]
    public void SuiteRepositoryEndpoint_ShouldReturnExpectedValue()
    {
        // Arrange
        var expectedValue = "--url";

        // Act
        var result = GetViciOneSuiteVersionsScriptOptionName.SuiteRepositoryEndpoint.ToString();

        // Assert
        result.Should()
            .NotBeNull().And
            .Be(expectedValue);
    }

    [Fact]
    public void SuiteRepositoryUsername_ShouldReturnExpectedValue()
    {
        // Arrange
        var expectedValue = "--username";

        // Act
        var result = GetViciOneSuiteVersionsScriptOptionName.SuiteRepositoryUsername.ToString();

        // Assert
        result.Should()
            .NotBeNull().And
            .Be(expectedValue);
    }

    [Fact]
    public void SuiteRepositoryPassword_ShouldReturnExpectedValue()
    {
        // Arrange
        var expectedValue = "--password";

        // Act
        var result = GetViciOneSuiteVersionsScriptOptionName.SuiteRepositoryPassword.ToString();

        // Assert
        result.Should()
            .NotBeNull().And
            .Be(expectedValue);
    }
}
