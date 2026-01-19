using HostManagement.SystemConfig.Contracts;

namespace HostManagement.SystemConfig;

internal interface IPersistence
{
    Task<SystemConfiguration> LoadSystemConfiguration(string configurationFilePath, CancellationToken cancellationToken);
    Task SaveSystemConfiguration(SystemConfiguration systemConfiguration, string configurationFilePath, CancellationToken cancellationToken);
}
