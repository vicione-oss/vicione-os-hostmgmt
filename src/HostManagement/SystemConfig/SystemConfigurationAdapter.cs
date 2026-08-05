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
    private readonly IReadOnlyList<ConfigurationAdapter<SystemConfigurationAdapter>> _adapters = [];

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
            : this(
                currentConfig,
                [
                    new NetworkConfigurationAdapter(
                        currentConfig,
                        scriptHandler,
                        bypassHasChanges,
                        bypassApplyConfig),
                    new ProxyConfigurationAdapter(
                        currentConfig,
                        scriptHandler,
                        bypassHasChanges,
                        bypassApplyConfig),
                    new NetworkNTPConfigurationAdapter(
                        scriptHandler,
                        bypassHasChanges,
                        bypassApplyConfig),
                    new ServiceConfigurationAdapter(
                        scriptHandler,
                        EnvironmentVariables.ConfigurableServices.Value,
                        bypassHasChanges,
                        bypassApplyConfig)
                ],
                bypassHasChanges,
                bypassApplyConfig)
    {
    }

    /// <summary>
    /// Initializes a new <see cref="SystemConfigurationAdapter"/> with the specified child adapters.
    /// </summary>
    /// <param name="currentConfig">The current system configuration.</param>
    /// <param name="adapters">The child configuration adapters.</param>
    /// <param name="bypassHasChanges">
    /// <inheritdoc cref="ConfigurationAdapter(bool, bool)" path="/param[@name='bypassHasChanges']"/>
    /// </param>
    /// <param name="bypassApplyConfig">
    /// <inheritdoc cref="ConfigurationAdapter(bool, bool)" path="/param[@name='bypassApplyConfig']"/>
    /// </param>
    internal SystemConfigurationAdapter(
        SystemConfiguration currentConfig,
        IReadOnlyList<ConfigurationAdapter<SystemConfigurationAdapter>> adapters,
        bool bypassHasChanges,
        bool bypassApplyConfig)
        : base(bypassHasChanges, bypassApplyConfig)
    {
        _currentConfig = currentConfig;
        _adapters = adapters;
    }

    protected internal override async Task<Result> GetConfiguration(
        SystemConfiguration config,
        CancellationToken cancellationToken)
    {
        var result = Result.SuccessResult;

        foreach (var adapter in _adapters)
        {
            result.AddResult(await adapter.GetConfiguration(config, cancellationToken).ConfigureAwait(false));

            if (result.OperationStatus is OperationStatus.Error)
                return result;
        }

        return result;
    }

    private protected override bool HasConfigurationChanges(SystemConfiguration current, SystemConfiguration proposed)
        => SystemConfigurationChangeDetector.HasChanges(current, proposed);

    private protected override async Task<Result> PrepareConfiguration(
        SystemConfiguration config,
        CancellationToken cancellationToken)
    {
        var result = Result.SuccessResult;

        foreach (var adapter in _adapters)
        {
            if (!adapter.HasChanges(_currentConfig, config))
                continue;

            result.AddResult(await adapter.SetConfiguration(config, cancellationToken).ConfigureAwait(false));

            if (result.OperationStatus is OperationStatus.Error)
                return result;
        }

        return result;
    }
}
