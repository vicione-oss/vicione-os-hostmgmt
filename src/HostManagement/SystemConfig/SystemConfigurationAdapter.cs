using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Contracts;
using HostManagement.SystemConfig.Network;
using HostManagement.SystemConfig.Service;

namespace HostManagement.SystemConfig;

/// <summary>
/// Represents the system configuration adapter.
/// </summary>
internal sealed class SystemConfigurationAdapter : ConfigurationAdapter
{
    private SystemConfiguration _currentConfig;
    private readonly List<ConfigurationAdapter<SystemConfigurationAdapter>> _adapters = [];

    /// <summary>
    /// Initializes a new <see cref="SystemConfigurationAdapter"/>.
    /// </summary>
    /// <param name="currentConfig">The current system configuration.</param>
    /// <param name="configurationResult">The configuration result.</param>
    /// <param name="scriptHandler">An <see cref="IScriptHandler"/> implementation.</param>
    /// <param name="bypassHasChangesInternal">If set to <see langword="true"/> bypasses the
    /// internal check for changes and returns <see langword="true"/> when
    /// <see cref="ConfigurationAdapter.HasChanges(SystemConfiguration, SystemConfiguration)"/>
    /// is called.</param>
    public SystemConfigurationAdapter(
        SystemConfiguration currentConfig,
        Result configurationResult,
        IScriptHandler scriptHandler,
        bool bypassHasChangesInternal = false)
        : base(bypassHasChangesInternal)
    {
        _currentConfig = currentConfig;

        _adapters.Add(
            new NetworkConfigurationAdapter(
                currentConfig,
                configurationResult,
                scriptHandler,
                bypassHasChangesInternal));

        _adapters.Add(
            new ProxyConfigurationAdapter(
                currentConfig,
                scriptHandler,
                bypassHasChangesInternal));

        _adapters.Add(
            new ServiceConfigurationAdapter(
                scriptHandler,
                bypassHasChangesInternal));
    }

    protected internal override async Task<OperationStatus> ApplyConfiguration(
        SystemConfiguration proposed,
        CancellationToken cancellationToken)
    {
        var hasWarnings = false;

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
        }

        if (hasWarnings)
            return OperationStatus.Warning;

        return OperationStatus.Success;
    }

    private protected override bool HasChangesInternal(SystemConfiguration current, SystemConfiguration proposed)
        => current != proposed;

    protected internal override async Task<OperationStatus> LoadConfiguration(
        SystemConfiguration config,
        CancellationToken cancellationToken)
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
