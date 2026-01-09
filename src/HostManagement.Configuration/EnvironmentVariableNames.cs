namespace HostManagement.Configuration;

/// <summary>
/// Provides environment variable names of the application.
/// </summary>
internal static class EnvironmentVariableNames
{
    internal const string DefaultSystemConfigPath = "HOSTMANAGEMENT:DEFAULT_SYSTEM_CONFIGURATION_DIRECTORY";
    internal const string SystemConfigPath = "HOSTMANAGEMENT:SYSTEM_CONFIGURATION_DIRECTORY";
    internal const string ConfigurableServices = "HOSTMANAGEMENT:CONFIGURABLE_SERVICES";
    internal const string LogLevel = "LOGGING:LOGLEVEL:DEFAULT";
    internal const string PipeGroup = "HOSTMANAGEMENT:PIPE:GROUP";
    internal const string PipePermission = "HOSTMANAGEMENT:PIPE:CHMOD";
    internal const string PipeUser = "HOSTMANAGEMENT:PIPE:USER";
    internal const string ResourcesPath = "HOSTMANAGEMENT:RESOURCE_DEPLOY_DIRECTORY";
    internal const string SuiteDownloadPath = "HOSTMANAGEMENT:SUITE_DOWNLOAD_DIRECTORY";
    internal const string SuiteRepoEndpoint = "HOSTMANAGEMENT:SUITE_REPOSITORY:ENDPOINT";
    internal const string SuiteRepoUsername = "HOSTMANAGEMENT:SUITE_REPOSITORY:USERNAME";
    internal const string SuiteRepoPassword = "HOSTMANAGEMENT:SUITE_REPOSITORY:PASSWORD";
}
