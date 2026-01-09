using HostManagement.Scripts.ScriptOptionNames;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Contracts;

namespace HostManagement.SystemConfig.Network;

internal sealed class NetworkNTPConfigurationAdapter
    : ConfigurationAdapter<NetworkConfigurationAdapter>
{
    private readonly IScriptHandler _scriptHandler;

    /// <summary>
    /// Initializes a new <see cref="NetworkNTPConfigurationAdapter"/>.
    /// </summary>
    /// <param name="scriptHandler">An <see cref="IScriptHandler"/> implementation.</param>
    /// <param name="bypassHasChangesInternal">If set to <see langword="true"/> bypasses the
    /// internal check for changes and returns <see langword="true"/> when
    /// <see cref="ConfigurationAdapter.HasChanges(SystemConfiguration, SystemConfiguration)"/>
    /// is called.</param>
    public NetworkNTPConfigurationAdapter(
        IScriptHandler scriptHandler,
        bool bypassHasChangesInternal = false)
        : base(bypassHasChangesInternal)
    {
        _scriptHandler = scriptHandler;
    }

    private protected override bool HasChangesInternal(SystemConfiguration currentConfig, SystemConfiguration proposedConfig)
        => !currentConfig.NetworkNTPSettings.Equals(proposedConfig.NetworkNTPSettings);

    protected internal override async Task<OperationStatus> LoadConfiguration(
        SystemConfiguration config,
        CancellationToken cancellationToken)
    {
        var (status, ntpSettings) = await _scriptHandler.ExecuteGetNetworkNTPSettingsScript();

        if (status == OperationStatus.Error)
            return OperationStatus.Error;

        config.NetworkNTPSettings = ntpSettings;

        return OperationStatus.Success;
    }

    protected internal override async Task<OperationStatus> ApplyConfiguration(
        SystemConfiguration proposed,
        CancellationToken cancellationToken)
    {
        var hasWarning = false;
        var scriptOptions = new ScriptOptions();

        scriptOptions.Add(
            NetworkNTPScriptOptionName.NTPServers,
            proposed.NetworkNTPSettings.NTPServers.ToArray());

        var status = await _scriptHandler.ExecuteSetNetworkNTPSettingsScript(scriptOptions);

        if (status == OperationStatus.Error)
            return OperationStatus.Error;

        if (status == OperationStatus.Warning)
            hasWarning = true;

        status = await _scriptHandler.ExecuteRestartNtpServiceScript();

        if (status == OperationStatus.Error)
            return OperationStatus.Error;

        if (status == OperationStatus.Warning)
            hasWarning = true;

        return hasWarning ? OperationStatus.Warning : OperationStatus.Success;
    }
}
