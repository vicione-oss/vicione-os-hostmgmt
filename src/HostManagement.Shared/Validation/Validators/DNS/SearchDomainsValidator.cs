using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace HostManagement.Shared.Validation.Validators.DNS;

/// <summary>
/// Represents the validator for a list of search domains.
/// </summary>
public sealed class SearchDomainsValidator : IValidator<IReadOnlyList<string>>
{
    /// <summary>
    /// Validates a list of search domains asynchronously.
    /// </summary>
    /// <param name="searchDomains">The list of search domains.</param>
    /// <param name="cancellationToken">A token to cancel the validation.</param>
    /// <inheritdoc/>
    public async Task<ValidationResult> ValidateAsync(
        IReadOnlyList<string> searchDomains,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (searchDomains.Count == 0)
                return ValidationResult.CreateWithError(DNSValidationResults.NoSearchDomains);

            if (HasDuplicateSearchDomains(searchDomains, out var duplicateSearchDomainsString))
            {
                return ValidationResult.CreateWithError(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        CompositeFormat.Parse(
                            DNSValidationResults.SearchDomainsHaveDuplicateEntries),
                        duplicateSearchDomainsString));
            }

            var result = new ValidationResult();
            var searchDomainValidator = new SearchDomainValidator();

            foreach (var searchDomain in searchDomains)
            {
                result.AddResult(
                    await searchDomainValidator
                        .ValidateAsync(searchDomain, cancellationToken)
                        .ConfigureAwait(false));
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            return ValidationResult.CreateWithError(ValidationResults.ValidationCancelled);
        }
    }

    private static bool HasDuplicateSearchDomains(
        IReadOnlyList<string> searchDomains,
        [NotNullWhen(true)] out string? duplicateSearchDomainsString)
    {
        duplicateSearchDomainsString = null;

        var duplicateSearchDomains = searchDomains
            .GroupBy(searchDomain => searchDomain)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        if (duplicateSearchDomains.Count > 0)
        {
            duplicateSearchDomainsString = string.Join(
                ", ",
                duplicateSearchDomains.Select(domain => $"'{domain}'"));

            return true;
        }

        return false;
    }
}
