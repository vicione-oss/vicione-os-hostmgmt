namespace HostManagement.Configuration;

/// <summary>
/// Provides shared information for managing Linux services.
/// </summary>
public static class Services
{
    /// <summary>
    /// List of known Linux service unit types.
    /// </summary>
    public static readonly HashSet<string> KnownUnitTypes =
    [
        ".service",
        ".socket",
        ".timer",
        ".mount",
        ".automount",
        ".target",
        ".path",
        ".slice",
        ".scope"
    ];
}
