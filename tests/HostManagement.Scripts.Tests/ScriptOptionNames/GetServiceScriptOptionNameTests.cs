using FluentAssertions;
using Xunit;
using HostManagement.Scripts.ScriptOptionNames;

namespace HostManagement.Scripts.Tests.ScriptOptionNames;

public class GetServiceScriptOptionNameTests
{
    [Fact]
    public void ServiceName_ShouldReturnExpectedValue()
    {
        // Arrange
        var expectedValue = "--name";

        // Act
        var result = GetServiceScriptOptionName.Name.ToString();

        // Assert
        result.Should()
            .NotBeNull().And
            .Be(expectedValue);
    }
}
