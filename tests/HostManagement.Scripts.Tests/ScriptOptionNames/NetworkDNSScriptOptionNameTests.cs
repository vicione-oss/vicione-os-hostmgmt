using FluentAssertions;
using Xunit;
using HostManagement.Scripts.ScriptOptionNames;

namespace HostManagement.Scripts.Tests.ScriptOptionNames;

public class NetworkDNSScriptOptionNameTests
{
    [Fact]
    public void Hostname_ShouldReturnExpectedValue()
    {
        // Arrange
        var expectedValue = "--hostname";

        // Act
        var result = NetworkDNSScriptOptionName.Hostname.ToString();

        // Assert
        result.Should()
            .NotBeNull().And
            .Be(expectedValue);
    }

    [Fact]
    public void NameServers_ShouldReturnExpectedValue()
    {
        // Arrange
        var expectedValue = "--nameservers";

        // Act
        var result = NetworkDNSScriptOptionName.NameServers.ToString();

        // Assert
        result.Should()
            .NotBeNull().And
            .Be(expectedValue);
    }

    [Fact]
    public void DNSSuffix_ShouldReturnExpectedValue()
    {
        // Arrange
        var expectedValue = "--dns-suffix";

        // Act
        var result = NetworkDNSScriptOptionName.DNSSuffix.ToString();

        // Assert
        result.Should()
            .NotBeNull().And
            .Be(expectedValue);
    }

    [Fact]
    public void SearchDomains_ShouldReturnExpectedValue()
    {
        // Arrange
        var expectedValue = "--search-domains";

        // Act
        var result = NetworkDNSScriptOptionName.SearchDomains.ToString();

        // Assert
        result.Should()
            .NotBeNull().And
            .Be(expectedValue);
    }
}
