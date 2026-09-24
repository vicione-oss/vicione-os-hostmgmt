using HostManagement.Shared.Communication.Enums;
using HostManagement.SystemConfig.Contracts;

namespace HostManagement.SystemConfig.Adapters;

/// <summary>
/// Represents an abstract generic configuration adapter.
/// </summary>
/// <typeparam name="TParentConfigurationAdapter">The type of the parent adapter.</typeparam>
/// <param name="bypassHasChanges">If set to <see langword="true"/> bypasses the change check.</param>
/// <param name="bypassApplyConfig">If set to <see langword="true"/> bypasses the apply logic.</param>
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
/// <param name="bypassHasChanges">If set to <see langword="true"/> bypasses the change check.</param>
/// <param name="bypassApplyConfig">If set to <see langword="true"/> bypasses the apply logic.</param>
internal abstract class ConfigurationAdapter(bool bypassHasChanges = false, bool bypassApplyConfig = false)
{
    private readonly bool _bypassHasChanges = bypassHasChanges;
    private readonly bool _bypassApplyConfig = bypassApplyConfig;

    /// <inheritdoc cref="HasConfigurationChanges(SystemConfiguration, SystemConfiguration)"/>
    /// <remarks>
    /// Can be bypassed via the adapter's constructor (so always returns <see langword="true"/>).
    /// </remarks>
    internal bool HasChanges(SystemConfiguration currentConfig, SystemConfiguration proposedConfig)
        => _bypassHasChanges || HasConfigurationChanges(currentConfig, proposedConfig);

    /// <summary>
    /// Gets the current <see cref="SystemConfiguration"/> from the system.
    /// </summary>
    /// <param name="config">The current configuration.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><see cref="Result"/></returns>
    protected internal abstract Task<Result> GetConfiguration(
        SystemConfiguration config,
        CancellationToken cancellationToken);

    /// <summary>
    /// Sets a <see cref="SystemConfiguration"/> to the system.
    /// </summary>
    /// <param name="config">The configuration.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><see cref="Result"/></returns>
    protected internal async Task<Result> SetConfiguration(
        SystemConfiguration config,
        CancellationToken cancellationToken)
    {
        var prepareConfigResult = await PrepareConfiguration(config, cancellationToken).ConfigureAwait(false);

        if (prepareConfigResult.OperationStatus is OperationStatus.Error)
            return prepareConfigResult;

        if (_bypassApplyConfig)
            return prepareConfigResult;

        var applyConfigResult = await ApplyConfiguration(config, cancellationToken).ConfigureAwait(false);

        return prepareConfigResult.AddResult(applyConfigResult);
    }

    /// <summary>
    /// Determines if the <paramref name="proposedConfig"/> has changes compared to the
    /// <paramref name="currentConfig"/>.
    /// </summary>
    /// <param name="currentConfig">The current configuration.</param>
    /// <param name="proposedConfig">The proposed configuration.</param>
    /// <returns><see langword="true"/> if changed, otherwise <see langword="false"/></returns>
    private protected abstract bool HasConfigurationChanges(
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
    /// <returns><see cref="Result"/></returns>
    private protected virtual Task<Result> PrepareConfiguration(
        SystemConfiguration config,
        CancellationToken cancellationToken)
            => Task.FromResult(Result.SuccessResult);

    /// <summary>
    /// Applies the <paramref name="config"/>, including prepared changes via
    /// <see cref="PrepareConfiguration(SystemConfiguration, CancellationToken)"/>.
    /// </summary>
    /// <remarks>
    /// If an apply phase is needed, override this method.
    /// </remarks>
    /// <param name="config">The configuration.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><see cref="Result"/></returns>
    private protected virtual Task<Result> ApplyConfiguration(
        SystemConfiguration config,
        CancellationToken cancellationToken)
            => Task.FromResult(Result.SuccessResult);
}
