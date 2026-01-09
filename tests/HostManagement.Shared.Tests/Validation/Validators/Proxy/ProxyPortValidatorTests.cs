using FluentAssertions;
using HostManagement.Shared.Validation;
using HostManagement.Shared.Validation.Validators.Proxy;
using Xunit;

namespace HostManagement.Shared.Tests.Validation.Validators.Proxy;

public class ProxyPortValidatorTests
{
    private const int MinPort = 1;
    private const int MaxPort = 65_535;

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("ff")]
    [InlineData("a42")]
    public void ShouldAddValidationError_WhenPortStringCanNotBeParsed(string? invalidPortString)
    {
        // Arrange
        var validationResult = new ValidationResult();

        var expectedValidationEntry = new ValidationEntry(
            $"Proxy port '{invalidPortString}' must be a number.",
            ErrorCodes.ValidationFailed);

        // Act
        ProxyPortValidator.Validate(invalidPortString, validationResult);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Entries.Count.Should().Be(1);
        validationResult.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public void ShouldAddValidationError_WhenPortNull()
    {
        // Arrange
        int? invalidPort = null;
        var validationResult = new ValidationResult();

        var expectedValidationEntry = new ValidationEntry(
            "Proxy port can not be null.",
            ErrorCodes.ValidationFailed);

        // Act
        ProxyPortValidator.Validate(invalidPort, validationResult);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Entries.Count.Should().Be(1);
        validationResult.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData(-42)]
    [InlineData(0)]
    public void ShouldAddValidationError_WhenPortSmallerThanMinimum(int invalidPort)
    {
        // Arrange
        var validationResult = new ValidationResult();

        var expectedValidationEntry = new ValidationEntry(
            $"Proxy port '{invalidPort}' can not be smaller than '{MinPort}'.",
            ErrorCodes.ValidationFailed);

        // Act
        ProxyPortValidator.Validate(invalidPort, validationResult);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Entries.Count.Should().Be(1);
        validationResult.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public void ShouldAddValidationError_WhenPortGreaterThanMaximum()
    {
        // Arrange
        var invalidPort = 65_536;
        var validationResult = new ValidationResult();

        var expectedValidationEntry = new ValidationEntry(
            $"Proxy port '{invalidPort}' can not be greater than '{MaxPort}'.",
            ErrorCodes.ValidationFailed);

        // Act
        ProxyPortValidator.Validate(invalidPort, validationResult);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Entries.Count.Should().Be(1);
        validationResult.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(42)]
    [InlineData(443)]
    [InlineData(8080)]
    [InlineData(65_535)]
    public void ShouldHaveNoValidationError_WhenPortValid(int port)
    {
        // Arrange
        var validationResult = new ValidationResult();

        // Act
        ProxyPortValidator.Validate(port, validationResult);

        // Assert
        validationResult.IsValid.Should().BeTrue();
        validationResult.Entries.Count.Should().Be(0);
    }
}
