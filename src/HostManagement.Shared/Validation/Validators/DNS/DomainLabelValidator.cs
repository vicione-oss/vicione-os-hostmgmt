using System.Globalization;
using System.Text;
using HostManagement.Shared.Validation.Utils;

namespace HostManagement.Shared.Validation.Validators.DNS;

/// <summary>
/// Represents the validator for a domain label.
/// </summary>
internal sealed class DomainLabelValidator
{
    /// <summary>
    /// Validates a domain label.
    /// </summary>
    /// <param name="domainLabel">The domain label.</param>
    /// <param name="result">The validation result.</param>
    /// <param name="customDomainLabelName">A custom name to be used in the result message, e.g.
    /// "Hostname" or "Static host hostname".</param>
    /// <returns><see cref="ValidationResult"/></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static Task Validate(string domainLabel, ValidationResult result, string customDomainLabelName = "Domain label")
    {
        if (string.IsNullOrWhiteSpace(customDomainLabelName))
            throw new ArgumentNullException(nameof(customDomainLabelName));

        if (FQDNValidatorUtils.HasDomainLabelInvalidCharacterCount(domainLabel))
        {
            result.AddErrorMessage(
                string.Format(
                    CultureInfo.InvariantCulture,
                    CompositeFormat.Parse(
                        ValidationResultMessages.DomainLabelHasTooManyCharacters),
                    customDomainLabelName,
                    domainLabel),
                ErrorCodes.ValidationFailed);

            return Task.CompletedTask;
        }

        if (FQDNValidatorUtils.HasDomainLabelOnlyDigits(domainLabel))
        {
            result.AddErrorMessage(
                string.Format(
                    CultureInfo.InvariantCulture,
                    CompositeFormat.Parse(
                        ValidationResultMessages.DomainLabelCanNotHaveOnlyDigits),
                    customDomainLabelName,
                    domainLabel),
                ErrorCodes.ValidationFailed);

            return Task.CompletedTask;
        }

        if (FQDNValidatorUtils.HasDomainLabelInvalidCharacter(domainLabel))
        {
            result.AddErrorMessage(
                string.Format(
                    CultureInfo.InvariantCulture,
                    CompositeFormat.Parse(
                        ValidationResultMessages.DomainLabelHasInvalidCharacter),
                    customDomainLabelName,
                    domainLabel),
                ErrorCodes.ValidationFailed);
        }

        return Task.CompletedTask;
    }
}
