using System.CommandLine;
using HostManagement.Communication.CommandLine.Extensions;

namespace HostManagement.Communication.CommandLine.Commands.Network.Proxy;

/// <summary>
/// Represents the <see cref="ProxyCommandGroupProvider"/>.
/// </summary>
internal static class ProxyCommandGroupProvider
{
    /// <summary>
    /// Adds the command group to configure proxy settings.
    /// </summary>
    /// <param name="parentCommand">The parent command.</param>
    internal static void AddProxyCommandGroup(this Command parentCommand)
    {
        var proxyCommandGroup = parentCommand.AddCommand(
            "proxy",
            "Configure proxy settings.");

        proxyCommandGroup.AddHttpProxyCommand();
        proxyCommandGroup.AddHttpsProxyCommand();
        proxyCommandGroup.AddFtpProxyCommand();
        proxyCommandGroup.AddSftpProxyCommand();
        proxyCommandGroup.AddSocksProxyCommand();
        proxyCommandGroup.AddNoProxyListCommand();
    }
}
