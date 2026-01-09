using System.Globalization;
using System.Text;
using HostManagement.Shared.Validation.Utils;

namespace HostManagement.Shared.Validation.Validators.DNS;

/// <summary>
/// Represents the validator for a fully qualified domain name (FQDN).
/// </summary>
/// <param name="customFQDNText">A custom FQDN text, to be used at the beginning of the
/// validation result message, e.g. "Hostname" or "Static host hostname".</param>
/// <param name="allowLeadingDot">Determines whether a leading dot is allowed during validation,
/// e.g. for no-proxy items.</param>
internal sealed class FQDNValidator(string? customFQDNText = null, bool allowLeadingDot = false) : IValidator<string>
{
    private readonly string _customFQDNText = customFQDNText is null ? "FQDN" : customFQDNText;
    private readonly bool _allowLeadingDot = allowLeadingDot;

    /// <summary>
    /// Validates a fully qualified domain name (FQDN) asynchronously.
    /// </summary>
    /// <param name="fqdn">The fully qualified domain name (FQDN).</param>
    /// <param name="cancellationToken">A token to cancel the validation.</param>
    /// <inheritdoc/>
    public Task<ValidationResult> ValidateAsync(string fqdn, CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (FQDNValidatorUtils.HasDomainNameTooManyCharacters(fqdn))
            {
                return Task.FromResult(
                    ValidationResult.CreateWithError(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            CompositeFormat.Parse(
                                DNSValidationResults.FQDNHasTooManyCharacters),
                            _customFQDNText,
                            fqdn)));
            }

            if (FQDNValidatorUtils.HasDomainNameLeadingOrTrailingDots(fqdn, _allowLeadingDot))
            {
                return Task.FromResult(
                    ValidationResult.CreateWithError(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            CompositeFormat.Parse(
                                DNSValidationResults.FQDNHasLeadingTrailingDots),
                            _customFQDNText,
                            fqdn)));
            }

            if (FQDNValidatorUtils.HasDomainNameConsecutiveDots(fqdn))
            {
                return Task.FromResult(
                    ValidationResult.CreateWithError(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            CompositeFormat.Parse(
                                DNSValidationResults.FQDNHasConsecutiveDots),
                            _customFQDNText,
                            fqdn)));
            }

            var domainLabels = fqdn.Split('.');

            if (FQDNValidatorUtils.HasLessThanTwoDomainLabels(domainLabels))
            {
                return Task.FromResult(
                    ValidationResult.CreateWithError(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            CompositeFormat.Parse(
                                DNSValidationResults.FQDNMustHaveAtLeastTwoDomainLabels),
                            _customFQDNText,
                            fqdn)));
            }

            if (FQDNValidatorUtils.HasMoreThanOneNumericLabel(domainLabels))
            {
                return Task.FromResult(
                    ValidationResult.CreateWithError(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            CompositeFormat.Parse(
                                DNSValidationResults.FQDNHasMoreThanOneNumericLabel),
                            _customFQDNText,
                            fqdn)));
            }

            var result = new ValidationResult();

            for (var i = 0; i < domainLabels.Length; i++)
            {
                if (i < domainLabels.Length - 1)
                {
                    if (FQDNValidatorUtils.HasDomainLabelInvalidCharacterCount(domainLabels[i]))
                    {
                        result.AddError(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                CompositeFormat.Parse(
                                    DNSValidationResults.FQDNHasInvalidDomainLabelCharacterCount),
                                _customFQDNText,
                                fqdn,
                                domainLabels[i]));
                    }
                }
                else
                {
                    if (FQDNValidatorUtils.HasTopLevelDomainInvalidCharacterCount(domainLabels[i]))
                    {
                        result.AddError(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                CompositeFormat.Parse(
                                    DNSValidationResults.FQDNHasInvalidTopLevelDomainCharacterCount),
                                _customFQDNText,
                                fqdn));
                    }
                }

                if (FQDNValidatorUtils.HasDomainLabelInvalidCharacter(domainLabels[i]))
                {
                    result.AddError(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            CompositeFormat.Parse(
                                DNSValidationResults.FQDNHasInvalidDomainLabelCharacter),
                            _customFQDNText,
                            fqdn,
                            domainLabels[i]));
                }
            }

            return Task.FromResult(result);

        }
        catch (OperationCanceledException)
        {
            return Task.FromResult(
                ValidationResult.CreateWithError(
                    ValidationResults.ValidationCancelled));
        }
    }
}
