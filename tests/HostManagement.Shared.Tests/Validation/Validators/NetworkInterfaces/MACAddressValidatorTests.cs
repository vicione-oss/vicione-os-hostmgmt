using AwesomeAssertions;
using HostManagement.Shared.Validation;
using HostManagement.Shared.Validation.Validators.NetworkInterfaces;
using Xunit;

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
        var result = await new MACAddressValidator(interfaceName)
            .ValidateAsync(invalidMACAddress!, CancellationToken.None);

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
        var result = await new MACAddressValidator(interfaceName)
            .ValidateAsync(invalidMACAddress, CancellationToken.None);

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
        var result = await new MACAddressValidator(interfaceName)
            .ValidateAsync(invalidMACAddress, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData(null, "032233445566")]
    [InlineData("ens33", "B72233445566")]
    public async Task ShouldReturnValidationError_WhenMACAddressWithoutSeparatorsIsMulticastAddress(
        string? interfaceName,
        string invalidMACAddress)
    {
        // Arrange
        var expectedValidationEntry = new ValidationEntry(
            $"MAC address '{invalidMACAddress}' of {GetCustomInterfaceNameText(interfaceName)} " +
            $"can not be a multicast address, expected first octet to be " +
            $"'X0', 'X2', 'X4', 'X6', 'X8', 'XA', 'XC' or 'XE' (where X is 0-9, a-f, A-F).");

        // Act
        var result = await new MACAddressValidator(interfaceName)
            .ValidateAsync(invalidMACAddress, CancellationToken.None);

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
        var result = await new MACAddressValidator(interfaceName)
            .ValidateAsync(invalidMACAddress, CancellationToken.None);

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
        var result = await new MACAddressValidator(interfaceName)
            .ValidateAsync(invalidMACAddress, CancellationToken.None);

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
        var result = await new MACAddressValidator(interfaceName)
            .ValidateAsync(invalidMACAddress, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData(null, "03:22:33:44:55:66")]
    [InlineData("ens33", "B7:22:33:44:55:66")]
    public async Task ShouldReturnValidationError_WhenMACAddressWithSeparatorsIsMulticastAddress(
        string? interfaceName,
        string invalidMACAddress)
    {
        // Arrange
        var expectedValidationEntry = new ValidationEntry(
            $"MAC address '{invalidMACAddress}' of {GetCustomInterfaceNameText(interfaceName)} " +
            $"can not be a multicast address, expected first octet to be " +
            $"'X0', 'X2', 'X4', 'X6', 'X8', 'XA', 'XC' or 'XE' (where X is 0-9, a-f, A-F).");

        // Act
        var result = await new MACAddressValidator(interfaceName)
            .ValidateAsync(invalidMACAddress, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [MemberData(nameof(ValidMACAddressData))]
    public async Task ShouldReturnValid_WhenMACAddressValid(string macAddress)
    {
        // Act
        var result = await new MACAddressValidator().ValidateAsync(macAddress, CancellationToken.None);

        var resultWithInterfaceName = await new MACAddressValidator("lan2")
            .ValidateAsync(macAddress, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Entries.Count.Should().Be(0);

        resultWithInterfaceName.IsValid.Should().BeTrue();
        resultWithInterfaceName.Entries.Count.Should().Be(0);
    }

    [Fact]
    public void ValidMACAddressData_HasExpectedCount() => ValidMACAddressData.Count.Should().Be(128 * 6);

    public static TheoryData<string> ValidMACAddressData => [.. EnumerateAllMACs()];

    public static IEnumerable<string> EnumerateAllMACs()
    {
        // Define lower/upper case tails of a MAC with or without separators...
        string[] lowerTails = ["b2c3d4e5f6", "-b2-c3-d4-e5-f6", ":b2:c3:d4:e5:f6"];
        string[] upperTails = ["B2C3D4E5F6", "-B2-C3-D4-E5-F6", ":B2:C3:D4:E5:F6"];

        // ...and concatenate with the first octet (universal/local)
        for (var firstOctet = 0; firstOctet <= 0xFF; firstOctet++)
        {
            /*
             * Unicast MACs are valid.
             * - Universally administered (first octets: 'X0', 'X4', 'X8', 'XC' where X 0-9, a-f, A-F)
             * - Locally administered (first octets: 'X2', 'X6', 'XA', 'XE' where X 0-9, a-f, A-F)
             */

            // Multicast MACs (least significant bit set to 1) are invalid.
            if ((firstOctet & 0x01) == 0x01)
                continue;

            // Lower and upper case are allowed.
            var firstOctetLower = firstOctet.ToString("x2");
            var firstOctetUpper = firstOctet.ToString("X2");

            foreach (var tail in lowerTails)
                yield return string.Concat(firstOctetLower, tail);

            foreach (var tail in upperTails)
                yield return string.Concat(firstOctetUpper, tail);
        }
    }
}
