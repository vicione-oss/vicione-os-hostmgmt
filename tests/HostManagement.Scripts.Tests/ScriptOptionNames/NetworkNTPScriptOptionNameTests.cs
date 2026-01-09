using FluentAssertions;
using Xunit;
using HostManagement.Scripts.ScriptOptionNames;

namespace HostManagement.Scripts.Tests.ScriptOptionNames;

public class NetworkNTPScriptOptionNameTests
{
    [Fact]
    public void NTPServers_ShouldReturnExpectedValue()
    {
        // Arrange
        var expectedValue = "--ntpservers";

        // Act
        var result = NetworkNTPScriptOptionName.NTPServers.ToString();

        // Assert
        result.Should()
            .NotBeNull().And
            .Be(expectedValue);
    }
}
