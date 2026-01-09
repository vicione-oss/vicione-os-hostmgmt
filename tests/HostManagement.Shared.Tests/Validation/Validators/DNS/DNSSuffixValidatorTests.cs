using FluentAssertions;
using Xunit;
using HostManagement.Shared.Validation;
using HostManagement.Shared.Validation.Validators.DNS;
using HostManagement.Shared.Contracts.Network;

namespace HostManagement.Shared.Tests.Validation.Validators.DNS;

public class DNSSuffixValidatorTests
{
    [Fact]
    public void ShouldHaveNoValidationError_WhenDNSSuffixDisabled()
    {
        // Arrange
        var dnsSettingsWithDisabledSuffix = new NetworkDNSSettings() with
        {
            Hostname = "debian42",
            DNSSuffixEnabled = false,
            DNSSuffix = "ifm.com"
        };
        var validationResult = new ValidationResult();

        // Act
        DNSSuffixValidator.Validate(
            dnsSettingsWithDisabledSuffix.Hostname,
            dnsSettingsWithDisabledSuffix.DNSSuffixEnabled,
            dnsSettingsWithDisabledSuffix.DNSSuffix,
            validationResult);

        // Assert
        validationResult.IsValid.Should().BeTrue();
        validationResult.Entries.Count.Should().Be(0);
    }

    [Fact]
    public void ShouldAddValidationError_WhenDNSSuffixEmpty()
    {
        // Arrange
        var hostname = "debian42";
        var dnsSuffixEnabled = true;
        var invalidDnsSuffix = string.Empty;
        var validationResult = new ValidationResult();

        var expectedValidationEntry = new ValidationEntry(
            "DNS suffix can not be empty or whitespace.",
            ErrorCodes.ValidationFailed);

        // Act
        DNSSuffixValidator.Validate(
            hostname,
            dnsSuffixEnabled,
            invalidDnsSuffix,
            validationResult);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Entries.Count.Should().Be(1);
        validationResult.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public void ShouldAddValidationError_WhenHostnameAndDNSSuffixTooLong()
    {
        // Arrange
        var hostname = new string('a', 222);
        var dnsSuffixEnabled = true;
        var dnsSuffix = string.Concat(new string('b', 27), ".com");

        var validationResult = new ValidationResult();
        var expectedValidationEntry = new ValidationEntry(
            $"Fully Qualified Domain Name (FQDN) consisting of hostname '{hostname}' and " +
            $"primary DNS suffix '{dnsSuffix}' has too many characters " +
            $"(expected max. 253 characters including dots).",
            ErrorCodes.ValidationFailed);

        // Act
        DNSSuffixValidator.Validate(
            hostname,
            dnsSuffixEnabled,
            dnsSuffix,
            validationResult);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Entries.Count.Should().Be(1);
        validationResult.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData(false, "ifm.com")]
    [InlineData(true, "ifm.com")]
    [InlineData(true, "company")]
    public void ShouldHaveNoValidationError_WhenDNSSuffixValid(bool dnsSuffixEnabled, string dnsSuffix)
    {
        // Arrange
        var hostname = "debian42";
        var validationResult = new ValidationResult();

        // Act
        DNSSuffixValidator.Validate(
            hostname,
            dnsSuffixEnabled,
            dnsSuffix,
            validationResult);

        // Assert
        validationResult.IsValid.Should().BeTrue();
        validationResult.Entries.Count.Should().Be(0);
    }
}
