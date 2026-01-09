using FluentAssertions;
using Xunit;
using HostManagement.Scripts.ScriptOptionNames;

namespace HostManagement.Scripts.Tests.ScriptOptionNames;

public class SetServiceScriptOptionNameTests
{
    [Fact]
    public void ServiceName_ShouldReturnExpectedValue()
    {
        // Arrange
        var expectedValue = "--name";

        // Act
        var result = SetServiceScriptOptionName.Name.ToString();

        // Assert
        result.Should()
            .NotBeNull().And
            .Be(expectedValue);
    }

    [Fact]
    public void ServiceState_ShouldReturnExpectedValue()
    {
        // Arrange
        var expectedValue = "--state";

        // Act
        var result = SetServiceScriptOptionName.State.ToString();

        // Assert
        result.Should()
            .NotBeNull().And
            .Be(expectedValue);
    }
}
