using FluentAssertions;
using Xunit;
using HostManagement.Scripts.ScriptOptionNames;

namespace HostManagement.Scripts.Tests.ScriptOptionNames;

public class CheckGroupExistsScriptOptionNameTests
{
    [Fact]
    public void Group_ShouldReturnExpectedValue()
        => CheckGroupExistsScriptOptionName.Group.ToString().Should().Be("--group");
}
