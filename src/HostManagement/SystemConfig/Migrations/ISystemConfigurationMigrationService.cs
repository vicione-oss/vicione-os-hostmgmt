using HostManagement.SystemConfig.Contracts;

namespace HostManagement.SystemConfig.Migrations;

/// <summary>
/// Represents a service for migrating persisted system configuration contracts to the current contract version.
/// </summary>
/// <remarks>
/// The service orchestrates migrations on typed contract level so that JSON parsing is completed before
/// migration starts.
/// </remarks>
internal interface ISystemConfigurationMigrationService
{
    /// <summary>
    /// Migrates the given system configuration contract to the current contract version.
    /// </summary>
    /// <param name="sourceConfiguration">The source configuration contract.</param>
    /// <returns>The current <see cref="SystemConfiguration"/> contract.</returns>
    /// <remarks>
    /// If the source configuration already uses the current contract version, it is returned directly.
    /// Otherwise, registered migrations are applied until the current contract version is reached.
    /// Unknown versions and invalid migration chains are rejected.
    /// </remarks>
    SystemConfiguration Migrate(ISystemConfigurationContract sourceConfiguration);
}
