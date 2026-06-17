namespace HostManagement.SystemConfig.Migrations;

/// <summary>
/// Represents a system configuration contract that can participate in contract migrations.
/// </summary>
internal interface ISystemConfigurationContract
{
    /// <summary>
    /// Gets the version of the system configuration contract.
    /// </summary>
    int Version { get; }
}
