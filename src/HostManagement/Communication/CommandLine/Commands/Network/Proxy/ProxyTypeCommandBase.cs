using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;

namespace HostManagement.Communication.CommandLine.Commands.Network.Proxy;

/// <summary>
/// Represents the abstract base class for a proxy type command.
/// </summary>
internal abstract class ProxyTypeCommandBase(string name, string description) : ConfigCommandBase(name, description)
{
    private protected static NetworkProxyDetail GetNetworkProxyDetail(ProxyType proxyType, SystemConfiguration config)
    {
        return proxyType switch
        {
            ProxyType.HTTP => config.NetworkProxySettings.HTTP,
            ProxyType.HTTPS => config.NetworkProxySettings.HTTPS,
            ProxyType.FTP => config.NetworkProxySettings.FTP,
            ProxyType.SFTP => config.NetworkProxySettings.SFTP,
            _ => throw new InvalidOperationException($"{proxyType} is not a valid proxy type.")
        };
    }
}
