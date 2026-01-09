using FluentAssertions;
using Xunit;
using HostManagement.Scripts.ScriptOptionNames;

namespace HostManagement.Scripts.Tests.ScriptOptionNames;

public class UpdateSystemScriptOptionNameTests
{
    [Fact]
    public void FilePath_ShouldReturnExpectedValue()
        => UpdateSystemScriptOptionName.FilePath.ToString().Should().Be("--file");
}
