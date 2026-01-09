namespace HostManagement.Communication.CommandLine.Commands.Network.Proxy;

/// <summary>
/// Represents the HTTP proxy command.
/// </summary>
internal sealed class HttpProxyCommand : ProxyTypeCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="HttpProxyCommand"/>.
    /// </summary>
    public HttpProxyCommand() : base(ProxyType.HTTP)
    {
    }
}
