using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Contracts;

namespace HostManagement.SystemConfig;

/// <summary>
/// Represents an abstract generic configuration adapter.
/// </summary>
/// <typeparam name="TParentConfigurationAdapter">The type of the parent
/// <see cref="ConfigurationAdapter"/>.</typeparam>
internal abstract class ConfigurationAdapter<TParentConfigurationAdapter> : ConfigurationAdapter
    where TParentConfigurationAdapter : ConfigurationAdapter
{
    /// <summary>
    /// Initializes a new <see cref="ConfigurationAdapter"/>.
    /// </summary>
    /// <param name="bypassHasChangesInternal">If set to <see langword="true"/> bypasses the
    /// internal check for changes and returns <see langword="true"/> when
    /// <see cref="ConfigurationAdapter.HasChanges(SystemConfiguration, SystemConfiguration)"/>
    /// is called.</param>
    public ConfigurationAdapter(bool bypassHasChangesInternal = false)
        : base(bypassHasChangesInternal)
    { }
}

/// <summary>
/// Represents an abstract configuration adapter.
/// </summary>
internal abstract class ConfigurationAdapter
{
    private readonly bool _bypassHasChangesInternal;

    /// <summary>
    /// Initializes a new <see cref="ConfigurationAdapter"/>.
    /// </summary>
    /// <param name="bypassHasChangesInternal">If set to <see langword="true"/> bypasses the
    /// internal check for changes and returns <see langword="true"/> when
    /// <see cref="HasChanges(SystemConfiguration, SystemConfiguration)"/> is called.</param>
    public ConfigurationAdapter(bool bypassHasChangesInternal = false)
    {
        _bypassHasChangesInternal = bypassHasChangesInternal;
    }

    /// <inheritdoc cref="HasChangesInternal(SystemConfiguration, SystemConfiguration)"/>
    /// <remarks>
    /// Can be bypassed via the adapter's constructor (so always returns <see langword="true"/>).
    /// </remarks>
    internal bool HasChanges(SystemConfiguration currentConfig, SystemConfiguration proposedConfig)
    {
        return
            _bypassHasChangesInternal ||
            HasChangesInternal(currentConfig, proposedConfig);
    }

    /// <summary>
    /// Applies a <see cref="SystemConfiguration"/>.
    /// </summary>
    /// <param name="config">The configuration.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><see cref="OperationStatus"/></returns>
    protected internal abstract Task<OperationStatus> ApplyConfiguration(
        SystemConfiguration config,
        CancellationToken cancellationToken);

    /// <summary>
    /// Loads the current configuration from the system.
    /// </summary>
    /// <param name="config">The current configuration.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><see cref="OperationStatus"/></returns>
    protected internal abstract Task<OperationStatus> LoadConfiguration(
        SystemConfiguration config,
        CancellationToken cancellationToken);

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
}
