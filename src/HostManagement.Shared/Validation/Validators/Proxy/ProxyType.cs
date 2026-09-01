namespace HostManagement.Shared.Validation.Validators.Proxy;

/// <summary>
/// Represents the type of a proxy.
/// </summary>
public enum ProxyType
{
    /// <summary>
    /// Unspecified proxy.
    /// </summary>
    /// <remarks>Used when the type is not specified or recognized.</remarks>
    Unspecified,

    /// <summary>
    /// HTTP proxy.
    /// </summary>
    /// <remarks>Typically used for web traffic.</remarks>
    HTTP,

    /// <summary>
    /// HTTPS proxy.
    /// </summary>
    /// <remarks>Typically used for secure web traffic.</remarks>
    HTTPS,

    /// <summary>
    /// FTP proxy.
    /// </summary>
    /// <remarks>Typically used for file transfers.</remarks>
    FTP,

    /// <summary>
    /// SFTP proxy.
    /// </summary>
    /// <remarks>Typically used for secure file transfers over SSH.</remarks>
    SFTP
}
