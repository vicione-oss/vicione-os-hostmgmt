using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Communication.Extensions;
using HostManagement.SystemConfig.Contracts;

namespace HostManagement.SystemConfig;

/// <summary>
/// Represents an abstract generic configuration adapter.
/// </summary>
/// <typeparam name="TParentConfigurationAdapter">The type of the parent adapter.</typeparam>
/// <param name="bypassHasChanges">If set to <see langword="true"/> bypasses the check for
/// changes and returns <see langword="true"/> when
/// <see cref="ConfigurationAdapter.HasChanges(SystemConfiguration, SystemConfiguration)"/>
/// is called.</param>
/// <param name="bypassApplyConfig">If set to <see langword="true"/> bypasses the apply logic,
/// only preparing the configuration by writing changes to the system when
/// <see cref="ConfigurationAdapter.SetConfiguration(SystemConfiguration, CancellationToken)"/>
/// is called.</param>
internal abstract class ConfigurationAdapter<TParentConfigurationAdapter>(
    bool bypassHasChanges = false,
    bool bypassApplyConfig = false)
        : ConfigurationAdapter(bypassHasChanges, bypassApplyConfig)
        where TParentConfigurationAdapter : ConfigurationAdapter
{
}

/// <summary>
/// Represents an abstract configuration adapter.
/// </summary>
/// <param name="bypassHasChanges">If set to <see langword="true"/> bypasses the check for
/// changes and returns <see langword="true"/> when
/// <see cref="HasChanges(SystemConfiguration, SystemConfiguration)"/> is called.</param>
/// <param name="bypassApplyConfig">If set to <see langword="true"/> bypasses the apply logic,
/// only preparing the configuration by writing changes to the system when
/// <see cref="SetConfiguration(SystemConfiguration, CancellationToken)"/> is called.</param>
internal abstract class ConfigurationAdapter(bool bypassHasChanges = false, bool bypassApplyConfig = false)
{
    private readonly bool _bypassHasChanges = bypassHasChanges;
    private readonly bool _bypassApplyConfig = bypassApplyConfig;

    /// <inheritdoc cref="HasChangesInternal(SystemConfiguration, SystemConfiguration)"/>
    /// <remarks>
    /// Can be bypassed via the adapter's constructor (so always returns <see langword="true"/>).
    /// </remarks>
    internal bool HasChanges(SystemConfiguration currentConfig, SystemConfiguration proposedConfig)
        => _bypassHasChanges || HasChangesInternal(currentConfig, proposedConfig);

    /// <summary>
    /// Sets a <see cref="SystemConfiguration"/> to the system.
    /// </summary>
    /// <param name="config">The configuration.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><see cref="OperationStatus"/></returns>
    protected internal async Task<OperationStatus> SetConfiguration(
        SystemConfiguration config,
        CancellationToken cancellationToken = default)
    {
        var prepareConfigStatus =
            await PrepareConfiguration(config, cancellationToken).ConfigureAwait(false);

        if (prepareConfigStatus == OperationStatus.Error)
            return prepareConfigStatus;

        if (_bypassApplyConfig)
            return prepareConfigStatus;

        var applyConfigStatus = await ApplyConfiguration(config, cancellationToken).ConfigureAwait(false);

        return prepareConfigStatus.GetCombinedStatus(applyConfigStatus);
    }

    /// <summary>
    /// Gets the current <see cref="SystemConfiguration"/> from the system.
    /// </summary>
    /// <param name="config">The current configuration.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><see cref="OperationStatus"/></returns>
    protected internal abstract Task<OperationStatus> GetConfiguration(
        SystemConfiguration config,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines if the <paramref name="proposedConfig"/> has changes compared to the
    /// <paramref name="currentConfig"/>.
    /// </summary>
    /// <param name="currentConfig">The current configuration.</param>
    /// <param name="proposedConfig">The proposed configuration.</param>
    /// <returns><see langword="true"/> if changed, otherwise <see langword="false"/></returns>
    private protected abstract bool HasChangesInternal(
        SystemConfiguration currentConfig,
        SystemConfiguration proposedConfig);

    /// <summary>
    /// Prepares the <paramref name="config"/> by writing changes to the system without applying them.
    /// </summary>
    /// <remarks>
    /// If a prepare phase is needed, override this method.
    /// </remarks>
    /// <param name="config">The configuration.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><see cref="OperationStatus"/></returns>
    private protected virtual Task<OperationStatus> PrepareConfiguration(
        SystemConfiguration config,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(OperationStatus.Success);
    }

    /// <summary>
    /// Applies the <paramref name="config"/>, including optionally prepared changes via
    /// <see cref="PrepareConfiguration(SystemConfiguration, CancellationToken)"/>.
    /// </summary>
    /// <remarks>
    /// If a separate apply phase is needed, override this method.
    /// </remarks>
    /// <param name="config">The configuration.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><see cref="OperationStatus"/></returns>
    private protected virtual Task<OperationStatus> ApplyConfiguration(
        SystemConfiguration config,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(OperationStatus.Success);
    }
}
