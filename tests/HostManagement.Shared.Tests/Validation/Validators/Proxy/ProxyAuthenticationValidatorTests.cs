using AwesomeAssertions;
using Xunit;
using HostManagement.Shared.Validation;
using HostManagement.Shared.Validation.Validators.Proxy;

namespace HostManagement.Shared.Tests.Validation.Validators.Proxy;

public class ProxyAuthenticationValidatorTests
{
    [Fact]
    public async Task ShouldReturnValidationError_WhenValidationCancelled()
    {
        // Arrange
        var expectedValidationEntry = new ValidationEntry(ValidationConstants.CancelledResult);

        // Act
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var result = await new ProxyAuthenticationValidator().ValidateAsync("user", "pass", cts.Token);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ShouldAddValidationError_WhenUsernameEmpty(string username)
    {
        // Arrange
        var expectedValidationEntry = new ValidationEntry("Proxy username can not be empty or whitespace.");

        // Act
        var result = await new ProxyAuthenticationValidator().ValidateAsync(username, "pass");

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ShouldAddValidationError_WhenPasswordEmpty(string password)
    {
        // Arrange
        var expectedValidationEntry = new ValidationEntry("Proxy password can not be empty or whitespace.");

        // Act
        var result = await new ProxyAuthenticationValidator().ValidateAsync("user", password);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldAddValidationError_WhenUsernameAndPasswordEmpty()
    {
        // Arrange
        var username = string.Empty;
        var password = "  ";

        var expectedValidationEntries = new List<ValidationEntry>()
        {
            new("Proxy username can not be empty or whitespace."),
            new("Proxy password can not be empty or whitespace.")
        };

        // Act
        var result = await new ProxyAuthenticationValidator().ValidateAsync(username, password);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(2);
        result.Entries.Should().Contain(expectedValidationEntries);
    }

    [Fact]
    public async Task ShouldAddValidationError_WhenUsernameHasInvalidCharacterCount()
    {
        // Arrange
        var invalidUsername = new string('n', 256);
        var expectedValidationEntry = new ValidationEntry(
            "Proxy username has an invalid character count (expected max. 255 characters).");

        // Act
        var result = await new ProxyAuthenticationValidator().ValidateAsync(invalidUsername, "pass");

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData(0x00)]
    [InlineData(0x1F)]
    [InlineData(0x7F)]
    public async Task ShouldAddValidationError_WhenUsernameContainsControlCharacter(char controlCharacter)
    {
        // Arrange
        var invalidUsername = "user" + controlCharacter;
        var expectedValidationEntry = new ValidationEntry(
            "Proxy username can not contain control characters.");

        // Act
        var result = await new ProxyAuthenticationValidator().ValidateAsync(invalidUsername, "pass");

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldAddValidationError_WhenPasswordHasInvalidCharacterCount()
    {
        // Arrange
        var invalidPassword = new string('n', 256);
        var expectedValidationEntry = new ValidationEntry(
            "Proxy password has an invalid character count (expected max. 255 characters).");

        // Act
        var result = await new ProxyAuthenticationValidator().ValidateAsync("user", invalidPassword);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData(0x00)]
    [InlineData(0x1F)]
    [InlineData(0x7F)]
    public async Task ShouldAddValidationError_WhenPasswordContainsControlCharacter(char controlCharacter)
    {
        // Arrange
        var invalidPassword = "pass" + controlCharacter;
        var expectedValidationEntry = new ValidationEntry(
            "Proxy password can not contain control characters.");

        // Act
        var result = await new ProxyAuthenticationValidator().ValidateAsync("user", invalidPassword);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData("1", "1")]
    [InlineData("admin", "admin")]
    [InlineData("USER", "123")]
    [InlineData("User#123?", "Pa$$w0rd!!!")]
    public async Task ShouldReturnValid_WhenAuthenticationValid(
        string username,
        string password)
    {
        // Act
        var result = await new ProxyAuthenticationValidator().ValidateAsync(username, password);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Entries.Count.Should().Be(0);
    }
}
