using System.Net;
using AwesomeAssertions;
using HostManagement.Shared.Validation;
using HostManagement.Shared.Validation.Validators.NetworkInterfaces;
using Xunit;

namespace HostManagement.Shared.Tests.Validation.Validators.NetworkInterfaces;

public class NetmaskValidatorTests
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

        var result = await new NetmaskValidator()
            .ValidateAsync(ValidationTestData.Netmask.ValidIPv4, cts.Token);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("lan1")]
    public async Task ShouldReturnValidationError_WhenNetmaskIsIPv6Address(string? interfaceName)
    {
        // Arrange
        var invalidIPv6Netmask = ValidationTestData.Netmask.InvalidIPv6;
        var expectedValidationEntry = new ValidationEntry(
            $"Netmask '{invalidIPv6Netmask}' of {GetCustomInterfaceNameText(interfaceName)} " +
            $"is invalid (expected contiguous 1-bits at the top, e.g. '255.255.255.0', '255.255.255.128', '128.0.0.0' etc.).");

        // Act
        var result = await new NetmaskValidator(interfaceName).ValidateAsync(invalidIPv6Netmask, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData(null, "255.255.255.1")]
    [InlineData(null, "255.255.0.255")]
    [InlineData(null, "255.0.255.0")]
    [InlineData("lan1", "255.255.255.127")]
    [InlineData("lan1", "255.255.255.250")]
    [InlineData("lan1", "255.255.255.253")]
    public async Task ShouldReturnValidationError_WhenNetmaskIsInvalid(
        string? interfaceName,
        string netmask)
    {
        // Arrange
        var invalidIPv4Netmask = IPAddress.Parse(netmask);
        var expectedValidationEntry = new ValidationEntry(
            $"Netmask '{netmask}' of {GetCustomInterfaceNameText(interfaceName)} " +
            $"is invalid (expected contiguous 1-bits at the top, e.g. '255.255.255.0', '255.255.255.128', '128.0.0.0' etc.).");

        // Act
        var result = await new NetmaskValidator(interfaceName)
            .ValidateAsync(invalidIPv4Netmask, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData(null, "255.255.255.0")]
    [InlineData(null, "255.255.0.0")]
    [InlineData(null, "255.0.0.0")]
    [InlineData(null, "255.255.255.128")]
    [InlineData(null, "255.255.255.252")]
    [InlineData("lan1", "255.255.255.254")]
    [InlineData("lan1", "255.255.255.255")]
    [InlineData("lan1", "255.255.254.0")]
    [InlineData("lan1", "128.0.0.0")]
    [InlineData("lan1", "0.0.0.0")]
    public async Task ShouldReturnValid_WhenNetmaskIsValid(
        string? interfaceName,
        string netmask)
    {
        // Act
        var validNetmask = IPAddress.Parse(netmask);
        var result = await new NetmaskValidator(interfaceName).ValidateAsync(validNetmask, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Entries.Count.Should().Be(0);
        result.EntriesString.Should().NotContain(ValidationConstants.CancelledResult);
    }
}
