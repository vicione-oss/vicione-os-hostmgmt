namespace HostManagement.Configuration;

/// <summary>
/// Represents a target environment or platform.
/// </summary>
public enum TargetEnvironment
{
    /// <summary>
    /// Unknown
    /// </summary>
    /// <remarks>
    /// This environment is unknown and not supported.
    /// </remarks>
    Unknown,

    /// <summary>
    /// Debian
    /// </summary>
    /// <remarks>
    /// This environment is a standard Debian 12.
    /// </remarks>
    Debian,

    /// <summary>
    /// EdgeS ViciOne
    /// </summary>
    /// <remarks>
    /// This environment is based on Debian 12 with <c>netplan</c>, <c>wget</c> and <c>systemd</c>.
    /// </remarks>
    EdgeSViciOne
}
