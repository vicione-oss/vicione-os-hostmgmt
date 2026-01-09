using Microsoft.Extensions.Logging;

namespace HostManagement.Configuration;

/// <summary>
/// Provides configuration values from environment variables.
/// </summary>
public static class EnvironmentVariables
{
    private const string TempDirectory = "/tmp";
    private const string ResourcesSubDirectory = "HostManagement_Resources";

    private static readonly List<string> s_invalidVariables = [];

    /// <summary>
    /// Determines if there are invalid (null, empty, whitespace) environment variables.
    /// </summary>
    public static bool HasInvalidVariables => s_invalidVariables.Count > 0;

    /// <summary>
    /// A collection of all invalid (null, empty, whitespace) environment variables.
    /// </summary>
    public static IReadOnlyCollection<string> InvalidVariables => s_invalidVariables.AsReadOnly();

    /// <summary>
    /// The current <see cref="LogLevel"/>.
    /// </summary>
    public static readonly LogLevel LogLevel =
        GetEnumValue("LOGGING:LOGLEVEL:DEFAULT", LogLevel.Warning);

    /// <summary>
    /// The directory where the SystemConfiguration.json is located.
    /// </summary>
    public static readonly string SystemConfigurationDirectory =
        GetConfigValue("HOSTMANAGEMENT:SYSTEM_CONFIGURATION_DIRECTORY", TempDirectory);

    /// <summary>
    /// The directory where all resources are deployed.
    /// </summary>
    public static readonly string ResourceDeployDirectory =
        GetConfigValue(
            "HOSTMANAGEMENT:RESOURCE_DEPLOY_DIRECTORY",
            Path.Combine(SystemConfigurationDirectory, ResourcesSubDirectory));

    /// <summary>
    /// The ViciOne Suite Debian package download directory.
    /// </summary>
    public static readonly string SuiteDownloadDirectory =
        GetConfigValue("HOSTMANAGEMENT:SUITE_DOWNLOAD_DIRECTORY", TempDirectory);

    /// <summary>
    /// The ViciOne Suite repository endpoint (URL).
    /// </summary>
    public static readonly string SuiteRepositoryEndpoint =
        GetConfigValue("HOSTMANAGEMENT:SUITE_REPOSITORY:ENDPOINT", string.Empty);

    /// <summary>
    /// The ViciOne Suite repository username.
    /// </summary>
    public static readonly string SuiteRepositoryUsername =
        GetConfigValue("HOSTMANAGEMENT:SUITE_REPOSITORY:USERNAME", string.Empty);

    /// <summary>
    /// The ViciOne Suite repository password.
    /// </summary>
    public static readonly string SuiteRepositoryPassword =
        GetConfigValue("HOSTMANAGEMENT:SUITE_REPOSITORY:PASSWORD", string.Empty);

    /// <summary>
    /// The list of configurable system wide linux services.
    /// </summary>
    public static readonly List<string> ConfigurableServices =
        GetConfigListValue("HOSTMANAGEMENT:CONFIGURABLE_SERVICES", string.Empty, true);

    private static TEnum GetEnumValue<TEnum>(string key, TEnum defaultEnumValue)
        where TEnum : struct
    {
        return Enum.TryParse(AppConfiguration.Configuration[key], true, out TEnum result)
            ? result
            : defaultEnumValue;
    }

    private static List<string> GetConfigListValue(string key, string defaultValue, bool acceptEmptyValue = false)
    {
        return GetConfigValue(key, defaultValue, acceptEmptyValue)
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .ToList();
    }

    private static string GetConfigValue(string key, string defaultValue, bool acceptEmptyValue = false)
    {
        var value = AppConfiguration.Configuration[key] ?? defaultValue;

        if (!acceptEmptyValue && string.IsNullOrWhiteSpace(value))
            s_invalidVariables.Add(key.Replace(":", "__"));

        return value;
    }
}
