using FluentAssertions;
using Xunit;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Validation;
using HostManagement.Shared.Validation.Validators.DNS;

namespace HostManagement.Shared.Tests.Validation.Validators.DNS;

public class StaticHostsValidatorTests
{
    [Fact]
    public async Task ShouldReturnValidationError_WhenValidationCancelled()
    {
        // Arrange
        var expectedValidationEntry = new ValidationEntry(ValidationConstants.CancelledResult);

        // Act
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var result = await new StaticHostsValidator().ValidateAsync([], cts.Token);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldReturnValidationError_WhenNoStaticHostsSpecified()
    {
        // Arrange
        var expectedValidationEntry = new ValidationEntry($"No static hosts specified.");

        // Act
        var result = await new StaticHostsValidator().ValidateAsync([]);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldReturnValidationError_WhenDuplicateStaticHostsFound()
    {
        // Arrange
        var duplicateStaticHost = ValidationTestData.StaticHost.StaticHost1;
        var staticHosts = new List<StaticHostDetail>
        {
            ValidationTestData.StaticHost.StaticHost1,
            duplicateStaticHost,
            ValidationTestData.StaticHost.StaticHost2
        };

        var expectedValidationEntry = new ValidationEntry(
            $"Static hosts have duplicate entries. Affected entries are: " +
            $"'{duplicateStaticHost.IPAddress}'  '{duplicateStaticHost.Hostname}'.");

        // Act
        var result = await new StaticHostsValidator().ValidateAsync(staticHosts);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldReturnValidationResult_WhenStaticHostValidatorCalled()
    {
        // Arrange
        var invalidStaticHosts = new List<StaticHostDetail>
        {
            ValidationTestData.StaticHost.InvalidLinkLocal,
            ValidationTestData.StaticHost.StaticHost2
        };

        var validStaticHosts = new List<StaticHostDetail>
        {
            ValidationTestData.StaticHost.StaticHost1,
            ValidationTestData.StaticHost.StaticHost2,
        };

        // Act
        var invalidResult = await new StaticHostsValidator().ValidateAsync(invalidStaticHosts);
        var validResult = await new StaticHostsValidator().ValidateAsync(validStaticHosts);

        // Assert
        invalidResult.IsValid.Should().BeFalse();
        invalidResult.EntriesString.Should().NotContain(ValidationConstants.CancelledResult);

        validResult.IsValid.Should().BeTrue();
        validResult.EntriesString.Should().NotContain(ValidationConstants.CancelledResult);
    }
}
