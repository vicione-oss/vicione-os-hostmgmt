using System.Globalization;
using System.Net.Sockets;
using System.Text;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Validation.Utils;

namespace HostManagement.Shared.Validation.Validators.NetworkInterfaces;

/// <summary>
/// Represents the validator for an <see cref="IPv4Detail"/>.
/// </summary>
public sealed class IPv4DetailValidator : IValidator<IPv4Detail>
{
    private const string CustomInterfaceText = "network interface";

    private readonly string? _interfaceName;
    private readonly string _customInterfaceNameText;

    /// <summary>
    /// Initializes a new <see cref="IPv4DetailValidator"/>.
    /// </summary>
    /// <param name="interfaceName">The name of the network interface, to be used in the
    /// validation result message, e.g. "ens33" or "lan1".</param>
    public IPv4DetailValidator(string? interfaceName = null)
    {
        _interfaceName = interfaceName;

        if (string.IsNullOrWhiteSpace(interfaceName))
            _customInterfaceNameText = CustomInterfaceText;
        else
            _customInterfaceNameText = CustomInterfaceText + $" '{interfaceName}'";
    }

    /// <summary>
    /// Validates an <see cref="IPv4Detail"/> asynchronously.
    /// </summary>
    /// <param name="ipv4Detail">The <see cref="IPv4Detail"/>.</param>
    /// <param name="cancellationToken">A token to cancel the validation.</param>
    /// <inheritdoc/>
    public async Task<ValidationResult> ValidateAsync(
        IPv4Detail ipv4Detail,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var address = ipv4Detail.IPAddress;
            var netmask = ipv4Detail.Netmask;

            if (address.AddressFamily != AddressFamily.InterNetwork)
            {
                return ValidationResult.CreateWithError(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        CompositeFormat.Parse(
                            NetworkInterfacesValidationResults.IPAddressIsNoValidIPv4Address),
                        address,
                        _customInterfaceNameText));
            }

            var netmaskValidationResult = await new NetmaskValidator(_interfaceName)
                .ValidateAsync(netmask, cancellationToken)
                .ConfigureAwait(false);

            if (!netmaskValidationResult.IsValid)
                return netmaskValidationResult;

            if (!IPAddressValidatorUtils.IsSubnetHostAddress(address, netmask, out var availableHostsString))
            {
                return ValidationResult.CreateWithError(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        CompositeFormat.Parse(
                            NetworkInterfacesValidationResults.NetmaskRangeInvalid),
                        netmask,
                        _customInterfaceNameText,
                        address,
                        availableHostsString));
            }

            return ValidationResult.Valid;
        }
        catch (OperationCanceledException)
        {
            return ValidationResult.CreateWithError(ValidationResults.ValidationCancelled);
        }
    }
}
