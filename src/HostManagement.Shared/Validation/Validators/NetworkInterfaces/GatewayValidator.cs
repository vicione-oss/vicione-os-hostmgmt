using System.Globalization;
using System.Net;
using System.Text;
using HostManagement.Shared.Validation.Utils;

namespace HostManagement.Shared.Validation.Validators.NetworkInterfaces;

/// <summary>
/// Represents the validator for a default gateway.
/// </summary>
public sealed class GatewayValidator : IValidator<IPAddress>
{
    private const string CustomInterfaceText = "network interface";

    private readonly string _customInterfaceNameText;

    /// <summary>
    /// Initializes a new <see cref="GatewayValidator"/>.
    /// </summary>
    /// <param name="interfaceName">The name of the network interface, to be used in the
    /// validation result message, e.g. "ens33" or "lan1".</param>
    public GatewayValidator(string? interfaceName = null)
    {
        if (string.IsNullOrWhiteSpace(interfaceName))
            _customInterfaceNameText = CustomInterfaceText;
        else
            _customInterfaceNameText = CustomInterfaceText + $" '{interfaceName}'";
    }

    /// <summary>
    /// Validates a default gateway asynchronously.
    /// </summary>
    /// <param name="gateway">The default gateway.</param>
    /// <param name="cancellationToken">A token to cancel the validation.</param>
    /// <inheritdoc/>
    public Task<ValidationResult> ValidateAsync(
        IPAddress gateway,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (IPAddressValidatorUtils.IsNotRoutable(gateway, out var notRoutableCIDRString))
            {
                return Task.FromResult(
                    ValidationResult.CreateWithError(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            CompositeFormat.Parse(
                                NetworkInterfacesValidationResults.GatewayCanNotBeNotRoutable),
                            _customInterfaceNameText,
                            notRoutableCIDRString)));
            }

            if (IPAddressValidatorUtils.IsLoopback(gateway, out var loopbackCIDRString))
            {
                return Task.FromResult(
                    ValidationResult.CreateWithError(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            CompositeFormat.Parse(
                                NetworkInterfacesValidationResults.GatewayCanNotBeLoopback),
                            _customInterfaceNameText,
                            loopbackCIDRString)));
            }

            if (IPAddressValidatorUtils.IsMulticast(gateway, out var multicastCIDRString))
            {
                return Task.FromResult(
                    ValidationResult.CreateWithError(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            CompositeFormat.Parse(
                                NetworkInterfacesValidationResults.GatewayCanNotBeMulticast),
                            _customInterfaceNameText,
                            multicastCIDRString)));
            }

            if (IPAddressValidatorUtils.IsBroadcast(gateway, out var broadcastCIDRString))
            {
                return Task.FromResult(
                    ValidationResult.CreateWithError(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            CompositeFormat.Parse(
                                NetworkInterfacesValidationResults.GatewayCanNotBeBroadcast),
                            _customInterfaceNameText,
                            broadcastCIDRString)));
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
