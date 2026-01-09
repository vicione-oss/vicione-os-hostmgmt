using AwesomeAssertions;
using Xunit;
using HostManagement.Shared.Validation;
using HostManagement.Shared.Validation.Validators.DNS;

namespace HostManagement.Shared.Tests.Validation.Validators.DNS;

public class DomainNameValidatorTests
{
    private const string CustomDomainNameText = "A custom domain name text";

    [Fact]
    public async Task ShouldReturnValidationError_WhenValidationCancelled()
    {
        // Arrange
        var expectedValidationEntry = new ValidationEntry(ValidationConstants.CancelledResult);

        // Act
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var result = await new DomainNameValidator()
            .ValidateAsync(ValidationTestData.Domain.Simple, cts.Token);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(CustomDomainNameText)]
    public async Task ShouldReturnValidationResult_WhenFQDNValidatorCalled(
        string? customDomainNameText)
    {
        // Act
        var result = await new DomainNameValidator(customDomainNameText)
            .ValidateAsync(ValidationTestData.Domain.FQDN);

        // Assert
        result.Should().BeOfType<ValidationResult>();
        result.EntriesString.Should().NotContain(ValidationConstants.CancelledResult);
    }

    [Theory]
    [InlineData(null, ValidationTestData.Domain.FQDN, true)]
    [InlineData(CustomDomainNameText, ValidationTestData.Domain.FQDN, true)]
    [InlineData(null, ValidationTestData.Domain.InvalidDomain, false)]
    [InlineData(CustomDomainNameText, ValidationTestData.Domain.InvalidDomain, false)]
    public async Task ShouldReturnValidationResult_WhenDomainLabelValidatorCalled(
        string? customDomainNameText,
        string domain,
        bool isValid)
    {
        // Act
        var result = await new DomainNameValidator(customDomainNameText).ValidateAsync(domain);

        // Assert
        result.IsValid.Should().Be(isValid);
        result.EntriesString.Should().NotContain(ValidationConstants.CancelledResult);
    }
}
