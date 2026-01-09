using FluentAssertions;
using Xunit;
using HostManagement.Scripts.ScriptOptionNames;

namespace HostManagement.Scripts.Tests.ScriptOptionNames;

public class ScriptOptionNameTests
{
    private class TestScriptOptionName(string name) : ScriptOptionName(name);

    public class ImplicitConversion : ScriptOptionNameTests
    {
        [Fact]
        public void ShouldReturnCorrectString_WhenImplicitOperatorUsed()
        {
            // Arrange
            var expectedName = "TestOption";

            // Act
            string result = new TestScriptOptionName(expectedName);

            // Assert
            result.Should().Be(expectedName);
        }
    }

    public class ToStringMethod : ScriptOptionNameTests
    {
        [Fact]
        public void ShouldReturnCorrectString_WhenToStringCalled()
        {
            // Arrange
            var expectedName = "TestOption";

            // Act
            var result = new TestScriptOptionName(expectedName).ToString();

            // Assert
            result.Should().Be(expectedName);
        }
    }
}

