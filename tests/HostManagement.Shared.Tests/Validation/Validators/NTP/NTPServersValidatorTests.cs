using AwesomeAssertions;
using HostManagement.Shared.Validation;
using HostManagement.Shared.Validation.Validators.NTP;
using Xunit;

namespace HostManagement.Shared.Tests.Validation.Validators.NTP;

public class NTPServersValidatorTests
{
    [Fact]
    public async Task ShouldReturnValidationError_WhenValidationCancelled()
    {
        // Arrange
        var ntpServers = new List<string>
        {
            ValidationTestData.NTPServer.IPv4,
            ValidationTestData.NTPServer.IPv6,
            ValidationTestData.NTPServer.FQDN
        };

        var expectedValidationEntry = new ValidationEntry(ValidationConstants.CancelledResult);

        // Act
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        var result = await new NTPServersValidator().ValidateAsync(ntpServers, cts.Token);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldReturnValidationError_WhenNoNTPServersSpecified()
    {
        // Arrange
        var ntpServers = new List<string>();
        var expectedValidationEntry = new ValidationEntry("No NTP servers specified.");

        // Act
        var result = await new NTPServersValidator().ValidateAsync(ntpServers, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldReturnValidationError_WhenDuplicateNTPServersFound()
    {
        // Arrange
        var duplicateNTPServer = ValidationTestData.NTPServer.FQDN;
        var ntpServers = new List<string>
        {
            ValidationTestData.NTPServer.FQDN,
            duplicateNTPServer,
            ValidationTestData.NTPServer.IPv4,
            ValidationTestData.NTPServer.IPv6,
        };

        var expectedValidationEntry = new ValidationEntry(
            $"NTP servers have duplicate entries. Affected entries are: '{duplicateNTPServer}'.");

        // Act
        var result = await new NTPServersValidator().ValidateAsync(ntpServers, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData(ValidationTestData.NTPServer.InvalidIPv4, false)]
    [InlineData(ValidationTestData.NTPServer.FQDN, true)]
    public async Task ShouldAddValidationResult_WhenNTPServerValidatorCalled(
        string ntpServer,
        bool isValid)
    {
        // Act
        var result = await new NTPServerValidator().ValidateAsync(ntpServer, CancellationToken.None);

        // Assert
        result.IsValid.Should().Be(isValid);
        result.EntriesString.Should().NotContain(ValidationConstants.CancelledResult);
    }

    [Fact]
    public async Task ShouldReturnValid_WhenNTPServersValid()
    {
        // Arrange
        var ntpServers = new List<string>
        {
            ValidationTestData.NTPServer.FQDN,
            ValidationTestData.NTPServer.IPv4,
            ValidationTestData.NTPServer.IPv6,
        };

        // Act
        var result = await new NTPServersValidator().ValidateAsync(ntpServers, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Entries.Count.Should().Be(0);
    }
}
