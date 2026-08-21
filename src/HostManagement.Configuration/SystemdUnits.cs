namespace HostManagement.Configuration;

/// <summary>
/// Provides shared information for managing Linux systemd units.
/// </summary>
public static class SystemdUnits
{
    private static readonly HashSet<string> s_knownUnitTypes =
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

    /// <summary>
    /// Determines whether the specified string ends with a known unit type suffix (see <see cref="s_knownUnitTypes"/>).
    /// </summary>
    /// <remarks>
    /// This method performs a case-sensitive comparison using ordinal string matching.
    /// </remarks>
    /// <param name="name">The string to evaluate for a known unit type suffix.</param>
    /// <returns>
    /// <see langword="true"/> if the string ends with a recognized unit type suffix; otherwise <see langword="false"/>.
    /// </returns>
    public static bool HasKnownUnitTypeSuffix(string name)
        => s_knownUnitTypes.Any(unit => name.EndsWith(unit, StringComparison.Ordinal));
}
