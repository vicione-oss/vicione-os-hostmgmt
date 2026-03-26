namespace HostManagement.Configuration;

/// <summary>
/// Provides environment variable names of the application.
/// </summary>
internal static class EnvironmentVariableNames
{
    /// <summary>
    /// List of Linux services that can be configured by HostManagement.
    /// </summary>
    internal const string ConfigurableServices = "HOSTMANAGEMENT:CONFIGURABLE_SERVICES";

    /// <summary>
    /// Directory in which <c>DefaultSystemConfiguration.json</c> is located.
    /// </summary>
    internal const string DefaultSystemConfigPath = "HOSTMANAGEMENT:DEFAULT_SYSTEM_CONFIGURATION_DIRECTORY";

    /// <summary>
    /// Configured application <c>LogLevel</c>.
    /// </summary>
    internal const string LogLevel = "LOGGING:LOGLEVEL:DEFAULT";

    /// <summary>
    /// Group of the named pipe.
    /// </summary>
    internal const string PipeGroup = "HOSTMANAGEMENT:PIPE:GROUP";

    /// <summary>
    /// Permissions of the named pipe (numeric notation).
    /// </summary>
    internal const string PipePermission = "HOSTMANAGEMENT:PIPE:CHMOD";

    /// <summary>
    /// User/owner of the named pipe.
    /// </summary>
    internal const string PipeUser = "HOSTMANAGEMENT:PIPE:USER";

    /// <summary>
    /// List of known public keys used to verify installation files etc.
    /// </summary>
    internal const string PublicKeys = "HOSTMANAGEMENT:PUBLIC_KEYS";

    /// <summary>
    /// Directory to which all resources (scripts) are deployed when the service starts.
    /// </summary>
    internal const string ResourcesPath = "HOSTMANAGEMENT:RESOURCE_DEPLOY_DIRECTORY";

    /// <summary>
    /// Directory in which <c>SystemConfiguration.json</c> is located.
    /// </summary>
    internal const string SystemConfigPath = "HOSTMANAGEMENT:SYSTEM_CONFIGURATION_DIRECTORY";

    /// <summary>
    /// Target environment/platform, e.g. <c>edges-vicione</c> for EdgeS or <c>debian</c> for generic Debian.
    /// </summary>
    internal const string TargetEnvironment = "HOSTMANAGEMENT:TARGET_ENVIRONMENT";
}
