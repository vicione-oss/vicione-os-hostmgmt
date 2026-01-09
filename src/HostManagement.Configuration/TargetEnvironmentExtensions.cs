namespace HostManagement.Configuration;

/// <summary>
/// Provides extension methods for the <see cref="TargetEnvironment"/> type.
/// </summary>
public static class TargetEnvironmentExtensions
{
    private const string Debian = "debian";
    private const string EdgeSViciOne = "edges-vicione";
    private const string Unknown = "unknown";

    /// <summary>
    /// Gets the string representation of the <see cref="TargetEnvironment"/>.
    /// </summary>
    /// <param name="value">The <see cref="TargetEnvironment"/> value.</param>
    /// <returns><see cref="string"/></returns>
    public static string ToEnvironmentString(this TargetEnvironment value) => value switch
    {
        TargetEnvironment.Debian => Debian,
        TargetEnvironment.EdgeSViciOne => EdgeSViciOne,
        TargetEnvironment.Unknown => Unknown,
        _ => Unknown
    };

    /// <summary>
    /// Gets the <see cref="TargetEnvironment"/> from its string representation.
    /// </summary>
    /// <param name="value">The string representation of the <see cref="TargetEnvironment"/>.</param>
    /// <returns><see cref="TargetEnvironment"/></returns>
    public static TargetEnvironment FromEnvironmentString(string? value) => value?.ToLowerInvariant() switch
    {
        Debian => TargetEnvironment.Debian,
        EdgeSViciOne => TargetEnvironment.EdgeSViciOne,
        Unknown => TargetEnvironment.Unknown,
        _ => TargetEnvironment.Unknown
    };
}
