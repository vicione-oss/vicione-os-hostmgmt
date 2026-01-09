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
        var hostname = proposed.NetworkDNSSettings.Hostname;
        var dnsSuffixEnabled = proposed.NetworkDNSSettings.DNSSuffixEnabled;
        var dnsSuffix = proposed.NetworkDNSSettings.DNSSuffix;

        HostnameValidator.Validate(hostname, result);
        DNSSuffixValidator.Validate(hostname, dnsSuffixEnabled, dnsSuffix, result);

        var staticHostsEnabled = proposed.NetworkDNSSettings.StaticHostsEnabled;
        var staticHosts = proposed.NetworkDNSSettings.StaticHosts;

        StaticHostsValidator.Validate(staticHostsEnabled, staticHosts, result);

        var searchDomainsEnabled = proposed.NetworkDNSSettings.SearchDomainsEnabled;
        var searchDomains = proposed.NetworkDNSSettings.SearchDomains;

        SearchDomainsValidator.Validate(searchDomainsEnabled, searchDomains, result);

        var nameServersEnabled = proposed.NetworkDNSSettings.NameServersEnabled;
        var nameServers = proposed.NetworkDNSSettings.NameServers;

        NameServersValidator.Validate(nameServersEnabled, nameServers, result);

        return Task.CompletedTask;
    }
}
