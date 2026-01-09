using FluentAssertions;
using Xunit;
using HostManagement.Shared.Validation;
using HostManagement.Shared.Validation.Validators.DNS;

namespace HostManagement.Shared.Tests.Validation.Validators.DNS;

public class DomainLabelValidatorTests
{
    private const string CustomDomainLabelText = "A custom domain label text";

    private static string GetCustomDomainLabelText(string? customDomainLabelText)
        => customDomainLabelText is null ? "Domain label" : customDomainLabelText;

    [Fact]
    public async Task ShouldReturnValidationError_WhenValidationCancelled()
    {
        // Arrange
        var expectedValidationEntry = new ValidationEntry(ValidationConstants.CancelledResult);

        // Act
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var result = await new DomainLabelValidator().ValidateAsync("domain", cts.Token);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(CustomDomainLabelText)]
    public async Task ShouldReturnValidationError_WhenDomainLabelHasInvalidCharacterCount(
        string? customDomainLabelText)
    {
        // Arrange
        var invalidDomainLabel = new string('n', 64);

        var expectedValidationEntry = new ValidationEntry(
            $"{GetCustomDomainLabelText(customDomainLabelText)} '{invalidDomainLabel}' " +
            $"has too many characters (expected max. 63 characters including dots).");

        // Act
        var result = await new DomainLabelValidator(customDomainLabelText).ValidateAsync(invalidDomainLabel);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(CustomDomainLabelText)]
    public async Task ShouldReturnValidationError_WhenDomainLabelHasOnlyDigits(
        string? customDomainLabelText)
    {
        // Arrange
        var invalidDomainLabel = "42";

        var expectedValidationEntry = new ValidationEntry(
            $"{GetCustomDomainLabelText(customDomainLabelText)} '{invalidDomainLabel}' " +
            $"can not have only digits.");

        // Act
        var result = await new DomainLabelValidator(customDomainLabelText).ValidateAsync(invalidDomainLabel);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(CustomDomainLabelText)]
    public async Task ShouldReturnValidationError_WhenDomainLabelHasInvalidCharacter(
        string? customDomainLabelText)
    {
        // Arrange
        var invalidDomainLabel = "-domainLabel";

        var expectedValidationEntry = new ValidationEntry(
            $"{GetCustomDomainLabelText(customDomainLabelText)} '{invalidDomainLabel}' " +
            $"has an invalid character " +
            $"(expected start/end character to be letter or digit; other characters can also be a hyphen).");

        // Act
        var result = await new DomainLabelValidator(customDomainLabelText).ValidateAsync(invalidDomainLabel);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(CustomDomainLabelText)]
    public async Task ShouldReturnValid_WhenDomainLabelValid(string? customDomainLabelText)
    {
        // Act
        var result = await new DomainLabelValidator(customDomainLabelText).ValidateAsync("domain");

        // Assert
        result.IsValid.Should().BeTrue();
        result.Entries.Count.Should().Be(0);
    }
}
