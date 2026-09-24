using System.Net.NetworkInformation;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Validation.Validators;
using HostManagement.Shared.Validation.Validators.DNS;
using HostManagement.Shared.Validation.Validators.NetworkInterfaces;
using HostManagement.Shared.Validation.Validators.NTP;
using HostManagement.Shared.Validation.Validators.Proxy;
using HostManagement.Shared.Validation.Validators.Syslog;

namespace HostManagement.Shared.Validation;

/// <summary>
/// Represents the validator for a <see cref="SystemConfiguration"/> object.
/// </summary>
public sealed class SystemConfigurationValidator : IValidator<SystemConfiguration, IReadOnlyList<PhysicalAddress>>
{
    /// <summary>
    /// Validates the specified <see cref="SystemConfiguration"/> asynchronously.
    /// </summary>
    /// <param name="configuration">The <see cref="SystemConfiguration"/>.</param>
    /// <param name="originalMACAddresses">A list of factory-assigned original MAC addresses,
    /// that can't be used as a user-defined MAC address.</param>
    /// <param name="cancellationToken">A token to cancel the validation.</param>
    /// <inheritdoc/>
    public async Task<ValidationResult> ValidateAsync(
        SystemConfiguration configuration,
        IReadOnlyList<PhysicalAddress>? originalMACAddresses = null,
        CancellationToken cancellationToken = default)
    {
        var result = new ValidationResult();

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            result.AddResult(
                await new NetworkInterfacesValidator()
                    .ValidateAsync(configuration.NetworkInterfaces, originalMACAddresses, cancellationToken)
                    .ConfigureAwait(false));

            result.AddResult(
                await new NetworkDNSSettingsValidator()
                    .ValidateAsync(configuration.NetworkDNSSettings, cancellationToken)
                    .ConfigureAwait(false));

            result.AddResult(
                await new NetworkProxySettingsValidator()
                    .ValidateAsync(
                        configuration.NetworkProxySettings,
                        configuration.NetworkInterfaces,
                        cancellationToken)
                    .ConfigureAwait(false));

            result.AddResult(
                await new NetworkNTPSettingsValidator()
                    .ValidateAsync(configuration.NetworkNTPSettings, cancellationToken)
                    .ConfigureAwait(false));

            result.AddResult(
                await new SyslogSettingsValidator()
                    .ValidateAsync(configuration.Syslog, cancellationToken)
                    .ConfigureAwait(false));

            return result;
        }
        catch (OperationCanceledException)
        {
            return ValidationResult.CreateWithError(ValidationResults.ValidationCancelled);
        }
        catch (Exception ex)
        {
            result.AddError(
                ValidationResults.ValidationFailed +
                Environment.NewLine +
                ex.Message);

            return result;
        }
    }
}
