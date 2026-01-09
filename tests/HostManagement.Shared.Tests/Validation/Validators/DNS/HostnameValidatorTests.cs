using FluentAssertions;
using HostManagement.Shared.Validation;
using HostManagement.Shared.Validation.Validators.DNS;
using Xunit;

namespace HostManagement.Shared.Tests.Validation.Validators.DNS;

public class HostnameValidatorTests
{
    [Fact]
    public void ShouldAddValidationError_WhenHostnameEmpty()
    {
        // Arrange
        var invalidHostname = string.Empty;
        var validationResult = new ValidationResult();

        var expectedValidationEntry = new ValidationEntry(
            "Hostname can not be empty or whitespace.",
            ErrorCodes.ValidationFailed);

        // Act
        HostnameValidator.Validate(invalidHostname, validationResult);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Entries.Count.Should().Be(1);
        validationResult.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public void ShouldAddValidationError_WhenFqdnHostnameHasTooManyCharacters()
    {
        // Arrange
        var invalidFqdnHostname = string.Concat(new string('a', 31), '.', new string('b', 31), 'x');
        var validationResult = new ValidationResult();

        var expectedValidationEntry = new ValidationEntry(
            $"Hostname '{invalidFqdnHostname}' has too many characters " +
            $"(expected max. 63 characters including dots).",
            ErrorCodes.ValidationFailed);

        // Act
        HostnameValidator.Validate(invalidFqdnHostname, validationResult);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Entries.Count.Should().Be(1);
        validationResult.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData("device42")]
    [InlineData("company.device42")]
    [InlineData("debian")]
    [InlineData("42.debian-12")]
    public void ShouldHaveNoValidationError_WhenHostnameValid(string hostname)
    {
        // Arrange
        var validationResult = new ValidationResult();

        // Act
        HostnameValidator.Validate(hostname, validationResult);

        // Assert
        validationResult.IsValid.Should().BeTrue();
        validationResult.Entries.Count.Should().Be(0);
    }
}
