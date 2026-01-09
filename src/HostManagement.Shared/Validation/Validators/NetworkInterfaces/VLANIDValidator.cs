using System.Globalization;
using System.Text;

namespace HostManagement.Shared.Validation.Validators.NetworkInterfaces;

/// <summary>
/// Represents the validator for a VLAN ID.
/// </summary>
public sealed class VLANIDValidator : IValidator<int>
{
    private const int MinVLANID = 1;
    private const int MaxVLANID = 4094;
    private const string CustomInterfaceText = "network interface";

    private readonly string _customInterfaceNameText;

    /// <summary>
    /// Initializes a new <see cref="VLANIDValidator"/>.
    /// </summary>
    /// <param name="interfaceName">The name of the network interface, to be used in the
    /// validation result message, e.g. "ens33" or "lan1".</param>
    public VLANIDValidator(string? interfaceName = null)
    {
        if (string.IsNullOrWhiteSpace(interfaceName))
            _customInterfaceNameText = CustomInterfaceText;
        else
            _customInterfaceNameText = CustomInterfaceText + $" '{interfaceName}'";
    }

    /// <summary>
    /// Validates a VLAN ID asynchronously.
    /// </summary>
    /// <param name="vlanID">The VLAN ID.</param>
    /// <param name="cancellationToken">A token to cancel the validation.</param>
    /// <inheritdoc/>
    public Task<ValidationResult> ValidateAsync(int vlanID, CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (vlanID < MinVLANID ||
                vlanID > MaxVLANID)
            {
                return Task.FromResult(
                    ValidationResult.CreateWithError(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            CompositeFormat.Parse(
                                NetworkInterfacesValidationResults.InterfaceVLANIDInvalid),
                            vlanID,
                            _customInterfaceNameText)));
            }

            return Task.FromResult(ValidationResult.Valid);
        }
        catch (OperationCanceledException)
        {
            return Task.FromResult(ValidationResult.CreateWithError(ValidationResults.ValidationCancelled));
        }
    }
}
