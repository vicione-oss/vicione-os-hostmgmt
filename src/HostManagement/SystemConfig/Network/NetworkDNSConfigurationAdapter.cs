using HostManagement.Scripts;
using HostManagement.Shared.Communication.Enums;
using HostManagement.SystemConfig.Contracts;

namespace HostManagement.SystemConfig.Network;

/// <summary>
/// Represents the network DNS configuration adapter.
/// </summary>
/// <param name="currentConfig">The current system configuration.</param>
/// <param name="scriptHandler">An <see cref="IScriptHandler"/> implementation.</param>
/// <param name="bypassHasChanges">
/// <inheritdoc cref="ConfigurationAdapter(bool, bool)" path="/param[@name='bypassHasChanges']"/>
/// </param>
/// <param name="bypassApplyConfig">
/// <inheritdoc cref="ConfigurationAdapter(bool, bool)" path="/param[@name='bypassApplyConfig']"/>
/// </param>
internal sealed class NetworkDNSConfigurationAdapter(
    SystemConfiguration currentConfig,
    IScriptHandler scriptHandler,
    bool bypassHasChanges,
    bool bypassApplyConfig)
        : ConfigurationAdapter<NetworkConfigurationAdapter>(bypassHasChanges, bypassApplyConfig)
{
    private readonly SystemConfiguration _currentConfig = currentConfig;
    private readonly IScriptHandler _scriptHandler = scriptHandler;

    protected internal override async Task<OperationStatus> GetConfiguration(
        SystemConfiguration config,
        CancellationToken cancellationToken = default)
    {
        var (status, networkDNSSettings) = await _scriptHandler
            .ExecuteGetNetworkDNSSettingsScript()
            .ConfigureAwait(false);

        if (status == OperationStatus.Error)
            return OperationStatus.Error;

        config.NetworkDNSSettings = networkDNSSettings;

        return OperationStatus.Success;
    }

    private protected override bool HasChangesInternal(
        SystemConfiguration currentConfig,
        SystemConfiguration proposedConfig)
    {
        // TODO Improve code/scripts to not call `NetworkDNSConfigurationAdapter` and
        // `NetworkInterfacesConfigurationAdapter`
        //
        // Currently both adapters are executed. Therefore both scripts write in the corresponding
        // interfaces' yaml file. That is because the DNS script depends on a clean file
        // (meaning no DNS info configured).
        return
            !EqualityComparer<NetworkDNSSettings>.Default.Equals(
                currentConfig.NetworkDNSSettings,
                proposedConfig.NetworkDNSSettings) ||
            !EqualityComparer<NetworkInterfaceDetail>.Default.SequenceEqual(
                currentConfig.NetworkInterfacesSettings?.NetworkInterfaces,
                proposedConfig.NetworkInterfacesSettings?.NetworkInterfaces);
    }

    private protected override async Task<OperationStatus> PrepareConfiguration(
        SystemConfiguration config,
        CancellationToken cancellationToken = default)
    {
        var dnsSettings = config.NetworkDNSSettings;

        if (dnsSettings is null)
            return OperationStatus.Success;

        if (await SetMulticastConfiguration(dnsSettings).ConfigureAwait(false) == OperationStatus.Error)
            return OperationStatus.Error;

        if (await SetStaticHostsConfiguration(dnsSettings.StaticHostsEnabled, dnsSettings.StaticHosts).ConfigureAwait(false) == OperationStatus.Error)
            return OperationStatus.Error;

        if (await SetDNSConfiguration(dnsSettings).ConfigureAwait(false) == OperationStatus.Error)
            return OperationStatus.Error;

        if (await SetHostname(dnsSettings.Hostname).ConfigureAwait(false) == OperationStatus.Error)
            return OperationStatus.Error;

        return OperationStatus.Success;
    }

    private async Task<OperationStatus> SetMulticastConfiguration(NetworkDNSSettings dnsSettings)
    {
        if (dnsSettings.MulticastDNSEnabled.HasValue &&
            dnsSettings.MulticastDNSEnabled.Value != _currentConfig.NetworkDNSSettings?.MulticastDNSEnabled)
        {
            var (status, rawInterfacesString) = await _scriptHandler
                .ExecuteGetNetworkInterfacesScript()
                .ConfigureAwait(false);

            if (status is OperationStatus.Error)
                return OperationStatus.Error;

            var scriptOptions = new ScriptOptions();

            scriptOptions.Add(
                ScriptOptionNames.NetworkMulticastDNS.MulticastDNSEnabled,
                dnsSettings.MulticastDNSEnabled.Value);

            scriptOptions.Add(
                ScriptOptionNames.NetworkMulticastDNS.Interfaces,
                rawInterfacesString);

            return await _scriptHandler
                .ExecuteSetNetworkMulticastDNSSettingsScript(scriptOptions)
                .ConfigureAwait(false);
        }

        return OperationStatus.Success;
    }

    private async Task<OperationStatus> SetStaticHostsConfiguration(
        bool? staticHostsEnabled, List<StaticHostDetail>? staticHosts)
    {
        if (staticHostsEnabled.HasValue)
        {
            var scriptOptions = new ScriptOptions();

            scriptOptions.Add(ScriptOptionNames.NetworkDNSStaticHosts.Enabled, staticHostsEnabled.Value);

            if (staticHosts is not null)
            {
                foreach (var staticHost in staticHosts)
                {
                    scriptOptions.Add(ScriptOptionNames.NetworkDNSStaticHosts.IPAddress, staticHost.IPAddress);
                    scriptOptions.Add(ScriptOptionNames.NetworkDNSStaticHosts.Hostname, staticHost.Hostname);
                }
            }

            return await _scriptHandler.ExecuteSetStaticHostsScript(scriptOptions).ConfigureAwait(false);
        }

        return OperationStatus.Success;
    }

    private async Task<OperationStatus> SetDNSConfiguration(NetworkDNSSettings dnsSettings)
    {
        var scriptOptions = new ScriptOptions();

        AddNameServersScriptOptions(scriptOptions, dnsSettings);
        AddSuffixScriptOptions(scriptOptions, dnsSettings);
        AddSearchDomainsScriptOptions(scriptOptions, dnsSettings);

        if (!scriptOptions.Options.Any())
            return OperationStatus.Success;

        var (status, rawInterfacesString) = await _scriptHandler
            .ExecuteGetNetworkInterfacesScript()
            .ConfigureAwait(false);

        if (status is OperationStatus.Error)
            return OperationStatus.Error;

        scriptOptions.Add(ScriptOptionNames.NetworkDNS.Interfaces, rawInterfacesString);

        return await _scriptHandler
            .ExecuteSetNetworkDNSSettingsScript(scriptOptions)
            .ConfigureAwait(false);
    }

    private async Task<OperationStatus> SetHostname(string? hostname)
    {
        if (hostname is null || hostname.Equals(_currentConfig.NetworkDNSSettings?.Hostname))
            return OperationStatus.Success;

        return await _scriptHandler.ExecuteSetHostnameScript(hostname).ConfigureAwait(false);
    }

    private static void AddNameServersScriptOptions(ScriptOptions scriptOptions, NetworkDNSSettings dnsSettings)
    {
        if (dnsSettings.NameServersEnabled.HasValue && dnsSettings.NameServers?.Count > 0)
        {
            scriptOptions.Add(ScriptOptionNames.NetworkDNS.NameServersEnabled, dnsSettings.NameServersEnabled.Value);
            scriptOptions.Add(ScriptOptionNames.NetworkDNS.NameServers, dnsSettings.NameServers.ToArray());
        }
    }

    private static void AddSuffixScriptOptions(ScriptOptions scriptOptions, NetworkDNSSettings dnsSettings)
    {
        if (dnsSettings.DNSSuffixEnabled.HasValue && dnsSettings.DNSSuffix is not null)
        {
            scriptOptions.Add(ScriptOptionNames.NetworkDNS.DNSSuffixEnabled, dnsSettings.DNSSuffixEnabled.Value);
            scriptOptions.Add(ScriptOptionNames.NetworkDNS.DNSSuffix, dnsSettings.DNSSuffix);
        }
    }

    private static void AddSearchDomainsScriptOptions(ScriptOptions scriptOptions, NetworkDNSSettings dnsSettings)
    {
        if (dnsSettings.SearchDomainsEnabled.HasValue && dnsSettings.SearchDomains?.Count > 0)
        {
            scriptOptions.Add(ScriptOptionNames.NetworkDNS.SearchDomainsEnabled, dnsSettings.SearchDomainsEnabled.Value);
            scriptOptions.Add(ScriptOptionNames.NetworkDNS.SearchDomains, dnsSettings.SearchDomains.ToArray());
        }
    }
}
