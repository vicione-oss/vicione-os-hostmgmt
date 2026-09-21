using System.Globalization;
using System.Net.NetworkInformation;
using System.Text;
using HostManagement.Shared.Extensions;

namespace HostManagement.Shared.Validation.Validators.NetworkInterfaces;

/// <summary>
/// Represents the validator for a MAC address.
/// </summary>
public sealed class MACAddressValidator : IValidator<PhysicalAddress>
{
    private const int MACAddressByteLength = 6;
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
        PhysicalAddress macAddress,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (macAddress is null || macAddress.GetAddressBytes().Length == 0)
            {
                return Task.FromResult(
                    ValidationResult.CreateWithError(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            CompositeFormat.Parse(
                                NetworkInterfacesValidationResults.MACAddressEmpty),
                            _customInterfaceNameText)));
            }

            var addressBytes = macAddress.GetAddressBytes();
            var formattedAddress = macAddress.ToColonNotation();

            if (addressBytes.Length != MACAddressByteLength)
            {
                return Task.FromResult(
                    ValidationResult.CreateWithError(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            CompositeFormat.Parse(
                                NetworkInterfacesValidationResults.MACAddressInvalidLength),
                            formattedAddress,
                            _customInterfaceNameText)));
            }

            if (IsLeastSignificantBitSet(addressBytes[0]))
            {
                return Task.FromResult(
                    ValidationResult.CreateWithError(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            CompositeFormat.Parse(
                                NetworkInterfacesValidationResults.MACAddressCanNotBeMulticast),
                            formattedAddress,
                            _customInterfaceNameText)));
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

    private static bool IsLeastSignificantBitSet(byte byteValue) => (byteValue & 0x01) == 0x01;
}
