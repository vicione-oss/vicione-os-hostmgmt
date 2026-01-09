using FluentAssertions;
using Xunit;
using HostManagement.Scripts.ScriptOptionNames;

namespace HostManagement.Scripts.Tests.ScriptOptionNames;

public class GetViciOneSuiteDebFileScriptOptionNameTests
{
    [Fact]
    public void SuiteRepositoryEndpoint_ShouldReturnExpectedValue()
        => GetViciOneSuiteDebFileScriptOptionName.SuiteRepositoryEndpoint.ToString().Should().Be("--url");

    [Fact]
    public void SuiteDebianFile_ShouldReturnExpectedValue()
        => GetViciOneSuiteDebFileScriptOptionName.SuiteDebianFile.ToString().Should().Be("--deb-file");

    [Fact]
    public void SuiteRepositoryUsername_ShouldReturnExpectedValue()
        => GetViciOneSuiteDebFileScriptOptionName.SuiteRepositoryUsername.ToString().Should().Be("--username");

    [Fact]
    public void SuiteRepositoryPassword_ShouldReturnExpectedValue()
        => GetViciOneSuiteDebFileScriptOptionName.SuiteRepositoryPassword.ToString().Should().Be("--password");

    [Fact]
    public void SuiteDebianFileDownloadDirectory_ShouldReturnExpectedValue()
        => GetViciOneSuiteDebFileScriptOptionName.SuiteDebianFileDownloadDirectory.ToString().Should().Be("--download-path");
}
