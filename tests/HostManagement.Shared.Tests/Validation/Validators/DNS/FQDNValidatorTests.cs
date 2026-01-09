using AwesomeAssertions;
using Xunit;
using HostManagement.Shared.Validation;
using HostManagement.Shared.Validation.Validators.DNS;

namespace HostManagement.Shared.Tests.Validation.Validators.DNS;

public class FQDNValidatorTests
{
    private const string CustomFQDNText = "A custom FQDN text";

    private static string GetCustomFQDNText(string? customFQDNText)
        => customFQDNText is null ? "FQDN" : customFQDNText;

    [Fact]
    public async Task ShouldReturnValidationError_WhenValidationCancelled()
    {
        // Arrange
        var expectedValidationEntry = new ValidationEntry(ValidationConstants.CancelledResult);

        // Act
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var result = await new FQDNValidator().ValidateAsync("time.google.com", cts.Token);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(CustomFQDNText)]
    public async Task ShouldReturnValidationError_WhenFQDNHasTooManyCharacters(string? customFQDNText)
    {
        // Arrange
        var invalidFQDN = string.Concat("my-domain.", new string('x', 241), ".de");
        var expectedValidationEntry = new ValidationEntry(
            $"{GetCustomFQDNText(customFQDNText)} '{invalidFQDN}' " +
            $"has too many characters (expected max. 253 characters including dots).");

        // Act
        var result = await new FQDNValidator(customFQDNText).ValidateAsync(invalidFQDN);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData(null, true)]
    [InlineData(CustomFQDNText, false)]
    [InlineData(CustomFQDNText, true)]
    public async Task ShouldReturnValidationError_WhenFQDNHasLeadingTrailingDots(
        string? customFQDNText,
        bool allowLeadingDot)
    {
        // Arrange
        var invalidFQDN = ".ptbtime1.ptb.de.";
        var expectedValidationEntry = new ValidationEntry(
            $"{GetCustomFQDNText(customFQDNText)} '{invalidFQDN}' can not have leading or trailing dots.");

        // Act
        var result = await new FQDNValidator(customFQDNText, allowLeadingDot).ValidateAsync(invalidFQDN);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(CustomFQDNText)]
    public async Task ShouldReturnValidationError_WhenFQDNHasConsecutiveDots(
        string? customFQDNText)
    {
        // Arrange
        var invalidFQDN = "ptbtime1..de";
        var expectedValidationEntry = new ValidationEntry(
            $"{GetCustomFQDNText(customFQDNText)} '{invalidFQDN}' can not have consecutive dots.");

        // Act
        var result = await new FQDNValidator(customFQDNText).ValidateAsync(invalidFQDN);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(CustomFQDNText)]
    public async Task ShouldReturnValidationError_WhenFQDNHasLessThanTwoDomainLabels(
        string? customFQDNText)
    {
        // Arrange
        var invalidFQDN = "ptbtime1";
        var expectedValidationEntry = new ValidationEntry(
            $"{GetCustomFQDNText(customFQDNText)} '{invalidFQDN}' " +
            $"must have at least two domain labels (e.g. \"example.com\").");

        // Act
        var result = await new FQDNValidator(customFQDNText).ValidateAsync(invalidFQDN);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(CustomFQDNText)]
    public async Task ShouldReturnValidationError_WhenFQDNHasMoreThanOneNumericLabel(
        string? customFQDNText)
    {
        // Arrange
        var invalidFQDN = "ptbtime1.123.456.de";
        var expectedValidationEntry = new ValidationEntry(
            $"{GetCustomFQDNText(customFQDNText)} '{invalidFQDN}' " +
            $"can not have more than one numerical label.");

        // Act
        var result = await new FQDNValidator(customFQDNText).ValidateAsync(invalidFQDN);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(CustomFQDNText)]
    public async Task ShouldReturnValidationError_WhenFQDNDomainLabelHasInvalidCharacterCount(
        string? customFQDNText)
    {
        // Arrange
        var invalidDomainLabel = "123456789a123456789b123456789c123456789d123456789e123456789f1234";
        var invalidFQDN = $"ptb.{invalidDomainLabel}.de";
        var expectedValidationEntry = new ValidationEntry(
            $"{GetCustomFQDNText(customFQDNText)} '{invalidFQDN}' " +
            $"has a domain label with an invalid character count (expected 1 to 63 characters). " +
            $"Affected domain label is: '{invalidDomainLabel}'.");

        // Act
        var result = await new FQDNValidator(customFQDNText).ValidateAsync(invalidFQDN);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(CustomFQDNText)]
    public async Task ShouldReturnValidationError_WhenFQDNTopLevelDomainHasInvalidCharacterCount(
        string? customFQDNText)
    {
        // Arrange
        var invalidFQDN = "ptb.time.d";
        var expectedValidationEntry = new ValidationEntry(
            $"{GetCustomFQDNText(customFQDNText)} '{invalidFQDN}' " +
            $"has a top-level domain with an invalid character count (expected 2 to 63 characters).");

        // Act
        var result = await new FQDNValidator(customFQDNText).ValidateAsync(invalidFQDN);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(CustomFQDNText)]
    public async Task ShouldReturnValidationError_WhenFQDNDomainLabelHasInvalidCharacter(
        string? customFQDNText)
    {
        // Arrange
        var invalidDomainLabel = "ptbtime1_ptb";
        var invalidFQDN = $"{invalidDomainLabel}.de";
        var expectedValidationEntry = new ValidationEntry(
            $"{GetCustomFQDNText(customFQDNText)} '{invalidFQDN}' has a domain label with an invalid character " +
            $"(expected start/end character to be letter or digit; other characters can also be a hyphen). " +
            $"Affected domain label is: '{invalidDomainLabel}'.");

        // Act
        var result = await new FQDNValidator(customFQDNText).ValidateAsync(invalidFQDN);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(CustomFQDNText)]
    public async Task ShouldReturnValid_WhenFQDNValid(string? customFQDNText)
    {
        // Act
        var result = await new FQDNValidator(customFQDNText).ValidateAsync(ValidationTestData.Domain.FQDN);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Entries.Count.Should().Be(0);
    }
}
