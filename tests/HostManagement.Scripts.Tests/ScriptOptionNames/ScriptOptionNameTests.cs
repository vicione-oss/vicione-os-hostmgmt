using FluentAssertions;
using Xunit;
using HostManagement.Scripts.ScriptOptionNames;

namespace HostManagement.Scripts.Tests.ScriptOptionNames;

public class ScriptOptionNameTests
{
    private sealed class TestScriptOptionName(string name) : ScriptOptionName(name);

    public class ImplicitConversion : ScriptOptionNameTests
    {
        [Fact]
        public void ShouldReturnCorrectString_WhenImplicitOperatorUsed()
            => ((string)new TestScriptOptionName("TestOption")).Should().Be("TestOption");
    }

    public class ToStringMethod : ScriptOptionNameTests
    {
        [Fact]
        public void ShouldReturnCorrectString_WhenToStringCalled()
            => new TestScriptOptionName("TestOption").ToString().Should().Be("TestOption");
    }
}

