namespace HostManagement.Configuration;

/// <summary>
/// Provides compile time application specific files, directories and paths.
/// </summary>
public static partial class AppPaths
{
    /// <summary>
    /// The defective default system configuration file ('DefaultSystemConfiguration.json.defective').
    /// </summary>
    public const string DefectiveDefaultSystemConfigurationFile = "DefaultSystemConfiguration.json.defective";

    /// <summary>
    /// The defective system configuration file ('SystemConfiguration.json.defective').
    /// </summary>
    public const string DefectiveSystemConfigurationFile = "SystemConfiguration.json.defective";

    /// <summary>
    /// The default system configuration file ('DefaultSystemConfiguration.json').
    /// </summary>
    public const string DefaultSystemConfigurationFile = "DefaultSystemConfiguration.json";

    /// <summary>
    /// The system configuration file ('SystemConfiguration.json').
    /// </summary>
    public const string SystemConfigurationFile = "SystemConfiguration.json";

    /// <summary>
    /// The path of the temp directory.
    /// </summary>
    public const string TempPath = "/tmp";
}

/// <summary>
/// Provides runtime application specific paths and directories.
/// </summary>
public static partial class AppPaths
{
    /// <summary>
    /// The path of the 'DefaultSystemConfiguration.json.defective' file.
    /// </summary>
    public static readonly string DefectiveDefaultSystemConfigurationFilePath =
        Path.Combine(EnvironmentVariables.DefaultSystemConfigurationDirectory.Value, DefectiveDefaultSystemConfigurationFile);

    /// <summary>
    /// The path of the 'SystemConfiguration.json.defective' file.
    /// </summary>
    public static readonly string DefectiveSystemConfigurationFilePath =
        Path.Combine(EnvironmentVariables.SystemConfigurationDirectory.Value, DefectiveSystemConfigurationFile);

    /// <summary>
    /// The path of the 'DefaultSystemConfiguration.json' file.
    /// </summary>
    public static readonly string DefaultSystemConfigurationFilePath =
        Path.Combine(EnvironmentVariables.DefaultSystemConfigurationDirectory.Value, DefaultSystemConfigurationFile);

    /// <summary>
    /// The path of the 'SystemConfiguration.json' file.
    /// </summary>
    public static readonly string SystemConfigurationFilePath =
        Path.Combine(EnvironmentVariables.SystemConfigurationDirectory.Value, SystemConfigurationFile);
}
