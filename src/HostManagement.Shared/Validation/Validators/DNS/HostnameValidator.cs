using System.Globalization;
using System.Text;
using HostManagement.Shared.Validation.Utils;

namespace HostManagement.Shared.Validation.Validators.DNS;

// TODO Add unit tests; Could be made public
/// <summary>
/// Represents the validator for a Linux hostname.
/// </summary>
internal sealed class HostnameValidator
{
    /// <summary>
    /// Validates a Linux hostname.
    /// </summary>
    /// <remarks>
    /// Enforce strict validation for system compatibility and only set short hostnames
    /// (see <see href="https://www.man7.org/linux/man-pages/man1/hostnamectl.1.html">manpage hostnamectl</see>).
    /// </remarks>
    /// <param name="hostname">The hostname to validate.</param>
    /// <param name="result">The validation result.</param>
    /// <returns><see cref="ValidationResult"/></returns>
    public static Task Validate(string hostname, ValidationResult result)
    {
        if (IsEmpty(hostname, result))
            return Task.CompletedTask;

        if (hostname.Contains('.'))
        {
            // Fully Qualified Domain Name but enforce strict validation with max. 63 characters
            if (FQDNValidatorUtils.HasDomainLabelInvalidCharacterCount(hostname))
            {
                result.AddErrorMessage(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        CompositeFormat.Parse(
                            ValidationResultMessages.HostnameHasTooManyCharacters),
                        hostname),
                    ErrorCodes.ValidationFailed);

                return Task.CompletedTask;
            }

            FQDNValidator.Validate(hostname, result, "Hostname");
        }
        else
        {
            // Simple hostname
            DomainLabelValidator.Validate(hostname, result, "Hostname");
        }

        return Task.CompletedTask;
    }

    internal static bool IsEmpty(string hostname, ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(hostname))
        {
            result.AddErrorMessage(
                ValidationResultMessages.HostnameCanNotBeEmpty,
                ErrorCodes.ValidationFailed);

            return true;
        }

        return false;
    }
}
