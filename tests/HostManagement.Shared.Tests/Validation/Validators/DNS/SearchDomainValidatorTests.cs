using AwesomeAssertions;
using HostManagement.Shared.Validation;
using HostManagement.Shared.Validation.Validators.DNS;
using Xunit;

namespace HostManagement.Shared.Tests.Validation.Validators.DNS;

public class SearchDomainValidatorTests
{
    [Fact]
    public async Task ShouldReturnValidationError_WhenValidationCancelled()
    {
        // Arrange
        var searchDomain = ValidationTestData.SearchDomain.FQDN;
        var expectedValidationEntry = new ValidationEntry(ValidationConstants.CancelledResult);

        // Act
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var result = await new SearchDomainValidator().ValidateAsync(searchDomain, cts.Token);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public async Task ShouldReturnValidationError_WhenSearchDomainEmpty(
        string emptySearchDomain)
    {
        // Arrange
        var expectedValidationEntry = new ValidationEntry("Search domain can not be empty or whitespace.");

        // Act
        var result = await new SearchDomainValidator().ValidateAsync(emptySearchDomain, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData(ValidationTestData.SearchDomain.InvalidDomain, false)]
    [InlineData(ValidationTestData.SearchDomain.FQDN, true)]
    public async Task ShouldReturnValidationResult_WhenDomainNameValidatorCalled(
        string searchDomain,
        bool isValid)
    {
        // Act
        var result = await new SearchDomainValidator().ValidateAsync(searchDomain, CancellationToken.None);

        // Assert
        result.IsValid.Should().Be(isValid);
        result.EntriesString.Should().NotContain(ValidationConstants.CancelledResult);
    }
}
