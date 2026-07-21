using HostManagement.Shared.Contracts.Network;

namespace HostManagement.Shared.Validation.Validators.NTP;

/// <summary>
/// Represents the validator for <see cref="NetworkNTPSettings"/>.
/// </summary>
public sealed class NetworkNTPSettingsValidator : IValidator<NetworkNTPSettings>
{
    /// <summary>
    /// Validates the specified <see cref="NetworkNTPSettings"/> asynchronously.
    /// </summary>
    /// <param name="ntpSettings">The NTP settings.</param>
    /// <param name="cancellationToken">A token to cancel the validation.</param>
    /// <inheritdoc/>
    public async Task<ValidationResult> ValidateAsync(
        NetworkNTPSettings ntpSettings,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (ntpSettings.Enabled)
            {
                return await new NTPServersValidator()
                    .ValidateAsync(ntpSettings.Servers, cancellationToken)
                    .ConfigureAwait(false);
            }

            return ValidationResult.Valid;
        }
        catch (OperationCanceledException)
        {
            return ValidationResult.CreateWithError(ValidationResults.ValidationCancelled);
        }
    }
}
