using AwesomeAssertions;
using HostManagement.Shared.Contracts.Syslog;
using HostManagement.Shared.Validation;
using HostManagement.Shared.Validation.Validators.Syslog;
using Xunit;

namespace HostManagement.Shared.Tests.Validation.Validators.Syslog;

public sealed class SyslogLevelsValidatorTests
{
    [Fact]
    public async Task ShouldReturnValidationError_WhenValidationCancelled()
    {
        // Arrange
        IReadOnlyList<SyslogLevel> levels = [SyslogLevel.Debug];
        var expectedValidationEntry = new ValidationEntry(ValidationConstants.CancelledResult);

        // Act
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        var result = await new SyslogLevelsValidator().ValidateAsync(levels, cts.Token);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldReturnValidationError_WhenLevelUndefined()
    {
        // Arrange
        IReadOnlyList<SyslogLevel> levels = [(SyslogLevel)999];

        // Act
        var result = await new SyslogLevelsValidator().ValidateAsync(levels, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
    }

    [Fact]
    public async Task ShouldReturnValidationError_WhenLevelsContainDuplicates()
    {
        // Arrange
        IReadOnlyList<SyslogLevel> levels = [SyslogLevel.Debug, SyslogLevel.Info, SyslogLevel.Debug];

        // Act
        var result = await new SyslogLevelsValidator().ValidateAsync(levels, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
    }

    [Fact]
    public async Task ShouldReturnValid_WhenLevelsEmpty()
    {
        // Act
        var result = await new SyslogLevelsValidator().ValidateAsync([], CancellationToken.None);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Entries.Count.Should().Be(0);
    }

    [Fact]
    public async Task ShouldReturnValid_WhenLevelsValid()
    {
        // Arrange
        IReadOnlyList<SyslogLevel> levels = [SyslogLevel.Debug, SyslogLevel.Info, SyslogLevel.Warning];

        // Act
        var result = await new SyslogLevelsValidator().ValidateAsync(levels, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Entries.Count.Should().Be(0);
    }
}
