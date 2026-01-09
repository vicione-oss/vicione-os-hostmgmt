using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;

namespace HostManagement.Shared.Validation.Validators.DNS;

/// <summary>
/// Represents the validator for a <see cref="NetworkDNSSettings"/> object.
/// </summary>
internal sealed class NetworkDNSSettingsValidator : IValidator
{
    public Task Validate(
        SystemConfiguration current,
        SystemConfiguration proposed,
        ValidationResult result)
    {
        HostnameValidator.Validate(proposed.NetworkDNSSettings.Hostname, result);
        new StaticHostsValidator().Validate(current, proposed, result);
        new NameServersValidator().Validate(current, proposed, result);

        return Task.CompletedTask;
    }
}
