using System.Globalization;
using System.Text;

namespace HostManagement.Shared.Validation.Validators.Syslog;

/// <summary>
/// Represents the validator for the syslog reconnect delay in seconds.
/// </summary>
public sealed class SyslogConnectionRetrySecondsValidator : IValidator<int>
{
    private const int MinConnectionRetrySeconds = 1;

    /// <summary>
    /// Validates the specified syslog reconnect delay asynchronously.
    /// </summary>
    /// <param name="connectionRetrySeconds">The reconnect delay in seconds.</param>
    /// <param name="cancellationToken">A token to cancel the validation.</param>
    /// <inheritdoc/>
    public Task<ValidationResult> ValidateAsync(
        int connectionRetrySeconds,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (connectionRetrySeconds < MinConnectionRetrySeconds)
            {
                return Task.FromResult(
                    ValidationResult.CreateWithError(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            CompositeFormat.Parse(SyslogValidationResults.ConnectionRetrySecondsTooSmall),
                            connectionRetrySeconds,
                            MinConnectionRetrySeconds)));
            }

            return Task.FromResult(ValidationResult.Valid);
        }
        catch (OperationCanceledException)
        {
            return Task.FromResult(
                ValidationResult.CreateWithError(ValidationResults.ValidationCancelled));
        }
    }
}
