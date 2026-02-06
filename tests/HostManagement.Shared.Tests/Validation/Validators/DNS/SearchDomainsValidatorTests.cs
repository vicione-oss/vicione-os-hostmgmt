using AwesomeAssertions;
using HostManagement.Shared.Validation;
using HostManagement.Shared.Validation.Validators.DNS;
using Xunit;

namespace HostManagement.Shared.Tests.Validation.Validators.DNS;

public class SearchDomainsValidatorTests
{
    [Fact]
    public async Task ShouldReturnValidationError_WhenValidationCancelled()
    {
        // Arrange
        var expectedValidationEntry = new ValidationEntry(ValidationConstants.CancelledResult);

        // Act
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var result = await new SearchDomainsValidator().ValidateAsync([], cts.Token);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldReturnValidationError_WhenNoSearchDomainsSpecified()
    {
        // Arrange
        var expectedValidationEntry = new ValidationEntry($"No search domains specified.");

        // Act
        var result = await new SearchDomainsValidator().ValidateAsync([], CancellationToken.None);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldReturnValidationError_WhenDuplicateSearchDomainsFound()
    {
        // Arrange
        var duplicateSearchDomain = ValidationTestData.SearchDomain.Simple;
        var searchDomains = new List<string>
        {
            ValidationTestData.SearchDomain.Simple,
            duplicateSearchDomain,
            ValidationTestData.SearchDomain.FQDN
        };

        var expectedValidationEntry = new ValidationEntry(
            $"Search domains have duplicate entries. Affected entries are: '{duplicateSearchDomain}'.");

        // Act
        var result = await new SearchDomainsValidator().ValidateAsync(searchDomains, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData(ValidationTestData.SearchDomain.InvalidDomain, false)]
    [InlineData(ValidationTestData.SearchDomain.FQDN, true)]
    public async Task ShouldReturnValidationResult_WhenSearchDomainValidatorCalled(
        string searchDomain,
        bool isValid)
    {
        // Arrange
        var searchDomains = new List<string>
        {
            ValidationTestData.SearchDomain.Simple,
            searchDomain
        };

        // Act
        var result = await new SearchDomainsValidator().ValidateAsync(searchDomains, CancellationToken.None);

        // Assert
        result.IsValid.Should().Be(isValid);
        result.EntriesString.Should().NotContain(ValidationConstants.CancelledResult);
    }
}
