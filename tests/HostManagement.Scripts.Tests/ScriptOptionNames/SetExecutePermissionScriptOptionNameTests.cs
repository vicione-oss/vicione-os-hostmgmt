using FluentAssertions;
using Xunit;
using HostManagement.Scripts.ScriptOptionNames;

namespace HostManagement.Scripts.Tests.ScriptOptionNames;

public class SetExecutePermissionScriptOptionNameTests
{
    [Fact]
    public void Permission_ShouldReturnExpectedValue()
        => SetExecutePermissionScriptOptionName.Permission.ToString().Should().Be("--permission");

    [Fact]
    public void File_ShouldReturnExpectedValue()
        => SetExecutePermissionScriptOptionName.File.ToString().Should().Be("--file");
}
