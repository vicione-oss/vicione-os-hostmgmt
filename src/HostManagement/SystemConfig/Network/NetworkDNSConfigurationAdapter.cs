using HostManagement.Scripts.ScriptOptionNames;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;

namespace HostManagement.SystemConfig.Network;

/// <summary>
/// Represents the network DNS configuration adapter.
/// </summary>
internal sealed class NetworkDNSConfigurationAdapter
    : ConfigurationAdapter<NetworkConfigurationAdapter>
{
    private readonly SystemConfiguration _currentConfig;
    private readonly IScriptHandler _scriptHandler;

    /// <summary>
    /// Initializes a new <see cref="NetworkDNSConfigurationAdapter"/>.
    /// </summary>
    /// <param name="currentConfig">The current system configuration.</param>
    /// <param name="scriptHandler">An <see cref="IScriptHandler"/> implementation.</param>
    /// <param name="bypassHasChangesInternal">If set to <see langword="true"/> bypasses the
    /// internal check for changes and returns <see langword="true"/> when
    /// <see cref="ConfigurationAdapter.HasChanges(SystemConfiguration, SystemConfiguration)"/>
    /// is called.</param>
    public NetworkDNSConfigurationAdapter(
        SystemConfiguration currentConfig,
        IScriptHandler scriptHandler,
        bool bypassHasChangesInternal = false)
        : base(bypassHasChangesInternal)
    {
        _currentConfig = currentConfig;
        _scriptHandler = scriptHandler;
    }

    private protected override bool HasChangesInternal(SystemConfiguration currentConfig, SystemConfiguration proposedConfig)
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

    protected internal override async Task<OperationStatus> LoadConfiguration(
        SystemConfiguration config,
        CancellationToken cancellationToken)
    {
        OperationStatus status;
        NetworkDNSSettings? networkDNSSettings;

        (status, networkDNSSettings) = await _scriptHandler.ExecuteGetNetworkDNSSettingsScript();

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

    protected internal override async Task<OperationStatus> ApplyConfiguration(
        SystemConfiguration proposed,
        CancellationToken cancellationToken)
    {
        var hasWarning = false;

        var status = await ApplyMulticastConfiguration(proposed);

        if (status == OperationStatus.Error)
            return OperationStatus.Error;

        if (status == OperationStatus.Warning)
            hasWarning = true;

        status = await ApplyStaticHostsConfiguration(proposed);

        if (status == OperationStatus.Error)
            return OperationStatus.Error;

        if (status == OperationStatus.Warning)
            hasWarning = true;

        status = await ApplyDNSConfiguration(proposed);

        if (status == OperationStatus.Error)
            return OperationStatus.Error;

        if (status == OperationStatus.Warning)
            hasWarning = true;

        return hasWarning ? OperationStatus.Warning : OperationStatus.Success;
    }

    private async Task<OperationStatus> ApplyMulticastConfiguration(SystemConfiguration proposed)
    {
        if (proposed.NetworkDNSSettings.MulticastDNSEnabled !=
            _currentConfig.NetworkDNSSettings.MulticastDNSEnabled)
        {
            var scriptOptions = new ScriptOptions();

            scriptOptions.Add(
                NetworkMulticastDNSScriptOptionName.MulticastDNSEnabled,
                proposed.NetworkDNSSettings.MulticastDNSEnabled);

            return await _scriptHandler.ExecuteSetNetworkMulticastDNSSettingsScript(scriptOptions);
        }

        return OperationStatus.Success;
    }

    private async Task<OperationStatus> ApplyStaticHostsConfiguration(SystemConfiguration proposed)
    {
        var scriptOptions = new ScriptOptions();

        if (proposed.NetworkDNSSettings.StaticHostsEnabled)
        {
            foreach (var staticHost in proposed.NetworkDNSSettings.StaticHosts)
            {
                scriptOptions.Add(
                    NetworkDNSStaticHostsScriptOptionName.IPAddress,
                    staticHost.IPAddress);

                scriptOptions.Add(
                    NetworkDNSStaticHostsScriptOptionName.Hostname,
                    staticHost.Hostname);
            }
        }

        return await _scriptHandler.ExecuteSetStaticHostsScript(scriptOptions);
    }

    private async Task<OperationStatus> ApplyDNSConfiguration(SystemConfiguration proposed)
    {
        var scriptOptions = new ScriptOptions();

        // TODO Currently hostname is always send to script even if not changed.
        // This is because the script must be called with at least one parameter.
        // Evaluate all cases where the script has to be called to prevent unneeded calls.
        scriptOptions.Add(
            NetworkDNSScriptOptionName.Hostname,
            proposed.NetworkDNSSettings.Hostname);

        if (proposed.NetworkDNSSettings.NameServersEnabled)
        {
            scriptOptions.Add(
                NetworkDNSScriptOptionName.NameServers,
                proposed.NetworkDNSSettings.NameServers.ToArray());
        }

        if (proposed.NetworkDNSSettings.DNSSuffixEnabled)
        {
            scriptOptions.Add(
                NetworkDNSScriptOptionName.DNSSuffix,
                proposed.NetworkDNSSettings.DNSSuffix);
        }

        if (proposed.NetworkDNSSettings.SearchDomainsEnabled)
        {
            scriptOptions.Add(
                NetworkDNSScriptOptionName.SearchDomains,
                proposed.NetworkDNSSettings.SearchDomains.ToArray());
        }

        return await _scriptHandler.ExecuteSetNetworkDNSSettingsScript(scriptOptions);
    }
}
