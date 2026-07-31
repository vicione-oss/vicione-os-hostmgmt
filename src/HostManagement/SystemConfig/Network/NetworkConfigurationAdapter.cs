using HostManagement.Shared.Communication.Enums;
using HostManagement.SystemConfig.Contracts;

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

    private protected override bool HasConfigurationChanges(
        SystemConfiguration currentConfig,
        SystemConfiguration proposedConfig)
    {
        return
            SystemConfigurationChangeDetector.HasSequenceChanges(
                currentConfig.NetworkInterfaces,
                proposedConfig.NetworkInterfaces) ||
            SystemConfigurationChangeDetector.HasChanges(
                currentConfig.NetworkDNSSettings,
                proposedConfig.NetworkDNSSettings);
    }

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

            _isNetplanApplyRequired = true;
        }

        return result;
    }

    private protected override async Task<Result> ApplyConfiguration(
        SystemConfiguration config,
        CancellationToken cancellationToken)
    {
        if (_isNetplanApplyRequired)
            return await _scriptHandler.ExecuteApplyNetplanConfigurationScript().ConfigureAwait(false);

        return Result.SuccessResult;
    }
}
