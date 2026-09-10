using System.CommandLine;
using HostManagement.Communication.CommandLine.Commands.Network.Proxy.NoProxy;
using HostManagement.Communication.CommandLine.Resources;

namespace HostManagement.Communication.CommandLine.Commands.Network.Proxy;

/// <summary>
/// Represents the proxy command.
/// </summary>
internal sealed class ProxyCommand : Command
{
    /// <summary>
    /// Initializes a new <see cref="ProxyCommand"/>.
    /// </summary>
    public ProxyCommand() : base(CliStrings.Network.Proxy.CommandName, CliStrings.Network.Proxy.CommandDescription)
    {
        Add(new ProxyTypeCommand(ProxyType.HTTP));
        Add(new ProxyTypeCommand(ProxyType.HTTPS));
        Add(new ProxyTypeCommand(ProxyType.FTP));
        Add(new ProxyTypeCommand(ProxyType.SFTP));
        Add(new NoProxyCommand());
    }
}
