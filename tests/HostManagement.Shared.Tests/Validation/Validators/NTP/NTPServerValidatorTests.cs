using AwesomeAssertions;
using HostManagement.Shared.Validation;
using HostManagement.Shared.Validation.Utils;
using HostManagement.Shared.Validation.Validators.NTP;
using Xunit;

namespace HostManagement.Shared.Tests.Validation.Validators.NTP;

public class NTPServerValidatorTests
{
    [Fact]
    public async Task ShouldReturnValidationError_WhenValidationCancelled()
    {
        // Arrange
        var expectedValidationEntry = new ValidationEntry(ValidationConstants.CancelledResult);

        // Act
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var result = await new NTPServerValidator()
            .ValidateAsync(ValidationTestData.NTPServer.FQDN, cts.Token);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ShouldReturnValidationError_WhenNTPServerEmpty(string? ntpServer)
    {
        // Arrange
        var expectedValidationEntry = new ValidationEntry("NTP server can not be empty or whitespace.");

        // Act
        var result = await new NTPServerValidator().ValidateAsync(ntpServer!, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldReturnValidationError_WhenNTPServerIsNoValidIPv4Address()
    {
        // Arrange
        var invalidNTPServer = ValidationTestData.NTPServer.InvalidIPv4;

        var expectedValidationEntry = new ValidationEntry(
            $"NTP server '{invalidNTPServer}' is no valid IPv4 address.");

        // Act
        var result = await new NTPServerValidator().ValidateAsync(invalidNTPServer, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldReturnValidationError_WhenNTPServerIsLinkLocalIPv4Address()
    {
        // Arrange
        var linkLocalNTPServer = ValidationTestData.NTPServer.InvalidLinkLocalIPv4;

        var expectedValidationEntry = new ValidationEntry(
            $"NTP server '{linkLocalNTPServer}' can not be a link-local address " +
            $"('{IPAddressValidatorUtils.IPv4LinkLocalNetworkCIDR}').");

        // Act
        var result = await new NTPServerValidator().ValidateAsync(linkLocalNTPServer, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldReturnValidationError_WhenNTPServerIsMulticastIPv4Address()
    {
        // Arrange
        var multicastNTPServer = ValidationTestData.NTPServer.InvalidMulticastIPv4;

        var expectedValidationEntry = new ValidationEntry(
            $"NTP server '{multicastNTPServer}' can not be a multicast address " +
            $"('{IPAddressValidatorUtils.IPv4MulticastNetworkCIDR}').");

        // Act
        var result = await new NTPServerValidator().ValidateAsync(multicastNTPServer, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldReturnValidationError_WhenNTPServerIsNoValidIPv6Address()
    {
        // Arrange
        var invalidNTPServer = ValidationTestData.NTPServer.InvalidIPv6;

        var expectedValidationEntry = new ValidationEntry(
            $"NTP server '{invalidNTPServer}' is no valid IPv6 address.");

        // Act
        var result = await new NTPServerValidator().ValidateAsync(invalidNTPServer, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldReturnValidationError_WhenNTPServerIsLinkLocalIPv6Address()
    {
        // Arrange
        var linkLocalNTPServer = ValidationTestData.NTPServer.InvalidLinkLocalIPv6;

        var expectedValidationEntry = new ValidationEntry(
            $"NTP server '{linkLocalNTPServer}' can not be a link-local address " +
            $"('{IPAddressValidatorUtils.IPv6LinkLocalNetworkCIDR}').");

        // Act
        var result = await new NTPServerValidator().ValidateAsync(linkLocalNTPServer, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldReturnValidationError_WhenNTPServerIsMulticastIPv6Address()
    {
        // Arrange
        var multicastNTPServer = ValidationTestData.NTPServer.InvalidMulticastIPv6;

        var expectedValidationEntry = new ValidationEntry(
            $"NTP server '{multicastNTPServer}' can not be a multicast address " +
            $"('{IPAddressValidatorUtils.IPv6MulticastNetworkCIDR}').");

        // Act
        var result = await new NTPServerValidator().ValidateAsync(multicastNTPServer, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldReturnValidationError_WhenNTPServerIsBroadcastAddress()
    {
        // Arrange
        var broadcastNTPServer = ValidationTestData.NTPServer.InvalidBroadcast;

        var expectedValidationEntry = new ValidationEntry(
            $"NTP server '{broadcastNTPServer}' can not be a broadcast address " +
            $"('{IPAddressValidatorUtils.IPv4BroadcastNetworkCIDR}').");

        // Act
        var result = await new NTPServerValidator().ValidateAsync(broadcastNTPServer, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldReturnValidationResult_WhenNTPServerIsFullyQualifiedDomainName()
    {
        // Arrange
        var ntpServer = ValidationTestData.NTPServer.FQDN;

        // Act
        var result = await new NTPServerValidator().ValidateAsync(ntpServer, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ValidationResult>();
        result.EntriesString.Should().NotContain(ValidationConstants.CancelledResult);
    }

    [Theory]
    [InlineData(ValidationTestData.NTPServer.IPv4)]
    [InlineData(ValidationTestData.NTPServer.IPv6)]
    [InlineData(ValidationTestData.NTPServer.FQDN)]
    public async Task ShouldReturnValid_WhenNTPServerValid(string ntpServer)
    {
        // Act
        var result = await new NTPServerValidator().ValidateAsync(ntpServer, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Entries.Count.Should().Be(0);
    }
}
