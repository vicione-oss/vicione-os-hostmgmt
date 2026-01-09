using System.Globalization;
using System.Text;
using HostManagement.Shared.Validation.Utils;

namespace HostManagement.Shared.Validation.Validators.DNS;

/// <summary>
/// Represents the validator for a Linux hostname. Takes the primary DNS suffix into account.
/// </summary>
public sealed class HostnameValidator : IValidator<string, string>
{
    private const string HostnameText = "Hostname";

    /// <summary>
    /// Validates a Linux hostname asynchronously, taking the primary DNS suffix into account.
    /// </summary>
    /// <remarks>
    /// Enforces strict validation for system compatibility and only allows setting short hostnames
    /// (see <see href="https://www.man7.org/linux/man-pages/man1/hostnamectl.1.html">manpage hostnamectl</see>).
    /// </remarks>
    /// <param name="hostname">The hostname.</param>
    /// <param name="dnsSuffix">The primary DNS suffix.</param>
    /// <param name="cancellationToken">A token to cancel the validation.</param>
    /// <inheritdoc/>
    public async Task<ValidationResult> ValidateAsync(
        string hostname,
        string dnsSuffix,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(hostname))
                return ValidationResult.CreateWithError(DNSValidationResults.HostnameCanNotBeEmpty);

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

            // Enforce strict validation with max. 63 characters
            if (FQDNValidatorUtils.HasDomainLabelInvalidCharacterCount(hostname))
            {
                return ValidationResult
                    .CreateWithError(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            CompositeFormat.Parse(
                                DNSValidationResults.HostnameHasTooManyCharacters),
                            hostname));
            }

            return await new DomainNameValidator(HostnameText)
                .ValidateAsync(hostname, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return ValidationResult.CreateWithError(ValidationResults.ValidationCancelled);
        }
    }
}
