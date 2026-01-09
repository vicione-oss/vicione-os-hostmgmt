using System.Net;
using FluentAssertions;
using Xunit;
using HostManagement.Shared.Validation;
using HostManagement.Shared.Validation.Utils;
using HostManagement.Shared.Validation.Validators.DNS;

namespace HostManagement.Shared.Tests.Validation.Validators.DNS;

public class NameServersValidatorTests
{
    [Fact]
    public void ShouldHaveNoValidationError_WhenNameServersDisabled()
    {
        // Arrange
        var nameServersEnabled = false;
        var nameServers = new List<IPAddress>()
        {
            IPAddress.Parse("1.1.1.1"),
            IPAddress.Parse("2606:4700:4700::1111"),
            IPAddress.Parse("8.8.8.8"),
            IPAddress.Parse("2001:4860:4860::8888"),
        };
        var validationResult = new ValidationResult();

        // Act
        NameServersValidator.Validate(nameServersEnabled, nameServers, validationResult);

        // Assert
        validationResult.IsValid.Should().BeTrue();
        validationResult.Entries.Count.Should().Be(0);
    }

    [Fact]
    public void ShouldAddValidationError_WhenDuplicateNameServersFound()
    {
        // Arrange
        var duplicateNameServer = IPAddress.Parse("1.1.1.1");
        var nameServersEnabled = true;
        var nameServers = new List<IPAddress>()
        {
            duplicateNameServer,
            IPAddress.Parse("2606:4700:4700::1111"),
            duplicateNameServer,
            IPAddress.Parse("2001:4860:4860::8888"),
        };
        var validationResult = new ValidationResult();

        var expectedValidationEntry = new ValidationEntry(
            $"Name servers are not unique. Affected IP addresses are: '{duplicateNameServer}'.",
            ErrorCodes.ValidationFailed);

        // Act
        NameServersValidator.Validate(nameServersEnabled, nameServers, validationResult);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Entries.Count.Should().Be(1);
        validationResult.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public void ShouldAddValidationError_WhenIPv4LinkLocalServersFound()
    {
        // Arrange
        var linkLocalIPAddress = IPAddress.Parse("169.254.0.1");
        var nameServersEnabled = true;
        var nameServers = new List<IPAddress>()
        {
            IPAddress.Parse("1.1.1.1"),
            linkLocalIPAddress
        };
        var validationResult = new ValidationResult();

        var expectedValidationEntry = new ValidationEntry(
            $"Name server '{linkLocalIPAddress}' can not be a link-local address " +
            $"('{IPAddressValidatorUtils.IPv4LinkLocalNetworkCIDR}').",
            ErrorCodes.ValidationFailed);

        // Act
        NameServersValidator.Validate(nameServersEnabled, nameServers, validationResult);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Entries.Count.Should().Be(1);
        validationResult.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public void ShouldAddValidationError_WhenIPv6LinkLocalServersFound()
    {
        // Arrange
        var linkLocalIPAddress = IPAddress.Parse("fe80::1");
        var nameServersEnabled = true;
        var nameServers = new List<IPAddress>()
        {
            IPAddress.Parse("2606:4700:4700::1111"),
            linkLocalIPAddress
        };
        var validationResult = new ValidationResult();

        var expectedValidationEntry = new ValidationEntry(
            $"Name server '{linkLocalIPAddress}' can not be a link-local address " +
            $"('{IPAddressValidatorUtils.IPv6LinkLocalNetworkCIDR}').",
            ErrorCodes.ValidationFailed);

        // Act
        NameServersValidator.Validate(nameServersEnabled, nameServers, validationResult);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Entries.Count.Should().Be(1);
        validationResult.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public void ShouldAddValidationError_WhenIPv4MulticastServersFound()
    {
        // Arrange
        var multicastIPAddress = IPAddress.Parse("224.0.0.1");
        var nameServersEnabled = true;
        var nameServers = new List<IPAddress>()
        {
            IPAddress.Parse("1.1.1.1"),
            multicastIPAddress
        };
        var validationResult = new ValidationResult();

        var expectedValidationEntry = new ValidationEntry(
            $"Name server '{multicastIPAddress}' can not be a multicast address " +
            $"('{IPAddressValidatorUtils.IPv4MulticastNetworkCIDR}').",
            ErrorCodes.ValidationFailed);

        // Act
        NameServersValidator.Validate(nameServersEnabled, nameServers, validationResult);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Entries.Count.Should().Be(1);
        validationResult.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public void ShouldAddValidationError_WhenIPv6MulticastServersFound()
    {
        // Arrange
        var multicastIPAddress = IPAddress.Parse("ff00::1");
        var nameServersEnabled = true;
        var nameServers = new List<IPAddress>()
        {
            IPAddress.Parse("2606:4700:4700::1111"),
            multicastIPAddress
        };
        var validationResult = new ValidationResult();

        var expectedValidationEntry = new ValidationEntry(
            $"Name server '{multicastIPAddress}' can not be a multicast address " +
            $"('{IPAddressValidatorUtils.IPv6MulticastNetworkCIDR}').",
            ErrorCodes.ValidationFailed);

        // Act
        NameServersValidator.Validate(nameServersEnabled, nameServers, validationResult);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Entries.Count.Should().Be(1);
        validationResult.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public void ShouldAddValidationError_WhenIPv4BroadcastServersFound()
    {
        // Arrange
        var broadcastIPAddress = IPAddress.Parse("255.255.255.255");
        var nameServersEnabled = true;
        var nameServers = new List<IPAddress>()
        {
            IPAddress.Parse("1.1.1.1"),
            broadcastIPAddress
        };
        var validationResult = new ValidationResult();

        var expectedValidationEntry = new ValidationEntry(
            $"Name server '{broadcastIPAddress}' can not be a broadcast address " +
            $"('{IPAddressValidatorUtils.IPv4BroadcastNetworkCIDR}').",
            ErrorCodes.ValidationFailed);

        // Act
        NameServersValidator.Validate(nameServersEnabled, nameServers, validationResult);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Entries.Count.Should().Be(1);
        validationResult.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public void ShouldHaveNoValidationError_WhenNameServersValid()
    {
        // Arrange
        var nameServersEnabled = true;
        var nameServers = new List<IPAddress>()
        {
            IPAddress.Parse("1.1.1.1"),
            IPAddress.Parse("2606:4700:4700::1111"),
            IPAddress.Parse("8.8.8.8"),
            IPAddress.Parse("2001:4860:4860::8888"),
        };
        var validationResult = new ValidationResult();

        // Act
        NameServersValidator.Validate(nameServersEnabled, nameServers, validationResult);

        // Assert
        validationResult.IsValid.Should().BeTrue();
        validationResult.Entries.Count.Should().Be(0);
    }
}
