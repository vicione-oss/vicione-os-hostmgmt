using FluentAssertions;
using Xunit;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Validation;
using HostManagement.Shared.Validation.Validators.Proxy;

namespace HostManagement.Shared.Tests.Validation.Validators.Proxy;

public class NetworkProxySettingsValidatorTests
{
    [Fact]
    public async Task ShouldReturnValidationError_WhenValidationCancelled()
    {
        // Arrange
        var proxySettings = new NetworkProxySettings();
        var expectedValidationEntry = new ValidationEntry(ValidationConstants.CancelledResult);

        // Act
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var result = await new NetworkProxySettingsValidator().ValidateAsync(proxySettings, [], cts.Token);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData(false, ValidationTestData.NoProxyItem.FQDN, true)]
    [InlineData(false, ValidationTestData.NoProxyItem.InvalidNoProxyItem, true)]
    [InlineData(true, ValidationTestData.NoProxyItem.InvalidNoProxyItem, false)]
    [InlineData(true, ValidationTestData.NoProxyItem.FQDN, true)]
    public async Task ShouldReturnValidationResult_WhenValidateAsyncCalled(
        bool doNotProxyListEnabled,
        string noProxyItem,
        bool isValid)
    {
        // Arrange
        var interfaces = new List<NetworkInterfaceDetail>
        {
            ValidationTestData.Interface.Static,
            ValidationTestData.Interface.DHCP
        };

        var proxySettings = new NetworkProxySettings
        {
            DoNotProxyListEnabled = doNotProxyListEnabled,
            DoNotProxyList =
            [
                ValidationTestData.NoProxyItem.IPv4,
                ValidationTestData.NoProxyItem.IPv4Port,
                ValidationTestData.NoProxyItem.IPv6,
                ValidationTestData.NoProxyItem.IPv6Port,
                ValidationTestData.NoProxyItem.IPv6LinkLocal,
                ValidationTestData.NoProxyItem.IPv6LinkLocalPort,
                ValidationTestData.NoProxyItem.Simple,
                noProxyItem
            ]
        };

        // Act
        var result = await new NetworkProxySettingsValidator().ValidateAsync(proxySettings, interfaces);

        // Assert
        result.IsValid.Should().Be(isValid);
        result.EntriesString.Should().NotBe(ValidationConstants.CancelledResult);
    }
}
