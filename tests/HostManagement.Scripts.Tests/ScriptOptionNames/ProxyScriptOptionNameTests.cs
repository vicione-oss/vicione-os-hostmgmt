using FluentAssertions;
using Xunit;
using HostManagement.Scripts.ScriptOptionNames;

namespace HostManagement.Scripts.Tests.ScriptOptionNames;

public class ProxyScriptOptionNameTests
{
    [Fact]
    public void Enabled_ShouldReturnExpectedValue()
    {
        // Arrange
        var expectedValue = "--enabled";

        // Act
        var result = ProxyScriptOptionName.Enabled.ToString();

        // Assert
        result.Should()
            .NotBeNull().And
            .Be(expectedValue);
    }

    [Fact]
    public void URL_ShouldReturnExpectedValue()
    {
        // Arrange
        var expectedValue = "--url";

        // Act
        var result = ProxyScriptOptionName.URL.ToString();

        // Assert
        result.Should()
            .NotBeNull().And
            .Be(expectedValue);
    }

    [Fact]
    public void Port_ShouldReturnExpectedValue()
    {
        // Arrange
        var expectedValue = "--port";

        // Act
        var result = ProxyScriptOptionName.Port.ToString();

        // Assert
        result.Should()
            .NotBeNull().And
            .Be(expectedValue);
    }

    [Fact]
    public void Username_ShouldReturnExpectedValue()
    {
        // Arrange
        var expectedValue = "--username";

        // Act
        var result = ProxyScriptOptionName.Username.ToString();

        // Assert
        result.Should()
            .NotBeNull().And
            .Be(expectedValue);
    }

    [Fact]
    public void Password_ShouldReturnExpectedValue()
    {
        // Arrange
        var expectedValue = "--password";

        // Act
        var result = ProxyScriptOptionName.Password.ToString();

        // Assert
        result.Should()
            .NotBeNull().And
            .Be(expectedValue);
    }
}
