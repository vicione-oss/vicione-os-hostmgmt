using System.Net;
using FluentAssertions;
using Xunit;
using HostManagement.Shared.Validation.Validators;
using HostManagement.Shared.Validation;
using HostManagement.Shared.Validation.Utils;

namespace HostManagement.Shared.Tests.Validators;

public class NetworkNTPSettingsValidatorTests
{
    public class HasNonUniqueNTPServer
    {
        [Fact]
        public void ShouldAddErrorMessage_WhenNonUniqueNTPServersFound()
        {
            // Arrange
            var nonUniqueNTPServer = "ptbtime1.ptb.de";
            var ntpServers = new List<string>
            {
                nonUniqueNTPServer,
                nonUniqueNTPServer,
                "ptbtime2.ptb.de"
            };

            var validationResult = new ValidationResult();
            var expectedValidationEntry = new ValidationEntry(
                $"NTP servers are not unique. Affected servers are: '{nonUniqueNTPServer}'.",
                ErrorCodes.ValidationFailed);

            // Act
            var result = NetworkNTPSettingsValidator
                .HasNonUniqueNTPServer(ntpServers, validationResult);

            // Assert
            result.Should().BeTrue();
            validationResult.IsValid.Should().BeFalse();
            validationResult.Entries.Count.Should().Be(1);
            validationResult.Entries.Should().Contain(expectedValidationEntry);
        }

        [Fact]
        public void ShouldReturnFalse_WhenNTPServerUnique()
        {
            // Arrange
            var ntpServers = new List<string>
            {
                "ptbtime1.ptb.de",
                "ptbtime2.ptb.de"
            };

            // Act
            var validationResult = new ValidationResult();
            var result = NetworkNTPSettingsValidator
                .HasNonUniqueNTPServer(ntpServers, validationResult);

            // Assert
            result.Should().BeFalse();
            validationResult.IsValid.Should().BeTrue();
            validationResult.Entries.Count.Should().Be(0);
        }
    }

    public class ValidateNTPServer
    {
        [Fact]
        public void ShouldAddValidationError_WhenPotentialIPv4AddressNotValid()
        {
            // Arrange
            var ntpServer = "192.168.444.333";
            var validationResult = new ValidationResult();
            var expectedValidationEntry = new ValidationEntry(
                $"NTP server '{ntpServer}' is no valid IPv4 address.",
                ErrorCodes.ValidationFailed);

            // Act
            NetworkNTPSettingsValidator.ValidateNTPServer(ntpServer, validationResult);

            // Assert
            validationResult.IsValid.Should().BeFalse();
            validationResult.Entries.Count.Should().Be(1);
            validationResult.Entries.Should().Contain(expectedValidationEntry);
        }
    }

    public class IsNTPServerEmpty
    {
        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void ShouldAddValidationError_WhenNTPServerIsEmptyOrWhitespace(string ntpServer)
        {
            // Arrange
            var validationResult = new ValidationResult();
            var expectedValidationEntry = new ValidationEntry(
                "NTP server can not be empty or whitespace.",
                ErrorCodes.ValidationFailed);

            // Act
            var result = NetworkNTPSettingsValidator.IsNTPServerEmpty(ntpServer, validationResult);

            // Assert
            result.Should().BeTrue();
            validationResult.IsValid.Should().BeFalse();
            validationResult.Entries.Count.Should().Be(1);
            validationResult.Entries.Should().Contain(expectedValidationEntry);
        }

        [Fact]
        public void ShouldReturnFalse_WhenNTPServerNonEmpty()
        {
            // Act
            var validationResult = new ValidationResult();
            var result = NetworkNTPSettingsValidator.IsNTPServerEmpty("ptbtime1.ptb.de", validationResult);

            // Assert
            result.Should().BeFalse();
            validationResult.IsValid.Should().BeTrue();
            validationResult.Entries.Count.Should().Be(0);
        }
    }

    public class IsPotentialIPv4Address
    {
        [Theory]
        [InlineData("192.168.42")]
        [InlineData("time.google.com")]
        public void ShouldReturnFalse_WhenPotentialIPv4AddressHasNotFourParts(string potentialIPAddress)
        {
            // Act
            var result = NetworkNTPSettingsValidator.IsPotentialIPv4Address(potentialIPAddress);

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData("192.168.invalid.42")]
        [InlineData("time.google.com")]
        public void ShouldReturnFalse_WhenPotentialIPv4AddressHasNoDigits(string potentialIPAddress)
        {
            // Act
            var result = NetworkNTPSettingsValidator.IsPotentialIPv4Address(potentialIPAddress);

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData("192.168.42.21")]
        [InlineData("192.168.444.333")]
        public void ShouldReturnTrue_WhenPotentialIPv4Address(string potentialIPAddress)
        {
            // Act
            var result = NetworkNTPSettingsValidator.IsPotentialIPv4Address(potentialIPAddress);

            // Assert
            result.Should().BeTrue();
        }
    }

    public class IsInvalidIPAddress
    {
        [Fact]
        public void ShouldAddValidationError_WhenIPv4LinkLocalAddressFound()
        {
            // Arrange
            var linkLocalIPAddress = IPAddress.Parse("169.254.255.254");
            var validationResult = new ValidationResult();
            var expectedValidationEntry = new ValidationEntry(
                $"NTP server '{linkLocalIPAddress}' can not be a link-local address " +
                $"('{IPAddressValidatorUtils.IPv4LinkLocalNetworkCIDR}').",
                ErrorCodes.ValidationFailed);

            // Act
            var result = NetworkNTPSettingsValidator.IsInvalidIPAddress(linkLocalIPAddress, validationResult);

            // Assert
            result.Should().BeTrue();
            validationResult.IsValid.Should().BeFalse();
            validationResult.Entries.Count.Should().Be(1);
            validationResult.Entries.Should().Contain(expectedValidationEntry);
        }

        [Fact]
        public void ShouldAddValidationError_WhenIPv4MulticastAddressFound()
        {
            // Arrange
            var multicastIPAddress = IPAddress.Parse("239.255.255.254");
            var validationResult = new ValidationResult();
            var expectedValidationEntry = new ValidationEntry(
                $"NTP server '{multicastIPAddress}' can not be a multicast address " +
                $"('{IPAddressValidatorUtils.IPv4MulticastNetworkCIDR}').",
                ErrorCodes.ValidationFailed);

            // Act
            var result = NetworkNTPSettingsValidator.IsInvalidIPAddress(multicastIPAddress, validationResult);

            // Assert
            result.Should().BeTrue();
            validationResult.IsValid.Should().BeFalse();
            validationResult.Entries.Count.Should().Be(1);
            validationResult.Entries.Should().Contain(expectedValidationEntry);
        }

        [Fact]
        public void ShouldAddValidationError_WhenIPv6LinkLocalAddressFound()
        {
            // Arrange
            var linkLocalIPAddress = IPAddress.Parse("fe80::1");
            var validationResult = new ValidationResult();
            var expectedValidationEntry = new ValidationEntry(
                $"NTP server '{linkLocalIPAddress}' can not be a link-local address " +
                $"('{IPAddressValidatorUtils.IPv6LinkLocalNetworkCIDR}').",
                ErrorCodes.ValidationFailed);

            // Act
            var result = NetworkNTPSettingsValidator.IsInvalidIPAddress(linkLocalIPAddress, validationResult);

            // Assert
            result.Should().BeTrue();
            validationResult.IsValid.Should().BeFalse();
            validationResult.Entries.Count.Should().Be(1);
            validationResult.Entries.Should().Contain(expectedValidationEntry);
        }

        [Fact]
        public void ShouldAddValidationError_WhenIPv6MulticastAddressFound()
        {
            // Arrange
            var multicastIPAddress = IPAddress.Parse("ff00::1");
            var validationResult = new ValidationResult();
            var expectedValidationEntry = new ValidationEntry(
                $"NTP server '{multicastIPAddress}' can not be a multicast address " +
                $"('{IPAddressValidatorUtils.IPv6MulticastNetworkCIDR}').",
                ErrorCodes.ValidationFailed);

            // Act
            var result = NetworkNTPSettingsValidator.IsInvalidIPAddress(multicastIPAddress, validationResult);

            // Assert
            result.Should().BeTrue();
            validationResult.IsValid.Should().BeFalse();
            validationResult.Entries.Count.Should().Be(1);
            validationResult.Entries.Should().Contain(expectedValidationEntry);
        }

        [Theory]
        [InlineData("2a01:4f8:131::123")]
        [InlineData("2001:4860:4806::123")]
        [InlineData("192.53.103.108")]
        public void ShouldReturnFalse_WhenIPAddressValid(string ipAddressString)
        {
            // Arrange
            var ipAddress = IPAddress.Parse(ipAddressString);

            // Act
            var validationResult = new ValidationResult();
            var result = NetworkNTPSettingsValidator.IsInvalidIPAddress(ipAddress, validationResult);

            // Assert
            result.Should().BeFalse();
            validationResult.IsValid.Should().BeTrue();
            validationResult.Entries.Count.Should().Be(0);
        }
    }

    public class IsInvalidFullQualifiedDomainName
    {
        [Fact]
        public void ShouldAddErrorMessage_WhenNTPServerDomainNameHasTooManyCharacters()
        {
            // Arrange
            var ntpServer = string.Concat("my-domain.", new string('x', 241), ".de");
            var validationResult = new ValidationResult();
            var expectedValidationEntry = new ValidationEntry(
                $"NTP server '{ntpServer}' has too many characters " +
                $"(expected max. 253 characters including dots).",
                ErrorCodes.ValidationFailed);

            // Act
            NetworkNTPSettingsValidator.ValidateNTPServer(ntpServer, validationResult);

            // Assert
            validationResult.IsValid.Should().BeFalse();
            validationResult.Entries.Count.Should().Be(1);
            validationResult.Entries.Should().Contain(expectedValidationEntry);
        }

        [Fact]
        public void ShouldAddErrorMessage_WhenNTPServerDomainNameHasLeadingOrTrailingDots()
        {
            // Arrange
            var ntpServer = ".ptbtime1.ptb.de";
            var validationResult = new ValidationResult();
            var expectedValidationEntry = new ValidationEntry(
                $"NTP server '{ntpServer}' can not have leading or trailing dots.",
                ErrorCodes.ValidationFailed);

            // Act
            NetworkNTPSettingsValidator.ValidateNTPServer(ntpServer, validationResult);

            // Assert
            validationResult.IsValid.Should().BeFalse();
            validationResult.Entries.Count.Should().Be(1);
            validationResult.Entries.Should().Contain(expectedValidationEntry);
        }

        [Fact]
        public void ShouldAddErrorMessage_WhenNTPServerDomainNameHasConsecutiveDots()
        {
            // Arrange
            var ntpServer = "ptbtime1..de";
            var validationResult = new ValidationResult();
            var expectedValidationEntry = new ValidationEntry(
                $"NTP server '{ntpServer}' can not have consecutive dots.",
                ErrorCodes.ValidationFailed);

            // Act
            NetworkNTPSettingsValidator.ValidateNTPServer(ntpServer, validationResult);

            // Assert
            validationResult.IsValid.Should().BeFalse();
            validationResult.Entries.Count.Should().Be(1);
            validationResult.Entries.Should().Contain(expectedValidationEntry);
        }

        [Fact]
        public void ShouldAddErrorMessage_WhenNTPServerHasLessThanTwoDomainLabels()
        {
            // Arrange
            var ntpServer = "ptbtime1";
            var validationResult = new ValidationResult();
            var expectedValidationEntry = new ValidationEntry(
                $"NTP server '{ntpServer}' must have at least two domain labels (e.g. \"example.com\").",
                ErrorCodes.ValidationFailed);

            // Act
            NetworkNTPSettingsValidator.ValidateNTPServer(ntpServer, validationResult);

            // Assert
            validationResult.IsValid.Should().BeFalse();
            validationResult.Entries.Count.Should().Be(1);
            validationResult.Entries.Should().Contain(expectedValidationEntry);
        }

        [Fact]
        public void ShouldAddErrorMessage_WhenDomainNameHasMoreThanOneNumericLabel()
        {
            // Arrange
            var ntpServer = "ptbtime.123.456.de";
            var validationResult = new ValidationResult();
            var expectedValidationEntry = new ValidationEntry(
                $"NTP server '{ntpServer}' can not have more than one numerical label.",
                ErrorCodes.ValidationFailed);

            // Act
            NetworkNTPSettingsValidator.ValidateNTPServer(ntpServer, validationResult);

            // Assert
            validationResult.IsValid.Should().BeFalse();
            validationResult.Entries.Count.Should().Be(1);
            validationResult.Entries.Should().Contain(expectedValidationEntry);
        }

        [Fact]
        public void ShouldAddErrorMessage_WhenTopLevelDomainHasInvalidCharacterCount()
        {
            // Arrange
            var ntpServer = "ptbtime1.ptb.d";
            var validationResult = new ValidationResult();
            var expectedValidationEntry = new ValidationEntry(
                $"NTP server '{ntpServer}' has a top-level domain with an invalid character count " +
                $"(expected 2 to 63 characters).",
                ErrorCodes.ValidationFailed);

            // Act
            NetworkNTPSettingsValidator.ValidateNTPServer(ntpServer, validationResult);

            // Assert
            validationResult.IsValid.Should().BeFalse();
            validationResult.Entries.Count.Should().Be(1);
            validationResult.Entries.Should().Contain(expectedValidationEntry);
        }

        [Fact]
        public void ShouldAddErrorMessage_WhenDomainLabelHasInvalidCharacterCount()
        {
            // Arrange
            var invalidDomainLabel = new string('x', 64);
            var ntpServer = string.Concat("ptbtime1.", invalidDomainLabel, ".de");
            var validationResult = new ValidationResult();
            var expectedValidationEntry = new ValidationEntry(
                $"NTP server '{ntpServer}' has a domain label with an invalid character count " +
                $"(expected 1 to 63 characters). Affected domain label is: '{invalidDomainLabel}'.",
                ErrorCodes.ValidationFailed);

            // Act
            NetworkNTPSettingsValidator.ValidateNTPServer(ntpServer, validationResult);

            // Assert
            validationResult.IsValid.Should().BeFalse();
            validationResult.Entries.Count.Should().Be(1);
            validationResult.Entries.Should().Contain(expectedValidationEntry);
        }

        [Fact]
        public void ShouldAddErrorMessage_WhenDomainLabelHasInvalidCharacters()
        {
            // Arrange
            var invalidDomainLabel = "ptb?";
            var ntpServer = string.Concat("ptbtime1.", invalidDomainLabel, ".de");
            var validationResult = new ValidationResult();
            var expectedValidationEntry = new ValidationEntry(
                $"NTP server '{ntpServer}' has a domain label with an invalid character " +
                $"(expected start/end character to be letter or digit; other characters can " +
                $"also be a hyphen). Affected domain label is: '{invalidDomainLabel}'.",
                ErrorCodes.ValidationFailed);

            // Act
            NetworkNTPSettingsValidator.ValidateNTPServer(ntpServer, validationResult);

            // Assert
            validationResult.IsValid.Should().BeFalse();
            validationResult.Entries.Count.Should().Be(1);
            validationResult.Entries.Should().Contain(expectedValidationEntry);
        }

        [Fact]
        public void ShouldReturnFalse_WhenFullQualifiedDomainNameValid()
        {
            // Act
            var validationResult = new ValidationResult();
            NetworkNTPSettingsValidator.ValidateNTPServer("ptbtime1.ptb.de", validationResult);

            // Assert
            validationResult.IsValid.Should().BeTrue();
            validationResult.Entries.Count.Should().Be(0);
        }
    }
}
