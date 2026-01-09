using FluentAssertions;
using Xunit;
using HostManagement.Scripts.ScriptOptionNames;

namespace HostManagement.Scripts.Tests.ScriptOptionNames;

public class SetSystemdUnitStateScriptOptionNameTests
{
    [Fact]
    public void Name_ShouldReturnExpectedValue()
        => SetSystemdUnitStateScriptOptionName.Name.ToString().Should().Be("--name");

    [Fact]
    public void State_ShouldReturnExpectedValue()
        => SetSystemdUnitStateScriptOptionName.State.ToString().Should().Be("--state");
}
