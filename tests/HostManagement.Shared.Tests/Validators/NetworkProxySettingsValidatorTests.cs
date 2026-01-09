using FluentAssertions;
using Xunit;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Validation.Validators;
using HostManagement.Shared.Validation;
using System.Globalization;

namespace HostManagement.Shared.Tests.Validators;

public class NetworkProxySettingsValidatorTests
{
    public class IsProxyUnconfigured
    {
        [Fact]
        public void ShouldReturnFalse_WhenProxyConfigured()
        {
            // Arrange
            var networkProxyDetail = new NetworkProxyDetail()
            {
                Enabled = false,
                Server = "http://test.de",
                Port = 8080
            };

            // Act
            var result = NetworkProxySettingsValidator.IsProxyUnconfigured(networkProxyDetail);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnTrue_WhenProxyUnconfigured()
        {
            // Arrange
            var unconfiguredNetworkProxyDetail = NetworkProxyDetail.Empty;

            // Act
            var result = NetworkProxySettingsValidator.IsProxyUnconfigured(unconfiguredNetworkProxyDetail);

            // Assert
            result.Should().BeTrue();
        }
    }

    public class ValidateServer
    {
        [Fact]
        public void ShouldAddValidationError_WhenServerNull()
        {
            // Arrange
            string[] transportProtocol = ["http"];
            string? server = null;
            var validationResult = new ValidationResult();

            var expectedValidationEntry = new ValidationEntry(
                "Proxy server can not be null.",
                ErrorCodes.ValidationFailed);

            // Act
            NetworkProxySettingsValidator
                .ValidateServer(transportProtocol, server, validationResult);

            // Assert
            validationResult.IsValid.Should().BeFalse();
            validationResult.Entries.Count.Should().Be(1);
            validationResult.Entries.Should().Contain(expectedValidationEntry);
        }

        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        public void ShouldAddValidationError_WhenServerEmpty(string server)
        {
            // Arrange
            string[] transportProtocol = ["http"];
            var validationResult = new ValidationResult();

            var expectedValidationEntry = new ValidationEntry(
                "Proxy server can not be empty or whitespace.",
                ErrorCodes.ValidationFailed);

            // Act
            NetworkProxySettingsValidator
                .ValidateServer(transportProtocol, server, validationResult);

            // Assert
            validationResult.IsValid.Should().BeFalse();
            validationResult.Entries.Count.Should().Be(1);
            validationResult.Entries.Should().Contain(expectedValidationEntry);
        }

        [Theory]
        [InlineData("http.test.de")]
        [InlineData("http:test.de")]
        [InlineData("http:/test.de")]
        [InlineData("http//test.de")]
        [InlineData("http/test.de")]
        public void ShouldAddValidationError_WhenServerURLInvalid(string server)
        {
            // Arrange
            string[] transportProtocol = ["http"];
            var validationResult = new ValidationResult();

            var expectedValidationEntry = new ValidationEntry(
                "Proxy server URL is invalid. Format: " +
                "[protocol]://[hostname]:[port] or " +
                "[protocol]://[user]:[password]@[hostname]:[port].",
                ErrorCodes.ValidationFailed);

            // Act
            NetworkProxySettingsValidator
                .ValidateServer(transportProtocol, server, validationResult);

            // Assert
            validationResult.IsValid.Should().BeFalse();
            validationResult.Entries.Count.Should().Be(1);
            validationResult.Entries.Should().Contain(expectedValidationEntry);
        }

        [Fact]
        public void ShouldAddValidationError_WhenTransportProtocolNotSpecified()
        {
            // Arrange
            string[] transportProtocol = ["http"];
            var invalidServer = @"://test.de";
            var validationResult = new ValidationResult();

            var expectedValidationEntry = new ValidationEntry(
                "No proxy server transport protocol specified.",
                ErrorCodes.ValidationFailed);

            // Act
            NetworkProxySettingsValidator
                .ValidateServer(transportProtocol, invalidServer, validationResult);

            // Assert
            validationResult.IsValid.Should().BeFalse();
            validationResult.Entries.Count.Should().Be(1);
            validationResult.Entries.Should().Contain(expectedValidationEntry);
        }

        [Theory]
        [InlineData(new string[] { "http" }, "https://test.de")]
        [InlineData(new string[] { "https" }, "ftp://test.de")]
        [InlineData(new string[] { "ftp" }, "sftp://test.de")]
        [InlineData(new string[] { "sftp" }, "socks://test.de")]
        [InlineData(new string[] { "socks", "socks5" }, "http://test.de")]

        [InlineData(new string[] { "http" }, "htpp://test.de")]
        [InlineData(new string[] { "https" }, "htts://test.de")]
        [InlineData(new string[] { "ftp" }, "fpt://test.de")]
        [InlineData(new string[] { "sftp" }, "sfpt://test.de")]
        [InlineData(new string[] { "socks", "socks5" }, "scoks://test.de")]
        [InlineData(new string[] { "socks", "socks5" }, "sock5://test.de")]
        public void ShouldAddValidationError_WhenTransportProtocolInvalid(
            string[] transportProtocol,
            string server)
        {
            // Arrange
            var proxyType = server.Split(':')[0];
            var validationResult = new ValidationResult();

            var expectedValidationEntry = new ValidationEntry(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Proxy server transport protocol is invalid. Expected '{0}' but got '{1}'.",
                    string.Join(", ", transportProtocol),
                    proxyType),
                ErrorCodes.ValidationFailed);

            // Act
            NetworkProxySettingsValidator
                .ValidateServer(transportProtocol, server, validationResult);

            // Assert
            validationResult.IsValid.Should().BeFalse();
            validationResult.Entries.Count.Should().Be(1);
            validationResult.Entries.Should().Contain(expectedValidationEntry);
        }

        [Theory]
        [InlineData(new string[] { "http" }, "http:///test.de")]
        [InlineData(new string[] { "https" }, @"https://test.de\about")]
        [InlineData(new string[] { "ftp" }, "ftp://test.de:notThePort")]
        public void ShouldAddValidationError_WhenServerUriNotWellFormed(
            string[] transportProtocol,
            string server)
        {
            // Arrange
            var validationResult = new ValidationResult();

            var expectedValidationEntry = new ValidationEntry(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Proxy server is not a well formed URI."),
                ErrorCodes.ValidationFailed);

            // Act
            NetworkProxySettingsValidator
                .ValidateServer(transportProtocol, server, validationResult);

            // Assert
            validationResult.IsValid.Should().BeFalse();
            validationResult.Entries.Count.Should().Be(1);
            validationResult.Entries.Should().Contain(expectedValidationEntry);
        }

        [Theory]
        [InlineData(new string[] { "http" }, "http://test.de")]
        [InlineData(new string[] { "https" }, "https://test.de")]
        [InlineData(new string[] { "ftp" }, "ftp://test.de")]
        [InlineData(new string[] { "sftp" }, "sftp://test.de")]
        [InlineData(new string[] { "socks", "socks5" }, "socks://test.de")]
        [InlineData(new string[] { "socks", "socks5" }, "socks5://test.de")]
        public void ShouldHaveNoValidationError_WhenServerValid(
            string[] transportProtocol,
            string server)
        {
            // Arrange
            var validationResult = new ValidationResult();

            // Act
            NetworkProxySettingsValidator
                .ValidateServer(transportProtocol, server, validationResult);

            // Assert
            validationResult.IsValid.Should().BeTrue();
            validationResult.Entries.Count.Should().Be(0);
        }
    }

    public class ValidatePort
    {
        [Fact]
        public void ShouldAddValidationError_WhenPortNull()
        {
            // Arrange
            int? port = null;
            var validationResult = new ValidationResult();

            var expectedValidationEntry = new ValidationEntry(
                "Proxy port can not be null.",
                ErrorCodes.ValidationFailed);

            // Act
            NetworkProxySettingsValidator.ValidatePort(port, validationResult);

            // Assert
            validationResult.IsValid.Should().BeFalse();
            validationResult.Entries.Count.Should().Be(1);
            validationResult.Entries.Should().Contain(expectedValidationEntry);
        }

        [Fact]
        public void ShouldAddValidationError_WhenPortSmallerThanMinimum()
        {
            // Arrange
            int? port = -42;
            var validationResult = new ValidationResult();

            var expectedValidationEntry = new ValidationEntry(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Proxy port '{0}' can not be smaller than '{1}'.",
                    port,
                    NetworkProxySettingsValidator.MinPort),
                ErrorCodes.ValidationFailed);

            // Act
            NetworkProxySettingsValidator.ValidatePort(port, validationResult);

            // Assert
            validationResult.IsValid.Should().BeFalse();
            validationResult.Entries.Count.Should().Be(1);
            validationResult.Entries.Should().Contain(expectedValidationEntry);
        }

        [Fact]
        public void ShouldAddValidationError_WhenPortGreaterThanMaximum()
        {
            // Arrange
            int? port = 65_536;
            var validationResult = new ValidationResult();

            var expectedValidationEntry = new ValidationEntry(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Proxy port '{0}' can not be greater than '{1}'.",
                    port,
                    NetworkProxySettingsValidator.MaxPort),
                ErrorCodes.ValidationFailed);

            // Act
            NetworkProxySettingsValidator.ValidatePort(port, validationResult);

            // Assert
            validationResult.IsValid.Should().BeFalse();
            validationResult.Entries.Count.Should().Be(1);
            validationResult.Entries.Should().Contain(expectedValidationEntry);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(42)]
        [InlineData(443)]
        [InlineData(8080)]
        [InlineData(65_535)]
        public void ShouldHaveNoValidationError_WhenPortValid(int port)
        {
            // Arrange
            var validationResult = new ValidationResult();

            // Act
            NetworkProxySettingsValidator.ValidatePort(port, validationResult);

            // Assert
            validationResult.IsValid.Should().BeTrue();
            validationResult.Entries.Count.Should().Be(0);
        }
    }

    public class ValidateAuthentication
    {
        [Fact]
        public void ShouldAddValidationError_WhenUsernameNullAndPasswordNotNull()
        {
            // Arrange
            string? username = null;
            var password = "Please";
            var validationResult = new ValidationResult();

            var expectedValidationEntry = new ValidationEntry(
                "Proxy username can not be null when password given.",
                ErrorCodes.ValidationFailed);

            // Act
            NetworkProxySettingsValidator
                .ValidateAuthentication(username, password, validationResult);

            // Assert
            validationResult.IsValid.Should().BeFalse();
            validationResult.Entries.Count.Should().Be(1);
            validationResult.Entries.Should().Contain(expectedValidationEntry);
        }

        [Fact]
        public void ShouldAddValidationError_WhenPasswordNullAndUsernameNotNull()
        {
            // Arrange
            var username = "Martin";
            string? password = null;
            var validationResult = new ValidationResult();

            var expectedValidationEntry = new ValidationEntry(
                "Proxy password can not be null when user given.",
                ErrorCodes.ValidationFailed);

            // Act
            NetworkProxySettingsValidator
                .ValidateAuthentication(username, password, validationResult);

            // Assert
            validationResult.IsValid.Should().BeFalse();
            validationResult.Entries.Count.Should().Be(1);
            validationResult.Entries.Should().Contain(expectedValidationEntry);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void ShouldAddValidationError_WhenUsernameEmpty(string username)
        {
            // Arrange
            var password = "Please";
            var validationResult = new ValidationResult();

            var expectedValidationEntry = new ValidationEntry(
                "Proxy username can not be empty or whitespace.",
                ErrorCodes.ValidationFailed);

            // Act
            NetworkProxySettingsValidator
                .ValidateAuthentication(username, password, validationResult);

            // Assert
            validationResult.IsValid.Should().BeFalse();
            validationResult.Entries.Count.Should().Be(1);
            validationResult.Entries.Should().Contain(expectedValidationEntry);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void ShouldAddValidationError_WhenPasswordEmpty(string password)
        {
            // Arrange
            var username = "Martin";
            var validationResult = new ValidationResult();

            var expectedValidationEntry = new ValidationEntry(
                "Proxy password can not be empty or whitespace.",
                ErrorCodes.ValidationFailed);

            // Act
            NetworkProxySettingsValidator
                .ValidateAuthentication(username, password, validationResult);

            // Assert
            validationResult.IsValid.Should().BeFalse();
            validationResult.Entries.Count.Should().Be(1);
            validationResult.Entries.Should().Contain(expectedValidationEntry);
        }

        [Fact]
        public void ShouldAddValidationError_WhenUsernameAndPasswordEmpty()
        {
            // Arrange
            var username = string.Empty;
            var password = string.Empty;
            var validationResult = new ValidationResult();

            var expectedValidationEntries = new List<ValidationEntry>()
            {
                new("Proxy username can not be empty or whitespace.", ErrorCodes.ValidationFailed),
                new("Proxy password can not be empty or whitespace.", ErrorCodes.ValidationFailed)
            };

            // Act
            NetworkProxySettingsValidator
                .ValidateAuthentication(username, password, validationResult);

            // Assert
            validationResult.IsValid.Should().BeFalse();
            validationResult.Entries.Count.Should().Be(2);
            validationResult.Entries.Should().Contain(expectedValidationEntries);
        }

        [Theory]
        [InlineData("1", "1")]
        [InlineData("admin", "admin")]
        [InlineData("USER", "123")]
        public void ShouldHaveNoValidationError_WhenAuthenticationValid(
            string username,
            string password)
        {
            // Arrange
            var validationResult = new ValidationResult();

            // Act
            NetworkProxySettingsValidator
                .ValidateAuthentication(username, password, validationResult);

            // Assert
            validationResult.IsValid.Should().BeTrue();
            validationResult.Entries.Count.Should().Be(0);
        }

        [Fact]
        public void ShouldHaveNoValidationError_WhenNoAuthenticationGiven()
        {
            // Arrange
            var validationResult = new ValidationResult();

            // Act
            NetworkProxySettingsValidator
                .ValidateAuthentication(null, null, validationResult);

            // Assert
            validationResult.IsValid.Should().BeTrue();
            validationResult.Entries.Count.Should().Be(0);
        }
    }
}
