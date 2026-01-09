using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Contracts;

namespace HostManagement.SystemConfig.Network;

internal sealed class NetworkConfigurationAdapter : ConfigurationAdapter<SystemConfigurationAdapter>
{
    private readonly SystemConfiguration _currentConfig;
    private readonly IScriptHandler _scriptHandler;
    private readonly List<ConfigurationAdapter<NetworkConfigurationAdapter>> _adapters = [];

    /// <summary>
    /// Initializes a new <see cref="NetworkConfigurationAdapter"/>.
    /// </summary>
    /// <param name="currentConfig">The current system configuration.</param>
    /// <param name="configurationResult">The configuration result.</param>
    /// <param name="scriptHandler">An <see cref="IScriptHandler"/> implementation.</param>
    /// <param name="bypassHasChangesInternal">If set to <see langword="true"/> bypasses the
    /// internal check for changes and returns <see langword="true"/> when
    /// <see cref="ConfigurationAdapter.HasChanges(SystemConfiguration, SystemConfiguration)"/>
    /// is called.</param>
    public NetworkConfigurationAdapter(
        SystemConfiguration currentConfig,
        Result configurationResult,
        IScriptHandler scriptHandler,
        bool bypassHasChangesInternal = false)
        : base(bypassHasChangesInternal)
    {
        _currentConfig = currentConfig;
        _scriptHandler = scriptHandler;

        _adapters.Add(
            new NetworkInterfacesConfigurationAdapter(
                configurationResult,
                scriptHandler,
                bypassHasChangesInternal));

        _adapters.Add(
            new NetworkDNSConfigurationAdapter(
                currentConfig,
                scriptHandler,
                bypassHasChangesInternal));

        _adapters.Add(
            new NetworkNTPConfigurationAdapter(
                scriptHandler,
                bypassHasChangesInternal));
    }

    protected internal override async Task<OperationStatus> ApplyConfiguration(
        SystemConfiguration proposed,
        CancellationToken cancellationToken)
    {
        var hasWarnings = false;
        var isRestartRequired = false;

        foreach (var adapter in _adapters)
        {
            if (!adapter.HasChanges(_currentConfig, proposed))
                continue;

            var operationStatus = await adapter
                .ApplyConfiguration(proposed, cancellationToken)
                .ConfigureAwait(false);

            if (operationStatus == OperationStatus.Error)
                return operationStatus;

            if (operationStatus == OperationStatus.Warning)
                hasWarnings = true;

            isRestartRequired = true;
        }

        if (isRestartRequired)
        {
            if (await _scriptHandler.ExecuteApplyNetplanConfigurationScript() == OperationStatus.Error)
                return OperationStatus.Error;
        }

        if (hasWarnings)
            return OperationStatus.Warning;

        return OperationStatus.Success;
    }

    private protected override bool HasChangesInternal(SystemConfiguration currentConfig, SystemConfiguration proposedConfig)
    {
        return
            !currentConfig.NetworkInterfacesSettings.NetworkInterfaces
                .SequenceEqual(proposedConfig.NetworkInterfacesSettings.NetworkInterfaces) ||
            !currentConfig.NetworkDNSSettings.Equals(proposedConfig.NetworkDNSSettings) ||
            !currentConfig.NetworkNTPSettings.Equals(proposedConfig.NetworkNTPSettings);
    }

    protected internal override async Task<OperationStatus> LoadConfiguration(SystemConfiguration config, CancellationToken cancellationToken)
    {
        var hasWarnings = false;

        foreach (var adapter in _adapters)
        {
            var operationStatus = await adapter
                .LoadConfiguration(config, cancellationToken)
                .ConfigureAwait(false);

            if (operationStatus == OperationStatus.Error)
                return operationStatus;

            if (operationStatus == OperationStatus.Warning)
                hasWarnings = true;
        }

        if (hasWarnings)
            return OperationStatus.Warning;

        return OperationStatus.Success;
    }
}
