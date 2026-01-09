using FluentAssertions;
using Xunit;
using HostManagement.Scripts.ScriptOptionNames;

namespace HostManagement.Scripts.Tests.ScriptOptionNames;

public class NetworkScriptOptionNameTests
{
    [Fact]
    public void InterfaceName_ShouldReturnExpectedValue()
    {
        // Arrange
        var expectedValue = "--interface-name";

        // Act
        var result = NetworkScriptOptionName.InterfaceName.ToString();

        // Assert
        result.Should()
            .NotBeNull().And
            .Be(expectedValue);
    }

    [Fact]
    public void InterfaceEnabled_ShouldReturnExpectedValue()
    {
        // Arrange
        var expectedValue = "--enabled";

        // Act
        var result = NetworkScriptOptionName.InterfaceEnabled.ToString();

        // Assert
        result.Should()
            .NotBeNull().And
            .Be(expectedValue);
    }

    [Fact]
    public void InterfaceDHCPEnabled_ShouldReturnExpectedValue()
    {
        // Arrange
        var expectedValue = "--dhcp-enabled";

        // Act
        var result = NetworkScriptOptionName.InterfaceDHCPEnabled.ToString();

        // Assert
        result.Should()
            .NotBeNull().And
            .Be(expectedValue);
    }

    [Fact]
    public void InterfaceIPAddress_ShouldReturnExpectedValue()
    {
        // Arrange
        var expectedValue = "--ip-address";

        // Act
        var result = NetworkScriptOptionName.InterfaceIPAddress.ToString();

        // Assert
        result.Should()
            .NotBeNull().And
            .Be(expectedValue);
    }

    [Fact]
    public void InterfaceGateway_ShouldReturnExpectedValue()
    {
        // Arrange
        var expectedValue = "--gateway";

        // Act
        var result = NetworkScriptOptionName.InterfaceGateway.ToString();

        // Assert
        result.Should()
            .NotBeNull().And
            .Be(expectedValue);
    }
}
