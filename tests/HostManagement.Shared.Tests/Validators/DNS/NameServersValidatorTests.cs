using FluentAssertions;
using Xunit;
using System.Net;
using HostManagement.Shared.Validation;
using HostManagement.Shared.Validation.Utils;
using HostManagement.Shared.Validation.Validators.DNS;

namespace HostManagement.Shared.Tests.Validators.DNS;

public class NameServersValidatorTests
{
    public class HasLinkLocalNameServers
    {
        [Fact]
        public void ShouldAddValidationError_WhenIPv4LinkLocalServersFound()
        {
            // Arrange
            var linkLocalIPAddress = "169.254.0.1";
            var nameServers = new List<IPAddress>()
            {
                IPAddress.Parse("1.1.1.1"),
                IPAddress.Parse(linkLocalIPAddress)
            };

            var validationResult = new ValidationResult();
            var expectedValidationEntry = new ValidationEntry(
                $"Name server '{linkLocalIPAddress}' can not be a link-local address " +
                $"('{IPAddressValidatorUtils.IPv4LinkLocalNetworkCIDR}').",
                ErrorCodes.ValidationFailed);

            // Act
            NameServersValidator.HasLinkLocalNameServers(nameServers, validationResult);

            // Assert
            validationResult.IsValid.Should().BeFalse();
            validationResult.Entries.Count.Should().Be(1);
            validationResult.Entries.Should().Contain(expectedValidationEntry);
        }

        [Fact]
        public void ShouldAddValidationError_WhenIPv6LinkLocalServersFound()
        {
            // Arrange
            var linkLocalIPAddress = "fe80::1";
            var nameServers = new List<IPAddress>()
            {
                IPAddress.Parse("2606:4700:4700::1111"),
                IPAddress.Parse(linkLocalIPAddress)
            };

            var validationResult = new ValidationResult();
            var expectedValidationEntry = new ValidationEntry(
                $"Name server '{linkLocalIPAddress}' can not be a link-local address " +
                $"('{IPAddressValidatorUtils.IPv6LinkLocalNetworkCIDR}').",
                ErrorCodes.ValidationFailed);

            // Act
            NameServersValidator.HasLinkLocalNameServers(nameServers, validationResult);

            // Assert
            validationResult.IsValid.Should().BeFalse();
            validationResult.Entries.Count.Should().Be(1);
            validationResult.Entries.Should().Contain(expectedValidationEntry);
        }

        [Fact]
        public void ShouldHaveNoValidationError_WhenNameServersValid()
        {
            // Arrange
            var nameServers = new List<IPAddress>()
            {
                IPAddress.Parse("1.1.1.1"),
                IPAddress.Parse("2606:4700:4700::1111"),
                IPAddress.Parse("8.8.8.8"),
                IPAddress.Parse("2001:4860:4860::8888"),
            };

            var validationResult = new ValidationResult();

            // Act
            NameServersValidator.HasLinkLocalNameServers(nameServers, validationResult);

            // Assert
            validationResult.IsValid.Should().BeTrue();
            validationResult.Entries.Count.Should().Be(0);
        }
    }

    public class HasMulticastNameServers
    {
        [Fact]
        public void ShouldAddValidationError_WhenIPv4MulticastServersFound()
        {
            // Arrange
            var multicastIPAddress = "224.0.0.1";
            var nameServers = new List<IPAddress>()
            {
                IPAddress.Parse("1.1.1.1"),
                IPAddress.Parse(multicastIPAddress)
            };

            var validationResult = new ValidationResult();
            var expectedValidationEntry = new ValidationEntry(
                $"Name server '{multicastIPAddress}' can not be a multicast address " +
                $"('{IPAddressValidatorUtils.IPv4MulticastNetworkCIDR}').",
                ErrorCodes.ValidationFailed);

            // Act
            NameServersValidator.HasMulticastNameServers(nameServers, validationResult);

            // Assert
            validationResult.IsValid.Should().BeFalse();
            validationResult.Entries.Count.Should().Be(1);
            validationResult.Entries.Should().Contain(expectedValidationEntry);
        }

        [Fact]
        public void ShouldAddValidationError_WhenIPv6MulticastServersFound()
        {
            // Arrange
            var multicastIPAddress = "ff00::1";
            var nameServers = new List<IPAddress>()
            {
                IPAddress.Parse("2606:4700:4700::1111"),
                IPAddress.Parse(multicastIPAddress)
            };

            var validationResult = new ValidationResult();
            var expectedValidationEntry = new ValidationEntry(
                $"Name server '{multicastIPAddress}' can not be a multicast address " +
                $"('{IPAddressValidatorUtils.IPv6MulticastNetworkCIDR}').",
                ErrorCodes.ValidationFailed);

            // Act
            NameServersValidator.HasMulticastNameServers(nameServers, validationResult);

            // Assert
            validationResult.IsValid.Should().BeFalse();
            validationResult.Entries.Count.Should().Be(1);
            validationResult.Entries.Should().Contain(expectedValidationEntry);
        }

        [Fact]
        public void ShouldHaveNoValidationError_WhenNameServersValid()
        {
            // Arrange
            var nameServers = new List<IPAddress>()
            {
                IPAddress.Parse("1.1.1.1"),
                IPAddress.Parse("2606:4700:4700::1111"),
                IPAddress.Parse("8.8.8.8"),
                IPAddress.Parse("2001:4860:4860::8888"),
            };

            var validationResult = new ValidationResult();

            // Act
            NameServersValidator.HasMulticastNameServers(nameServers, validationResult);

            // Assert
            validationResult.IsValid.Should().BeTrue();
            validationResult.Entries.Count.Should().Be(0);
        }
    }

    public class HasNonUniqueNameServers
    {
        [Fact]
        public void ShouldAddValidationError_WhenNonUniqueNameServersFound()
        {
            // Arrange
            var nameServers = new List<IPAddress>()
            {
                IPAddress.Parse("1.1.1.1"),
                IPAddress.Parse("2606:4700:4700::1111"),
                IPAddress.Parse("1.1.1.1"),
                IPAddress.Parse("2001:4860:4860::8888"),
            };

            var validationResult = new ValidationResult();

            var expectedValidationEntry = new ValidationEntry(
                "Name servers are not unique. Affected IP addresses are: '1.1.1.1'.",
                ErrorCodes.ValidationFailed);

            // Act
            NameServersValidator.HasNonUniqueNameServers(nameServers, validationResult);

            // Assert
            validationResult.IsValid.Should().BeFalse();
            validationResult.Entries.Count.Should().Be(1);
            validationResult.Entries.Should().Contain(expectedValidationEntry);
        }

        [Fact]
        public void ShouldHaveNoValidationError_WhenNameServersValid()
        {
            // Arrange
            var nameServers = new List<IPAddress>()
            {
                IPAddress.Parse("1.1.1.1"),
                IPAddress.Parse("2606:4700:4700::1111"),
                IPAddress.Parse("8.8.8.8"),
                IPAddress.Parse("2001:4860:4860::8888"),
            };

            var validationResult = new ValidationResult();

            // Act
            NameServersValidator.HasNonUniqueNameServers(nameServers, validationResult);

            // Assert
            validationResult.IsValid.Should().BeTrue();
            validationResult.Entries.Count.Should().Be(0);
        }
    }
}
