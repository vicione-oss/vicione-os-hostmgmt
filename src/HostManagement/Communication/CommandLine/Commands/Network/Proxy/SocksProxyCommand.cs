namespace HostManagement.Communication.CommandLine.Commands.Network.Proxy;

/// <summary>
/// Represents the SOCKS proxy command.
/// </summary>
internal sealed class SocksProxyCommand : ProxyTypeCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="SocksProxyCommand"/>.
    /// </summary>
    public SocksProxyCommand() : base(ProxyType.SOCKS)
    {
    }
}
