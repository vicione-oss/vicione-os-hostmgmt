using FluentAssertions;
using Xunit;
using HostManagement.Shared.Validation;
using HostManagement.Shared.Validation.Validators.NetworkInterfaces;

namespace HostManagement.Shared.Tests.Validation.Validators.NetworkInterfaces;

public class MACAddressValidatorTests
{
    private static string GetCustomInterfaceNameText(string? interfaceName)
        => interfaceName is null ? "network interface" : $"network interface '{interfaceName}'";

    [Fact]
    public async Task ShouldReturnValidationError_WhenValidationCancelled()
    {
        // Arrange
        var expectedValidationEntry = new ValidationEntry(ValidationConstants.CancelledResult);

        // Act
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var result = await new MACAddressValidator().ValidateAsync("01:23:45:67:89:AB", cts.Token);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData(null, "")]
    [InlineData(null, "  ")]
    [InlineData("ens33", null)]
    [InlineData("ens33", "")]
    [InlineData("ens33", "  ")]
    public async Task ShouldReturnValidationError_WhenMACAddressEmpty(
        string? interfaceName,
        string? invalidMACAddress)
    {
        // Arrange
        var expectedValidationEntry = new ValidationEntry(
            $"MAC address of {GetCustomInterfaceNameText(interfaceName)} can not be empty or whitespace.");

        // Act
        var result = await new MACAddressValidator(interfaceName).ValidateAsync(invalidMACAddress!);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData(null, "A1B2C3D4E5")]
    [InlineData("ens33", "A1B2C3D4E5")]
    public async Task ShouldReturnValidationError_WhenMACAddressWithoutSeparatorsHasInvalidLength(
        string? interfaceName,
        string invalidMACAddress)
    {
        // Arrange
        var expectedValidationEntry = new ValidationEntry(
            $"MAC address '{invalidMACAddress}' of {GetCustomInterfaceNameText(interfaceName)} " +
            $"has invalid length (expected 12).");

        // Act
        var result = await new MACAddressValidator(interfaceName).ValidateAsync(invalidMACAddress);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData(null, "AøB2C3D4E5F6")]
    [InlineData("ens33", "AøB2C3D4E5F6")]
    public async Task ShouldReturnValidationError_WhenMACAddressWithoutSeparatorsHasNonAsciiHexDigit(
        string? interfaceName,
        string invalidMACAddress)
    {
        // Arrange
        var expectedValidationEntry = new ValidationEntry(
            $"MAC address '{invalidMACAddress}' of {GetCustomInterfaceNameText(interfaceName)} " +
            $"has an invalid character (expected 0-9, a-f, A-F).");

        // Act
        var result = await new MACAddressValidator(interfaceName).ValidateAsync(invalidMACAddress);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData(null, "AA:BB:CC:DD:EE")]
    [InlineData("ens33", "AA:BB:CC:DD:EE")]
    public async Task ShouldReturnValidationError_WhenMACAddressWithSeparatorsHasInvalidOctetCount(
        string? interfaceName,
        string invalidMACAddress)
    {
        // Arrange
        var expectedValidationEntry = new ValidationEntry(
            $"MAC address '{invalidMACAddress}' of {GetCustomInterfaceNameText(interfaceName)} " +
            $"has an invalid octet count (expected 6).");

        // Act
        var result = await new MACAddressValidator(interfaceName).ValidateAsync(invalidMACAddress);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData(null, "AA:BB:CC:DD:EEE:FF")]
    [InlineData("ens33", "AA:BB:CC:DD:EEE:FF")]
    public async Task ShouldReturnValidationError_WhenMACAddressWithSeparatorsHasOctetWithInvalidLength(
        string? interfaceName,
        string invalidMACAddress)
    {
        // Arrange
        var expectedValidationEntry = new ValidationEntry(
            $"MAC address '{invalidMACAddress}' of {GetCustomInterfaceNameText(interfaceName)} " +
            $"has an octet with invalid length (expected 2).");

        // Act
        var result = await new MACAddressValidator(interfaceName).ValidateAsync(invalidMACAddress);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData(null, "AA:BB:CC:DD:EE:٩F")]
    [InlineData("ens33", "AA:BB:CC:DD:EE:٩F")]
    public async Task ShouldReturnValidationError_WhenMACAddressWithSeparatorsHasOctetWithNonAsciiHexDigit(
        string? interfaceName,
        string invalidMACAddress)
    {
        // Arrange
        var expectedValidationEntry = new ValidationEntry(
            $"MAC address '{invalidMACAddress}' of {GetCustomInterfaceNameText(interfaceName)} " +
            $"has an invalid character (expected 0-9, a-f, A-F).");

        // Act
        var result = await new MACAddressValidator(interfaceName).ValidateAsync(invalidMACAddress);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData(null, "a1b2c3d4e5f6")]
    [InlineData(null, "a1-b2-c3-d4-e5-f6")]
    [InlineData(null, "a1:b2:c3:d4:e5:f6")]
    [InlineData(null, "A1B2C3D4E5F6")]
    [InlineData(null, "A1-B2-C3-D4-E5-F6")]
    [InlineData(null, "A1:B2:C3:D4:E5:F6")]
    [InlineData("ens33", "a1b2c3d4e5f6")]
    [InlineData("ens33", "a1-b2-c3-d4-e5-f6")]
    [InlineData("ens33", "a1:b2:c3:d4:e5:f6")]
    [InlineData("ens33", "A1B2C3D4E5F6")]
    [InlineData("ens33", "A1-B2-C3-D4-E5-F6")]
    [InlineData("ens33", "A1:B2:C3:D4:E5:F6")]
    public async Task ShouldReturnValid_WhenMACAddressValid(
        string? interfaceName,
        string macAddress)
    {
        // Act
        var result = await new MACAddressValidator(interfaceName).ValidateAsync(macAddress);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Entries.Count.Should().Be(0);
    }
}
