using System.Net.NetworkInformation;
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
        var macAddress = PhysicalAddress.Parse("01:23:45:67:89:AB");

        // Act
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var result = await new MACAddressValidator().ValidateAsync(macAddress, cts.Token);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Should().ContainSingle().Which.Should().Be(expectedValidationEntry);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("ens33")]
    public async Task ShouldReturnValidationError_WhenMACAddressEmpty(string? interfaceName)
    {
        // Arrange
        var expectedValidationEntry = new ValidationEntry(
            $"MAC address of {GetCustomInterfaceNameText(interfaceName)} can not be empty or whitespace.");

        // Act
        var result = await new MACAddressValidator(interfaceName)
            .ValidateAsync(PhysicalAddress.None, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Should().ContainSingle().Which.Should().Be(expectedValidationEntry);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("ens33")]
    public async Task ShouldReturnValidationError_WhenMACAddressNull(string? interfaceName)
    {
        // Arrange
        var expectedValidationEntry = new ValidationEntry(
            $"MAC address of {GetCustomInterfaceNameText(interfaceName)} can not be empty or whitespace.");

        // Act
        var result = await new MACAddressValidator(interfaceName)
            .ValidateAsync(null!, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Should().ContainSingle().Which.Should().Be(expectedValidationEntry);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("ens33")]
    public async Task ShouldReturnValidationError_WhenMACAddressHasInvalidLength(string? interfaceName)
    {
        // Arrange
        var invalidMACAddress = new PhysicalAddress([0xA1, 0xB2, 0xC3, 0xD4, 0xE5]);
        var expectedValidationEntry = new ValidationEntry(
            $"MAC address 'A1:B2:C3:D4:E5' of {GetCustomInterfaceNameText(interfaceName)} " +
            $"has invalid length (expected 6 bytes).");

        // Act
        var result = await new MACAddressValidator(interfaceName)
            .ValidateAsync(invalidMACAddress, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Should().ContainSingle().Which.Should().Be(expectedValidationEntry);
    }

    [Theory]
    [InlineData(null, "03:22:33:44:55:66")]
    [InlineData("ens33", "B7:22:33:44:55:66")]
    public async Task ShouldReturnValidationError_WhenMACAddressIsMulticastAddress(
        string? interfaceName,
        string invalidMACAddress)
    {
        // Arrange
        var physicalAddress = PhysicalAddress.Parse(invalidMACAddress);
        var expectedValidationEntry = new ValidationEntry(
            $"MAC address '{invalidMACAddress}' of {GetCustomInterfaceNameText(interfaceName)} " +
            $"can not be a multicast address, expected first octet to be " +
            $"'X0', 'X2', 'X4', 'X6', 'X8', 'XA', 'XC' or 'XE' (where X is 0-9, a-f, A-F).");

        // Act
        var result = await new MACAddressValidator(interfaceName)
            .ValidateAsync(physicalAddress, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Should().ContainSingle().Which.Should().Be(expectedValidationEntry);
    }

    [Theory]
    [MemberData(nameof(ValidMACAddressData))]
    public async Task ShouldReturnValid_WhenMACAddressValid(string macAddress)
    {
        // Arrange
        var physicalAddress = PhysicalAddress.Parse(macAddress);

        // Act
        var result = await new MACAddressValidator().ValidateAsync(physicalAddress, CancellationToken.None);

        var resultWithInterfaceName = await new MACAddressValidator("lan2")
            .ValidateAsync(physicalAddress, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Entries.Should().BeEmpty();

        resultWithInterfaceName.IsValid.Should().BeTrue();
        resultWithInterfaceName.Entries.Should().BeEmpty();
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
