using AwesomeAssertions;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Validation;
using HostManagement.Shared.Validation.Validators.Proxy;
using Xunit;

namespace HostManagement.Shared.Tests.Validation.Validators.Proxy;

public class ZoneIdentifierValidatorTests
{
    private static readonly List<NetworkInterfaceDetail> s_interfaces =
    [
        ValidationTestData.Interface.Static,
        ValidationTestData.Interface.DHCP
    ];

    private static string GetCustomNoProxyItemText(string? noProxyItem)
        => noProxyItem is null ? "no-proxy item" : $"no-proxy item '{noProxyItem}'";

    [Fact]
    public async Task ShouldReturnValidationError_WhenValidationCancelled()
    {
        // Arrange
        var expectedValidationEntry = new ValidationEntry(ValidationConstants.CancelledResult);

        // Act
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var result = await new ZoneIdentifierValidator()
            .ValidateAsync(ValidationTestData.InterfaceName.Static, s_interfaces, cts.Token);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("fe80::1%", "")]
    [InlineData("[fe80::1%]:443", "")]
    [InlineData(null, "  ")]
    [InlineData("fe80::1%  ", "  ")]
    [InlineData("[fe80::1%  ]:443", "  ")]
    public async Task ShouldReturnValidationError_WhenZoneIdentifierEmpty(
        string? invalidNoProxyItem,
        string emptyZoneIdentifier)
    {
        // Arrange
        var expectedValidationEntry = new ValidationEntry(
            $"Zone identifier of " +
            $"{GetCustomNoProxyItemText(invalidNoProxyItem)} can not be empty or whitespace.");

        // Act
        var result = await new ZoneIdentifierValidator(invalidNoProxyItem)
            .ValidateAsync(emptyZoneIdentifier, s_interfaces, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }


    [Theory]
    [InlineData(null, "ens456789012345x")]
    [InlineData("[fe80::%ens456789012345x]:443", "ens456789012345x")]
    public async Task ShouldReturnValidationError_WhenZoneIdentifierHasInvalidCharacterCount(
        string? invalidNoProxyItem,
        string invalidZoneIdentifier)
    {
        // Arrange
        var expectedValidationEntry = new ValidationEntry(
            $"Zone identifier '{invalidZoneIdentifier}' of {GetCustomNoProxyItemText(invalidNoProxyItem)} " +
            $"has an invalid character count (expected max. 15 characters).");

        // Act
        var result = await new ZoneIdentifierValidator(invalidNoProxyItem)
            .ValidateAsync(invalidZoneIdentifier, s_interfaces, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData(null, "ensµ35")]
    [InlineData(null, "lan1@ens33")]
    [InlineData("[fe80::%ensµ35]:443", "ensµ35")]
    [InlineData("[fe80::%lan1@ens33]:443", "lan1@ens33")]
    public async Task ShouldReturnValidationError_WhenZoneIdentifierHasInvalidCharacter(
        string? invalidNoProxyItem,
        string invalidZoneIdentifier)
    {
        // Arrange
        var expectedValidationEntry = new ValidationEntry(
            $"Zone identifier '{invalidZoneIdentifier}' of {GetCustomNoProxyItemText(invalidNoProxyItem)} " +
            $"has an invalid character (expected 0-9, a-f, A-F, _ or -).");

        var result = await new ZoneIdentifierValidator(invalidNoProxyItem)
            .ValidateAsync(invalidZoneIdentifier, s_interfaces, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData(null, "ens43")]
    [InlineData("[fe80::%ens43]:443", "ens43")]
    public async Task ShouldReturnValidationError_WhenZoneIdentifierHasInvalidInterfaceName(
        string? invalidNoProxyItem,
        string invalidZoneIdentifier)
    {
        // Arrange
        var expectedValidationEntry = new ValidationEntry(
            $"Zone identifier '{invalidZoneIdentifier}' of {GetCustomNoProxyItemText(invalidNoProxyItem)} " +
            $"has an unknown interface name.");

        // Act
        var result = await new ZoneIdentifierValidator(invalidNoProxyItem)
            .ValidateAsync(invalidZoneIdentifier, s_interfaces, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData(null, ValidationTestData.InterfaceName.Static)]
    [InlineData($"[fe80::%{ValidationTestData.InterfaceName.Static}]:443", ValidationTestData.InterfaceName.Static)]
    public async Task ShouldReturnValid_WhenZoneIdentifierValid(string? noProxyItem, string zoneIdentifier)
    {
        // Act
        var result = await new ZoneIdentifierValidator(noProxyItem)
            .ValidateAsync(zoneIdentifier, s_interfaces, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Entries.Count.Should().Be(0);
    }
}
