using System.CommandLine;
using HostManagement.Communication.CommandLine.Commands.Network.Proxy.NoProxy;

namespace HostManagement.Communication.CommandLine.Commands.Network.Proxy;

/// <summary>
/// Represents the proxy command.
/// </summary>
internal sealed class ProxyCommand : Command
{
    /// <summary>
    /// Initializes a new <see cref="ProxyCommand"/>.
    /// </summary>
    public ProxyCommand() : base(
        CommandLineStrings_Proxy.ProxyCommandName,
        CommandLineStrings_Proxy.ProxyCommandDescription)
    {
        Add(new ProxyTypeCommand(ProxyType.HTTP));
        Add(new ProxyTypeCommand(ProxyType.HTTPS));
        Add(new ProxyTypeCommand(ProxyType.FTP));
        Add(new ProxyTypeCommand(ProxyType.SFTP));
        Add(new ProxyTypeCommand(ProxyType.SOCKS));
        Add(new NoProxyCommand());
    }
}
