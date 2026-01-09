using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using HostManagement.Shared.Validation.Utils;

namespace HostManagement.Shared.Validation.Validators.NetworkInterfaces;

/// <summary>
/// Represents the validator for a netmask.
/// </summary>
public sealed class NetmaskValidator : IValidator<IPAddress>
{
    private const string CustomInterfaceText = "network interface";

    private readonly string _customInterfaceNameText;

    /// <summary>
    /// Initializes a new <see cref="NetmaskValidator"/>.
    /// </summary>
    /// <param name="interfaceName">The name of the network interface, to be used in the
    /// validation result message, e.g. "ens33" or "lan1".</param>
    public NetmaskValidator(string? interfaceName = null)
    {
        if (string.IsNullOrWhiteSpace(interfaceName))
            _customInterfaceNameText = CustomInterfaceText;
        else
            _customInterfaceNameText = CustomInterfaceText + $" '{interfaceName}'";
    }

    /// <summary>
    /// Validates an IPv4 netmask asynchronously.
    /// </summary>
    /// <param name="netmask">The netmask.</param>
    /// <param name="cancellationToken">A token to cancel the validation.</param>
    /// <inheritdoc/>
    public Task<ValidationResult> ValidateAsync(
        IPAddress netmask,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (netmask.AddressFamily == AddressFamily.InterNetwork)
            {
                var netmaskAsUInt = IPAddressValidatorUtils.GetIPv4AddressAsUInt(netmask);

                // Invert the bits
                var invertedNetmaskAsUInt = ~netmaskAsUInt;

                /*
                 * The netmask is valid, if all contiguous bits at the low end are 1-bits.
                 * The classic test for this is: '(x & (x + 1)) == 0'.
                 * 
                 * The heart of this test is that it exactly characterizes the numbers of the form
                 * 'x = 2^n − 1' (for some n ≥ 0). Those numbers are called Mersenne numbers.
                 * 
                 * In binary these look like:
                 * 
                 * 2^1 − 1 = 1  → ...0001
                 * 2^2 − 1 = 3  → ...0011
                 * 2^3 − 1 = 7  → ...0111
                 * 2^4 − 1 = 15 → ...1111
                 * ...
                */

                var isValid = (invertedNetmaskAsUInt & (invertedNetmaskAsUInt + 1)) == 0;

                if (!isValid)
                {
                    return Task.FromResult(
                        ValidationResult.CreateWithError(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                CompositeFormat.Parse(
                                    NetworkInterfacesValidationResults.NetmaskInvalid),
                                netmask,
                                _customInterfaceNameText)));
                }

                return Task.FromResult(ValidationResult.Valid);
            }
            else
            {
                // Currently only IPv4 is supported

                return Task.FromResult(
                    ValidationResult.CreateWithError(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            CompositeFormat.Parse(
                                NetworkInterfacesValidationResults.NetmaskInvalid),
                            netmask,
                            _customInterfaceNameText)));
            }
        }
        catch (OperationCanceledException)
        {
            return Task.FromResult(
                ValidationResult.CreateWithError(
                    ValidationResults.ValidationCancelled));
        }
    }
}
