using FluentAssertions;
using Xunit;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Validation;
using HostManagement.Shared.Validation.Validators.Proxy;

namespace HostManagement.Shared.Tests.Validation.Validators.Proxy;

public class NoProxyListValidatorTests
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

        var result = await new NoProxyListValidator().ValidateAsync([], s_interfaces, cts.Token);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldReturnValidationError_WhenNoNoProxyItemsSpecified()
    {
        // Arrange
        var noProxyItems = new List<string>();
        var expectedValidationEntry = new ValidationEntry("No no-proxy items specified.");

        // Act
        var result = await new NoProxyListValidator().ValidateAsync(noProxyItems, s_interfaces);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldReturnValidationError_WhenDuplicateNoProxyItemsFound()
    {
        // Arrange
        var duplicateNoProxyItem = ValidationTestData.NoProxyItem.IPv4;
        var noProxyItems = new List<string>()
        {
            ValidationTestData.NoProxyItem.IPv4,
            duplicateNoProxyItem,
            ValidationTestData.NoProxyItem.IPv4Port
        };

        var expectedValidationEntry = new ValidationEntry(
            $"No-proxy items have duplicate entries. Affected entries are: '{duplicateNoProxyItem}'.");

        // Act
        var result = await new NoProxyListValidator().ValidateAsync(noProxyItems, s_interfaces);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData(ValidationTestData.NoProxyItem.IPv4, true)]
    [InlineData(ValidationTestData.NoProxyItem.InvalidNoProxyItem, false)]
    public async Task ShouldAddValidationResult_WhenNoProxyItemValidatorCalled(
        string noProxyItem,
        bool isValid)
    {
        // Act
        var result = await new NoProxyItemValidator().ValidateAsync(noProxyItem, s_interfaces);

        // Assert
        result.IsValid.Should().Be(isValid);
        result.EntriesString.Should().NotContain(ValidationConstants.CancelledResult);
    }

    [Fact]
    public async Task ShouldReturnValid_WhenNoProxyListValid()
    {
        // Act
        var noProxyItems = new List<string>
        {
            ValidationTestData.NoProxyItem.IPv4,
            ValidationTestData.NoProxyItem.IPv4Port,
            ValidationTestData.NoProxyItem.IPv6,
            ValidationTestData.NoProxyItem.IPv6Port,
            ValidationTestData.NoProxyItem.IPv6LinkLocal,
            ValidationTestData.NoProxyItem.IPv6LinkLocalPort,
            ValidationTestData.NoProxyItem.Simple,
            ValidationTestData.NoProxyItem.FQDN
        };

        var result = await new NoProxyListValidator().ValidateAsync(noProxyItems, s_interfaces);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Entries.Count.Should().Be(0);
    }
}
