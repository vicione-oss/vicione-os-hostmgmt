namespace HostManagement.Configuration;

/// <summary>
/// Provides shared information for managing Linux systemd units.
/// </summary>
public static class SystemdUnits
{
    /// <summary>
    /// List of known Linux systemd unit types.
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
