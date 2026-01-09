using System.Net;
using AwesomeAssertions;
using Xunit;
using HostManagement.Scripts;

namespace HostManagement.Tests;

public class ScriptOptionsTests
{
    private const string ArgumentsParameterName = "arguments";

    public static TheoryData<string, string> ScriptOptionNamesData => new()
    {
        { ScriptOptionNames.GetNetworkStatusInformation.InterfaceName, "--interface-name" },
        { ScriptOptionNames.GetSystemdUnitState.Name, "--name" },
        { ScriptOptionNames.GetViciOneSuiteDebFile.SuiteRepositoryEndpoint, "--url" },
        { ScriptOptionNames.InstallViciOneSuiteDebFile.SuiteDebianFile, "--deb-file" },
        { ScriptOptionNames.NetworkDNS.SearchDomains, "--search-domains" }
    };

    public static TheoryData<string[], IPAddress[]> IPAddressData => new()
    {
        { [ "192.168.178.42" ], [ IPAddress.Parse("192.168.178.42") ] },
        { [ "192.168.178.42", "192.168.178.21" ], [ IPAddress.Parse("192.168.178.42"), IPAddress.Parse("192.168.178.21") ] }
    };

    [Theory]
    [MemberData(nameof(ScriptOptionNamesData))]
    public void ShouldAddName_WhenNameGiven(string name, string expectedName)
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
        var name = ScriptOptionNames.Proxy.URL;
        string[]? arguments = null;
        var scriptOptions = new ScriptOptions();

        // Act
        var result = () => scriptOptions.Add(name, arguments!);

        // Assert
        result.Should()
            .Throw<ArgumentNullException>()
            .WithParameterName(ArgumentsParameterName);
    }

    [Fact]
    public void ShouldThrow_WhenArgumentInStringArgumentsNull()
    {
        // Arrange
        var name = ScriptOptionNames.Proxy.URL;
        var scriptOptions = new ScriptOptions();

        // Act
        var result = () => scriptOptions.Add(name, ["proxy.com", null!, "proxy.net"]);

        // Assert
        result.Should()
            .Throw<ArgumentException>()
            .WithParameterName(ArgumentsParameterName)
            .WithMessage($"*{name}*can not be null*");
    }

    [Theory]
    [InlineData("--")]
    [InlineData("--name")]
    [InlineData("--enabled")]
    [InlineData("--name", "--enabled")]
    public void ShouldThrow_WhenArgumentInStringArgumentsIsName(params string[] arguments)
    {
        // Arrange
        var name = ScriptOptionNames.Proxy.URL;
        var scriptOptions = new ScriptOptions();

        // Act
        var result = () => scriptOptions.Add(name, arguments!);

        // Assert
        result.Should()
            .Throw<ArgumentException>()
            .WithParameterName(ArgumentsParameterName)
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
        var name = ScriptOptionNames.Proxy.URL;
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
        var name = ScriptOptionNames.Network.Enabled;
        bool[]? arguments = null;
        var scriptOptions = new ScriptOptions();

        // Act
        var result = () => scriptOptions.Add(name, arguments!);

        // Assert
        result.Should()
            .Throw<ArgumentNullException>()
            .WithParameterName(ArgumentsParameterName);
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
        var name = ScriptOptionNames.Network.Enabled;
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
        var name = ScriptOptionNames.Proxy.Port;
        int[]? arguments = null;
        var scriptOptions = new ScriptOptions();

        // Act
        var result = () => scriptOptions.Add(name, arguments!);

        // Assert
        result.Should()
            .Throw<ArgumentNullException>()
            .WithParameterName(ArgumentsParameterName);
    }

    [Theory]
    [InlineData(new string[] { "42" }, 42)]
    [InlineData(new string[] { "1", "2", "4", "8" }, 1, 2, 4, 8)]
    public void ShouldAddNameAndArguments_WhenNameAndIntegerArgumentsGiven(
        string[] expectedArguments,
        params int[] arguments)
    {
        // Arrange
        var name = ScriptOptionNames.Proxy.Port;
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
        var name = ScriptOptionNames.Network.IPAddress;
        IPAddress[]? arguments = null;
        var scriptOptions = new ScriptOptions();

        // Act
        var result = () => scriptOptions.Add(name, arguments!);

        // Assert
        result.Should()
            .Throw<ArgumentNullException>()
            .WithParameterName(ArgumentsParameterName);
    }

    [Theory]
    [MemberData(nameof(IPAddressData))]
    public void ShouldAddNameAndArguments_WhenNameAndIPArgumentsGiven(
        string[] expectedArguments,
        params IPAddress[] arguments)
    {
        // Arrange
        var name = ScriptOptionNames.Network.IPAddress;
        var scriptOptions = new ScriptOptions();

        var expectedName = "--ip-address";

        // Act
        scriptOptions.Add(name, arguments);

        // Assert
        scriptOptions.Options.Should().Contain(expectedName).And.Contain(expectedArguments);
    }

    // TODO nameof Vorschlag bei Testfällen deaktivieren
}
