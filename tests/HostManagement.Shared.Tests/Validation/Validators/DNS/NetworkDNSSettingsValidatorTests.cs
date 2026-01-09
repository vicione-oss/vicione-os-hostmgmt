using System.Net;
using AwesomeAssertions;
using Xunit;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Validation;
using HostManagement.Shared.Validation.Validators.DNS;

namespace HostManagement.Shared.Tests.Validation.Validators.DNS;

public class NetworkDNSSettingsValidatorTests
{
    [Fact]
    public async Task ShouldReturnValidationError_WhenValidationCancelled()
    {
        // Arrange
        var expectedValidationEntry = new ValidationEntry(ValidationConstants.CancelledResult);

        // Act
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var result = await new NetworkDNSSettingsValidator()
            .ValidateAsync(new NetworkDNSSettings(), cts.Token);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData(false, false, false, false)]
    [InlineData(false, false, false, true)]
    [InlineData(false, false, true, false)]
    [InlineData(false, false, true, true)]
    [InlineData(false, true, false, false)]
    [InlineData(false, true, false, true)]
    [InlineData(false, true, true, false)]
    [InlineData(false, true, true, true)]
    [InlineData(true, false, false, false)]
    [InlineData(true, false, false, true)]
    [InlineData(true, false, true, false)]
    [InlineData(true, false, true, true)]
    [InlineData(true, true, false, false)]
    [InlineData(true, true, false, true)]
    [InlineData(true, true, true, false)]
    [InlineData(true, true, true, true)]
    public async Task ShouldReturnValidationResult_WhenValidateAsyncCalled(
        bool dnsSuffixEnabled,
        bool staticHostsEnabled,
        bool searchDomainsEnabled,
        bool nameServersEnabled)
    {
        // Arrange
        var dnsSettings = new NetworkDNSSettings
        {
            Hostname = ValidationTestData.Hostname.Simple,
            DNSSuffixEnabled = dnsSuffixEnabled,
            DNSSuffix = ValidationTestData.PrimarySuffix.FQDN,
            StaticHostsEnabled = staticHostsEnabled,
            StaticHosts =
            [
                ValidationTestData.StaticHost.StaticHost1,
                ValidationTestData.StaticHost.StaticHost2
            ],
            SearchDomainsEnabled = searchDomainsEnabled,
            SearchDomains =
            [
                ValidationTestData.SearchDomain.Simple,
                ValidationTestData.SearchDomain.FQDN
            ],
            NameServersEnabled = nameServersEnabled,
            NameServers =
            [
                IPAddress.Parse(ValidationTestData.NameServer.IPv4),
                IPAddress.Parse(ValidationTestData.NameServer.IPv6)
            ]
        };

        // Act
        var result = await new NetworkDNSSettingsValidator().ValidateAsync(dnsSettings);

        // Assert
        result.Should().BeOfType<ValidationResult>();
        result.EntriesString.Should().NotBe(ValidationConstants.CancelledResult);
    }
}
