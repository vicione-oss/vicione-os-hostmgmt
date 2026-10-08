using AwesomeAssertions;
using HostManagement.Shared.Contracts.Syslog;
using HostManagement.Shared.Validation;
using HostManagement.Shared.Validation.Validators.Syslog;
using Xunit;

namespace HostManagement.Shared.Tests.Validation.Validators.Syslog;

public sealed class SyslogSettingsValidatorTests
{
    [Fact]
    public async Task ShouldReturnValidationError_WhenValidationCancelled()
    {
        // Arrange
        var expectedValidationEntry = new ValidationEntry(ValidationConstants.CancelledResult);

        // Act
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        var result = await new SyslogSettingsValidator()
            .ValidateAsync(ValidationTestData.Syslog.ConfiguredEnabled, cts.Token);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldReturnValidationError_WhenDisabledConfigurationInvalid()
    {
        // Arrange
        var settings = ValidationTestData.Syslog.ConfiguredDisabled with
        {
            Address = "invalid-address"
        };

        // Act
        var result = await new SyslogSettingsValidator().ValidateAsync(settings, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ShouldReturnValidationError_WhenTransportProtocolUndefined()
    {
        // Arrange
        var settings = ValidationTestData.Syslog.ConfiguredEnabled with
        {
            TransportProtocol = (SyslogTransportProtocol)999
        };

        // Act
        var result = await new SyslogSettingsValidator().ValidateAsync(settings, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ShouldReturnValidationError_WhenLogFormatUndefined()
    {
        // Arrange
        var settings = ValidationTestData.Syslog.ConfiguredEnabled with
        {
            LogFormat = (SyslogLogFormat)999
        };

        // Act
        var result = await new SyslogSettingsValidator().ValidateAsync(settings, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ShouldReturnValidationError_WhenRetainedSettingsHaveNoAddress()
    {
        // Arrange
        var settings = new SyslogSettings
        {
            Enabled = false,
            ConnectionRetrySeconds = 5
        };

        // Act
        var result = await new SyslogSettingsValidator().ValidateAsync(settings, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ShouldReturnValidationError_WhenFacilitiesNull()
    {
        // Arrange
        var settings = ValidationTestData.Syslog.ConfiguredEnabled with
        {
            ExcludeSyslogFacilities = null!
        };

        // Act
        var result = await new SyslogSettingsValidator().ValidateAsync(settings, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Should().Contain(new ValidationEntry("Excluded syslog facilities can not be null."));
    }

    [Fact]
    public async Task ShouldReturnValidationError_WhenLevelsNull()
    {
        // Arrange
        var settings = ValidationTestData.Syslog.ConfiguredEnabled with
        {
            ExcludeSyslogLevels = null!
        };

        // Act
        var result = await new SyslogSettingsValidator().ValidateAsync(settings, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Should().Contain(new ValidationEntry("Excluded syslog levels can not be null."));
    }

    [Fact]
    public async Task ShouldReturnValid_WhenSettingsCleared()
    {
        // Act
        var result = await new SyslogSettingsValidator().ValidateAsync(new SyslogSettings(), CancellationToken.None);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Entries.Count.Should().Be(0);
    }

    [Fact]
    public async Task ShouldReturnValid_WhenSettingsConfiguredAndDisabled()
    {
        // Act
        var result = await new SyslogSettingsValidator()
            .ValidateAsync(ValidationTestData.Syslog.ConfiguredDisabled, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Entries.Count.Should().Be(0);
    }

    [Fact]
    public async Task ShouldReturnValid_WhenSettingsConfiguredAndEnabled()
    {
        // Act
        var result = await new SyslogSettingsValidator()
            .ValidateAsync(ValidationTestData.Syslog.ConfiguredEnabled, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Entries.Count.Should().Be(0);
    }
}
