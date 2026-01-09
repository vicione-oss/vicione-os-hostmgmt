using System.Net;
using FluentAssertions;
using Xunit;
using HostManagement.Shared.Validation;
using HostManagement.Shared.Validation.Utils;
using HostManagement.Shared.Validation.Validators.DNS;

namespace HostManagement.Shared.Tests.Validation.Validators.DNS;

public class NameServerValidatorTests
{
    [Fact]
    public async Task ShouldReturnValidationError_WhenValidationCancelled()
    {
        // Arrange
        var expectedValidationEntry = new ValidationEntry(ValidationConstants.CancelledResult);

        // Act
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var result = await new NameServerValidator()
            .ValidateAsync(IPAddress.Parse(ValidationTestData.NameServer.IPv4), cts.Token);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldReturnValidationError_WhenLinkLocalNameServerFound()
    {
        // Arrange
        var linkLocalNameServer = IPAddress.Parse(ValidationTestData.NameServer.InvalidLinkLocalIPv4);

        var expectedValidationEntry = new ValidationEntry(
            $"Name server '{linkLocalNameServer}' can not be a link-local address " +
            $"('{IPAddressValidatorUtils.IPv4LinkLocalNetworkCIDR}').");

        // Act
        var result = await new NameServerValidator().ValidateAsync(linkLocalNameServer);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldReturnValidationError_WhenMulticastNameServerFound()
    {
        // Arrange
        var multicastNameServer = IPAddress.Parse(ValidationTestData.NameServer.InvalidMulticastIPv6);

        var expectedValidationEntry = new ValidationEntry(
            $"Name server '{multicastNameServer}' can not be a multicast address " +
            $"('{IPAddressValidatorUtils.IPv6MulticastNetworkCIDR}').");

        // Act
        var result = await new NameServerValidator().ValidateAsync(multicastNameServer);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldReturnValidationError_WhenBroadcastNameServerFound()
    {
        // Arrange
        var broadcastNameServer = IPAddress.Parse(ValidationTestData.NameServer.InvalidBroadcast);

        var expectedValidationEntry = new ValidationEntry(
            $"Name server '{broadcastNameServer}' can not be a broadcast address " +
            $"('{IPAddressValidatorUtils.IPv4BroadcastNetworkCIDR}').");

        // Act
        var result = await new NameServerValidator().ValidateAsync(broadcastNameServer);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldReturnValid_WhenNameServerValid()
    {
        // Act
        var nameServer = IPAddress.Parse(ValidationTestData.NameServer.IPv6);
        var result = await new NameServerValidator().ValidateAsync(nameServer);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Entries.Count.Should().Be(0);
    }
}
