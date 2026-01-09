using FluentAssertions;
using Xunit;
using HostManagement.Shared.Validation;
using HostManagement.Shared.Validation.Validators.DNS;

namespace HostManagement.Shared.Tests.Validation.Validators.DNS;

public class DomainLabelValidatorTests
{
    [Fact]
    public void ShouldThrow_WhenCustomDomainLabelNameNull()
    {
        // Arrange
        var domainLabel = "domain";
        var validationResult = new ValidationResult();
        string? customDomainLabelName = null;

        var expectedParameterName = "customDomainLabelName";

        // Act
        var result = () => DomainLabelValidator.Validate(domainLabel, validationResult, customDomainLabelName!);

        // Assert
        result.Should()
            .ThrowAsync<ArgumentNullException>()
            .WithParameterName(expectedParameterName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void ShouldThrow_WhenCustomDomainLabelNameEmpty(string customDomainLabelName)
    {
        // Arrange
        var domainLabel = "domain";
        var validationResult = new ValidationResult();

        var expectedParameterName = "customDomainLabelName";

        // Act
        var result = () => DomainLabelValidator.Validate(domainLabel, validationResult, customDomainLabelName);

        // Assert
        result.Should()
            .ThrowAsync<ArgumentException>()
            .WithParameterName(expectedParameterName);
    }

    [Fact]
    public void ShouldAddValidationError_WhenDomainLabelHasInvalidCharacterCount()
    {
        // Arrange
        var invalidDomainLabel = new string('n', 64);
        var validationResult = new ValidationResult();
        var customDomainLabelName = "Hostname";

        var expectedValidationEntry = new ValidationEntry(
            $"{customDomainLabelName} '{invalidDomainLabel}' has too many characters " +
            $"(expected max. 63 characters including dots).",
            ErrorCodes.ValidationFailed);

        // Act
        DomainLabelValidator.Validate(invalidDomainLabel, validationResult, customDomainLabelName);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Entries.Count.Should().Be(1);
        validationResult.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public void ShouldAddValidationError_WhenDomainLabelHasOnlyDigits()
    {
        // Arrange
        var invalidDomainLabel = "42";
        var validationResult = new ValidationResult();
        var customDomainLabelName = "Hostname";

        var expectedValidationEntry = new ValidationEntry(
            $"{customDomainLabelName} '{invalidDomainLabel}' can not have only digits.",
            ErrorCodes.ValidationFailed);

        // Act
        DomainLabelValidator.Validate(invalidDomainLabel, validationResult, customDomainLabelName);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Entries.Count.Should().Be(1);
        validationResult.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData("-domainLabel")]
    [InlineData("domainLabel-")]
    [InlineData(".domainLabel")]
    [InlineData("domainLabel.")]
    [InlineData("#domainLabel")]
    [InlineData("domainLabel#")]
    [InlineData("domain-#42-Label")]
    [InlineData("domain-ß42-Label")]
    public void ShouldAddValidationError_WhenDomainLabelHasInvalidCharacter(string invalidDomainLabel)
    {
        // Arrange
        var validationResult = new ValidationResult();
        var customDomainLabelName = "Hostname";

        var expectedValidationEntry = new ValidationEntry(
            $"{customDomainLabelName} '{invalidDomainLabel}' has an invalid character " +
            $"(expected start/end character to be letter or digit; other characters can also be a hyphen).",
            ErrorCodes.ValidationFailed);

        // Act
        DomainLabelValidator.Validate(invalidDomainLabel, validationResult, customDomainLabelName);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Entries.Count.Should().Be(1);
        validationResult.Entries.Should().Contain(expectedValidationEntry);
    }
}
