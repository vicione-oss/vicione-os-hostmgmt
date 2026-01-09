using FluentAssertions;
using Xunit;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Validation;
using HostManagement.Shared.Validation.Validators.Proxy;

namespace HostManagement.Shared.Tests.Validation.Validators.Proxy;

public class ProxyValidatorTests
{
    [Fact]
    public async Task ShouldReturnValidationError_WhenValidationCancelled()
    {
        // Arrange
        var expectedValidationEntry = new ValidationEntry(ValidationConstants.CancelledResult);

        // Act
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var result = await new ProxyValidator()
            .ValidateAsync(ValidationTestData.Proxy.HTTP, ProxyType.HTTP, cts.Token);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldReturnValid_WhenProxyUnconfigured()
    {
        // Act
        var result = await new ProxyValidator().ValidateAsync(NetworkProxyDetail.Unconfigured);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Entries.Count.Should().Be(0);
    }

    [Fact]
    public async Task ShouldReturnValidationError_WhenServerNull()
    {
        // Arrange
        var invalidProxyDetail = ValidationTestData.Proxy.HTTP with
        {
            Server = null
        };

        var expectedValidationEntry = new ValidationEntry("Proxy server can not be null.");

        // Act
        var result = await new ProxyValidator().ValidateAsync(invalidProxyDetail);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldAddValidationResult_WhenProxyServerValidatorCalled()
    {
        // Arrange
        var invalidProxyDetail = ValidationTestData.Proxy.HTTP with { Server = string.Empty };
        var validProxyDetail = ValidationTestData.Proxy.HTTP;

        // Act
        var invalidResult = await new ProxyValidator().ValidateAsync(invalidProxyDetail);
        var validResult = await new ProxyValidator().ValidateAsync(validProxyDetail);

        // Assert
        invalidResult.IsValid.Should().BeFalse();
        invalidResult.Entries.Count.Should().Be(1);
        invalidResult.EntriesString.Should().NotContain(ValidationConstants.CancelledResult);

        validResult.IsValid.Should().BeTrue();
        validResult.Entries.Count.Should().Be(0);
    }

    [Fact]
    public async Task ShouldReturnValidationError_WhenPortNull()
    {
        // Arrange
        var invalidProxyDetail = ValidationTestData.Proxy.HTTP with { Port = null };
        var expectedValidationEntry = new ValidationEntry("Proxy port can not be null.");

        // Act
        var result = await new ProxyValidator().ValidateAsync(invalidProxyDetail);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldAddValidationResult_WhenProxyPortValidatorCalled()
    {
        // Arrange
        var invalidProxyDetail = ValidationTestData.Proxy.HTTP with
        {
            Server = string.Empty,
            Port = 42
        };
        var validProxyDetail = ValidationTestData.Proxy.HTTP with
        {
            Port = 42
        };

        // Act
        var invalidResult = await new ProxyValidator().ValidateAsync(invalidProxyDetail);
        var validResult = await new ProxyValidator().ValidateAsync(validProxyDetail);

        // Assert
        invalidResult.IsValid.Should().BeFalse();
        invalidResult.Entries.Count.Should().Be(1);
        invalidResult.EntriesString.Should().NotContain(ValidationConstants.CancelledResult);

        validResult.IsValid.Should().BeTrue();
        validResult.Entries.Count.Should().Be(0);
    }

    [Fact]
    public async Task ShouldReturnValidationResult_WhenNoAuthenticationGiven()
    {
        // Arrange
        var invalidProxyDetail = ValidationTestData.Proxy.HTTP with
        {
            Server = string.Empty,
            Username = null,
            Password = null
        };

        var validProxyDetail = ValidationTestData.Proxy.HTTP with
        {
            Username = null,
            Password = null
        };

        // Act
        var invalidResult = await new ProxyValidator().ValidateAsync(invalidProxyDetail);
        var validResult = await new ProxyValidator().ValidateAsync(validProxyDetail);

        // Assert
        invalidResult.IsValid.Should().BeFalse();
        invalidResult.Entries.Count.Should().Be(1);
        invalidResult.EntriesString.Should().NotContain(ValidationConstants.CancelledResult);

        validResult.IsValid.Should().BeTrue();
        validResult.Entries.Count.Should().Be(0);
    }

    [Fact]
    public async Task ShouldReturnValidationError_WhenUsernameNull()
    {
        // Arrange
        var invalidProxyDetail = ValidationTestData.Proxy.HTTP with
        {
            Username = null,
            Password = "Please"
        };

        var expectedValidationEntry = new ValidationEntry(
            "Proxy username can not be null.");

        // Act
        var result = await new ProxyValidator().ValidateAsync(invalidProxyDetail);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldReturnValidationError_WhenPasswordNull()
    {
        // Arrange
        var invalidProxyDetail = ValidationTestData.Proxy.HTTP with
        {
            Username = "User",
            Password = null
        };

        var expectedValidationEntry = new ValidationEntry(
            "Proxy password can not be null.");

        // Act
        var result = await new ProxyValidator().ValidateAsync(invalidProxyDetail);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData("User\0x7F", "\0x7FPass", false)] // invalid control characters in username/password
    [InlineData("User", "Password", true)]
    public async Task ShouldAddValidationResult_WhenProxyAuthenticationValidatorCalled(
        string username,
        string password,
        bool isValid)
    {
        // Act
        var proxyDetail = ValidationTestData.Proxy.HTTP with { Username = username, Password = password };
        var result = await new ProxyValidator().ValidateAsync(proxyDetail);

        // Assert
        result.IsValid.Should().Be(isValid);
        result.EntriesString.Should().NotContain(ValidationConstants.CancelledResult);
    }

    [Fact]
    public async Task ShouldReturnValid_WhenProxyValid()
    {
        // Act
        var result = await new ProxyValidator().ValidateAsync(ValidationTestData.Proxy.HTTP, ProxyType.HTTP);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Entries.Count.Should().Be(0);
    }
}
