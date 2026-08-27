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
                currentConfig.NetworkInterfaces,
                proposedConfig.NetworkInterfaces);
    }

    private protected override async Task<Result> PrepareConfiguration(
        SystemConfiguration config,
        CancellationToken cancellationToken)
    {
        if (config.NetworkDNSSettings is not { } dnsSettings)
            return Result.SuccessResult;

        var result = Result.SuccessResult;

        result.AddResult(await SetMulticastConfiguration(dnsSettings).ConfigureAwait(false));

        if (result.OperationStatus is OperationStatus.Error)
            return result;

        result.AddResult(await SetStaticHostsConfiguration(dnsSettings.StaticHosts).ConfigureAwait(false));

        if (result.OperationStatus is OperationStatus.Error)
            return result;

        result.AddResult(await SetDNSConfiguration(dnsSettings).ConfigureAwait(false));

        if (result.OperationStatus is OperationStatus.Error)
            return result;

        return result.AddResult(await SetHostname(dnsSettings.Hostname).ConfigureAwait(false));
    }

    private async Task<Result> SetMulticastConfiguration(NetworkDNSSettings dnsSettings)
    {
        if (dnsSettings.MulticastDNSEnabled is not bool enabled ||
            enabled.Equals(_currentConfig.NetworkDNSSettings?.MulticastDNSEnabled))
        {
            return Result.SuccessResult;
        }

        var combinedResult = Result.SuccessResult;

        var (result, rawInterfacesString) = await _scriptHandler
            .ExecuteGetNetworkInterfacesScript()
            .ConfigureAwait(false);

        combinedResult.AddResult(result);

        if (combinedResult.OperationStatus is OperationStatus.Error)
            return combinedResult;

        var scriptOptions = new ScriptOptions();
        scriptOptions.Add(ScriptOptionNames.NetworkMulticastDNS.MulticastDNSEnabled, enabled);
        scriptOptions.Add(ScriptOptionNames.NetworkMulticastDNS.Interfaces, rawInterfacesString);

        result = await _scriptHandler
            .ExecuteSetNetworkMulticastDNSSettingsScript(scriptOptions)
            .ConfigureAwait(false);

        return combinedResult.AddResult(result);
    }

    private async Task<Result> SetStaticHostsConfiguration(StaticHostsSettings? staticHosts)
    {
        if (staticHosts is null)
            return Result.SuccessResult;

        if (staticHosts.Enabled is not bool enabled)
            return Result.SuccessResult;

        var scriptOptions = new ScriptOptions();
        scriptOptions.Add(ScriptOptionNames.NetworkDNSStaticHosts.Enabled, enabled);

        if (staticHosts.Hosts is { } hosts)
        {
            foreach (var host in hosts)
            {
                scriptOptions.Add(ScriptOptionNames.NetworkDNSStaticHosts.IPAddress, host.IPAddress);
                scriptOptions.Add(ScriptOptionNames.NetworkDNSStaticHosts.Hostname, host.Hostname);
            }
        }

        return await _scriptHandler.ExecuteSetStaticHostsScript(scriptOptions).ConfigureAwait(false);
    }

    private async Task<Result> SetDNSConfiguration(NetworkDNSSettings dnsSettings)
    {
        var scriptOptions = new ScriptOptions();

        AddNameServersScriptOptions(scriptOptions, dnsSettings.NameServers);
        AddPrimaryDNSSuffixScriptOptions(scriptOptions, dnsSettings.PrimaryDNSSuffix);
        AddSearchDomainsScriptOptions(scriptOptions, dnsSettings.SearchDomains);

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

    private static void AddNameServersScriptOptions(
        ScriptOptions scriptOptions,
        NameServersSettings? nameServersSettings)
    {
        if (nameServersSettings is null)
            return;

        if (nameServersSettings.Enabled is not bool enabled ||
            nameServersSettings.Addresses is not { } addresses ||
            addresses.Count <= 0)
        {
            return;
        }

        scriptOptions.Add(ScriptOptionNames.NetworkDNS.NameServersEnabled, enabled);
        scriptOptions.Add(ScriptOptionNames.NetworkDNS.NameServers, addresses.ToArray());
    }

    private static void AddPrimaryDNSSuffixScriptOptions(
        ScriptOptions scriptOptions,
        PrimaryDNSSuffixSettings? primarySuffixSettings)
    {
        if (primarySuffixSettings is null)
            return;

        if (primarySuffixSettings.Enabled is not bool enabled ||
            primarySuffixSettings.Suffix is not string suffix)
        {
            return;
        }

        scriptOptions.Add(ScriptOptionNames.NetworkDNS.DNSSuffixEnabled, enabled);
        scriptOptions.Add(ScriptOptionNames.NetworkDNS.DNSSuffix, suffix);
    }

    private static void AddSearchDomainsScriptOptions(
        ScriptOptions scriptOptions,
        SearchDomainsSettings? searchDomainsSettings)
    {
        if (searchDomainsSettings is null)
            return;

        if (searchDomainsSettings.Enabled is not bool enabled ||
            searchDomainsSettings.Domains is not { } domains ||
            domains.Count <= 0)
        {
            return;
        }

        scriptOptions.Add(ScriptOptionNames.NetworkDNS.SearchDomainsEnabled, enabled);
        scriptOptions.Add(ScriptOptionNames.NetworkDNS.SearchDomains, domains.ToArray());
    }
}
