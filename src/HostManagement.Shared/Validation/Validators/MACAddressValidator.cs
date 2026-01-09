using System.Globalization;
using System.Text;
using HostManagement.Shared.Validation.Utils;

namespace HostManagement.Shared.Validation.Validators;

/// <summary>
/// Represents the validator for a MAC address.
/// </summary>
internal sealed class MACAddressValidator
{
    /// <summary>
    /// Validates a MAC address.
    /// </summary>
    /// <param name="macAddress">The MAC address.</param>
    /// <param name="result">The validation result.</param>
    /// <param name="customInterfaceName">A custom name to be used in the result message, e.g. "ens33".</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static Task Validate(
        string macAddress,
        ValidationResult result,
        string customInterfaceName = "network interface")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(customInterfaceName);

        if (string.IsNullOrWhiteSpace(macAddress))
        {
            result.AddErrorMessage(
                string.Format(
                    CultureInfo.InvariantCulture,
                    CompositeFormat.Parse(
                        ValidationResultMessages.MACAddressEmpty),
                    customInterfaceName),
                ErrorCodes.ValidationFailed);

            return Task.CompletedTask;
        }

        if (MACAddressValidatorUtils.HasNoSeparators(macAddress))
        {
            if (macAddress.Length != 12)
            {
                result.AddErrorMessage(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        CompositeFormat.Parse(
                            ValidationResultMessages.MACAddressInvalidLength),
                        macAddress,
                        customInterfaceName),
                    ErrorCodes.ValidationFailed);

                return Task.CompletedTask;
            }

            if (MACAddressValidatorUtils.HasNonAsciiHexDigit(macAddress))
            {
                result.AddErrorMessage(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        CompositeFormat.Parse(
                            ValidationResultMessages.MACAddressInvalidCharacter),
                        macAddress,
                        customInterfaceName),
                    ErrorCodes.ValidationFailed);

                return Task.CompletedTask;
            }
        }
        else
        {
            var octets = macAddress.Split(MACAddressValidatorUtils.s_separators);

            if (MACAddressValidatorUtils.HasInvalidOctetCount(octets))
            {
                result.AddErrorMessage(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        CompositeFormat.Parse(
                            ValidationResultMessages.MACAddressInvalidOctetCount),
                        macAddress,
                        customInterfaceName),
                    ErrorCodes.ValidationFailed);

                return Task.CompletedTask;
            }

            foreach (var octet in octets)
            {
                if (MACAddressValidatorUtils.HasOctetInvalidLength(octet))
                {
                    result.AddErrorMessage(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            CompositeFormat.Parse(
                                ValidationResultMessages.MACAddressOctetInvalidLength),
                            macAddress,
                            customInterfaceName),
                        ErrorCodes.ValidationFailed);

                    return Task.CompletedTask;
                }

                if (MACAddressValidatorUtils.HasOctetNonAsciiHexDigit(octet))
                {
                    result.AddErrorMessage(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            CompositeFormat.Parse(
                                ValidationResultMessages.MACAddressInvalidCharacter),
                            macAddress,
                            customInterfaceName),
                        ErrorCodes.ValidationFailed);

                    return Task.CompletedTask;
                }
            }
        }

        return Task.CompletedTask;
    }
}
