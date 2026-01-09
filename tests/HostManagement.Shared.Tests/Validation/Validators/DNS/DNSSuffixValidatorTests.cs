using FluentAssertions;
using Xunit;
using HostManagement.Shared.Validation;
using HostManagement.Shared.Validation.Validators.DNS;

namespace HostManagement.Shared.Tests.Validation.Validators.DNS;

public class DNSSuffixValidatorTests
{
    [Fact]
    public async Task ShouldReturnValidationError_WhenValidationCancelled()
    {
        // Arrange
        var expectedValidationEntry = new ValidationEntry(ValidationConstants.CancelledResult);

        // Act
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var result = await new DNSSuffixValidator()
            .ValidateAsync(ValidationTestData.PrimarySuffix.Simple, "hostname", cts.Token);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public async Task ShouldReturnValidationError_WhenDNSSuffixEmpty(
        string emptySuffix)
    {
        // Arrange
        var expectedValidationEntry = new ValidationEntry("DNS suffix can not be empty or whitespace.");

        // Act
        var result = await new DNSSuffixValidator().ValidateAsync(emptySuffix, "hostname");

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldReturnValidationError_WhenHostnameAndDNSSuffixTooLong()
    {
        // Arrange
        var hostname = new string('a', 222);
        var dnsSuffix = string.Concat(new string('b', 27), ".com");

        var expectedValidationEntry = new ValidationEntry(
            $"Fully Qualified Domain Name (FQDN) consisting of hostname '{hostname}' and " +
            $"primary DNS suffix '{dnsSuffix}' has too many characters " +
            $"(expected max. 253 characters including dots).");

        // Act
        var result = await new DNSSuffixValidator().ValidateAsync(dnsSuffix, hostname);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData(ValidationTestData.PrimarySuffix.InvalidSuffix, false)]
    [InlineData(ValidationTestData.PrimarySuffix.Simple, true)]
    [InlineData(ValidationTestData.PrimarySuffix.FQDN, true)]
    public async Task ShouldReturnValidationResult_WhenDomainNameValidatorCalled(
        string dnsSuffix,
        bool isValid)
    {
        // Act
        var result = await new DNSSuffixValidator().ValidateAsync(dnsSuffix, "hostname");

        // Assert
        result.IsValid.Should().Be(isValid);
        result.EntriesString.Should().NotContain(ValidationConstants.CancelledResult);
    }
}
