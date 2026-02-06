using AwesomeAssertions;
using HostManagement.Shared.Validation;
using HostManagement.Shared.Validation.Utils;
using HostManagement.Shared.Validation.Validators.NetworkInterfaces;
using Xunit;

namespace HostManagement.Shared.Tests.Validation.Validators.NetworkInterfaces;

public class GatewayValidatorTests
{
    private static string GetCustomInterfaceNameText(string? interfaceName)
        => interfaceName is null ? "network interface" : $"network interface '{interfaceName}'";

    [Fact]
    public async Task ShouldReturnValidationError_WhenValidationCancelled()
    {
        // Arrange
        var expectedValidationEntry = new ValidationEntry(ValidationConstants.CancelledResult);

        // Act
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var result = await new GatewayValidator()
            .ValidateAsync(ValidationTestData.Gateway.Gateway1, cts.Token);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("lan1")]
    public async Task ShouldReturnValidationError_WhenNotRoutableGatewayFound(string? interfaceName)
    {
        // Arrange
        var expectedValidationEntry = new ValidationEntry(
            $"Gateway of {GetCustomInterfaceNameText(interfaceName)} can not be the non-routable address " +
            $"('{IPAddressValidatorUtils.IPv4NotRoutableNetworkCIDR}').");

        // Act
        var result = await new GatewayValidator(interfaceName)
            .ValidateAsync(ValidationTestData.Gateway.InvalidNotRoutableIPv4, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("lan1")]
    public async Task ShouldReturnValidationError_WhenLoopbackGatewayFound(string? interfaceName)
    {
        // Arrange
        var expectedValidationEntry = new ValidationEntry(
            $"Gateway of {GetCustomInterfaceNameText(interfaceName)} can not be a loopback address " +
            $"('{IPAddressValidatorUtils.IPv4LoopbackNetworkCIDR}').");

        // Act
        var result = await new GatewayValidator(interfaceName)
            .ValidateAsync(ValidationTestData.Gateway.InvalidLoopbackIPv4, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("lan1")]
    public async Task ShouldReturnValidationError_WhenMulticastGatewayFound(string? interfaceName)
    {
        // Arrange
        var expectedValidationEntry = new ValidationEntry(
            $"Gateway of {GetCustomInterfaceNameText(interfaceName)} can not be a multicast address " +
            $"('{IPAddressValidatorUtils.IPv4MulticastNetworkCIDR}').");

        // Act
        var result = await new GatewayValidator(interfaceName)
            .ValidateAsync(ValidationTestData.Gateway.InvalidMulticastIPv4, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("lan1")]
    public async Task ShouldReturnValidationError_WhenBroadcastGatewayFound(string? interfaceName)
    {
        // Arrange
        var expectedValidationEntry = new ValidationEntry(
            $"Gateway of {GetCustomInterfaceNameText(interfaceName)} can not be a broadcast address " +
            $"('{IPAddressValidatorUtils.IPv4BroadcastNetworkCIDR}').");

        // Act
        var result = await new GatewayValidator(interfaceName)
            .ValidateAsync(ValidationTestData.Gateway.InvalidBroadcastIPv4, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("lan1")]
    public async Task ShouldReturnValid_WhenGatewayValid(string? interfaceName)
    {
        // Act
        var result = await new GatewayValidator(interfaceName)
            .ValidateAsync(ValidationTestData.Gateway.Gateway1, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Entries.Count.Should().Be(0);
    }
}
