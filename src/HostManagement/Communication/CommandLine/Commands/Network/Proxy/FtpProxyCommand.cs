namespace HostManagement.Communication.CommandLine.Commands.Network.Proxy;

/// <summary>
/// Represents the FTP proxy command.
/// </summary>
internal sealed class FtpProxyCommand : ProxyTypeCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="FtpProxyCommand"/>.
    /// </summary>
    public FtpProxyCommand() : base(ProxyType.FTP)
    {
    }
}
