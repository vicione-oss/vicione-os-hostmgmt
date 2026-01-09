using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Contracts;

namespace HostManagement.SystemConfig.Network;

/// <summary>
/// Represents the network configuration adapter.
/// </summary>
internal sealed class NetworkConfigurationAdapter : ConfigurationAdapter<SystemConfigurationAdapter>
{
    private readonly SystemConfiguration _currentConfig;
    private readonly IScriptHandler _scriptHandler;
    private readonly List<ConfigurationAdapter<NetworkConfigurationAdapter>> _adapters = [];

    private bool _isNetplanApplyRequired;

    /// <summary>
    /// Initializes a new <see cref="NetworkConfigurationAdapter"/>.
    /// </summary>
    /// <param name="currentConfig">The current system configuration.</param>
    /// <param name="scriptHandler">An <see cref="IScriptHandler"/> implementation.</param>
    /// <param name="bypassHasChanges">
    /// <inheritdoc cref="ConfigurationAdapter(bool, bool)" path="/param[@name='bypassHasChanges']"/>
    /// </param>
    /// <param name="bypassApplyConfig">
    /// <inheritdoc cref="ConfigurationAdapter(bool, bool)" path="/param[@name='bypassApplyConfig']"/>
    /// </param>
    public NetworkConfigurationAdapter(
        SystemConfiguration currentConfig,
        IScriptHandler scriptHandler,
        bool bypassHasChanges,
        bool bypassApplyConfig)
        : base(bypassHasChanges, bypassApplyConfig)
    {
        _currentConfig = currentConfig;
        _scriptHandler = scriptHandler;

        _adapters.Add(
            new NetworkInterfacesConfigurationAdapter(
                scriptHandler,
                bypassHasChanges,
                bypassApplyConfig));

        _adapters.Add(
            new NetworkDNSConfigurationAdapter(
                currentConfig,
                scriptHandler,
                bypassHasChanges,
                bypassApplyConfig));
    }

    protected internal override async Task<OperationStatus> GetConfiguration(
        SystemConfiguration config,
        CancellationToken cancellationToken = default)
    {
        var hasWarnings = false;

        foreach (var adapter in _adapters)
        {
            var operationStatus = await adapter
                .GetConfiguration(config, cancellationToken)
                .ConfigureAwait(false);

            if (operationStatus == OperationStatus.Error)
                return operationStatus;

            if (operationStatus == OperationStatus.Warning)
                hasWarnings = true;
        }

        return hasWarnings ? OperationStatus.Warning : OperationStatus.Success;
    }

    private protected override bool HasChangesInternal(
        SystemConfiguration currentConfig,
        SystemConfiguration proposedConfig)
    {
        return
            !currentConfig.NetworkInterfacesSettings.NetworkInterfaces
                .SequenceEqual(proposedConfig.NetworkInterfacesSettings.NetworkInterfaces) ||
            !currentConfig.NetworkDNSSettings.Equals(proposedConfig.NetworkDNSSettings);
    }

    private protected override async Task<OperationStatus> PrepareConfiguration(
        SystemConfiguration config,
        CancellationToken cancellationToken = default)
    {
        var hasWarnings = false;

        foreach (var adapter in _adapters)
        {
            if (!adapter.HasChanges(_currentConfig, config))
                continue;

            var operationStatus = await adapter
                .SetConfiguration(config, cancellationToken)
                .ConfigureAwait(false);

            if (operationStatus == OperationStatus.Error)
                return operationStatus;

            if (operationStatus == OperationStatus.Warning)
                hasWarnings = true;

            _isNetplanApplyRequired = true;
        }

        return hasWarnings ? OperationStatus.Warning : OperationStatus.Success;
    }

    private protected override async Task<OperationStatus> ApplyConfiguration(
        SystemConfiguration config,
        CancellationToken cancellationToken = default)
    {
        if (_isNetplanApplyRequired)
        {
            return await _scriptHandler
                .ExecuteApplyNetplanConfigurationScript()
                .ConfigureAwait(false);
        }

        return OperationStatus.Success;
    }
}
