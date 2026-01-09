using FluentAssertions;
using Xunit;
using HostManagement.Shared.Validation;
using HostManagement.Shared.Validation.Utils;
using HostManagement.Shared.Validation.Validators.DNS;

namespace HostManagement.Shared.Tests.Validation.Validators.DNS;

public class StaticHostValidatorTests
{
    [Fact]
    public async Task ShouldReturnValidationError_WhenValidationCancelled()
    {
        // Arrange
        var expectedValidationEntry = new ValidationEntry(ValidationConstants.CancelledResult);

        // Act
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var result = await new StaticHostValidator()
            .ValidateAsync(ValidationTestData.StaticHost.StaticHost1, cts.Token);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldAddValidationResult_WhenLoopbackAddressFound()
    {
        // Arrange
        var staticHost = ValidationTestData.StaticHost.InvalidLoopback;

        var expectedValidationEntry = new ValidationEntry(
            $"Static host IP address '{staticHost.IPAddress}' can not be a loopback address " +
            $"('{IPAddressValidatorUtils.IPv4LoopbackNetworkCIDR}').");

        // Act
        var result = await new StaticHostValidator().ValidateAsync(staticHost);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldAddValidationResult_WhenLinkLocalAddressFound()
    {
        // Arrange
        var staticHost = ValidationTestData.StaticHost.InvalidLinkLocal;

        var expectedValidationEntry = new ValidationEntry(
            $"Static host IP address '{staticHost.IPAddress}' can not be a link-local address " +
            $"('{IPAddressValidatorUtils.IPv4LinkLocalNetworkCIDR}').");

        // Act
        var result = await new StaticHostValidator().ValidateAsync(staticHost);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldAddValidationResult_WhenMulticastAddressFound()
    {
        // Arrange
        var staticHost = ValidationTestData.StaticHost.InvalidMulticast;

        var expectedValidationEntry = new ValidationEntry(
            $"Static host IP address '{staticHost.IPAddress}' can not be a multicast address " +
            $"('{IPAddressValidatorUtils.IPv4MulticastNetworkCIDR}').");

        // Act
        var result = await new StaticHostValidator().ValidateAsync(staticHost);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldAddValidationResult_WhenBroadcastAddressFound()
    {
        // Arrange
        var staticHost = ValidationTestData.StaticHost.InvalidBroadcast;

        var expectedValidationEntry = new ValidationEntry(
            $"Static host IP address '{staticHost.IPAddress}' can not be a broadcast address " +
            $"('{IPAddressValidatorUtils.IPv4BroadcastNetworkCIDR}').");

        // Act
        var result = await new StaticHostValidator().ValidateAsync(staticHost);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldReturnValid_WhenStaticHostValid()
    {
        // Act
        var result = await new StaticHostValidator().ValidateAsync(ValidationTestData.StaticHost.StaticHost2);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Entries.Count.Should().Be(0);
    }
}
