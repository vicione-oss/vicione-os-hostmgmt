using HostManagement.Scripts.ScriptOptionNames;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Contracts;

namespace HostManagement.SystemConfig.Network;

/// <summary>
/// Represents the network ntp configuration adapter.
/// </summary>
/// <param name="scriptHandler">An <see cref="IScriptHandler"/> implementation.</param>
/// <param name="bypassHasChanges">
/// <inheritdoc cref="ConfigurationAdapter(bool, bool)" path="/param[@name='bypassHasChanges']"/>
/// </param>
/// <param name="bypassApplyConfig">
/// <inheritdoc cref="ConfigurationAdapter(bool, bool)" path="/param[@name='bypassApplyConfig']"/>
/// </param>
internal sealed class NetworkNTPConfigurationAdapter(
    IScriptHandler scriptHandler,
    bool bypassHasChanges,
    bool bypassApplyConfig)
        : ConfigurationAdapter<SystemConfigurationAdapter>(bypassHasChanges, bypassApplyConfig)
{
    private const string SystemdTimesyncd = "systemd-timesyncd";
    private const string RestartState = "Restart";

    private readonly IScriptHandler _scriptHandler = scriptHandler;

    protected internal override async Task<OperationStatus> GetConfiguration(
        SystemConfiguration config,
        CancellationToken cancellationToken = default)
    {
        var (status, ntpSettings) = await _scriptHandler
            .ExecuteGetNetworkNTPSettingsScript()
            .ConfigureAwait(false);

        if (status == OperationStatus.Error)
            return OperationStatus.Error;

        config.NetworkNTPSettings = ntpSettings;

        return OperationStatus.Success;
    }

    private protected override bool HasChangesInternal(
        SystemConfiguration currentConfig,
        SystemConfiguration proposedConfig)
    {
        return !currentConfig.NetworkNTPSettings.Equals(proposedConfig.NetworkNTPSettings);
    }

    private protected override async Task<OperationStatus> PrepareConfiguration(
        SystemConfiguration config,
        CancellationToken cancellationToken = default)
    {
        var scriptOptions = new ScriptOptions();

        scriptOptions.Add(
            NetworkNTPScriptOptionName.NTPServersEnabled,
            config.NetworkNTPSettings.NTPServersEnabled);

        var ntpList = config.NetworkNTPSettings.NTPServers.Count > 0
            ? config.NetworkNTPSettings.NTPServers
            : ["none"];

        scriptOptions.Add(
            NetworkNTPScriptOptionName.NTPServers,
            ntpList.ToArray());

        return await _scriptHandler
            .ExecuteSetNetworkNTPSettingsScript(scriptOptions)
            .ConfigureAwait(false);
    }

    private protected override async Task<OperationStatus> ApplyConfiguration(
        SystemConfiguration config,
        CancellationToken cancellationToken = default)
    {
        var scriptOptions = new ScriptOptions();

        scriptOptions.Add(
            SetSystemdUnitStateScriptOptionName.Name,
            SystemdTimesyncd);

        scriptOptions.Add(
            SetSystemdUnitStateScriptOptionName.State,
            RestartState);

        return await _scriptHandler
            .ExecuteSetSystemdUnitStateScript(scriptOptions)
            .ConfigureAwait(false);
    }
}
