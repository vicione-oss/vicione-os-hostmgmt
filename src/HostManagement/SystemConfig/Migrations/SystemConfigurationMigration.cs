namespace HostManagement.SystemConfig.Migrations;

/// <summary>
/// Provides a typed base implementation for system configuration contract migrations.
/// </summary>
/// <typeparam name="TSourceConfiguration">The source configuration contract type.</typeparam>
/// <typeparam name="TTargetConfiguration">The target configuration contract type.</typeparam>
/// <remarks>
/// This class bridges the untyped migration orchestration and the typed migration implementation.
/// The public <see cref="Migrate(ISystemConfigurationContract)"/> validates the source contract
/// type and versions before delegating to <see cref="Migrate(TSourceConfiguration)"/>.
/// </remarks>
internal abstract class SystemConfigurationMigration<TSourceConfiguration, TTargetConfiguration>
    : ISystemConfigurationMigration
        where TSourceConfiguration : class, ISystemConfigurationContract
        where TTargetConfiguration : class, ISystemConfigurationContract
{
    /// <inheritdoc/>
    public abstract int SourceVersion { get; }

    /// <inheritdoc/>
    public abstract int TargetVersion { get; }

    /// <inheritdoc/>
    public ISystemConfigurationContract Migrate(ISystemConfigurationContract sourceConfiguration)
    {
        if (sourceConfiguration is not TSourceConfiguration typedSourceConfiguration)
        {
            throw new InvalidOperationException(
                $"Migration '{GetType().Name}' expected source type '{typeof(TSourceConfiguration).Name}' " +
                $"but received '{sourceConfiguration.GetType().Name}'.");
        }

        if (typedSourceConfiguration.Version != SourceVersion)
        {
            throw new InvalidOperationException(
                $"Migration '{GetType().Name}' expected source version '{SourceVersion}' " +
                $"but received version '{typedSourceConfiguration.Version}'.");
        }

        var typedTargetConfiguration = Migrate(typedSourceConfiguration);

        if (typedTargetConfiguration.Version != TargetVersion)
        {
            throw new InvalidOperationException(
                $"Migration '{GetType().Name}' expected target version '{TargetVersion}' " +
                $"but produced version '{typedTargetConfiguration.Version}'.");
        }

        return typedTargetConfiguration;
    }

    /// <summary>
    /// Migrates the typed source configuration to the typed target configuration.
    /// </summary>
    /// <param name="sourceConfiguration">The source configuration.</param>
    /// <returns>The migrated target configuration.</returns>
    /// <remarks>
    /// Implementations should set the target configuration's <c>Version</c> property to
    /// <see cref="ISystemConfigurationMigration.TargetVersion"/>.
    /// </remarks>
    protected abstract TTargetConfiguration Migrate(TSourceConfiguration sourceConfiguration);
}
