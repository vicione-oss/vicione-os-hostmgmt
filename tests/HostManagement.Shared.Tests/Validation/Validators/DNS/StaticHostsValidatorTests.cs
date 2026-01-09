using System.Net;
using FluentAssertions;
using Xunit;
using HostManagement.Shared.Validation;
using HostManagement.Shared.Validation.Validators.DNS;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Validation.Utils;

namespace HostManagement.Shared.Tests.Validation.Validators.DNS;

public class StaticHostsValidatorTests
{
    [Fact]
    public void ShouldHaveNoValidationError_WhenStaticHostsDisabled()
    {
        // Arrange
        var staticHostsEnabled = false;
        var staticHosts = new List<StaticHostDetail>()
        {
            new() { IPAddress = IPAddress.Parse("172.16.0.42"), Hostname = "Device42" },
            new() { IPAddress = IPAddress.Parse("172.16.0.42"), Hostname = "Device.42.Too" },
            new() { IPAddress = IPAddress.Parse("172.16.0.43"), Hostname = "Device43" },
            new() { IPAddress = IPAddress.Parse("172.16.0.44"), Hostname = "Device44" }
        };
        var validationResult = new ValidationResult();

        // Act
        StaticHostsValidator.Validate(staticHostsEnabled, staticHosts, validationResult);

        // Assert
        validationResult.IsValid.Should().BeTrue();
        validationResult.Entries.Count.Should().Be(0);
    }

    [Fact]
    public void ShouldAddValidationError_WhenDuplicateStaticHostsFound()
    {
        // Arrange
        var duplicateIP = IPAddress.Parse("172.16.0.42");
        var duplicateHost = "Device42";
        var staticHostsEnabled = true;
        var staticHosts = new List<StaticHostDetail>()
        {
            new() { IPAddress = duplicateIP, Hostname = duplicateHost },
            new() { IPAddress = IPAddress.Parse("172.16.0.43"), Hostname = "Device43" },
            new() { IPAddress = duplicateIP, Hostname = duplicateHost }
        };
        var validationResult = new ValidationResult();

        var expectedValidationEntry = new ValidationEntry(
            $"Static hosts do not have unique entries. " +
            $"Affected entries are: '{duplicateIP}'  '{duplicateHost}'.",
            ErrorCodes.ValidationFailed);

        // Act
        StaticHostsValidator.Validate(staticHostsEnabled, staticHosts, validationResult);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Entries.Count.Should().Be(1);
        validationResult.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public void ShouldAddValidationError_WhenIPv4LoopbackAddressFound()
    {
        // Arrange
        var loopbackIP = IPAddress.Parse("127.0.0.0");
        var staticHostsEnabled = true;
        var staticHosts = new List<StaticHostDetail>()
        {
            new() { IPAddress = IPAddress.Parse("172.16.0.42"), Hostname = "Device42" },
            new() { IPAddress = IPAddress.Parse("172.16.0.42"), Hostname = "Device.42.Too" },
            new() { IPAddress = loopbackIP, Hostname = "Device43" },
            new() { IPAddress = IPAddress.Parse("172.16.0.44"), Hostname = "Device44" }
        };
        var validationResult = new ValidationResult();

        var expectedValidationEntry = new ValidationEntry(
            $"Static host IP address '{loopbackIP}' can not be a loopback address " +
            $"('{IPAddressValidatorUtils.IPv4LoopbackNetworkCIDR}').",
            ErrorCodes.ValidationFailed);

        // Act
        StaticHostsValidator.Validate(staticHostsEnabled, staticHosts, validationResult);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Entries.Count.Should().Be(1);
        validationResult.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public void ShouldAddValidationError_WhenIPv6LoopbackAddressFound()
    {
        // Arrange
        var loopbackIP = IPAddress.Parse("::1");
        var staticHostsEnabled = true;
        var staticHosts = new List<StaticHostDetail>()
        {
            new() { IPAddress = IPAddress.Parse("172.16.0.42"), Hostname = "Device42" },
            new() { IPAddress = IPAddress.Parse("172.16.0.42"), Hostname = "Device.42.Too" },
            new() { IPAddress = IPAddress.Parse("fd12:4356:789a:1::1"), Hostname = "Device43" },
            new() { IPAddress = loopbackIP, Hostname = "Device44" }
        };
        var validationResult = new ValidationResult();

        var expectedValidationEntry = new ValidationEntry(
            $"Static host IP address '{loopbackIP}' can not be a loopback address " +
            $"('{IPAddressValidatorUtils.IPv6LoopbackNetworkCIDR}').",
            ErrorCodes.ValidationFailed);

        // Act
        StaticHostsValidator.Validate(staticHostsEnabled, staticHosts, validationResult);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Entries.Count.Should().Be(1);
        validationResult.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public void ShouldAddValidationError_WhenIPv4LinkLocalAddressFound()
    {
        // Arrange
        var linkLocalIP = IPAddress.Parse("169.254.0.0");
        var staticHostsEnabled = true;
        var staticHosts = new List<StaticHostDetail>()
        {
            new() { IPAddress = IPAddress.Parse("172.16.0.42"), Hostname = "Device42" },
            new() { IPAddress = IPAddress.Parse("172.16.0.42"), Hostname = "Device.42.Too" },
            new() { IPAddress = IPAddress.Parse("fd12:4356:789a:1::1"), Hostname = "Device43" },
            new() { IPAddress = linkLocalIP, Hostname = "Device44" }
        };
        var validationResult = new ValidationResult();

        var expectedValidationEntry = new ValidationEntry(
            $"Static host IP address '{linkLocalIP}' can not be a link-local address " +
            $"('{IPAddressValidatorUtils.IPv4LinkLocalNetworkCIDR}').",
            ErrorCodes.ValidationFailed);

        // Act
        StaticHostsValidator.Validate(staticHostsEnabled, staticHosts, validationResult);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Entries.Count.Should().Be(1);
        validationResult.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public void ShouldAddValidationError_WhenIPv6LinkLocalAddressFound()
    {
        // Arrange
        var linkLocalIP = IPAddress.Parse("fe80::");
        var staticHostsEnabled = true;
        var staticHosts = new List<StaticHostDetail>()
        {
            new() { IPAddress = IPAddress.Parse("172.16.0.42"), Hostname = "Device42" },
            new() { IPAddress = IPAddress.Parse("172.16.0.42"), Hostname = "Device.42.Too" },
            new() { IPAddress = IPAddress.Parse("fd12:4356:789a:1::1"), Hostname = "Device43" },
            new() { IPAddress = linkLocalIP, Hostname = "Device44" }
        };
        var validationResult = new ValidationResult();

        var expectedValidationEntry = new ValidationEntry(
            $"Static host IP address '{linkLocalIP}' can not be a link-local address " +
            $"('{IPAddressValidatorUtils.IPv6LinkLocalNetworkCIDR}').",
            ErrorCodes.ValidationFailed);

        // Act
        StaticHostsValidator.Validate(staticHostsEnabled, staticHosts, validationResult);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Entries.Count.Should().Be(1);
        validationResult.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public void ShouldAddValidationError_WhenIPv4MulticastAddressFound()
    {
        // Arrange
        var multicasIP = IPAddress.Parse("224.0.0.0");
        var staticHostsEnabled = true;
        var staticHosts = new List<StaticHostDetail>()
        {
            new() { IPAddress = IPAddress.Parse("172.16.0.42"), Hostname = "Device42" },
            new() { IPAddress = IPAddress.Parse("172.16.0.42"), Hostname = "Device.42.Too" },
            new() { IPAddress = IPAddress.Parse("fd12:4356:789a:1::1"), Hostname = "Device43" },
            new() { IPAddress = multicasIP, Hostname = "Device44" }
        };
        var validationResult = new ValidationResult();

        var expectedValidationEntry = new ValidationEntry(
            $"Static host IP address '{multicasIP}' can not be a multicast address " +
            $"('{IPAddressValidatorUtils.IPv4MulticastNetworkCIDR}').",
            ErrorCodes.ValidationFailed);

        // Act
        StaticHostsValidator.Validate(staticHostsEnabled, staticHosts, validationResult);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Entries.Count.Should().Be(1);
        validationResult.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public void ShouldAddValidationError_WhenIPv6MulticastAddressFound()
    {
        // Arrange
        var multicasIP = IPAddress.Parse("ff00::");
        var staticHostsEnabled = true;
        var staticHosts = new List<StaticHostDetail>()
        {
            new() { IPAddress = IPAddress.Parse("172.16.0.42"), Hostname = "Device42" },
            new() { IPAddress = IPAddress.Parse("172.16.0.42"), Hostname = "Device.42.Too" },
            new() { IPAddress = IPAddress.Parse("fd12:4356:789a:1::1"), Hostname = "Device43" },
            new() { IPAddress = multicasIP, Hostname = "Device44" }
        };
        var validationResult = new ValidationResult();

        var expectedValidationEntry = new ValidationEntry(
            $"Static host IP address '{multicasIP}' can not be a multicast address " +
            $"('{IPAddressValidatorUtils.IPv6MulticastNetworkCIDR}').",
            ErrorCodes.ValidationFailed);

        // Act
        StaticHostsValidator.Validate(staticHostsEnabled, staticHosts, validationResult);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Entries.Count.Should().Be(1);
        validationResult.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public void ShouldAddValidationError_WhenIPv4BroadcastAddressFound()
    {
        // Arrange
        var braodcastIP = IPAddress.Parse("255.255.255.255");
        var staticHostsEnabled = true;
        var staticHosts = new List<StaticHostDetail>()
        {
            new() { IPAddress = IPAddress.Parse("172.16.0.42"), Hostname = "Device42" },
            new() { IPAddress = IPAddress.Parse("172.16.0.42"), Hostname = "Device.42.Too" },
            new() { IPAddress = IPAddress.Parse("fd12:4356:789a:1::1"), Hostname = "Device43" },
            new() { IPAddress = braodcastIP, Hostname = "Device44" }
        };
        var validationResult = new ValidationResult();

        var expectedValidationEntry = new ValidationEntry(
            $"Static host IP address '{braodcastIP}' can not be a broadcast address " +
            $"('{IPAddressValidatorUtils.IPv4BroadcastNetworkCIDR}').",
            ErrorCodes.ValidationFailed);

        // Act
        StaticHostsValidator.Validate(staticHostsEnabled, staticHosts, validationResult);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Entries.Count.Should().Be(1);
        validationResult.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public void ShouldHaveNoValidationError_WhenStaticHostsValid()
    {
        // Arrange
        var staticHostsEnabled = true;
        var staticHosts = new List<StaticHostDetail>()
        {
            new() { IPAddress = IPAddress.Parse("172.16.0.42"), Hostname = "Device42" },
            new() { IPAddress = IPAddress.Parse("172.16.0.42"), Hostname = "Device.42.Too" },
            new() { IPAddress = IPAddress.Parse("fd12:4356:789a:1::1"), Hostname = "Device43" },
            new() { IPAddress = IPAddress.Parse("172.16.0.44"), Hostname = "Device44" }
        };
        var validationResult = new ValidationResult();

        // Act
        StaticHostsValidator.Validate(staticHostsEnabled, staticHosts, validationResult);

        // Assert
        validationResult.IsValid.Should().BeTrue();
        validationResult.Entries.Count.Should().Be(0);
    }
}
