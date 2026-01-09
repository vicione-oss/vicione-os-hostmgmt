using FluentAssertions;
using Xunit;

namespace HostManagement.Scripts.Tests;

public class ScriptArgumentFormatterTests
{
    [Fact]
    public void ShouldReturnEmptyString_WhenArgumentsNull()
    {
        // Act
        var result = ScriptArgumentFormatter.GetFormattedArguments(null);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ShouldReturnEmptyString_WhenNoArgumentsGiven()
    {
        // Act
        var result = ScriptArgumentFormatter.GetFormattedArguments();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ShouldEncloseArgumentsWithDoubleQuotes_WhenArgumentsGiven()
    {
        // Arrange
        var argument1 = "arg1";
        var argument2 = "arg2";
        IEnumerable<string> arguments = [argument1, argument2];

        var expectedArgument1 = "\"arg1\"";
        var expectedArgument2 = "\"arg2\"";
        var expectedArgumentsString = expectedArgument1 + " " + expectedArgument2;

        // Act
        var result = ScriptArgumentFormatter.GetFormattedArguments(arguments);

        // Assert
        result.Should().Be(expectedArgumentsString);
    }

    [Fact]
    public void ShouldEscapeDoubleQuotes_WhenPresentInArgument()
    {
        // Arrange
        var argument1 = "arg1";
        var argument2 = "arg2 with \"quotes\"";
        IEnumerable<string> arguments = [argument1, argument2];

        var expectedArgument1 = "\"arg1\"";
        var expectedArgument2 = "\"arg2 with \\\"quotes\\\"\"";
        var expectedArgumentsString = expectedArgument1 + " " + expectedArgument2;

        // Act
        var result = ScriptArgumentFormatter.GetFormattedArguments(arguments);

        // Assert
        result.Should().Be(expectedArgumentsString);
    }
}
