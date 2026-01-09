using FluentAssertions;
using Xunit;
using HostManagement.Scripts.ScriptOptionNames;

namespace HostManagement.Scripts.Tests.ScriptOptionNames;

public class GetViciOneSuiteDebFileScriptOptionNameTests
{
    [Fact]
    public void SuiteRepositoryEndpoint_ShouldReturnExpectedValue()
    {
        // Arrange
        var expectedValue = "--url";

        // Act
        var result = GetViciOneSuiteDebFileScriptOptionName.SuiteRepositoryEndpoint.ToString();

        // Assert
        result.Should()
            .NotBeNull().And
            .Be(expectedValue);
    }

    [Fact]
    public void SuiteDebianFile_ShouldReturnExpectedValue()
    {
        // Arrange
        var expectedValue = "--deb-file";

        // Act
        var result = GetViciOneSuiteDebFileScriptOptionName.SuiteDebianFile.ToString();

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
        var result = GetViciOneSuiteDebFileScriptOptionName.SuiteRepositoryUsername.ToString();

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
        var result = GetViciOneSuiteDebFileScriptOptionName.SuiteRepositoryPassword.ToString();

        // Assert
        result.Should()
            .NotBeNull().And
            .Be(expectedValue);
    }

    [Fact]
    public void SuiteDebianFileDownloadDirectory_ShouldReturnExpectedValue()
    {
        // Arrange
        var expectedValue = "--download-path";

        // Act
        var result = GetViciOneSuiteDebFileScriptOptionName.SuiteDebianFileDownloadDirectory.ToString();

        // Assert
        result.Should()
            .NotBeNull().And
            .Be(expectedValue);
    }
}
