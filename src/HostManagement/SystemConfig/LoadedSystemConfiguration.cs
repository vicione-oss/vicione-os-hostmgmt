using HostManagement.SystemConfig.Contracts;

namespace HostManagement.SystemConfig;

/// <summary>
/// Represents a loaded persisted system configuration together with its source contract version.
/// </summary>
internal sealed record LoadedSystemConfiguration
{
    /// <summary>
    /// Gets the configuration migrated to the current internal contract version.
    /// </summary>
    public required SystemConfiguration Configuration { get; init; }

    /// <summary>
    /// Gets the contract version that was read from the persisted JSON before migration.
    /// </summary>
    public required int SourceVersion { get; init; }
}
