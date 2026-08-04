using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;

namespace HostManagement.Shared.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="SystemConfiguration"/>.
/// </summary>
public static class SystemConfigurationExtensions
{
    /// <summary>
    /// Gets the network interface with the specified <paramref name="interfaceName"/>.
    /// </summary>
    /// <param name="config">The system configuration.</param>
    /// <param name="interfaceName">The interface name.</param>
    /// <returns><see cref="NetworkInterfaceDetail"/> or <see langword="null"/> if not found</returns>
    public static NetworkInterfaceDetail? GetNetworkInterface(this SystemConfiguration config, string interfaceName)
        => config.NetworkInterfaces.FirstOrDefault(@interface
            => @interface.CommonInformation.Name.Equals(interfaceName));
}
