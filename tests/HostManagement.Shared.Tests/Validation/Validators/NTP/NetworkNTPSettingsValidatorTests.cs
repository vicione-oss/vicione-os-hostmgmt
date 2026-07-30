using AwesomeAssertions;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Validation;
using HostManagement.Shared.Validation.Validators.NTP;
using Xunit;

namespace HostManagement.Shared.Tests.Validation.Validators;

public class NetworkNTPSettingsValidatorTests
{
    [Fact]
    public async Task ShouldReturnValidationError_WhenValidationCancelled()
    {
        // Arrange
        var ntpSettings = new NetworkNTPSettings();
        var expectedValidationEntry = new ValidationEntry(ValidationConstants.CancelledResult);

        // Act
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        var result = await new NetworkNTPSettingsValidator().ValidateAsync(ntpSettings, cts.Token);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData(false, ValidationTestData.NTPServer.FQDN, true)]
    [InlineData(false, ValidationTestData.NTPServer.InvalidIPv6, true)]
    [InlineData(true, ValidationTestData.NTPServer.InvalidIPv6, false)]
    [InlineData(true, ValidationTestData.NTPServer.FQDN, true)]
    public async Task ShouldReturnValidationResult_WhenValidateAsyncCalled(bool enabled, string server, bool isValid)
    {
        // Arrange
        var ntpSettings = new NetworkNTPSettings
        {
            Enabled = enabled,
            Servers = [ValidationTestData.NTPServer.IPv4, server]
        };

        // Act
        var result = await new NetworkNTPSettingsValidator().ValidateAsync(ntpSettings, CancellationToken.None);

        // Assert
        result.IsValid.Should().Be(isValid);
        result.EntriesString.Should().NotBe(ValidationConstants.CancelledResult);
    }
}
