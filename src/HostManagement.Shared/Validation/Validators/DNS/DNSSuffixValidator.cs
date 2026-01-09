using System.Globalization;
using System.Text;
using HostManagement.Shared.Validation.Utils;

namespace HostManagement.Shared.Validation.Validators.DNS;

/// <summary>
/// Represents the validator for primary DNS suffix. Takes the hostname into account.
/// </summary>
public sealed class DNSSuffixValidator : IValidator<string, string>
{
    private const string PrimaryDnsSuffixText = "Primary DNS suffix";

    /// <summary>
    /// Validates a primary DNS suffix asynchronously, taking the hostname into account.
    /// </summary>
    /// <param name="dnsSuffix">The primary DNS suffix.</param>
    /// <param name="hostname">The hostname.</param>
    /// <param name="cancellationToken">A token to cancel the validation.</param>
    /// <inheritdoc/>
    public async Task<ValidationResult> ValidateAsync(
        string dnsSuffix,
        string hostname,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(dnsSuffix))
                return ValidationResult.CreateWithError(DNSValidationResults.SuffixCanNotBeEmpty);

            // Hostname + '.' + DNSSuffix = FQDN must be max. 253 characters
            if (FQDNValidatorUtils.HasDomainNameTooManyCharacters(hostname + '.' + dnsSuffix))
            {
                return ValidationResult.CreateWithError(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        CompositeFormat.Parse(
                            DNSValidationResults.HostnameAndSuffixHaveTooManyCharacters),
                        hostname,
                        dnsSuffix));
            }

            /*
             * A trailing dot in a DNS suffix is regarded as invalid. Most resolvers automatically
             * append the root dot during DNS queries. Linux typically strips the trailing dot when
             * saving the primary DNS suffix.
             * 
             * Single-label DNS suffixes should only be used for internal networks.
            */

            return await new DomainNameValidator(PrimaryDnsSuffixText)
                .ValidateAsync(dnsSuffix, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return ValidationResult.CreateWithError(ValidationResults.ValidationCancelled);
        }
    }
}
