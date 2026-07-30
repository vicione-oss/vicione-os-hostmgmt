using System.Text.Json.Serialization;

namespace HostManagement.Shared.Contracts.Network;

/// <summary>
/// Represents the <see cref="NetworkDNSSettings"/>.
/// </summary>
public sealed record NetworkDNSSettings
{
    /// <summary>
    /// Represents the empty DNS settings.
    /// </summary>
    [JsonIgnore]
    public static NetworkDNSSettings Empty { get; } = new NetworkDNSSettings();

    /// <summary>
    /// The hostname.
    /// </summary>
    public string Hostname { get; set; } = string.Empty;

    /// <summary>
    /// Determines whether multicast DNS (mDNS) is enabled.
    /// </summary>
    public bool MulticastDNSEnabled { get; set; }

    /// <summary>
    /// The <see cref="StaticHostsSettings"/>.
    /// </summary>
    public StaticHostsSettings StaticHosts { get; set; } = new();

    /// <summary>
    /// The <see cref="NameServersSettings"/>.
    /// </summary>
    public NameServersSettings NameServers { get; set; } = new();

    /// <summary>
    /// The <see cref="PrimaryDNSSuffixSettings"/>.
    /// </summary>
    public PrimaryDNSSuffixSettings PrimaryDNSSuffix { get; set; } = new();

    /// <summary>
    /// The <see cref="SearchDomainsSettings"/>.
    /// </summary>
    public SearchDomainsSettings SearchDomains { get; set; } = new();
}
