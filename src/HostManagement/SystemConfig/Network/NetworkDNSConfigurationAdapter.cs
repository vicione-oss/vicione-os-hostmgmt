using HostManagement.Scripts.ScriptOptionNames;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;

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
        OperationStatus status;
        NetworkDNSSettings? networkDNSSettings;

        (status, networkDNSSettings) = await _scriptHandler
            .ExecuteGetNetworkDNSSettingsScript()
            .ConfigureAwait(false);

        if (status == OperationStatus.Error)
            return OperationStatus.Error;

        // TODO Workaround to ensure static host, name servers, DNS suffix and search domains
        // already configured but disabled are not discarded. May change with actual implementation
        // of general system configuration json enabled functionality.
        var currentStaticHosts = networkDNSSettings.StaticHosts;
        var currentNameServers = networkDNSSettings.NameServers;
        var currentDNSSuffix = networkDNSSettings.DNSSuffix;
        var currentSearchDomains = networkDNSSettings.SearchDomains;

        if (!networkDNSSettings.StaticHostsEnabled &&
            _currentConfig.NetworkDNSSettings.StaticHosts.Count != 0)
        {
            currentStaticHosts = _currentConfig.NetworkDNSSettings.StaticHosts;
        }

        if (!networkDNSSettings.NameServersEnabled &&
            _currentConfig.NetworkDNSSettings.NameServers.Count != 0)
        {
            currentNameServers = _currentConfig.NetworkDNSSettings.NameServers;
        }

        if (!networkDNSSettings.DNSSuffixEnabled &&
            !string.IsNullOrWhiteSpace(_currentConfig.NetworkDNSSettings.DNSSuffix))
        {
            currentDNSSuffix = _currentConfig.NetworkDNSSettings.DNSSuffix;
        }

        if (!networkDNSSettings.SearchDomainsEnabled &&
            _currentConfig.NetworkDNSSettings.SearchDomains.Count != 0)
        {
            currentSearchDomains = _currentConfig.NetworkDNSSettings.SearchDomains;
        }

        config.NetworkDNSSettings = new NetworkDNSSettings
        {
            Hostname = networkDNSSettings.Hostname,
            MulticastDNSEnabled = networkDNSSettings.MulticastDNSEnabled,
            StaticHostsEnabled = networkDNSSettings.StaticHostsEnabled,
            StaticHosts = currentStaticHosts,
            NameServersEnabled = networkDNSSettings.NameServersEnabled,
            NameServers = currentNameServers,
            DNSSuffixEnabled = networkDNSSettings.DNSSuffixEnabled,
            DNSSuffix = currentDNSSuffix,
            SearchDomains = currentSearchDomains,
            SearchDomainsEnabled = networkDNSSettings.SearchDomainsEnabled
        };

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
            !currentConfig.NetworkDNSSettings.Equals(proposedConfig.NetworkDNSSettings) ||
            !currentConfig.NetworkInterfacesSettings.NetworkInterfaces
                .SequenceEqual(proposedConfig.NetworkInterfacesSettings.NetworkInterfaces);
    }

    private protected override async Task<OperationStatus> PrepareConfiguration(
        SystemConfiguration config,
        CancellationToken cancellationToken = default)
    {
        var hasWarning = false;

        var status = await SetMulticastConfiguration(config).ConfigureAwait(false);

        if (status == OperationStatus.Error)
            return OperationStatus.Error;

        if (status == OperationStatus.Warning)
            hasWarning = true;

        status = await SetStaticHostsConfiguration(config).ConfigureAwait(false);

        if (status == OperationStatus.Error)
            return OperationStatus.Error;

        if (status == OperationStatus.Warning)
            hasWarning = true;

        status = await SetDNSConfiguration(config).ConfigureAwait(false);

        if (status == OperationStatus.Error)
            return OperationStatus.Error;

        if (status == OperationStatus.Warning)
            hasWarning = true;

        return hasWarning ? OperationStatus.Warning : OperationStatus.Success;
    }

    private async Task<OperationStatus> SetMulticastConfiguration(SystemConfiguration config)
    {
        if (config.NetworkDNSSettings.MulticastDNSEnabled !=
            _currentConfig.NetworkDNSSettings.MulticastDNSEnabled)
        {
            var scriptOptions = new ScriptOptions();

            scriptOptions.Add(
                NetworkMulticastDNSScriptOptionName.MulticastDNSEnabled,
                config.NetworkDNSSettings.MulticastDNSEnabled);

            return await _scriptHandler
                .ExecuteSetNetworkMulticastDNSSettingsScript(scriptOptions)
                .ConfigureAwait(false);
        }

        return OperationStatus.Success;
    }

    private async Task<OperationStatus> SetStaticHostsConfiguration(SystemConfiguration config)
    {
        var scriptOptions = new ScriptOptions();

        if (config.NetworkDNSSettings.StaticHostsEnabled)
        {
            foreach (var staticHost in config.NetworkDNSSettings.StaticHosts)
            {
                scriptOptions.Add(
                    NetworkDNSStaticHostsScriptOptionName.IPAddress,
                    staticHost.IPAddress);

                scriptOptions.Add(
                    NetworkDNSStaticHostsScriptOptionName.Hostname,
                    staticHost.Hostname);
            }
        }

        return await _scriptHandler.ExecuteSetStaticHostsScript(scriptOptions).ConfigureAwait(false);
    }

    private async Task<OperationStatus> SetDNSConfiguration(SystemConfiguration config)
    {
        var scriptOptions = new ScriptOptions();

        // TODO Currently hostname is always send to script even if not changed.
        // This is because the script must be called with at least one parameter.
        // Evaluate all cases where the script has to be called to prevent unneeded calls.
        scriptOptions.Add(
            NetworkDNSScriptOptionName.Hostname,
            config.NetworkDNSSettings.Hostname);

        if (config.NetworkDNSSettings.NameServersEnabled)
        {
            scriptOptions.Add(
                NetworkDNSScriptOptionName.NameServers,
                config.NetworkDNSSettings.NameServers.ToArray());
        }

        if (config.NetworkDNSSettings.DNSSuffixEnabled)
        {
            scriptOptions.Add(
                NetworkDNSScriptOptionName.DNSSuffix,
                config.NetworkDNSSettings.DNSSuffix);
        }

        if (config.NetworkDNSSettings.SearchDomainsEnabled)
        {
            scriptOptions.Add(
                NetworkDNSScriptOptionName.SearchDomains,
                config.NetworkDNSSettings.SearchDomains.ToArray());
        }

        return await _scriptHandler
            .ExecuteSetNetworkDNSSettingsScript(scriptOptions)
            .ConfigureAwait(false);
    }
}
