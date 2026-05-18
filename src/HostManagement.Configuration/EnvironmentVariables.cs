using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HostManagement.Configuration;

/// <summary>
/// Provides configuration values from environment variables.
/// </summary>
public static class EnvironmentVariables
{
    private static readonly IConfigurationRoot s_configuration =
        new ConfigurationBuilder().AddEnvironmentVariables().Build();

    private static readonly List<string> s_invalidVariables = [];

    /// <summary>
    /// The list of Linux services, that can be configured by HostManagement.
    /// </summary>
    /// <remarks>
    /// <b>Default:</b> []
    /// </remarks>
    public static readonly EnvironmentVariable<IReadOnlyCollection<string>> ConfigurableServices =
        NormalizeConfigurableServices(
            Create<IReadOnlyCollection<string>>("HOSTMANAGEMENT:CONFIGURABLE_SERVICES", []));

    /// <summary>
    /// The directory/path where <see cref="AppPaths.DefaultSystemConfigurationFile"/> is located.
    /// </summary>
    /// <remarks>
    /// <b>Default:</b> /etc/hostmanagement
    /// </remarks>
    public static readonly EnvironmentVariable<string> DefaultSystemConfigurationDirectory =
        Create("HOSTMANAGEMENT:DEFAULT_SYSTEM_CONFIGURATION_DIRECTORY", "/etc/hostmanagement");

    /// <summary>
    /// The configured application <see cref="Microsoft.Extensions.Logging.LogLevel"/>.
    /// </summary>
    /// <remarks>
    /// <b>Default:</b> <see cref="LogLevel.Warning"/>
    /// </remarks>
    public static readonly EnvironmentVariable<LogLevel> LogLevel =
        Create("LOGGING:LOGLEVEL:DEFAULT", Microsoft.Extensions.Logging.LogLevel.Warning);

    /// <summary>
    /// The group of the named pipe.
    /// </summary>
    /// <remarks>
    /// <b>Default:</b> vicione-suite <i>(root, if group not exists)</i>
    /// </remarks>
    public static readonly EnvironmentVariable<string> NamedPipeGroup =
        Create("HOSTMANAGEMENT:PIPE:GROUP", "vicione-suite");

    /// <summary>
    /// The permission of the named pipe (numeric notation).
    /// </summary>
    /// <remarks>
    /// <b>Default:</b> 664 (User: rw- | Group: rw- | Others: r--)
    /// </remarks>
    public static readonly EnvironmentVariable<string> NamedPipePermission =
        Create("HOSTMANAGEMENT:PIPE:CHMOD", "664");

    /// <summary>
    /// The user/owner of the named pipe.
    /// </summary>
    /// <remarks>
    /// <b>Default:</b> vicione-suite <i>(root, if user not exists)</i>
    /// </remarks>
    public static readonly EnvironmentVariable<string> NamedPipeUser =
        Create("HOSTMANAGEMENT:PIPE:USER", "vicione-suite");

    /// <summary>
    /// The list of known public keys, used to verify installation files etc.
    /// </summary>
    /// <remarks>
    /// <b>Default:</b> [] <i>(must be configured via Debian packaging)</i>
    /// </remarks>
    public static readonly EnvironmentVariable<IReadOnlyCollection<string>> PublicKeys =
        Create<IReadOnlyCollection<string>>("HOSTMANAGEMENT:PUBLIC_KEYS", [], isIndexed: true);

    /// <summary>
    /// The directory/path where all resources will be deployed to.
    /// </summary>
    /// <remarks>
    /// <b>Default:</b> /var/lib/hostmanagement
    /// </remarks>
    public static readonly EnvironmentVariable<string> ResourceDeployDirectory =
        Create("HOSTMANAGEMENT:RESOURCE_DEPLOY_DIRECTORY", "/var/lib/hostmanagement");

    /// <summary>
    /// The cache lifetime of the 'SystemConfiguration' in seconds.
    /// </summary>
    /// <remarks>
    /// <b>Default:</b> 30 seconds
    /// </remarks>
    public static readonly EnvironmentVariable<long> SystemConfigurationCacheLifetimeInSeconds =
        Create("HOSTMANAGEMENT:SYSTEM_CONFIGURATION_CACHE_LIFETIME_SECONDS", 30L);

    /// <summary>
    /// The directory/path where <see cref="AppPaths.SystemConfigurationFile"/> is located.
    /// </summary>
    /// <remarks>
    /// <b>Default:</b> /mnt/persistent/hostmanagement
    /// </remarks>
    public static readonly EnvironmentVariable<string> SystemConfigurationDirectory =
        Create("HOSTMANAGEMENT:SYSTEM_CONFIGURATION_DIRECTORY", "/mnt/persistent/hostmanagement");

    /// <summary>
    /// The target environment/platform, e.g. 'edges-vicione' (EdgeS) or 'debian' (generic Debian).
    /// </summary>
    /// <remarks>
    /// <b>Default:</b> <see cref="TargetEnvironment.Unknown"/>
    /// </remarks>
    public static readonly EnvironmentVariable<TargetEnvironment> Environment =
        CreateRequired("HOSTMANAGEMENT:TARGET_ENVIRONMENT", TargetEnvironment.Unknown);

    /// <summary>
    /// Determines whether there are invalid environment variables.
    /// </summary>
    public static bool HasInvalidVariables => s_invalidVariables.Count > 0;

    /// <summary>
    /// A collection of all invalid environment variables.
    /// </summary>
    public static IReadOnlyCollection<string> InvalidVariables => s_invalidVariables.AsReadOnly();

    private static EnvironmentVariable<T> Create<T>(string key, T defaultValue, bool isIndexed = false)
        where T : notnull
        => SetAndValidate(
            new EnvironmentVariable<T>(s_configuration, key, defaultValue, isIndexed: isIndexed));

    private static EnvironmentVariable<T> CreateRequired<T>(string key, T defaultValue, bool isIndexed = false)
        where T : notnull
        => SetAndValidate(
            new EnvironmentVariable<T>(s_configuration, key, defaultValue, isRequired: true, isIndexed: isIndexed));

    private static EnvironmentVariable<T> SetAndValidate<T>(EnvironmentVariable<T> variable) where T : notnull
    {
        if (variable.IsInvalid)
            s_invalidVariables.Add(variable.InvalidVariableString);

        return variable;
    }

    private static EnvironmentVariable<IReadOnlyCollection<string>> NormalizeConfigurableServices(
        EnvironmentVariable<IReadOnlyCollection<string>> configurableServices)
    {
        // Normalizes values only. Does not perform validation.
        return configurableServices with
        {
            Value = NormalizeConfigurableServiceNames(configurableServices.Value)
        };
    }

    private static IReadOnlyCollection<string> NormalizeConfigurableServiceNames(IReadOnlyCollection<string> names)
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
