using FluentAssertions;
using Xunit;
using HostManagement.Scripts.ScriptOptionNames;

namespace HostManagement.Scripts.Tests.ScriptOptionNames;

public class InstallViciOneSuiteDebFileScriptOptionNameTests
{
    [Fact]
    public void SuiteDebianFileDownloadDirectory_ShouldReturnExpectedValue()
        => InstallViciOneSuiteDebFileScriptOptionName.SuiteDebianFileDownloadDirectory.ToString().Should().Be("--download-path");

    [Fact]
    public void SuiteDebianFile_ShouldReturnExpectedValue()
        => InstallViciOneSuiteDebFileScriptOptionName.SuiteDebianFile.ToString().Should().Be("--deb-file");

    [Fact]
    public void ForceReinstall_ShouldReturnExpectedValue()
        => InstallViciOneSuiteDebFileScriptOptionName.ForceReinstall.ToString().Should().Be("--force-reinstall");
}
