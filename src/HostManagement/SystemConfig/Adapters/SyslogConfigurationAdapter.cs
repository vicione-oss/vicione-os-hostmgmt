using HostManagement.Scripts;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Enums;
using HostManagement.SystemConfig.Contracts;

namespace HostManagement.SystemConfig.Adapters;

/// <summary>
/// Represents the syslog configuration adapter.
/// </summary>
/// <param name="scriptHandler">An <see cref="IScriptHandler"/> implementation.</param>
/// <param name="bypassHasChanges">
/// <inheritdoc cref="ConfigurationAdapter(bool, bool)" path="/param[@name='bypassHasChanges']"/>
/// </param>
/// <param name="bypassApplyConfig">
/// <inheritdoc cref="ConfigurationAdapter(bool, bool)" path="/param[@name='bypassApplyConfig']"/>
/// </param>
internal sealed class SyslogConfigurationAdapter(
    IScriptHandler scriptHandler,
    bool bypassHasChanges,
    bool bypassApplyConfig)
    : ConfigurationAdapter<SystemConfigurationAdapter>(bypassHasChanges, bypassApplyConfig)
{
    private const string NetlogdServiceName = "systemd-netlogd.service";

    private readonly IScriptHandler _scriptHandler = scriptHandler;

    protected internal override async Task<Result> GetConfiguration(
        SystemConfiguration config,
        CancellationToken cancellationToken)
    {
        var (result, syslogSettings) = await _scriptHandler.ExecuteGetSyslogSettingsScript().ConfigureAwait(false);

        if (result.OperationStatus is OperationStatus.Error)
            return result;

        config.Syslog = syslogSettings;

        return result;
    }

    private protected override bool HasConfigurationChanges(
        SystemConfiguration currentConfig,
        SystemConfiguration proposedConfig)
        => SystemConfigurationChangeDetector.HasChanges(currentConfig.Syslog, proposedConfig.Syslog);

    private protected override async Task<Result> PrepareConfiguration(
        SystemConfiguration config,
        CancellationToken cancellationToken)
    {
        var scriptOptions = new ScriptOptions();

        AddSyslogScriptOptions(scriptOptions, config.Syslog);

        if (!scriptOptions.Options.Any())
            return Result.SuccessResult;

        return await _scriptHandler.ExecuteSetSyslogSettingsScript(scriptOptions).ConfigureAwait(false);
    }

    private protected override async Task<Result> ApplyConfiguration(
        SystemConfiguration config,
        CancellationToken cancellationToken)
    {
        var scriptOptions = new ScriptOptions();
        scriptOptions.Add(ScriptOptionNames.SetSystemdUnitState.Name, NetlogdServiceName);
        scriptOptions.Add(ScriptOptionNames.SetSystemdUnitState.State, ServiceControlAction.Restart.ToString());

        return await _scriptHandler.ExecuteSetSystemdUnitStateScript(scriptOptions).ConfigureAwait(false);
    }

    private static void AddSyslogScriptOptions(ScriptOptions scriptOptions, SyslogSettings? syslogSettings)
    {
        if (syslogSettings is not { } syslog)
            return;

        var newSharedSyslogEquivalent = new SyslogSettings()
        {
            Enabled = false,
            Address = string.Empty,
            TransportProtocol = SyslogTransportProtocol.Udp,
            LogFormat = SyslogLogFormat.Rfc5424,
            ConnectionRetrySeconds = 30,
            ExcludeSyslogFacilities = [],
            ExcludeSyslogLevels = []
        };

        if (syslog.Equals(newSharedSyslogEquivalent))
        {
            scriptOptions.Add(ScriptOptionNames.Syslog.Clear);
            return;
        }

        if (syslog.Enabled is not bool enabled ||
            syslog.Address is not string address ||
            syslog.TransportProtocol is not SyslogTransportProtocol protocol ||
            syslog.LogFormat is not SyslogLogFormat format ||
            syslog.ConnectionRetrySeconds is not int retrySec ||
            syslog.ExcludeSyslogFacilities is not { } facilities ||
            syslog.ExcludeSyslogLevels is not { } levels)
        {
            return;
        }

        scriptOptions.Add(ScriptOptionNames.Syslog.Enabled, enabled);
        scriptOptions.Add(ScriptOptionNames.Syslog.Address, address);
        scriptOptions.Add(ScriptOptionNames.Syslog.Protocol, protocol.ToString().ToLowerInvariant());
        scriptOptions.Add(ScriptOptionNames.Syslog.LogFormat, format.ToString().ToLowerInvariant());
        scriptOptions.Add(ScriptOptionNames.Syslog.ConnectionRetrySec, retrySec);

        if (facilities.Count > 0)
        {
            var facilitiesArray = facilities
                .Select(facility =>
                {
                    if (facility == SyslogFacility.SolarisCron)
                        return "solaris-cron";
                    else
                        return facility.ToString().ToLowerInvariant();
                })
                .ToArray();

            scriptOptions.Add(ScriptOptionNames.Syslog.ExcludeSyslogFacility, facilitiesArray);
        }

        if (levels.Count > 0)
        {
            var levelsArray = levels
                .Select(level => level.ToString().ToLowerInvariant())
                .ToArray();

            scriptOptions.Add(ScriptOptionNames.Syslog.ExcludeSyslogLevel, levelsArray);
        }
    }
}
