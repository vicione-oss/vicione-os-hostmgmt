namespace HostManagement.Configuration;

/// <summary>
/// Provides application specific paths.
/// </summary>
public static class ApplicationPaths
{
    /// <summary>
    /// The path of the SystemConfiguration.json file.
    /// </summary>
    public static readonly string SystemConfigurationFilePath;

#pragma warning disable CA1810
    static ApplicationPaths()
        => SystemConfigurationFilePath = GetSystemConfigurationFilePath();
#pragma warning restore CA1810

    private static string GetSystemConfigurationFilePath()
        => Path.Combine(EnvironmentVariables.SystemConfigurationDirectory, "SystemConfiguration.json");
}
