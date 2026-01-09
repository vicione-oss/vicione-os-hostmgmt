namespace HostManagement.Communication.CommandLine.Commands.Network.Proxy;

/// <summary>
/// Represents the SFTP proxy command.
/// </summary>
internal sealed class SftpProxyCommand : ProxyTypeCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="SftpProxyCommand"/>.
    /// </summary>
    public SftpProxyCommand() : base(ProxyType.SFTP)
    {
    }
}
