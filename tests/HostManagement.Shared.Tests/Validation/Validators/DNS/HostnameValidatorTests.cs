using FluentAssertions;
using Xunit;
using HostManagement.Shared.Validation;
using HostManagement.Shared.Validation.Validators.DNS;

namespace HostManagement.Shared.Tests.Validation.Validators.DNS;

public class HostnameValidatorTests
{
    [Fact]
    public async Task ShouldReturnValidationError_WhenValidationCancelled()
    {
        // Arrange
        var expectedValidationEntry = new ValidationEntry(ValidationConstants.CancelledResult);

        // Act
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var result = await new HostnameValidator()
            .ValidateAsync(ValidationTestData.Hostname.Simple, "dnsSuffix", cts.Token);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public async Task ShouldReturnValidationError_WhenHostnameEmpty(
        string emptyHostname)
    {
        // Arrange
        var expectedValidationEntry = new ValidationEntry("Hostname can not be empty or whitespace.");

        // Act
        var result = await new HostnameValidator().ValidateAsync(emptyHostname, "dnsSuffix");

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
        var result = await new HostnameValidator().ValidateAsync(hostname, dnsSuffix);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldReturnValidationError_WhenHostnameHasTooManyCharacters()
    {
        // Arrange
        var invalidFqdnHostname = string.Concat(new string('a', 31), '.', new string('b', 31), 'x');

        var expectedValidationEntry = new ValidationEntry(
            $"Hostname '{invalidFqdnHostname}' has too many characters " +
            $"(expected max. 63 characters including dots).",
            ErrorCodes.ValidationFailed);

        // Act
        var result = await new HostnameValidator().ValidateAsync(invalidFqdnHostname, "dnsSuffix");

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData(ValidationTestData.Hostname.InvalidHostname, false)]
    [InlineData(ValidationTestData.Hostname.Simple, true)]
    [InlineData(ValidationTestData.Hostname.FQDN, true)]
    public async Task ShouldReturnValidationResult_WhenDomainNameValidatorCalled(
        string hostname,
        bool isValid)
    {
        // Act
        var result = await new HostnameValidator().ValidateAsync(hostname, "dnsSuffix");

        // Assert
        result.IsValid.Should().Be(isValid);
        result.EntriesString.Should().NotContain(ValidationConstants.CancelledResult);
    }
}
