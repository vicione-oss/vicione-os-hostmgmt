namespace HostManagement.SystemConfig.Migrations;

/// <summary>
/// Represents a single migration step between two system configuration contract versions.
/// </summary>
/// <remarks>
/// Implementations are used to build a migration chain from an older persisted contract version
/// to the current contract version.
/// </remarks>
internal interface ISystemConfigurationMigration
{
    /// <summary>
    /// Gets the source contract version supported by this migration.
    /// </summary>
    /// <remarks>
    /// This value is used to select the migration for a persisted configuration with the same version.
    /// </remarks>
    int SourceVersion { get; }

    /// <summary>
    /// Gets the target contract version produced by this migration.
    /// </summary>
    int TargetVersion { get; }

    /// <summary>
    /// Migrates the given system configuration contract to the target contract version.
    /// </summary>
    /// <param name="sourceConfiguration">The source configuration contract.</param>
    /// <returns>
    /// The migrated system configuration contract. Its <see cref="ISystemConfigurationContract.Version"/>
    /// must match <see cref="TargetVersion"/>.
    /// </returns>
    /// <remarks>
    /// Concrete implementations should usually derive from
    /// <see cref="SystemConfigurationMigration{TSourceConfiguration, TTargetConfiguration}"/> to perform
    /// the actual migration with fully typed contract objects.
    /// </remarks>
    ISystemConfigurationContract Migrate(ISystemConfigurationContract sourceConfiguration);
}
