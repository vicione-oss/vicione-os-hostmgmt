using System.Text.Json.Serialization;
using HostManagement.Shared.Extensions;

namespace HostManagement.Shared.Contracts.Network;

/// <summary>
/// Represents the <see cref="NetworkProxySettings"/>.
/// </summary>
public sealed record NetworkProxySettings
{
    /// <summary>
    /// Initializes new <see cref="NetworkProxySettings"/>.
    /// </summary>
    [JsonConstructor]
    public NetworkProxySettings(List<string>? doNotProxyList = null)
    {
        DoNotProxyList = doNotProxyList ?? [];
    }

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
    /// The enabled state of the DoNotProxyList.
    /// </summary>
    public bool DoNotProxyListEnabled { get; set; }

    /// <summary>
    /// The list of IPv4/IPv6 addresses or domain names that should be ignored by the proxy.
    /// </summary>
    /// <remarks>
    /// If a <b>port</b> needs to be specified the following format is valid:
    /// <para>
    /// IPv4:PORT<br/>
    /// [IPv6]:PORT
    /// </para>
    /// <para>
    /// If a <b>link-local IPv6</b> address has to be used a <b>zone identifier</b> for the interface
    /// needs to be added, e.g. eth0. The following format is valid:
    /// <para>
    /// IPv6%ID<br/>
    /// [IPv6%ID]:PORT
    /// </para>
    /// </para>
    /// </remarks>
    public List<string> DoNotProxyList { get; init; }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hashCode = new HashCode();

        hashCode.Add(HTTP);
        hashCode.Add(HTTPS);
        hashCode.Add(FTP);
        hashCode.Add(SFTP);
        hashCode.Add(SOCKS);
        hashCode.Add(DoNotProxyListEnabled);
        hashCode.Add(DoNotProxyList.GetSequenceHashCode());

        return hashCode.ToHashCode();
    }

    /// <inheritdoc/>
    public bool Equals(NetworkProxySettings? other) =>
        other is not null &&
        HTTP == other.HTTP &&
        HTTPS == other.HTTPS &&
        FTP == other.FTP &&
        SFTP == other.SFTP &&
        SOCKS == other.SOCKS &&
        DoNotProxyListEnabled == other.DoNotProxyListEnabled &&
        DoNotProxyList.SequenceEqual(other.DoNotProxyList);
}
