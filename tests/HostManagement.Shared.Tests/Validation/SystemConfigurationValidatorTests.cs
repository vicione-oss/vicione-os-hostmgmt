using System.Net;
using AwesomeAssertions;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Validation;
using Xunit;

namespace HostManagement.Shared.Tests.Validation;

public class SystemConfigurationValidatorTests
{
    [Fact]
    public async Task ShouldReturnValidationError_WhenValidationCancelled()
    {
        // Arrange
        var expectedValidationEntry = new ValidationEntry(ValidationConstants.CancelledResult);

        // Act
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var result = await new SystemConfigurationValidator()
            .ValidateAsync(new SystemConfiguration(), cts.Token);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldAddValidationError_WhenExceptionOccurs()
    {
        // Arrange
        SystemConfiguration? invalidProposedConfig = null;

        var expectedValidationEntry = new ValidationEntry(
            $"Validation failed." +
            Environment.NewLine +
            $"Object reference not set to an instance of an object.",
            ErrorCodes.ValidationFailed);

        // Act
        var validationResult = await new SystemConfigurationValidator()
            .ValidateAsync(invalidProposedConfig!, CancellationToken.None);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Entries.Count.Should().Be(1);
        validationResult.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldReturnValidationResult_WhenValidateAsyncCalled()
    {
        // Arrange
        var systemConfiguration = new SystemConfiguration
        {
            Version = 2,
            NetworkInterfacesSettings = new()
            {
                NetworkInterfaces =
                [
                    ValidationTestData.Interface.Static,
                    ValidationTestData.Interface.DHCP
                ]
            },
            NetworkDNSSettings = new()
            {
                Hostname = ValidationTestData.Hostname.Simple,
                DNSSuffixEnabled = true,
                DNSSuffix = ValidationTestData.PrimarySuffix.FQDN,
                StaticHosts = new()
                {
                    Enabled = true,
                    Hosts =
                    [
                        ValidationTestData.StaticHost.StaticHost1,
                        ValidationTestData.StaticHost.StaticHost2
                    ]
                },
                SearchDomainsEnabled = true,
                SearchDomains =
                [
                    ValidationTestData.SearchDomain.Simple,
                    ValidationTestData.SearchDomain.FQDN
                ],
                NameServers = new()
                {
                    Enabled = true,
                    Addresses =
                    [
                        IPAddress.Parse(ValidationTestData.NameServer.IPv4),
                        IPAddress.Parse(ValidationTestData.NameServer.IPv6)
                    ]
                }
            },
            NetworkProxySettings = new NetworkProxySettings() with
            {
                HTTP = ValidationTestData.Proxy.HTTP,
                NoProxy = new NoProxySettings()
                {
                    Enabled = true,
                    Entries =
                    [
                        ValidationTestData.NoProxyItem.IPv4,
                        ValidationTestData.NoProxyItem.IPv4Port,
                        ValidationTestData.NoProxyItem.IPv6,
                        ValidationTestData.NoProxyItem.IPv6Port,
                        ValidationTestData.NoProxyItem.IPv6LinkLocal,
                        ValidationTestData.NoProxyItem.IPv6LinkLocalPort,
                        ValidationTestData.NoProxyItem.Simple,
                        ValidationTestData.NoProxyItem.FQDN
                    ]
                }
            },
            NetworkNTPSettings = new()
            {
                Enabled = true,
                Servers =
                [
                    ValidationTestData.NTPServer.IPv4,
                    ValidationTestData.NTPServer.FQDN
                ]
            }
        };

        // Act
        var result = await new SystemConfigurationValidator()
            .ValidateAsync(systemConfiguration, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ValidationResult>();
        result.EntriesString.Should().NotBe(ValidationConstants.CancelledResult);
    }
}
