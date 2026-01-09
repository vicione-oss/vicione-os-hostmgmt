using AwesomeAssertions;
using Xunit;
using HostManagement.Shared.Validation;
using HostManagement.Shared.Validation.Validators.Proxy;

namespace HostManagement.Shared.Tests.Validation.Validators.Proxy;

public class ProxyServerValidatorTests
{
    [Fact]
    public async Task ShouldReturnValidationError_WhenValidationCancelled()
    {
        // Arrange
        var expectedValidationEntry = new ValidationEntry(ValidationConstants.CancelledResult);

        // Act
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var result = await new ProxyServerValidator()
            .ValidateAsync(ValidationTestData.Proxy.HTTP.Server!, ProxyType.HTTP, cts.Token);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public async Task ShouldReturnValidationError_WhenServerEmpty(string invalidServer)
    {
        // Arrange
        var expectedValidationEntry = new ValidationEntry("Proxy server can not be empty or whitespace.");

        // Act
        var result = await new ProxyServerValidator().ValidateAsync(invalidServer);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldReturnValidationError_WhenServerURLContainsPort()
    {
        // Arrange
        var expectedValidationEntry = new ValidationEntry("Proxy server is invalid. Format: <protocol>://<url>.");

        // Act
        var result = await new ProxyServerValidator().ValidateAsync("http://test.de:8080", ProxyType.HTTP);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData("test.de", ProxyType.Unspecified)]
    [InlineData("http:test.de", ProxyType.HTTP)]
    [InlineData("http:/test.de", ProxyType.HTTP)]
    [InlineData("http//test.de", ProxyType.HTTP)]
    [InlineData("http/test.de", ProxyType.HTTP)]
    public async Task ShouldReturnValidationError_WhenServerURLInvalid(
        string invalidServer,
        ProxyType proxyType)
    {
        // Arrange
        var expectedValidationEntry = new ValidationEntry("Proxy server is invalid. Format: <protocol>://<url>.");

        // Act
        var result = await new ProxyServerValidator().ValidateAsync(invalidServer, proxyType);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldReturnValidationError_WhenTransportProtocolNotSpecified()
    {
        // Arrange
        var expectedValidationEntry = new ValidationEntry("No proxy server transport protocol specified.");

        // Act
        var result = await new ProxyServerValidator().ValidateAsync("://test.de", ProxyType.HTTP);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData("https://test.de", ProxyType.HTTP)]
    [InlineData("ftp://test.de", ProxyType.HTTPS)]
    [InlineData("sftp://test.de", ProxyType.FTP)]
    [InlineData("socks://test.de", ProxyType.SFTP)]
    [InlineData("socks5://test.de", ProxyType.SFTP)]
    [InlineData("http://test.de", ProxyType.SOCKS)]
    [InlineData("http://test.de", ProxyType.SOCKS5)]
    [InlineData("htpp://test.de", ProxyType.HTTP)]
    [InlineData("htts://test.de", ProxyType.HTTPS)]
    [InlineData("fpt://test.de", ProxyType.FTP)]
    [InlineData("sfpt://test.de", ProxyType.SFTP)]
    [InlineData("scoks://test.de", ProxyType.SOCKS)]
    [InlineData("sock5://test.de", ProxyType.SOCKS5)]
    public async Task ShouldReturnValidationError_WhenTransportProtocolSpecifiedButInvalid(
        string server,
        ProxyType proxyType)
    {
        // Arrange
        var transportProtocol = server.Split(':')[0];

        var expectedValidationEntry = new ValidationEntry(
            "Proxy server transport protocol is invalid. " +
            $"Expected '{proxyType.ToString().ToLowerInvariant()}' but got '{transportProtocol}'.");

        // Act
        var result = await new ProxyServerValidator().ValidateAsync(server, proxyType);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldReturnValidationError_WhenTransportProtocolNotSpecifiedAndInvalid()
    {
        // Arrange
        var invalidProtocol = "htts";
        var invalidServer = $"{invalidProtocol}://test.de";

        var expectedTransportProtocols = string.Join(
            ", ",
            ["http", "https", "ftp", "sftp", "socks", "socks5"]);

        var expectedValidationEntry = new ValidationEntry(
            "Proxy server transport protocol is invalid. " +
            $"Expected '{expectedTransportProtocols}' but got '{invalidProtocol}'.");

        // Act
        var result = await new ProxyServerValidator().ValidateAsync(invalidServer);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData("http:///test.de")]
    [InlineData(@"https://test.de\about")]
    public async Task ShouldReturnValidationError_WhenServerUriNotWellFormed(string invalidServer)
    {
        // Arrange
        var expectedValidationEntry = new ValidationEntry(
            $"Proxy server '{invalidServer}' has a not well formed URI.");

        // Act
        var result = await new ProxyServerValidator().ValidateAsync(invalidServer);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData("https://test1.de")]
    [InlineData("ftp://test2.de")]
    [InlineData("sftp://test3.de")]
    [InlineData("socks://test4.de")]
    [InlineData("socks5://test5.de")]
    public async Task ShouldReturnValid_WhenServerValid(string server)
    {
        // Act
        var result = await new ProxyServerValidator().ValidateAsync(server);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Entries.Count.Should().Be(0);
    }
}
