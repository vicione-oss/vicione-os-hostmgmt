using FluentAssertions;
using Xunit;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Validation;
using HostManagement.Shared.Validation.Validators.Proxy;
using HostManagement.Shared.Validation.Utils;

namespace HostManagement.Shared.Tests.Validation.Validators.Proxy;

public class NoProxyItemValidatorTests
{
    private static readonly List<NetworkInterfaceDetail> s_interfaces =
    [
        ValidationTestData.Interface.Static,
        ValidationTestData.Interface.DHCP
    ];

    [Fact]
    public async Task ShouldReturnValidationError_WhenValidationCancelled()
    {
        // Arrange
        var expectedValidationEntry = new ValidationEntry(ValidationConstants.CancelledResult);

        // Act
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var result = await new NoProxyItemValidator()
            .ValidateAsync(ValidationTestData.NoProxyItem.IPv6, s_interfaces, cts.Token);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ShouldReturnValidationError_WhenNoProxyItemEmpty(string? noProxyItem)
    {
        // Arrange
        var expectedValidationEntry = new ValidationEntry("No-proxy item can not be empty or whitespace.");

        // Act
        var result = await new NoProxyItemValidator()
            .ValidateAsync(noProxyItem!, s_interfaces);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldReturnValidationError_WhenNoProxyItemIsNoValidIPv4Address()
    {
        // Arrange
        var invalidNoProxyItem = "192.168.444.333";

        var expectedValidationEntry = new ValidationEntry(
            $"No-proxy item '{invalidNoProxyItem}' is no valid IPv4 address.");

        // Act
        var result = await new NoProxyItemValidator()
            .ValidateAsync(invalidNoProxyItem, s_interfaces);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldReturnValidationError_WhenNoProxyItemIsMulticastIPv4Address()
    {
        // Arrange
        var multicastNoProxyItem = "224.0.0.0";

        var expectedValidationEntry = new ValidationEntry(
            $"No-proxy item '{multicastNoProxyItem}' can not be a multicast address " +
            $"('{IPAddressValidatorUtils.IPv4MulticastNetworkCIDR}').");

        // Act
        var result = await new NoProxyItemValidator()
            .ValidateAsync(multicastNoProxyItem, s_interfaces);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldReturnValidationError_WhenNoProxyItemIsBroadcastIPv4Address()
    {
        // Arrange
        var broadcastNoProxyItem = "255.255.255.255";

        var expectedValidationEntry = new ValidationEntry(
            $"No-proxy item '{broadcastNoProxyItem}' can not be a broadcast address " +
            $"('{IPAddressValidatorUtils.IPv4BroadcastNetworkCIDR}').");

        // Act
        var result = await new NoProxyItemValidator()
            .ValidateAsync(broadcastNoProxyItem, s_interfaces);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldReturnValidationError_WhenNoProxyItemIsNoValidIPv6Address()
    {
        // Arrange
        var invalidNoProxyItem = "xe80::";

        var expectedValidationEntry = new ValidationEntry(
            $"No-proxy item '{invalidNoProxyItem}' is no valid IPv6 address.");

        // Act
        var result = await new NoProxyItemValidator()
            .ValidateAsync(invalidNoProxyItem, s_interfaces);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldReturnValidationError_WhenNoProxyItemIsMulticastIPv6Address()
    {
        // Arrange
        var multicastNoProxyItem = "ff00::";

        var expectedValidationEntry = new ValidationEntry(
            $"No-proxy item '{multicastNoProxyItem}' can not be a multicast address " +
            $"('{IPAddressValidatorUtils.IPv6MulticastNetworkCIDR}').");

        // Act
        var result = await new NoProxyItemValidator()
            .ValidateAsync(multicastNoProxyItem, s_interfaces);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData("[2606:4700:4700::1111]:")]
    [InlineData("[2606:4700:4700::1111]")]
    public async Task ShouldReturnValidationError_WhenIPv6NoProxyItemHasMissingPort(
        string invalidNoProxyItem)
    {
        // Arrange
        var expectedValidationEntry = new ValidationEntry(
            $"No-proxy item '{invalidNoProxyItem}' has a missing proxy port.");

        // Act
        var result = await new NoProxyItemValidator()
            .ValidateAsync(invalidNoProxyItem, s_interfaces);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("ff")]
    [InlineData("a42")]

    [InlineData("1.23")]
    [InlineData("543.21e2")]

    [InlineData("1.23f")]
    [InlineData("3.21F")]
    [InlineData("543.21E-2f")]

    [InlineData("1.23d")]
    [InlineData("3.21D")]

    [InlineData("1.23m")]
    [InlineData("3.21M")]
    [InlineData("543.21E6m")]

    [InlineData("0x2A")]
    [InlineData("0b_0010_1010")]
    public async Task ShouldReturnValidationError_WhenPortStringCanNotBeParsed(string? invalidPortString)
    {
        // Arrange
        var invalidNoProxyItem = $"{ValidationTestData.NoProxyItem.IPv4}:{invalidPortString}";

        var expectedValidationEntry = new ValidationEntry(
            $"Proxy port '{invalidPortString}' must be a number.");

        // Act
        var result = await new NoProxyItemValidator().ValidateAsync(invalidNoProxyItem, s_interfaces);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }
}
