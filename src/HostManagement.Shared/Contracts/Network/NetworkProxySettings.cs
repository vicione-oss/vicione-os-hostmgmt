namespace HostManagement.Shared.Contracts.Network;

/// <summary>
/// Represents the <see cref="NetworkProxySettings"/>.
/// </summary>
public sealed record NetworkProxySettings
{
    /// <summary>
    /// The FTP proxy settings.
    /// </summary>
    public NetworkProxyDetail FTP { get; set; } = new();

    /// <summary>
    /// The HTTP proxy settings.
    /// </summary>
    public NetworkProxyDetail HTTP { get; set; } = new();

    /// <summary>
    /// The HTTPS proxy settings.
    /// </summary>
    public NetworkProxyDetail HTTPS { get; set; } = new();

    /// <summary>
    /// The SFTP proxy settings.
    /// </summary>
    public NetworkProxyDetail SFTP { get; set; } = new();

    /// <summary>
    /// The SOCKS proxy settings.
    /// </summary>
    public NetworkProxyDetail SOCKS { get; set; } = new();

    /// <summary>
    /// The no proxy settings.
    /// </summary>
    public NoProxySettings NoProxy { get; set; } = new();
}
