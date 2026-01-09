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

    /// <inheritdoc cref="ValidateAsync(ReadOnlySpan{char}, CancellationToken)"/>
    public Task<ValidationResult> ValidateAsync(string macAddress, CancellationToken cancellationToken = default)
        => ValidateAsync(macAddress.AsSpan(), cancellationToken);

    /// <summary>
    /// Validates a MAC address asynchronously.
    /// </summary>
    /// <param name="macAddress">The MAC address.</param>
    /// <param name="cancellationToken">A token to cancel the validation.</param>
    /// <inheritdoc/>
    public Task<ValidationResult> ValidateAsync(
        ReadOnlySpan<char> macAddress,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (macAddress.IsWhiteSpace())
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
                                macAddress.ToString(),
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
                                macAddress.ToString(),
                                _customInterfaceNameText)));
                }

                return Task.FromResult(IsMulticastMacAddress(macAddress, macAddress[..2]));
            }
            else
            {
                var octets = macAddress.SplitAny(MACAddressValidatorUtils.Separators);

                if (MACAddressValidatorUtils.HasInvalidOctetCount(octets))
                {
                    return Task.FromResult(
                        ValidationResult.CreateWithError(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                CompositeFormat.Parse(
                                    NetworkInterfacesValidationResults.MACAddressInvalidOctetCount),
                                macAddress.ToString(),
                                _customInterfaceNameText)));
                }

                var octetNumber = 0;

                foreach (var octet in octets)
                {
                    octetNumber++;

                    if (MACAddressValidatorUtils.HasOctetInvalidLength(macAddress[octet]))
                    {
                        return Task.FromResult(
                            ValidationResult.CreateWithError(
                                string.Format(
                                    CultureInfo.InvariantCulture,
                                    CompositeFormat.Parse(
                                        NetworkInterfacesValidationResults.MACAddressOctetInvalidLength),
                                    macAddress.ToString(),
                                    _customInterfaceNameText)));
                    }

                    if (MACAddressValidatorUtils.HasNonAsciiHexDigit(macAddress[octet]))
                    {
                        return Task.FromResult(
                            ValidationResult.CreateWithError(
                                string.Format(
                                    CultureInfo.InvariantCulture,
                                    CompositeFormat.Parse(
                                        NetworkInterfacesValidationResults.MACAddressInvalidCharacter),
                                    macAddress.ToString(),
                                    _customInterfaceNameText)));
                    }

                    if (octetNumber == 1)
                    {
                        var result = IsMulticastMacAddress(macAddress, macAddress[octet]);

                        if (!result.IsValid)
                            return Task.FromResult(result);
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

    private ValidationResult IsMulticastMacAddress(
        ReadOnlySpan<char> macAddress,
        ReadOnlySpan<char> firstOctet)
    {
        var firstOctetByte = byte.Parse(firstOctet, NumberStyles.HexNumber);

        if (MACAddressValidatorUtils.IsLeastSignificantBitSet(firstOctetByte))
        {
            return ValidationResult.CreateWithError(
                string.Format(
                    CultureInfo.InvariantCulture,
                    CompositeFormat.Parse(
                        NetworkInterfacesValidationResults.MACAddressCanNotBeMulticast),
                    macAddress.ToString(),
                    _customInterfaceNameText));
        }

        return ValidationResult.Valid;
    }
}
