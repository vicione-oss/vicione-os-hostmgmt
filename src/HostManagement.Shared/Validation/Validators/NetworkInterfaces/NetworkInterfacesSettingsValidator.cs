using HostManagement.Shared.Contracts.Network;

namespace HostManagement.Shared.Validation.Validators.NetworkInterfaces;

/// <summary>
/// Represents the validator for <see cref="NetworkInterfacesSettings"/>.
/// </summary>
public sealed class NetworkInterfacesSettingsValidator : IValidator<NetworkInterfacesSettings>
{
    /// <summary>
    /// Validates the specified <see cref="NetworkInterfacesSettings"/> asynchronously.
    /// </summary>
    /// <param name="interfacesSettings">The interfaces settings.</param>
    /// <param name="cancellationToken">A token to cancel the validation.</param>
    /// <inheritdoc/>
    public async Task<ValidationResult> ValidateAsync(
        NetworkInterfacesSettings interfacesSettings,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            return await new NetworkInterfacesValidator()
                .ValidateAsync(interfacesSettings.NetworkInterfaces, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return ValidationResult.CreateWithError(ValidationResults.ValidationCancelled);
        }
    }
}
