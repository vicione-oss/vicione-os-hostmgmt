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
    /// The target environment/platform, e.g. 'edges-vicione' (EdgeS) or 'debian' (generic Debian).
    /// </summary>
    /// <remarks>
    /// <b>Default:</b> <see cref="TargetEnvironment.Unknown"/>
    /// </remarks>
    public static readonly TargetEnvironment TargetEnvironment =
        GetTargetEnvironmentEnumValue("HOSTMANAGEMENT:TARGET_ENVIRONMENT");

    /// <summary>
    /// The configured application <see cref="Microsoft.Extensions.Logging.LogLevel"/>.
    /// </summary>
    /// <remarks>
    /// <b>Default:</b> <see cref="LogLevel.Warning"/>
    /// </remarks>
    public static readonly LogLevel LogLevel =
        GetEnumValue("LOGGING:LOGLEVEL:DEFAULT", LogLevel.Warning);

    /// <summary>
    /// The user/owner of the named pipe.
    /// </summary>
    /// <remarks>
    /// <b>Default:</b> vicione-suite
    /// </remarks>
    public static readonly string NamedPipeUser =
        GetValue("HOSTMANAGEMENT:PIPE:USER", "vicione-suite");

    /// <summary>
    /// The group of the named pipe.
    /// </summary>
    /// <remarks>
    /// <b>Default:</b> vicione-suite
    /// </remarks>
    public static readonly string NamedPipeGroup =
        GetValue("HOSTMANAGEMENT:PIPE:GROUP", "vicione-suite");

    /// <summary>
    /// The permission of the named pipe (numeric notation).
    /// </summary>
    /// <remarks>
    /// <b>Default:</b> 664 (User: rw- | Group: rw- | Others: r--)
    /// </remarks>
    public static readonly string NamedPipePermission =
        GetValue("HOSTMANAGEMENT:PIPE:CHMOD", "664");

    /// <summary>
    /// The directory/path where <see cref="AppPaths.DefaultSystemConfigurationFile"/> is located.
    /// </summary>
    /// <remarks>
    /// <b>Default:</b> /tmp
    /// </remarks>
    public static readonly string DefaultSystemConfigurationDirectory =
        GetValue("HOSTMANAGEMENT:DEFAULT_SYSTEM_CONFIGURATION_DIRECTORY", AppPaths.TempPath);

    /// <summary>
    /// The directory/path where <see cref="AppPaths.SystemConfigurationFile"/> is located.
    /// </summary>
    /// <remarks>
    /// <b>Default:</b> /tmp
    /// </remarks>
    public static readonly string SystemConfigurationDirectory =
        GetValue("HOSTMANAGEMENT:SYSTEM_CONFIGURATION_DIRECTORY", AppPaths.TempPath);

    /// <summary>
    /// The directory/path where all resources will be deployed to.
    /// </summary>
    /// <remarks>
    /// <b>Default:</b> /tmp/HostManagement_Resources
    /// </remarks>
    public static readonly string ResourceDeployDirectory =
        GetValue(
            "HOSTMANAGEMENT:RESOURCE_DEPLOY_DIRECTORY",
            Path.Combine(AppPaths.TempPath, AppPaths.ResourcesDirectory));

    /// <summary>
    /// The list of Linux services, that can be configured by HostManagement.
    /// </summary>
    public static readonly IReadOnlyCollection<string> ConfigurableServices =
        GetUniqueConfigurableServicesWithEnsuredSystemdUnitType(
            GetListValue("HOSTMANAGEMENT:CONFIGURABLE_SERVICES", string.Empty, true));

    /// <summary>
    /// The list of known public keys, used to verify installation files etc.
    /// </summary>
    public static readonly List<string> PublicKeys =
        GetArrayValues("HOSTMANAGEMENT:PUBLIC_KEYS", string.Empty);

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
