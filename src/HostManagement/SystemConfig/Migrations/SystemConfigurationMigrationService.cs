using HostManagement.SystemConfig.Contracts;

namespace HostManagement.SystemConfig.Migrations;

/// <summary>
/// Provides functionality to migrate system configuration contracts to the current contract version.
/// </summary>
/// <param name="migrations">The available system configuration migrations.</param>
internal sealed class SystemConfigurationMigrationService(IEnumerable<ISystemConfigurationMigration> migrations)
    : ISystemConfigurationMigrationService
{
    private readonly SystemConfigurationMigrationRegistry _migrations = new(migrations);

    /// <inheritdoc/>
    public SystemConfiguration Migrate(ISystemConfigurationContract sourceConfiguration)
    {
        var sourceVersion = sourceConfiguration.Version;

        if (sourceVersion == SystemConfiguration.CurrentVersion)
        {
            return sourceConfiguration as SystemConfiguration
                ?? throw new InvalidOperationException(
                    $"System configuration contract version '{sourceVersion}' is not represented by " +
                    $"'{nameof(SystemConfiguration)}'.");
        }

        return MigrateToCurrentVersion(sourceConfiguration, sourceVersion);
    }

    /// <summary>
    /// Migrates the given system configuration contract to the current contract version.
    /// </summary>
    /// <param name="sourceConfiguration">The source configuration contract.</param>
    /// <param name="sourceVersion">The already resolved source contract version.</param>
    /// <returns>The migrated current <see cref="SystemConfiguration"/> contract.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a cyclic migration is detected, a migration produces an unexpected version
    /// or no current configuration result is produced.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// Thrown when no migration is registered for a required source version.
    /// </exception>
    private SystemConfiguration MigrateToCurrentVersion(
        ISystemConfigurationContract sourceConfiguration,
        int sourceVersion)
    {
        var workingConfiguration = sourceConfiguration;
        var workingVersion = sourceVersion;
        HashSet<int> visitedVersions = [];

        while (workingVersion != SystemConfiguration.CurrentVersion)
        {
            if (!visitedVersions.Add(workingVersion))
            {
                throw new InvalidOperationException(
                    $"A cyclic system configuration migration was detected for version '{workingVersion}'.");
            }

            if (!_migrations.TryGetMigration(workingVersion, out var migration))
            {
                throw new NotSupportedException(
                    $"System configuration contract version '{workingVersion}' is not supported.");
            }

            workingConfiguration = migration.Migrate(workingConfiguration);
            workingVersion = workingConfiguration.Version;

            if (workingVersion != migration.TargetVersion)
            {
                throw new InvalidOperationException(
                    $"Migration from version '{migration.SourceVersion}' to version '{migration.TargetVersion}' " +
                    $"produced version '{workingVersion}'.");
            }
        }

        return workingConfiguration as SystemConfiguration
            ?? throw new InvalidOperationException(
                "System configuration migration did not produce a current configuration.");
    }
}
