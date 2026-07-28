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

    protected internal override async Task<Result> GetConfiguration(
        SystemConfiguration config,
        CancellationToken cancellationToken)
    {
        var (result, networkDNSSettings) = await _scriptHandler
            .ExecuteGetNetworkDNSSettingsScript()
            .ConfigureAwait(false);

        if (result.OperationStatus is not OperationStatus.Error)
            config.NetworkDNSSettings = networkDNSSettings;

        return result;
    }

    private protected override bool HasConfigurationChanges(
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
            SystemConfigurationChangeDetector.HasChanges(
                currentConfig.NetworkDNSSettings,
                proposedConfig.NetworkDNSSettings) ||
            SystemConfigurationChangeDetector.HasSequenceChanges(
                currentConfig.NetworkInterfacesSettings?.NetworkInterfaces,
                proposedConfig.NetworkInterfacesSettings?.NetworkInterfaces);
    }

    private protected override async Task<Result> PrepareConfiguration(
        SystemConfiguration config,
        CancellationToken cancellationToken)
    {
        var dnsSettings = config.NetworkDNSSettings;
        var result = Result.SuccessResult;

        if (dnsSettings is null)
            return result;

        result.AddResult(await SetMulticastConfiguration(dnsSettings).ConfigureAwait(false));

        if (result.OperationStatus is OperationStatus.Error)
            return result;

        result.AddResult(
            await SetStaticHostsConfiguration(dnsSettings.StaticHostsEnabled, dnsSettings.StaticHosts)
                .ConfigureAwait(false));

        if (result.OperationStatus is OperationStatus.Error)
            return result;

        result.AddResult(await SetDNSConfiguration(dnsSettings).ConfigureAwait(false));

        if (result.OperationStatus is OperationStatus.Error)
            return result;

        return result.AddResult(await SetHostname(dnsSettings.Hostname).ConfigureAwait(false));
    }

    private async Task<Result> SetMulticastConfiguration(NetworkDNSSettings dnsSettings)
    {
        if (dnsSettings.MulticastDNSEnabled.HasValue &&
            dnsSettings.MulticastDNSEnabled.Value != _currentConfig.NetworkDNSSettings?.MulticastDNSEnabled)
        {
            var combinedResult = Result.SuccessResult;

            var (result, rawInterfacesString) = await _scriptHandler
                .ExecuteGetNetworkInterfacesScript()
                .ConfigureAwait(false);

            combinedResult.AddResult(result);

            if (combinedResult.OperationStatus is OperationStatus.Error)
                return combinedResult;

            var scriptOptions = new ScriptOptions();

            scriptOptions.Add(
                ScriptOptionNames.NetworkMulticastDNS.MulticastDNSEnabled,
                dnsSettings.MulticastDNSEnabled.Value);

            scriptOptions.Add(
                ScriptOptionNames.NetworkMulticastDNS.Interfaces,
                rawInterfacesString);

            result = await _scriptHandler
                .ExecuteSetNetworkMulticastDNSSettingsScript(scriptOptions)
                .ConfigureAwait(false);

            return combinedResult.AddResult(result);
        }

        return Result.SuccessResult;
    }

    private async Task<Result> SetStaticHostsConfiguration(
        bool? staticHostsEnabled,
        List<StaticHostDetail>? staticHosts)
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

        return Result.SuccessResult;
    }

    private async Task<Result> SetDNSConfiguration(NetworkDNSSettings dnsSettings)
    {
        var scriptOptions = new ScriptOptions();

        AddNameServersScriptOptions(scriptOptions, dnsSettings);
        AddSuffixScriptOptions(scriptOptions, dnsSettings);
        AddSearchDomainsScriptOptions(scriptOptions, dnsSettings);

        if (!scriptOptions.Options.Any())
            return Result.SuccessResult;

        var combinedResult = Result.SuccessResult;

        var (result, rawInterfacesString) = await _scriptHandler
            .ExecuteGetNetworkInterfacesScript()
            .ConfigureAwait(false);

        combinedResult.AddResult(result);

        if (combinedResult.OperationStatus is OperationStatus.Error)
            return combinedResult;

        scriptOptions.Add(ScriptOptionNames.NetworkDNS.Interfaces, rawInterfacesString);

        result = await _scriptHandler.ExecuteSetNetworkDNSSettingsScript(scriptOptions).ConfigureAwait(false);

        return combinedResult.AddResult(result);
    }

    private async Task<Result> SetHostname(string? hostname)
    {
        if (hostname is null || hostname.Equals(_currentConfig.NetworkDNSSettings?.Hostname))
            return Result.SuccessResult;

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
