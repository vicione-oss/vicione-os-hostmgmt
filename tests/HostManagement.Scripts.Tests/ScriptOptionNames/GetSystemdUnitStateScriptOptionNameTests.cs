using FluentAssertions;
using Xunit;
using HostManagement.Scripts.ScriptOptionNames;

namespace HostManagement.Scripts.Tests.ScriptOptionNames;

public class GetSystemdUnitStateScriptOptionNameTests
{
    [Fact]
    public void Name_ShouldReturnExpectedValue()
        => GetSystemdUnitStateScriptOptionName.Name.ToString().Should().Be("--name");
}
