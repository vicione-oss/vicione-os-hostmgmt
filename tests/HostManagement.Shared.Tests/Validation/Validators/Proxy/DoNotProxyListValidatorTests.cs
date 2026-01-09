using System.Net;
using FluentAssertions;
using Xunit;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Validation;
using HostManagement.Shared.Validation.Validators.Proxy;
using HostManagement.Shared.Validation.Utils;

namespace HostManagement.Shared.Tests.Validation.Validators.Proxy;

public class DoNotProxyListValidatorTests
{
    private static readonly NetworkInterfaceDetail s_testInterfaceEns33 = new()
    {
        CommonInformation = new()
        {
            Enabled = true,
            Name = "ens33",
            PhysicalAddress = "33-33-33-33-33-33"
        },
        IPv4 = new()
        {
            Gateway = IPAddress.Parse("192.168.123.1"),
            IPv4Details =
            [
                new()
                {
                    IPAddress = IPAddress.Parse("192.168.123.33"),
                    Netmask = IPAddress.Parse("255.255.255.0")
                }
            ]
        }
    };

    private static readonly NetworkInterfaceDetail s_testInterfaceEns35 = new()
    {
        CommonInformation = new()
        {
            Enabled = true,
            Name = "ens35",
            PhysicalAddress = "35-35-35-35-35-35"
        },
        IPv4 = new() { DHCPEnabled = true }
    };

    private static readonly List<NetworkInterfaceDetail> s_interfaces =
        [s_testInterfaceEns33, s_testInterfaceEns35];

    private static readonly List<string> s_validDoNotProxyItems =
    [
        "192.168.178.42",
        "192.168.178.42:443",
        "2606:4700:4700::1111",
        "[2606:4700:4700::1111]:443",
        "fe80::1%ens35",
        "[fe80::1%ens35]:443",
        "my.fully-qualified-domain.com",
        "mySimpleDomainname",
    ];

    [Fact]
    public void ShouldHaveNoValidationError_WhenDoNotProxyListDisabled()
    {
        // Arrange
        var doNotProxyListEnabled = false;
        var validationResult = new ValidationResult();

        // Act
        DoNotProxyListValidator.Validate(
            s_interfaces,
            doNotProxyListEnabled,
            s_validDoNotProxyItems,
            validationResult);

        // Assert
        validationResult.IsValid.Should().BeTrue();
        validationResult.Entries.Count.Should().Be(0);
    }

    [Fact]
    public void ShouldAddValidationError_WhenDoNotProxyListHasEmptyItems()
    {
        // Arrange
        var doNotProxyListEnabled = true;
        var doNotProxyList = new List<string>(s_validDoNotProxyItems)
        {
            "",
            "   "
        };
        var validationResult = new ValidationResult();

        var expectedValidationEntry = new ValidationEntry(
            "A DoNotProxy item can not be empty or whitespace.",
            ErrorCodes.ValidationFailed);

        // Act
        DoNotProxyListValidator.Validate(
            s_interfaces,
            doNotProxyListEnabled,
            doNotProxyList,
            validationResult);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Entries.Count.Should().Be(1);
        validationResult.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData("192.168.178.420")]
    [InlineData("192.168.178.1234")]
    public void ShouldAddValidationError_WhenDoNotProxyItemIsNoValidIPv4Address(
        string invalidDoNotProxyItem)
    {
        // Arrange
        var doNotProxyListEnabled = true;
        var doNotProxyList = new List<string>(s_validDoNotProxyItems)
        {
            invalidDoNotProxyItem
        };
        var validationResult = new ValidationResult();

        var expectedValidationEntry = new ValidationEntry(
            $"DoNotProxy item '{invalidDoNotProxyItem}' is no valid IPv4 address.",
            ErrorCodes.ValidationFailed);

        // Act
        DoNotProxyListValidator.Validate(
            s_interfaces,
            doNotProxyListEnabled,
            doNotProxyList,
            validationResult);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Entries.Count.Should().Be(1);
        validationResult.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public void ShouldAddValidationError_WhenDoNotProxyItemIsIPv4MulticastAddress()
    {
        // Arrange
        var invalidDoNotProxyItem = "224.0.0.42";
        var doNotProxyListEnabled = true;
        var doNotProxyList = new List<string>(s_validDoNotProxyItems)
        {
            invalidDoNotProxyItem
        };
        var validationResult = new ValidationResult();

        var expectedValidationEntry = new ValidationEntry(
            $"DoNotProxy IP address '{invalidDoNotProxyItem}' can not be a multicast address " +
            $"('{IPAddressValidatorUtils.IPv4MulticastNetworkCIDR}').",
            ErrorCodes.ValidationFailed);

        // Act
        DoNotProxyListValidator.Validate(
            s_interfaces,
            doNotProxyListEnabled,
            doNotProxyList,
            validationResult);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Entries.Count.Should().Be(1);
        validationResult.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public void ShouldAddValidationError_WhenDoNotProxyItemIsIPv4BroadcastAddress()
    {
        // Arrange
        var invalidDoNotProxyItem = "255.255.255.255";
        var doNotProxyListEnabled = true;
        var doNotProxyList = new List<string>(s_validDoNotProxyItems)
        {
            invalidDoNotProxyItem
        };
        var validationResult = new ValidationResult();

        var expectedValidationEntry = new ValidationEntry(
            $"DoNotProxy IP address '{invalidDoNotProxyItem}' can not be a broadcast address " +
            $"('{IPAddressValidatorUtils.IPv4BroadcastNetworkCIDR}').",
            ErrorCodes.ValidationFailed);

        // Act
        DoNotProxyListValidator.Validate(
            s_interfaces,
            doNotProxyListEnabled,
            doNotProxyList,
            validationResult);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Entries.Count.Should().Be(1);
        validationResult.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData("2606:4700:4700::1111ff")]
    [InlineData("2606:4700:4700::123456")]
    public void ShouldAddValidationError_WhenDoNotProxyItemIsNoValidIPv6Address(
        string invalidDoNotProxyItem)
    {
        // Arrange
        var doNotProxyListEnabled = true;
        var doNotProxyList = new List<string>(s_validDoNotProxyItems)
        {
            invalidDoNotProxyItem
        };
        var validationResult = new ValidationResult();

        var expectedValidationEntry = new ValidationEntry(
            $"DoNotProxy item '{invalidDoNotProxyItem}' is no valid IPv6 address.",
            ErrorCodes.ValidationFailed);

        // Act
        DoNotProxyListValidator.Validate(
            s_interfaces,
            doNotProxyListEnabled,
            doNotProxyList,
            validationResult);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Entries.Count.Should().Be(1);
        validationResult.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public void ShouldAddValidationError_WhenDoNotProxyItemIsIPv6MulticastAddress()
    {
        // Arrange
        var invalidDoNotProxyItem = "ff05::1234";
        var doNotProxyListEnabled = true;
        var doNotProxyList = new List<string>(s_validDoNotProxyItems)
        {
            invalidDoNotProxyItem
        };
        var validationResult = new ValidationResult();

        var expectedValidationEntry = new ValidationEntry(
            $"DoNotProxy IP address '{invalidDoNotProxyItem}' can not be a multicast address " +
            $"('{IPAddressValidatorUtils.IPv6MulticastNetworkCIDR}').",
            ErrorCodes.ValidationFailed);

        // Act
        DoNotProxyListValidator.Validate(
            s_interfaces,
            doNotProxyListEnabled,
            doNotProxyList,
            validationResult);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Entries.Count.Should().Be(1);
        validationResult.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData("[2606:4700:4700::1111]:")]
    [InlineData("[2606:4700:4700::1111]")]
    public void ShouldAddValidationError_WhenComplexDoNotProxyItemHasMissingPort(
        string invalidDoNotProxyItem)
    {
        // Arrange
        var doNotProxyListEnabled = true;
        var doNotProxyList = new List<string>(s_validDoNotProxyItems)
        {
            invalidDoNotProxyItem
        };
        var validationResult = new ValidationResult();

        var expectedValidationEntry = new ValidationEntry(
            $"DoNotProxy item '{invalidDoNotProxyItem}' has a missing proxy port.",
            ErrorCodes.ValidationFailed);

        // Act
        DoNotProxyListValidator.Validate(
            s_interfaces,
            doNotProxyListEnabled,
            doNotProxyList,
            validationResult);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Entries.Count.Should().Be(1);
        validationResult.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void ShouldAddValidationError_WhenComplexDoNotProxyItemHasMissingZoneIdentifier(
        string? emptyZoneIdentifier)
    {
        // Arrange
        var invalidDoNotProxyItem = $"[fe80::%{emptyZoneIdentifier}]:443";
        var doNotProxyListEnabled = true;
        var doNotProxyList = new List<string>(s_validDoNotProxyItems)
        {
            invalidDoNotProxyItem
        };
        var validationResult = new ValidationResult();

        var expectedValidationEntry = new ValidationEntry(
            $"DoNotProxy item '{invalidDoNotProxyItem}' has an empty zone identifier.",
            ErrorCodes.ValidationFailed);

        // Act
        DoNotProxyListValidator.Validate(
            s_interfaces,
            doNotProxyListEnabled,
            doNotProxyList,
            validationResult);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Entries.Count.Should().Be(1);
        validationResult.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public void ShouldAddValidationError_WhenComplexDoNotProxyItemHasTooLongZoneIdentifier()
    {
        // Arrange
        var invalidZoneIdentifier = "ens456789012345x";
        var invalidDoNotProxyItem = $"[fe80::%{invalidZoneIdentifier}]:443";
        var doNotProxyListEnabled = true;
        var doNotProxyList = new List<string>(s_validDoNotProxyItems)
        {
            invalidDoNotProxyItem
        };
        var validationResult = new ValidationResult();

        var expectedValidationEntry = new ValidationEntry(
            $"Zone identifier '{invalidZoneIdentifier}' of DoNotProxy item '{invalidDoNotProxyItem}' " +
            $"has an invalid character count (expected max. 15 characters).",
            ErrorCodes.ValidationFailed);

        // Act
        DoNotProxyListValidator.Validate(
            s_interfaces,
            doNotProxyListEnabled,
            doNotProxyList,
            validationResult);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Entries.Count.Should().Be(1);
        validationResult.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData("ensµ35")]
    [InlineData("ensπ35")]
    public void ShouldAddValidationError_WhenComplexDoNotProxyItemHasZoneIdentifierWithInvalidCharacter(
        string invalidZoneIdentifier)
    {
        // Arrange
        var invalidDoNotProxyItem = $"[fe80::%{invalidZoneIdentifier}]:443";
        var doNotProxyListEnabled = true;
        var doNotProxyList = new List<string>(s_validDoNotProxyItems)
        {
            invalidDoNotProxyItem
        };
        var validationResult = new ValidationResult();

        var expectedValidationEntry = new ValidationEntry(
            $"Zone identifier '{invalidZoneIdentifier}' of DoNotProxy item '{invalidDoNotProxyItem}' " +
            $"has an invalid character (expected 0-9, a-f, A-F, _ or -).",
            ErrorCodes.ValidationFailed);

        // Act
        DoNotProxyListValidator.Validate(
            s_interfaces,
            doNotProxyListEnabled,
            doNotProxyList,
            validationResult);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Entries.Count.Should().Be(1);
        validationResult.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData("ens43")]
    [InlineData("ens45")]
    public void ShouldAddValidationError_WhenComplexDoNotProxyItemHasZoneIdentifierWithInvalidInterfaceName(
        string invalidZoneIdentifier)
    {
        // Arrange
        var invalidDoNotProxyItem = $"[fe80::%{invalidZoneIdentifier}]:443";
        var doNotProxyListEnabled = true;
        var doNotProxyList = new List<string>(s_validDoNotProxyItems)
        {
            invalidDoNotProxyItem
        };
        var validationResult = new ValidationResult();

        var expectedValidationEntry = new ValidationEntry(
            $"Zone identifier '{invalidZoneIdentifier}' of DoNotProxy item '{invalidDoNotProxyItem}' " +
            $"has an unknown interface name.",
            ErrorCodes.ValidationFailed);

        // Act
        DoNotProxyListValidator.Validate(
            s_interfaces,
            doNotProxyListEnabled,
            doNotProxyList,
            validationResult);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Entries.Count.Should().Be(1);
        validationResult.Entries.Should().Contain(expectedValidationEntry);
    }
}
