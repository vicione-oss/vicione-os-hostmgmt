using FluentAssertions;
using Xunit;
using HostManagement.Scripts.ScriptOptionNames;

namespace HostManagement.Scripts.Tests.ScriptOptionNames;

public class GetViciOneSuiteVersionsScriptOptionNameTests
{
    [Fact]
    public void SuiteRepositoryEndpoint_ShouldReturnExpectedValue()
        => GetViciOneSuiteVersionsScriptOptionName.SuiteRepositoryEndpoint.ToString().Should().Be("--url");

    [Fact]
    public void SuiteRepositoryUsername_ShouldReturnExpectedValue()
        => GetViciOneSuiteVersionsScriptOptionName.SuiteRepositoryUsername.ToString().Should().Be("--username");

    [Fact]
    public void SuiteRepositoryPassword_ShouldReturnExpectedValue()
        => GetViciOneSuiteVersionsScriptOptionName.SuiteRepositoryPassword.ToString().Should().Be("--password");
}
