using HostManagement.Shared.Contracts.Network;

namespace HostManagement.Shared.Validation.Validators.DNS;

/// <summary>
/// Represents the validator for <see cref="NetworkDNSSettings"/>.
/// </summary>
public sealed class NetworkDNSSettingsValidator : IValidator<NetworkDNSSettings>
{
    /// <summary>
    /// Validates the specified <see cref="NetworkDNSSettings"/> asynchronously.
    /// </summary>
    /// <param name="dnsSettings">The DNS settings.</param>
    /// <param name="cancellationToken">A token to cancel the validation.</param>
    /// <inheritdoc/>
    public async Task<ValidationResult> ValidateAsync(
        NetworkDNSSettings dnsSettings,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result = new ValidationResult();

            result.AddResult(
                await new HostnameValidator()
                    .ValidateAsync(dnsSettings.Hostname, dnsSettings.PrimaryDNSSuffix.Suffix, cancellationToken)
                    .ConfigureAwait(false));

            if (dnsSettings.PrimaryDNSSuffix.Enabled)
            {
                result.AddResult(
                    await new DNSSuffixValidator()
                        .ValidateAsync(dnsSettings.PrimaryDNSSuffix.Suffix, dnsSettings.Hostname, cancellationToken)
                        .ConfigureAwait(false));
            }

            if (dnsSettings.StaticHosts.Enabled)
            {
                result.AddResult(
                    await new StaticHostsValidator()
                        .ValidateAsync(dnsSettings.StaticHosts.Hosts, cancellationToken)
                        .ConfigureAwait(false));
            }

            if (dnsSettings.SearchDomains.Enabled)
            {
                result.AddResult(
                    await new SearchDomainsValidator()
                        .ValidateAsync(dnsSettings.SearchDomains.Domains, cancellationToken)
                        .ConfigureAwait(false));
            }

            if (dnsSettings.NameServers.Enabled)
            {
                result.AddResult(
                    await new NameServersValidator()
                        .ValidateAsync(dnsSettings.NameServers.Addresses, cancellationToken)
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
