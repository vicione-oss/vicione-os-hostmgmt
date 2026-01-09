namespace HostManagement.Shared.Validation.Validators.DNS;

/// <summary>
/// Represents the validator for a search domain.
/// </summary>
public sealed class SearchDomainValidator : IValidator<string>
{
    private const string SearchDomainText = "Search domain";

    /// <summary>
    /// Validates a search domain asynchronously.
    /// </summary>
    /// <param name="searchDomain">The search domain.</param>
    /// <param name="cancellationToken">A token to cancel the validation.</param>
    /// <inheritdoc/>
    public async Task<ValidationResult> ValidateAsync(
        string searchDomain,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(searchDomain))
                return ValidationResult.CreateWithError(DNSValidationResults.SearchDomainCanNotBeEmpty);

            return await new DomainNameValidator(SearchDomainText)
                .ValidateAsync(searchDomain, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return ValidationResult.CreateWithError(ValidationResults.ValidationCancelled);
        }
    }
}
