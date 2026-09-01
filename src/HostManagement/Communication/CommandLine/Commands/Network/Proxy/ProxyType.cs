namespace HostManagement.Communication.CommandLine.Commands.Network.Proxy;

/// <summary>
/// Represents a proxy type.
/// </summary>
internal enum ProxyType
{
    /// <summary>
    /// The HTTP proxy.
    /// </summary>
    HTTP,

    /// <summary>
    /// The HTTPS proxy.
    /// </summary>
    HTTPS,

    /// <summary>
    /// The FTP proxy.
    /// </summary>
    FTP,

    /// <summary>
    /// The SFTP proxy.
    /// </summary>
    SFTP
}
