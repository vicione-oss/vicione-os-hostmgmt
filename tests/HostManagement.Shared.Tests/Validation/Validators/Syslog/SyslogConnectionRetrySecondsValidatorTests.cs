using AwesomeAssertions;
using HostManagement.Shared.Validation;
using HostManagement.Shared.Validation.Validators.Syslog;
using Xunit;

namespace HostManagement.Shared.Tests.Validation.Validators.Syslog;

public sealed class SyslogConnectionRetrySecondsValidatorTests
{
    [Fact]
    public async Task ShouldReturnValidationError_WhenValidationCancelled()
    {
        // Arrange
        var expectedValidationEntry = new ValidationEntry(ValidationConstants.CancelledResult);

        // Act
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        var result = await new SyslogConnectionRetrySecondsValidator().ValidateAsync(30, cts.Token);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    [InlineData(0)]
    public async Task ShouldReturnValidationError_WhenConnectionRetrySecondsTooSmall(int retrySeconds)
    {
        // Act
        var result = await new SyslogConnectionRetrySecondsValidator()
            .ValidateAsync(retrySeconds, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(30)]
    [InlineData(int.MaxValue)]
    public async Task ShouldReturnValid_WhenConnectionRetrySecondsValid(int retrySeconds)
    {
        // Act
        var result = await new SyslogConnectionRetrySecondsValidator()
            .ValidateAsync(retrySeconds, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Entries.Count.Should().Be(0);
    }
}
