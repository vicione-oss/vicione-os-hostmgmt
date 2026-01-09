using FluentAssertions;
using Xunit;
using HostManagement.Scripts.ScriptOptionNames;

namespace HostManagement.Scripts.Tests.ScriptOptionNames;

public class InstallViciOneSuiteDebFileScriptOptionNameTests
{
    [Fact]
    public void SuiteDebianFileDownloadDirectory_ShouldReturnExpectedValue()
    {
        // Arrange
        var expectedValue = "--download-path";

        // Act
        var result = InstallViciOneSuiteDebFileScriptOptionName.SuiteDebianFileDownloadDirectory.ToString();

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
        var result = InstallViciOneSuiteDebFileScriptOptionName.SuiteDebianFile.ToString();

        // Assert
        result.Should()
            .NotBeNull().And
            .Be(expectedValue);
    }

    [Fact]
    public void ForceReinstall_ShouldReturnExpectedValue()
    {
        // Arrange
        var expectedValue = "--force-reinstall";

        // Act
        var result = InstallViciOneSuiteDebFileScriptOptionName.ForceReinstall.ToString();

        // Assert
        result.Should()
            .NotBeNull().And
            .Be(expectedValue);
    }
}
