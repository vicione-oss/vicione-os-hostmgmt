using System.Globalization;
using System.Text;
using HostManagement.Shared.Validation.Utils;

namespace HostManagement.Shared.Validation.Validators.DNS;

/// <summary>
/// Represents the validator for a domain label.
/// </summary>
/// <param name="customDomainLabelText">A custom domain label text, to be used at the beginning
/// of the validation result message, e.g. "Hostname" or "Static host hostname".</param>
internal sealed class DomainLabelValidator(string? customDomainLabelText = null) : IValidator<string>
{
    private readonly string _customDomainLabelText = customDomainLabelText is null
        ? "Domain label"
        : customDomainLabelText;

    /// <summary>
    /// Validates a domain label asynchronously.
    /// </summary>
    /// <param name="domainLabel">The domain label.</param>
    /// <param name="cancellationToken">A token to cancel the validation.</param>
    /// <inheritdoc/>
    public Task<ValidationResult> ValidateAsync(string domainLabel, CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (FQDNValidatorUtils.HasDomainLabelInvalidCharacterCount(domainLabel))
            {
                return Task.FromResult(
                    ValidationResult.CreateWithError(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            CompositeFormat.Parse(
                                DNSValidationResults.DomainLabelHasTooManyCharacters),
                            _customDomainLabelText,
                            domainLabel)));
            }

            if (FQDNValidatorUtils.HasDomainLabelOnlyDigits(domainLabel))
            {
                return Task.FromResult(
                    ValidationResult.CreateWithError(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            CompositeFormat.Parse(
                                DNSValidationResults.DomainLabelCanNotHaveOnlyDigits),
                            _customDomainLabelText,
                            domainLabel)));
            }

            if (FQDNValidatorUtils.HasDomainLabelInvalidCharacter(domainLabel))
            {
                return Task.FromResult(
                    ValidationResult.CreateWithError(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            CompositeFormat.Parse(
                                DNSValidationResults.DomainLabelHasInvalidCharacter),
                            _customDomainLabelText,
                            domainLabel)));
            }

            return Task.FromResult(ValidationResult.Valid);
        }
        catch (OperationCanceledException)
        {
            return Task.FromResult(
                ValidationResult.CreateWithError(
                    ValidationResults.ValidationCancelled));
        }
    }
}
