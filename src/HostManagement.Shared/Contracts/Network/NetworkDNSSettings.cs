using System.Text.Json.Serialization;
using HostManagement.Shared.Extensions;

namespace HostManagement.Shared.Contracts.Network;

/// <summary>
/// Represents the <see cref="NetworkDNSSettings"/>.
/// </summary>
public sealed record NetworkDNSSettings
{
    /// <summary>
    /// Initializes new <see cref="NetworkDNSSettings"/>.
    /// </summary>
    [JsonConstructor]
    public NetworkDNSSettings(
        List<string>? searchDomains = null)
    {
        SearchDomains = searchDomains ?? [];
    }

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
    /// Determines whether the primary DNS suffix is enabled.
    /// </summary>
    public bool DNSSuffixEnabled { get; set; }

    /// <summary>
    /// The primary DNS suffix.
    /// </summary>
    public string DNSSuffix { get; set; } = string.Empty;

    /// <summary>
    /// Determines whether search domains are enabled.
    /// </summary>
    public bool SearchDomainsEnabled { get; set; }

    /// <summary>
    /// The list of DNS search domains.
    /// </summary>
    /// <remarks>
    /// DNS suffixes excluding the primary DNS suffix (<see cref="DNSSuffix"/>).
    /// </remarks>
    public List<string> SearchDomains { get; init; }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hashCode = new HashCode();

        hashCode.Add(Hostname);
        hashCode.Add(MulticastDNSEnabled);
        hashCode.Add(StaticHosts);
        hashCode.Add(NameServers);
        hashCode.Add(DNSSuffixEnabled);
        hashCode.Add(DNSSuffix);
        hashCode.Add(SearchDomainsEnabled);
        hashCode.Add(SearchDomains.GetSequenceHashCode());

        return hashCode.ToHashCode();
    }

    /// <inheritdoc/>
    public bool Equals(NetworkDNSSettings? other) =>
        other is not null &&
        Hostname.Equals(other.Hostname) &&
        MulticastDNSEnabled.Equals(other.MulticastDNSEnabled) &&
        StaticHosts.Equals(other.StaticHosts) &&
        NameServers.Equals(other.NameServers) &&
        DNSSuffixEnabled.Equals(other.DNSSuffixEnabled) &&
        DNSSuffix.Equals(other.DNSSuffix) &&
        SearchDomainsEnabled.Equals(other.SearchDomainsEnabled) &&
        SearchDomains.SequenceEqual(other.SearchDomains);
}
