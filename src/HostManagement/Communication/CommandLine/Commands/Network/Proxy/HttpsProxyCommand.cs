namespace HostManagement.Communication.CommandLine.Commands.Network.Proxy;

/// <summary>
/// Represents the HTTPS proxy command.
/// </summary>
internal sealed class HttpsProxyCommand : ProxyTypeCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="HttpsProxyCommand"/>.
    /// </summary>
    public HttpsProxyCommand() : base(ProxyType.HTTPS)
    {
    }
}
