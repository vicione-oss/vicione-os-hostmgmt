using System.Net;
using FluentAssertions;
using HostManagement.Scripts.ScriptOptionNames;
using Xunit;

namespace HostManagement.Tests;

public class ScriptOptionsTests
{
    public static TheoryData<ScriptOptionName, string> ScriptOptionNamesData => new()
    {
        { GetNetworkStatusInformationOptionName.InterfaceName, "--interface-name" },
        { GetServiceScriptOptionName.Name, "--name" },
        { GetViciOneSuiteDebFileScriptOptionName.SuiteRepositoryEndpoint, "--url" },
        { InstallViciOneSuiteDebFileScriptOptionName.SuiteDebianFile, "--deb-file" },
        { NetworkDNSScriptOptionName.SearchDomains, "--search-domains" }
    };

    public static TheoryData<string[], IPAddress[]> IPAddressData => new()
    {
        { [ "192.168.178.42" ], [ IPAddress.Parse("192.168.178.42") ] },
        { [ "192.168.178.42", "192.168.178.21" ], [ IPAddress.Parse("192.168.178.42"), IPAddress.Parse("192.168.178.21") ] }
    };

    [Theory]
    [MemberData(nameof(ScriptOptionNamesData))]
    public void ShouldAddName_WhenNameGiven(ScriptOptionName name, string expectedName)
    {
        // Arrange
        var scriptOptions = new ScriptOptions();

        // Act
        scriptOptions.Add(name);

        // Assert
        scriptOptions.Options.Should().Contain(expectedName);
    }

    [Fact]
    public void ShouldThrow_WhenStringArgumentsNull()
    {
        // Arrange
        var name = ProxyScriptOptionName.URL;
        string[]? arguments = null;
        var scriptOptions = new ScriptOptions();

        // Act
        var result = () => scriptOptions.Add(name, arguments!);

        // Assert
        result.Should()
            .Throw<ArgumentNullException>()
            .WithParameterName("arguments");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("proxy.com", null, "proxy.net")]
    public void ShouldThrow_WhenArgumentInStringArgumentsNullOrWhiteSpace(params string?[] arguments)
    {
        // Arrange
        var name = ProxyScriptOptionName.URL;
        var scriptOptions = new ScriptOptions();

        // Act
        var result = () => scriptOptions.Add(name, arguments!);

        // Assert
        result.Should()
            .Throw<ArgumentException>()
            .WithParameterName("arguments")
            .WithMessage($"*{name}*can not be null, empty or whitespace*");
    }

    [Theory]
    [InlineData("--")]
    [InlineData("--name")]
    [InlineData("--enabled")]
    [InlineData("--name", "--enabled")]
    public void ShouldThrow_WhenArgumentInStringArgumentsIsName(params string[] arguments)
    {
        // Arrange
        var name = ProxyScriptOptionName.URL;
        var scriptOptions = new ScriptOptions();

        // Act
        var result = () => scriptOptions.Add(name, arguments!);

        // Assert
        result.Should()
            .Throw<ArgumentException>()
            .WithParameterName("arguments")
            .WithMessage($"*{name}*can not start with '--'*");
    }

    [Theory]
    [InlineData(new string[] { "proxy.com" }, "proxy.com")]
    [InlineData(new string[] { "proxy.com", "proxy.net" }, "proxy.com", "proxy.net")]
    public void ShouldAddNameAndArgument_WhenNameAndStringArgumentsGiven(
        string[] expectedArgument,
        params string[] arguments)
    {
        // Arrange
        var name = ProxyScriptOptionName.URL;
        var scriptOptions = new ScriptOptions();

        var expectedName = "--url";

        // Act
        scriptOptions.Add(name, arguments);

        // Assert
        scriptOptions.Options.Should().Contain(expectedName).And.Contain(expectedArgument);
    }

    [Fact]
    public void ShouldThrow_WhenBooleanArgumentsNull()
    {
        // Arrange
        var name = NetworkScriptOptionName.InterfaceEnabled;
        bool[]? arguments = null;
        var scriptOptions = new ScriptOptions();

        // Act
        var result = () => scriptOptions.Add(name, arguments!);

        // Assert
        result.Should()
            .Throw<ArgumentNullException>()
            .WithParameterName("arguments");
    }

    [Theory]
    [InlineData(new string[] { "true" }, true)]
    [InlineData(new string[] { "false" }, false)]
    [InlineData(new string[] { "true", "false", "true" }, true, false, true)]
    public void ShouldAddNameAndArguments_WhenNameAndBooleanArgumentsGiven(
        string[] expectedArguments,
        params bool[] arguments)
    {
        // Arrange
        var name = NetworkScriptOptionName.InterfaceEnabled;
        var scriptOptions = new ScriptOptions();

        var expectedName = "--enabled";

        // Act
        scriptOptions.Add(name, arguments);

        // Assert
        scriptOptions.Options.Should().Contain(expectedName).And.Contain(expectedArguments);
    }

    [Fact]
    public void ShouldThrow_WhenIntegerArgumentsNull()
    {
        // Arrange
        var name = ProxyScriptOptionName.Port;
        int[]? arguments = null;
        var scriptOptions = new ScriptOptions();

        // Act
        var result = () => scriptOptions.Add(name, arguments!);

        // Assert
        result.Should()
            .Throw<ArgumentNullException>()
            .WithParameterName("arguments");
    }

    [Theory]
    [InlineData(new string[] { "42" }, 42)]
    [InlineData(new string[] { "1", "2", "4", "8" }, 1, 2, 4, 8)]
    public void ShouldAddNameAndArguments_WhenNameAndIntegerArgumentsGiven(
        string[] expectedArguments,
        params int[] arguments)
    {
        // Arrange
        var name = ProxyScriptOptionName.Port;
        var scriptOptions = new ScriptOptions();

        var expectedName = "--port";

        // Act
        scriptOptions.Add(name, arguments);

        // Assert
        scriptOptions.Options.Should().Contain(expectedName).And.Contain(expectedArguments);
    }

    [Fact]
    public void ShouldThrow_WhenIPArgumentsNull()
    {
        // Arrange
        var name = NetworkScriptOptionName.InterfaceIPAddress;
        IPAddress[]? arguments = null;
        var scriptOptions = new ScriptOptions();

        // Act
        var result = () => scriptOptions.Add(name, arguments!);

        // Assert
        result.Should()
            .Throw<ArgumentNullException>()
            .WithParameterName("arguments");
    }

    [Theory]
    [MemberData(nameof(IPAddressData))]
    public void ShouldAddNameAndArguments_WhenNameAndIPArgumentsGiven(
        string[] expectedArguments,
        params IPAddress[] arguments)
    {
        // Arrange
        var name = NetworkScriptOptionName.InterfaceIPAddress;
        var scriptOptions = new ScriptOptions();

        var expectedName = "--ip-address";

        // Act
        scriptOptions.Add(name, arguments);

        // Assert
        scriptOptions.Options.Should().Contain(expectedName).And.Contain(expectedArguments);
    }

    // TODO nameof Vorschlag bei Testfällen deaktivieren
}
