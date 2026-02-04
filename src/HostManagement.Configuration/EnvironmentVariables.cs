using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HostManagement.Configuration;

/// <summary>
/// Provides configuration values from environment variables.
/// </summary>
public static class EnvironmentVariables
{
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
    /// The <see cref="TargetEnvironment"/>.
    /// </summary>
    /// <remarks>
    /// <b>Default:</b> <see cref="TargetEnvironment.Unknown"/>
    /// </remarks>
    public static readonly TargetEnvironment TargetEnvironment =
        GetTargetEnvironmentEnumValue(EnvironmentVariableNames.TargetEnvironment);

    /// <summary>
    /// The current <see cref="LogLevel"/>.
    /// </summary>
    /// <remarks>
    /// <b>Default:</b> <see cref="LogLevel.Warning"/>
    /// </remarks>
    public static readonly LogLevel LogLevel =
        GetEnumValue(EnvironmentVariableNames.LogLevel, LogLevel.Warning);

    /// <summary>
    /// The user of the named pipe.
    /// </summary>
    /// <remarks>
    /// <b>Default:</b> vicione-suite
    /// </remarks>
    public static readonly string NamedPipeUser =
        GetValue(EnvironmentVariableNames.PipeUser, "vicione-suite");

    /// <summary>
    /// The group of the named pipe.
    /// </summary>
    /// <remarks>
    /// <b>Default:</b> vicione-suite
    /// </remarks>
    public static readonly string NamedPipeGroup =
        GetValue(EnvironmentVariableNames.PipeGroup, "vicione-suite");

    /// <summary>
    /// The permission of the named pipe.
    /// </summary>
    /// <remarks>
    /// <b>Default:</b> User: rw- | Group: rw- | Others: r--
    /// </remarks>
    public static readonly string NamedPipePermission =
        GetValue(EnvironmentVariableNames.PipePermission, "664");

    /// <summary>
    /// The path where the 'DefaultSystemConfiguration.json' is located.
    /// </summary>
    /// <remarks>
    /// <b>Default:</b> /tmp
    /// </remarks>
    public static readonly string DefaultSystemConfigurationDirectory =
        GetValue(EnvironmentVariableNames.DefaultSystemConfigPath, AppPaths.TempPath);

    /// <summary>
    /// The path where the 'SystemConfiguration.json' is located.
    /// </summary>
    /// <remarks>
    /// <b>Default:</b> /tmp
    /// </remarks>
    public static readonly string SystemConfigurationDirectory =
        GetValue(EnvironmentVariableNames.SystemConfigPath, AppPaths.TempPath);

    /// <summary>
    /// The path where all resources are deployed.
    /// </summary>
    /// <remarks>
    /// <b>Default:</b> /tmp/HostManagement_Resources
    /// </remarks>
    public static readonly string ResourceDeployDirectory =
        GetValue(
            EnvironmentVariableNames.ResourcesPath,
            Path.Combine(AppPaths.TempPath, AppPaths.ResourcesDirectory));

    /// <summary>
    /// The ViciOne Suite Debian package download path.
    /// </summary>
    /// <remarks>
    /// <b>Default:</b> /tmp
    /// </remarks>
    public static readonly string SuiteDownloadDirectory =
        GetValue(EnvironmentVariableNames.SuiteDownloadPath, AppPaths.TempPath);

    /// <summary>
    /// The ViciOne Suite repository endpoint (URL).
    /// </summary>
    public static readonly string SuiteRepositoryEndpoint =
        GetValue(EnvironmentVariableNames.SuiteRepoEndpoint, string.Empty, true);

    /// <summary>
    /// The ViciOne Suite repository username.
    /// </summary>
    public static readonly string SuiteRepositoryUsername =
        GetValue(EnvironmentVariableNames.SuiteRepoUsername, string.Empty, true);

    /// <summary>
    /// The ViciOne Suite repository password.
    /// </summary>
    public static readonly string SuiteRepositoryPassword =
        GetValue(EnvironmentVariableNames.SuiteRepoPassword, string.Empty, true);

    /// <summary>
    /// The list of configurable system wide Linux services.
    /// </summary>
    public static readonly IReadOnlyCollection<string> ConfigurableServices =
        GetUniqueConfigurableServicesWithEnsuredSystemdUnitType(
            GetListValue(EnvironmentVariableNames.ConfigurableServices, string.Empty, true));

    /// <summary>
    /// The list of known public keys.
    /// </summary>
    public static readonly List<string> PublicKeys =
        GetArrayValues(EnvironmentVariableNames.PublicKeys, string.Empty);

    private static List<string> GetArrayValues(string key, string defaultValue, bool acceptEmptyValue = false)
    {
        var values = AppConfiguration.Configuration
            .AsEnumerable()
            .Where(kvp => kvp.Key.StartsWith(key + ":"))
            .Select(kvp => kvp.Value ?? defaultValue)
            .ToList();

        foreach (var value in values)
        {
            if (!acceptEmptyValue && string.IsNullOrWhiteSpace(value))
                s_invalidVariables.Add(key.Replace(":", "__"));
        }

        return values;
    }

    private static string GetValue(string key, string defaultValue, bool acceptEmptyValue = false)
    {
        var value = AppConfiguration.Configuration[key] ?? defaultValue;

        if (!acceptEmptyValue && string.IsNullOrWhiteSpace(value))
            s_invalidVariables.Add(key.Replace(":", "__"));

        return value;
    }

    private static TEnum GetEnumValue<TEnum>(string key, TEnum defaultEnumValue) where TEnum : struct
        => Enum.TryParse(AppConfiguration.Configuration[key], true, out TEnum result)
            ? result
            : defaultEnumValue;

    private static TargetEnvironment GetTargetEnvironmentEnumValue(string key)
    {
        var value = TargetEnvironmentExtensions.FromEnvironmentString(AppConfiguration.Configuration[key]);

        if (value is TargetEnvironment.Unknown)
            s_invalidVariables.Add(key.Replace(":", "__"));

        return value;
    }

    private static List<string> GetListValue(string key, string defaultValue, bool acceptEmptyValue = false)
        => GetValue(key, defaultValue, acceptEmptyValue)
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .ToList();

    private static List<string> GetUniqueConfigurableServicesWithEnsuredSystemdUnitType(
        List<string> configurableServices)
    {
        var uniqueServicesList = new HashSet<string>();

        foreach (var name in configurableServices)
        {
            var finalName = SystemdUnits.KnownUnitTypes.Any(name.EndsWith)
                ? name
                : name + ".service";

            uniqueServicesList.Add(finalName);
        }

        return [.. uniqueServicesList];
    }
}
