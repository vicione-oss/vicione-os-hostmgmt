namespace HostManagement.Configuration;

/// <summary>
/// Provides normalization for configurable service names used in environment variables.
/// </summary>
internal static class ConfigurableServiceNamesNormalizer
{
    /// <summary>
    /// Systemd units that do not have a known unit type suffix get ".service" appended.
    /// </summary>
    /// <param name="names">The configurable service names to normalize.</param>
    /// <returns>The normalized configurable service names.</returns>
    internal static IReadOnlyCollection<string> Normalize(IReadOnlyCollection<string> names)
    {
        var uniqueServiceNames = new HashSet<string>(StringComparer.Ordinal);

        foreach (var name in names)
        {
            var finalName = SystemdUnits.HasKnownUnitTypeSuffix(name)
                ? name
                : name + ".service";

            uniqueServiceNames.Add(finalName);
        }

        return [.. uniqueServiceNames];
    }
}
