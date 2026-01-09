using System.CommandLine;

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
        Add(new HttpProxyCommand());
        Add(new HttpsProxyCommand());
        Add(new FtpProxyCommand());
        Add(new SftpProxyCommand());
        Add(new SocksProxyCommand());
        Add(new NoProxyListCommand());
    }
}
