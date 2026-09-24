using System.Globalization;
using System.Text;
using HostManagement.Shared.Contracts.Syslog;

namespace HostManagement.Shared.Validation.Validators.Syslog;

/// <summary>
/// Represents the validator for <see cref="SyslogSettings"/>.
/// </summary>
public sealed class SyslogSettingsValidator : IValidator<SyslogSettings>
{
    /// <summary>
    /// Validates the specified <see cref="SyslogSettings"/> asynchronously.
    /// </summary>
    /// <param name="syslogSettings">The syslog settings.</param>
    /// <param name="cancellationToken">A token to cancel the validation.</param>
    /// <inheritdoc/>
    public async Task<ValidationResult> ValidateAsync(
        SyslogSettings syslogSettings,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (syslogSettings.Equals(new SyslogSettings()))
                return ValidationResult.Valid;

            var result = new ValidationResult();

            if (!Enum.IsDefined(syslogSettings.TransportProtocol))
            {
                result.AddError(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        CompositeFormat.Parse(SyslogValidationResults.TransportProtocolInvalid),
                        syslogSettings.TransportProtocol));
            }

            if (!Enum.IsDefined(syslogSettings.LogFormat))
            {
                result.AddError(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        CompositeFormat.Parse(SyslogValidationResults.LogFormatInvalid),
                        syslogSettings.LogFormat));
            }

            result.AddResult(
                await new SyslogAddressValidator()
                    .ValidateAsync(syslogSettings.Address, syslogSettings.TransportProtocol, cancellationToken)
                    .ConfigureAwait(false));

            result.AddResult(
                await new SyslogConnectionRetrySecondsValidator()
                    .ValidateAsync(syslogSettings.ConnectionRetrySeconds, cancellationToken)
                    .ConfigureAwait(false));

            if (syslogSettings.ExcludeSyslogFacilities is null)
            {
                result.AddError(SyslogValidationResults.FacilitiesCanNotBeNull);
            }
            else
            {
                result.AddResult(
                    await new SyslogFacilitiesValidator()
                        .ValidateAsync(syslogSettings.ExcludeSyslogFacilities, cancellationToken)
                        .ConfigureAwait(false));
            }

            if (syslogSettings.ExcludeSyslogLevels is null)
            {
                result.AddError(SyslogValidationResults.LevelsCanNotBeNull);
            }
            else
            {
                result.AddResult(
                    await new SyslogLevelsValidator()
                        .ValidateAsync(syslogSettings.ExcludeSyslogLevels, cancellationToken)
                        .ConfigureAwait(false));
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            return ValidationResult.CreateWithError(ValidationResults.ValidationCancelled);
        }
    }
}
