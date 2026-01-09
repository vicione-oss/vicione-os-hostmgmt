using FluentAssertions;
using Xunit;
using HostManagement.Shared.Validation;
using HostManagement.Shared.Validation.Validators;

namespace HostManagement.Shared.Tests.Validation.Validators;

public class MACAddressValidatorTests
{
    [Theory]
    [InlineData("")]
    [InlineData("", "ens27")]
    [InlineData("   ", "ens29")]
    public void ShouldAddValidationError_WhenMACAddressEmpty(
        string macAddress,
        string customInterfaceName = "network interface")
    {
        // Arrange
        var validationResult = new ValidationResult();

        var expectedValidationEntry = new ValidationEntry(
            $"Physical address (MAC) of {customInterfaceName} can not be empty or whitespace.",
            ErrorCodes.ValidationFailed);

        // Act
        MACAddressValidator.Validate(macAddress, validationResult, customInterfaceName);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Entries.Count.Should().Be(1);
        validationResult.Entries.Should().Contain(expectedValidationEntry);
    }

    public class MACAddressWithoutSeparators
    {
        [Fact]
        public void ShouldAddValidationError_WhenMACAddressHasInvalidLength()
        {
            // Arrange
            var macAddress = "A1B2C3D4E5";
            var interfaceName = "ens31";
            var validationResult = new ValidationResult();

            var expectedValidationEntry = new ValidationEntry(
                $"Physical address (MAC) '{macAddress}' of {interfaceName} has " +
                $"invalid length (expected 12).",
                ErrorCodes.ValidationFailed);

            // Act
            MACAddressValidator.Validate(macAddress, validationResult, interfaceName);

            // Assert
            validationResult.IsValid.Should().BeFalse();
            validationResult.Entries.Count.Should().Be(1);
            validationResult.Entries.Should().Contain(expectedValidationEntry);
        }

        [Fact]
        public void ShouldAddValidationError_WhenMACAddressHasNonAsciiHexDigit()
        {
            // Arrange
            var macAddress = "AøB2C3D4E5F6";
            var interfaceName = "ens33";
            var validationResult = new ValidationResult();

            var expectedValidationEntry = new ValidationEntry(
                $"Physical address (MAC) '{macAddress}' of {interfaceName} has " +
                $"an invalid character (expected 0-9, a-f, A-F).",
                ErrorCodes.ValidationFailed);

            // Act
            MACAddressValidator.Validate(macAddress, validationResult, interfaceName);

            // Assert
            validationResult.IsValid.Should().BeFalse();
            validationResult.Entries.Count.Should().Be(1);
            validationResult.Entries.Should().Contain(expectedValidationEntry);
        }

        [Fact]
        public void ShouldHaveNoValidationError_WhenMACAddressValid()
        {
            // Arrange
            var macAddress = "A1B2C3D4E5F6";
            var interfaceName = "ens35";
            var validationResult = new ValidationResult();

            // Act
            MACAddressValidator.Validate(macAddress, validationResult, interfaceName);

            // Assert
            validationResult.IsValid.Should().BeTrue();
            validationResult.Entries.Count.Should().Be(0);
        }
    }

    public class MACAddressWithSeparators
    {
        [Fact]
        public void ShouldAddValidationError_WhenMACAddressHasInvalidOctetCount()
        {
            // Arrange
            var macAddress = "aa:bb:cc:dd:ee";
            var interfaceName = "ens37";
            var validationResult = new ValidationResult();

            var expectedValidationEntry = new ValidationEntry(
                $"Physical address (MAC) '{macAddress}' of {interfaceName} has " +
                $"invalid octet count (expected 6).",
                ErrorCodes.ValidationFailed);

            // Act
            MACAddressValidator.Validate(macAddress, validationResult, interfaceName);

            // Assert
            validationResult.IsValid.Should().BeFalse();
            validationResult.Entries.Count.Should().Be(1);
            validationResult.Entries.Should().Contain(expectedValidationEntry);
        }

        [Fact]
        public void ShouldAddValidationError_WhenMACAddressHasOctetWithInvalidLength()
        {
            // Arrange
            var macAddress = "aa:bb:cc:dd:eee:ff";
            var interfaceName = "ens39";
            var validationResult = new ValidationResult();

            var expectedValidationEntry = new ValidationEntry(
                $"Physical address (MAC) '{macAddress}' of {interfaceName} has " +
                $"an octet with invalid length (expected 2).",
                ErrorCodes.ValidationFailed);

            // Act
            MACAddressValidator.Validate(macAddress, validationResult, interfaceName);

            // Assert
            validationResult.IsValid.Should().BeFalse();
            validationResult.Entries.Count.Should().Be(1);
            validationResult.Entries.Should().Contain(expectedValidationEntry);
        }

        [Fact]
        public void ShouldAddValidationError_WhenMACAddressHasOctetWithNonAsciiHexDigit()
        {
            // Arrange
            var macAddress = "aa:bb:cc:dd:ee:٩f";
            var interfaceName = "ens41";
            var validationResult = new ValidationResult();

            var expectedValidationEntry = new ValidationEntry(
                $"Physical address (MAC) '{macAddress}' of {interfaceName} has " +
                $"an invalid character (expected 0-9, a-f, A-F).",
                ErrorCodes.ValidationFailed);

            // Act
            MACAddressValidator.Validate(macAddress, validationResult, interfaceName);

            // Assert
            validationResult.IsValid.Should().BeFalse();
            validationResult.Entries.Count.Should().Be(1);
            validationResult.Entries.Should().Contain(expectedValidationEntry);
        }

        [Fact]
        public void ShouldHaveNoValidationError_WhenMACAddressValid()
        {
            // Arrange
            var macAddress = "A1:B2:C3:D4:E5:F6";
            var interfaceName = "ens43";
            var validationResult = new ValidationResult();

            // Act
            MACAddressValidator.Validate(macAddress, validationResult, interfaceName);

            // Assert
            validationResult.IsValid.Should().BeTrue();
            validationResult.Entries.Count.Should().Be(0);
        }
    }
}
