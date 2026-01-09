namespace HostManagement.Shared.Validation.Validators.DNS;

/// <summary>
/// Represents the validator for a simple domain name or a fully qualified domain name (FQDN).
/// </summary>
/// <param name="customDomainNameText">A custom domain name text, to be used at the beginning
/// of the validation result message, e.g. "Hostname" or "Static host hostname".</param>
/// <param name="allowLeadingDot">Determines whether a leading dot is allowed during validation,
/// e.g. for no-proxy items.</param>
internal sealed class DomainNameValidator(string? customDomainNameText = null, bool allowLeadingDot = false)
    : IValidator<string>
{
    private readonly string _customDomainNameText = customDomainNameText is null
        ? "Domain"
        : customDomainNameText;

    private readonly bool _allowLeadingDot = allowLeadingDot;

    /// <summary>
    /// Validates a simple domain name or a fully qualified domain name (FQDN) asynchronously.
    /// </summary>
    /// <param name="domainName">The domain name.</param>
    /// <param name="cancellationToken">A token to cancel the validation.</param>
    /// <inheritdoc/>
    public async Task<ValidationResult> ValidateAsync(
        string domainName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (domainName.Contains('.'))
            {
                // Fully Qualified Domain Name (FQDN)

                return await new FQDNValidator(_customDomainNameText, _allowLeadingDot)
                    .ValidateAsync(domainName, cancellationToken)
                    .ConfigureAwait(false);
            }
            else
            {
                // Simple domain name

                return await new DomainLabelValidator(_customDomainNameText)
                    .ValidateAsync(domainName, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            return ValidationResult.CreateWithError(ValidationResults.ValidationCancelled);
        }
    }
}
