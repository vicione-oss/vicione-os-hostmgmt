using FluentAssertions;
using Xunit;
using HostManagement.Scripts.ScriptOptionNames;

namespace HostManagement.Scripts.Tests.ScriptOptionNames;

public class SetOwnershipScriptOptionNameTests
{
    [Fact]
    public void User_ShouldReturnExpectedValue()
        => SetOwnershipScriptOptionName.User.ToString().Should().Be("--user");

    [Fact]
    public void Group_ShouldReturnExpectedValue()
        => SetOwnershipScriptOptionName.Group.ToString().Should().Be("--group");

    [Fact]
    public void File_ShouldReturnExpectedValue()
        => SetOwnershipScriptOptionName.File.ToString().Should().Be("--file");
}
