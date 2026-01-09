using System.Globalization;
using System.Text;
using HostManagement.Shared.Validation.Utils;

namespace HostManagement.Shared.Validation.Validators.NetworkInterfaces;

/// <summary>
/// Represents the validator for a MAC address.
/// </summary>
public sealed class MACAddressValidator : IValidator<string>
{
    private const string CustomInterfaceText = "network interface";

    private readonly string _customInterfaceNameText;

    /// <summary>
    /// Initializes a new <see cref="MACAddressValidator"/>.
    /// </summary>
    /// <param name="interfaceName">The name of the network interface, to be used in the
    /// validation result message, e.g. "ens33" or "lan1".</param>
    public MACAddressValidator(string? interfaceName = null)
    {
        if (string.IsNullOrWhiteSpace(interfaceName))
            _customInterfaceNameText = CustomInterfaceText;
        else
            _customInterfaceNameText = CustomInterfaceText + $" '{interfaceName}'";
    }

    /// <summary>
    /// Validates a MAC address asynchronously.
    /// </summary>
    /// <param name="macAddress">The MAC address.</param>
    /// <param name="cancellationToken">A token to cancel the validation.</param>
    /// <inheritdoc/>
    public Task<ValidationResult> ValidateAsync(
        string macAddress,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(macAddress))
            {
                return Task.FromResult(
                    ValidationResult.CreateWithError(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            CompositeFormat.Parse(
                                NetworkInterfacesValidationResults.MACAddressEmpty),
                            _customInterfaceNameText)));
            }

            if (MACAddressValidatorUtils.HasNoSeparators(macAddress))
            {
                if (macAddress.Length != 12)
                {
                    return Task.FromResult(
                        ValidationResult.CreateWithError(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                CompositeFormat.Parse(
                                    NetworkInterfacesValidationResults.MACAddressInvalidLength),
                                macAddress,
                                _customInterfaceNameText)));
                }

                if (MACAddressValidatorUtils.HasNonAsciiHexDigit(macAddress))
                {
                    return Task.FromResult(
                        ValidationResult.CreateWithError(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                CompositeFormat.Parse(
                                    NetworkInterfacesValidationResults.MACAddressInvalidCharacter),
                                macAddress,
                                _customInterfaceNameText)));
                }
            }
            else
            {
                var octets = macAddress.Split(MACAddressValidatorUtils.s_separators);

                if (MACAddressValidatorUtils.HasInvalidOctetCount(octets))
                {
                    return Task.FromResult(
                        ValidationResult.CreateWithError(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                CompositeFormat.Parse(
                                    NetworkInterfacesValidationResults.MACAddressInvalidOctetCount),
                                macAddress,
                                _customInterfaceNameText)));
                }

                foreach (var octet in octets)
                {
                    if (MACAddressValidatorUtils.HasOctetInvalidLength(octet))
                    {
                        return Task.FromResult(
                            ValidationResult.CreateWithError(
                                string.Format(
                                    CultureInfo.InvariantCulture,
                                    CompositeFormat.Parse(
                                        NetworkInterfacesValidationResults.MACAddressOctetInvalidLength),
                                    macAddress,
                                    _customInterfaceNameText)));
                    }

                    if (MACAddressValidatorUtils.HasOctetNonAsciiHexDigit(octet))
                    {
                        return Task.FromResult(
                            ValidationResult.CreateWithError(
                                string.Format(
                                    CultureInfo.InvariantCulture,
                                    CompositeFormat.Parse(
                                        NetworkInterfacesValidationResults.MACAddressInvalidCharacter),
                                    macAddress,
                                    _customInterfaceNameText)));
                    }
                }
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
