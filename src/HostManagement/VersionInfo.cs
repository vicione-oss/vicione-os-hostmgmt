using System.Reflection;

namespace HostManagement;

/// <summary>
/// Version information for the application.
/// </summary>
internal static class VersionInfo
{
    private static readonly char[] s_commitHashSeparators = ['+', '-'];

    /// <summary>
    /// Full qualified version of the application including git commit hash.
    /// </summary>
    internal static string FullQualifiedVersion { get; } = ReadFullQualifiedVersion();

    /// <summary>
    /// Three part SemVer version of the application.
    /// </summary>
    internal static Version Version { get; } = ReadVersion();

    private static string ReadFullQualifiedVersion()
    {
        try
        {
            return
                typeof(VersionInfo)
                .Assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion
                ?? throw new InvalidOperationException("Version info was null");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Version information could not be read.", ex);
        }
    }

    private static Version ReadVersion()
    {
        try
        {
            var trimmedVersion = FullQualifiedVersion.Split(s_commitHashSeparators)[0];
            return Version.Parse(trimmedVersion);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Version information could not be read.", ex);
        }
    }
}
