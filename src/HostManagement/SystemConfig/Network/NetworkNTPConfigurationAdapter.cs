using HostManagement.Scripts;
using HostManagement.Shared.Communication.Enums;
using HostManagement.SystemConfig.Contracts;

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

    protected internal override async Task<Result> GetConfiguration(
        SystemConfiguration config,
        CancellationToken cancellationToken)
    {
        var (result, ntpSettings) = await _scriptHandler.ExecuteGetNetworkNTPSettingsScript().ConfigureAwait(false);

        if (result.OperationStatus is not OperationStatus.Error)
            config.NetworkNTPSettings = ntpSettings;

        return result;
    }

    private protected override bool HasConfigurationChanges(
        SystemConfiguration currentConfig,
        SystemConfiguration proposedConfig)
    {
        return SystemConfigurationChangeDetector.HasChanges(
            currentConfig.NetworkNTPSettings,
            proposedConfig.NetworkNTPSettings);
    }

    private protected override async Task<Result> PrepareConfiguration(
        SystemConfiguration config,
        CancellationToken cancellationToken)
    {
        if (config.NetworkNTPSettings is not { } ntpSettings)
            return Result.SuccessResult;

        if (ntpSettings.Enabled is not bool enabled)
            return Result.SuccessResult;

        var ntpList = ntpSettings.Servers?.Count > 0
            ? ntpSettings.Servers.ToArray()
            : ["none"];

        var scriptOptions = new ScriptOptions();
        scriptOptions.Add(ScriptOptionNames.NetworkNTP.NTPServersEnabled, enabled);
        scriptOptions.Add(ScriptOptionNames.NetworkNTP.NTPServers, ntpList);

        return await _scriptHandler.ExecuteSetNetworkNTPSettingsScript(scriptOptions).ConfigureAwait(false);
    }

    private protected override async Task<Result> ApplyConfiguration(
        SystemConfiguration config,
        CancellationToken cancellationToken)
    {
        var scriptOptions = new ScriptOptions();
        scriptOptions.Add(ScriptOptionNames.SetSystemdUnitState.Name, SystemdTimesyncd);
        scriptOptions.Add(ScriptOptionNames.SetSystemdUnitState.State, RestartState);

        return await _scriptHandler.ExecuteSetSystemdUnitStateScript(scriptOptions).ConfigureAwait(false);
    }
}
