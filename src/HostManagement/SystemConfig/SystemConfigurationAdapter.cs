using HostManagement.Configuration;
using HostManagement.Shared.Communication.Enums;
using HostManagement.SystemConfig.Contracts;
using HostManagement.SystemConfig.Network;
using HostManagement.SystemConfig.Service;

namespace HostManagement.SystemConfig;

/// <summary>
/// Represents the system configuration adapter.
/// </summary>
internal sealed class SystemConfigurationAdapter : ConfigurationAdapter
{
    private readonly SystemConfiguration _currentConfig;
    private readonly List<ConfigurationAdapter<SystemConfigurationAdapter>> _adapters = [];

    /// <summary>
    /// Initializes a new <see cref="SystemConfigurationAdapter"/>.
    /// </summary>
    /// <param name="currentConfig">The current system configuration.</param>
    /// <param name="scriptHandler">An <see cref="IScriptHandler"/> implementation.</param>
    /// <param name="bypassHasChanges">
    /// <inheritdoc cref="ConfigurationAdapter(bool, bool)" path="/param[@name='bypassHasChanges']"/>
    /// </param>
    /// <param name="bypassApplyConfig">
    /// <inheritdoc cref="ConfigurationAdapter(bool, bool)" path="/param[@name='bypassApplyConfig']"/>
    /// </param>
    public SystemConfigurationAdapter(
        SystemConfiguration currentConfig,
        IScriptHandler scriptHandler,
        bool bypassHasChanges,
        bool bypassApplyConfig)
        : base(bypassHasChanges, bypassApplyConfig)
    {
        _currentConfig = currentConfig;

        _adapters.Add(
            new NetworkConfigurationAdapter(
                currentConfig,
                scriptHandler,
                bypassHasChanges,
                bypassApplyConfig));

        _adapters.Add(
            new ProxyConfigurationAdapter(
                currentConfig,
                scriptHandler,
                bypassHasChanges,
                bypassApplyConfig));

        _adapters.Add(
            new NetworkNTPConfigurationAdapter(
                scriptHandler,
                bypassHasChanges,
                bypassApplyConfig));

        _adapters.Add(
            new ServiceConfigurationAdapter(
                scriptHandler,
                EnvironmentVariables.ConfigurableServices,
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

    private protected override bool HasChangesInternal(SystemConfiguration current, SystemConfiguration proposed)
        => current != proposed;

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
        }

        return hasWarnings ? OperationStatus.Warning : OperationStatus.Success;
    }
}
