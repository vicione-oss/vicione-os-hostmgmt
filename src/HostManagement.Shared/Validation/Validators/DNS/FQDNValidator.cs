using System.Globalization;
using System.Text;
using HostManagement.Shared.Validation.Utils;

namespace HostManagement.Shared.Validation.Validators.DNS;

/// <summary>
/// Represents the validator for a fully qualified domain name (FQDN).
/// </summary>
internal sealed class FQDNValidator
{
    /// <summary>
    /// Validates a fully qualified domain name (FQDN).
    /// </summary>
    /// <param name="fqdn">The fully qualified domain name (FQDN).</param>
    /// <param name="result">The validation result.</param>
    /// <param name="customFQDNName">A custom name to be used in the result message, e.g.
    /// "Hostname" or "Static host hostname".</param>
    /// <param name="shouldAllowLeadingDot">Should a leading dot be allowed? For example for
    /// "DoNotProxy" items like '.company.com'.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static Task Validate(
        string fqdn,
        ValidationResult result,
        string customFQDNName = "FQDN",
        bool shouldAllowLeadingDot = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(customFQDNName);

        if (FQDNValidatorUtils.HasDomainNameTooManyCharacters(fqdn))
        {
            result.AddErrorMessage(
                string.Format(
                    CultureInfo.InvariantCulture,
                    CompositeFormat.Parse(
                        ValidationResultMessages.FQDNHasTooManyCharacters),
                    customFQDNName,
                    fqdn),
                ErrorCodes.ValidationFailed);

            return Task.CompletedTask;
        }

        if (FQDNValidatorUtils.HasDomainNameLeadingOrTrailingDots(fqdn, shouldAllowLeadingDot))
        {
            result.AddErrorMessage(
                string.Format(
                    CultureInfo.InvariantCulture,
                    CompositeFormat.Parse(
                        ValidationResultMessages.FQDNHasLeadingTrailingDots),
                    customFQDNName,
                    fqdn),
                ErrorCodes.ValidationFailed);

            return Task.CompletedTask;
        }

        if (FQDNValidatorUtils.HasDomainNameConsecutiveDots(fqdn))
        {
            result.AddErrorMessage(
                string.Format(
                    CultureInfo.InvariantCulture,
                    CompositeFormat.Parse(
                        ValidationResultMessages.FQDNHasConsecutiveDots),
                    customFQDNName,
                    fqdn),
                ErrorCodes.ValidationFailed);

            return Task.CompletedTask;
        }

        var domainLabels = fqdn.Split('.');

        if (FQDNValidatorUtils.HasLessThanTwoDomainLabels(domainLabels))
        {
            result.AddErrorMessage(
                string.Format(
                    CultureInfo.InvariantCulture,
                    CompositeFormat.Parse(
                        ValidationResultMessages.FQDNMustHaveAtLeastTwoDomainLabels),
                    customFQDNName,
                    fqdn),
                ErrorCodes.ValidationFailed);

            return Task.CompletedTask;
        }

        if (FQDNValidatorUtils.HasMoreThanOneNumericLabel(domainLabels))
        {
            result.AddErrorMessage(
                string.Format(
                    CultureInfo.InvariantCulture,
                    CompositeFormat.Parse(
                        ValidationResultMessages.FQDNHasMoreThanOneNumericLabel),
                    customFQDNName,
                    fqdn),
                ErrorCodes.ValidationFailed);

            return Task.CompletedTask;
        }

        for (var i = 0; i < domainLabels.Length; i++)
        {
            if (i < domainLabels.Length - 1)
            {
                if (FQDNValidatorUtils.HasDomainLabelInvalidCharacterCount(domainLabels[i]))
                {
                    result.AddErrorMessage(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            CompositeFormat.Parse(
                                ValidationResultMessages.FQDNHasInvalidDomainLabelCharacterCount),
                            customFQDNName,
                            fqdn,
                            domainLabels[i]),
                        ErrorCodes.ValidationFailed);
                }
            }
            else
            {
                if (FQDNValidatorUtils.HasTopLevelDomainInvalidCharacterCount(domainLabels[i]))
                {
                    result.AddErrorMessage(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            CompositeFormat.Parse(
                                ValidationResultMessages.FQDNHasInvalidTopLevelDomainCharacterCount),
                            customFQDNName,
                            fqdn),
                        ErrorCodes.ValidationFailed);
                }
            }

            if (FQDNValidatorUtils.HasDomainLabelInvalidCharacter(domainLabels[i]))
            {
                result.AddErrorMessage(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        CompositeFormat.Parse(
                            ValidationResultMessages.FQDNHasInvalidDomainLabelCharacter),
                        customFQDNName,
                        fqdn,
                        domainLabels[i]),
                    ErrorCodes.ValidationFailed);
            }
        }

        return Task.CompletedTask;
    }
}
