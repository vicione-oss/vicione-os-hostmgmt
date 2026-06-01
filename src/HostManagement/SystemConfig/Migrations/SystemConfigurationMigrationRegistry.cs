using System.Diagnostics.CodeAnalysis;
using HostManagement.SystemConfig.Contracts;

namespace HostManagement.SystemConfig.Migrations;

/// <summary>
/// Provides lookup functionality for system configuration migrations by source contract version.
/// </summary>
/// <param name="migrations">The available system configuration migrations.</param>
internal sealed class SystemConfigurationMigrationRegistry(IEnumerable<ISystemConfigurationMigration> migrations)
{
    private readonly IReadOnlyDictionary<int, ISystemConfigurationMigration> _migrations =
        CreateMigrationDictionary(migrations);

    /// <summary>
    /// Tries to get the migration for the specified source contract version.
    /// </summary>
    /// <param name="sourceVersion">The source contract version.</param>
    /// <param name="migration">The matching migration, if one is registered.</param>
    /// <returns><see langword="true"/> when a matching migration was found; otherwise <see langword="false"/>.</returns>
    internal bool TryGetMigration(
        int sourceVersion,
        [NotNullWhen(true)] out ISystemConfigurationMigration? migration)
        => _migrations.TryGetValue(sourceVersion, out migration);

    private static IReadOnlyDictionary<int, ISystemConfigurationMigration> CreateMigrationDictionary(
        IEnumerable<ISystemConfigurationMigration> migrations)
    {
        var migrationList = migrations.ToList();

        ValidateMigrationVersions(migrationList);
        ValidateUniqueSourceVersions(migrationList);

        var migrationDictionary = migrationList.ToDictionary(static migration => migration.SourceVersion);

        ValidateMigrationChains(migrationDictionary);

        return migrationDictionary;
    }

    private static void ValidateMigrationVersions(IEnumerable<ISystemConfigurationMigration> migrations)
    {
        foreach (var migration in migrations)
        {
            if (migration.SourceVersion < 1)
            {
                throw new InvalidOperationException(
                    $"System configuration migration '{migration.GetType().Name}' has invalid source version " +
                    $"'{migration.SourceVersion}'.");
            }

            if (migration.TargetVersion <= migration.SourceVersion)
            {
                throw new InvalidOperationException(
                    $"System configuration migration '{migration.GetType().Name}' must migrate forward, " +
                    $"but migrates from version '{migration.SourceVersion}' to version '{migration.TargetVersion}'.");
            }

            if (migration.TargetVersion > SystemConfiguration.CurrentVersion)
            {
                throw new InvalidOperationException(
                    $"System configuration migration '{migration.GetType().Name}' targets version " +
                    $"'{migration.TargetVersion}', which is newer than the current supported version " +
                    $"'{SystemConfiguration.CurrentVersion}'.");
            }
        }
    }

    private static void ValidateUniqueSourceVersions(IEnumerable<ISystemConfigurationMigration> migrations)
    {
        var duplicateSourceVersions = migrations
            .GroupBy(static migration => migration.SourceVersion)
            .Where(static group => group.Count() > 1)
            .Select(static group => group.Key)
            .ToList();

        if (duplicateSourceVersions.Count > 0)
        {
            throw new InvalidOperationException(
                $"Multiple system configuration migrations are registered for source version " +
                $"'{string.Join(", ", duplicateSourceVersions)}'.");
        }
    }

    private static void ValidateMigrationChains(IReadOnlyDictionary<int, ISystemConfigurationMigration> migrations)
    {
        foreach (var migration in migrations.Values)
        {
            if (migration.TargetVersion == SystemConfiguration.CurrentVersion)
                continue;

            if (!migrations.ContainsKey(migration.TargetVersion))
            {
                throw new InvalidOperationException(
                    $"System configuration migration chain is incomplete. Migration " +
                    $"'{migration.GetType().Name}' targets version '{migration.TargetVersion}', " +
                    $"but no migration from version '{migration.TargetVersion}' is registered.");
            }
        }
    }
}
