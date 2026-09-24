using AwesomeAssertions;
using HostManagement.Shared.Contracts.Syslog;
using HostManagement.Shared.Validation;
using HostManagement.Shared.Validation.Validators.Syslog;
using Xunit;

namespace HostManagement.Shared.Tests.Validation.Validators.Syslog;

public sealed class SyslogFacilitiesValidatorTests
{
    [Fact]
    public async Task ShouldReturnValidationError_WhenValidationCancelled()
    {
        // Arrange
        IReadOnlyList<SyslogFacility> facilities = [SyslogFacility.Auth];
        var expectedValidationEntry = new ValidationEntry(ValidationConstants.CancelledResult);

        // Act
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        var result = await new SyslogFacilitiesValidator().ValidateAsync(facilities, cts.Token);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldReturnValidationError_WhenFacilityUndefined()
    {
        // Arrange
        IReadOnlyList<SyslogFacility> facilities = [(SyslogFacility)999];

        // Act
        var result = await new SyslogFacilitiesValidator().ValidateAsync(facilities, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
    }

    [Fact]
    public async Task ShouldReturnValidationError_WhenFacilitiesContainDuplicates()
    {
        // Arrange
        IReadOnlyList<SyslogFacility> facilities =
        [
            SyslogFacility.Auth,
            SyslogFacility.Authpriv,
            SyslogFacility.Auth
        ];

        // Act
        var result = await new SyslogFacilitiesValidator().ValidateAsync(facilities, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
    }

    [Fact]
    public async Task ShouldReturnValid_WhenFacilitiesEmpty()
    {
        // Act
        var result = await new SyslogFacilitiesValidator().ValidateAsync([], CancellationToken.None);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Entries.Count.Should().Be(0);
    }

    [Fact]
    public async Task ShouldReturnValid_WhenFacilitiesValid()
    {
        // Arrange
        IReadOnlyList<SyslogFacility> facilities =
        [
            SyslogFacility.Auth,
            SyslogFacility.Authpriv,
            SyslogFacility.SolarisCron,
            SyslogFacility.Local0
        ];

        // Act
        var result = await new SyslogFacilitiesValidator().ValidateAsync(facilities, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Entries.Count.Should().Be(0);
    }
}
