using System.Net;

namespace HostManagement.SystemConfig.Migrations.Contracts.V1;

internal sealed record NetworkDNSSettings
{
    public string? Hostname { get; set; }
    public bool? MulticastDNSEnabled { get; set; }
    public bool? StaticHostsEnabled { get; set; }
    public List<StaticHostDetail>? StaticHosts { get; set; }
    public bool? NameServersEnabled { get; set; }
    public List<IPAddress>? NameServers { get; set; }
    public bool? DNSSuffixEnabled { get; set; }
    public string? DNSSuffix { get; set; }
    public bool? SearchDomainsEnabled { get; set; }
    public List<string>? SearchDomains { get; set; }

    public override int GetHashCode()
    {
        HashCode hashCode = new();
        hashCode.Add(Hostname);
        hashCode.Add(MulticastDNSEnabled);
        hashCode.Add(StaticHostsEnabled);
        hashCode.AddRange(StaticHosts, EqualityComparer<StaticHostDetail>.Default);
        hashCode.Add(NameServersEnabled);
        hashCode.AddRange(NameServers, EqualityComparer<IPAddress>.Default);
        hashCode.Add(DNSSuffixEnabled);
        hashCode.Add(DNSSuffix);
        hashCode.Add(SearchDomainsEnabled);
        hashCode.AddRange(SearchDomains, EqualityComparer<string>.Default);
        return hashCode.ToHashCode();
    }

    public bool Equals(NetworkDNSSettings? other) =>
        other is not null &&
        EqualityComparer<string>.Default.Equals(Hostname, other.Hostname) &&
        EqualityComparer<bool?>.Default.Equals(MulticastDNSEnabled, other.MulticastDNSEnabled) &&
        EqualityComparer<bool?>.Default.Equals(StaticHostsEnabled, other.StaticHostsEnabled) &&
        EqualityComparer<StaticHostDetail>.Default.SequenceEqual(StaticHosts, other.StaticHosts) &&
        EqualityComparer<bool?>.Default.Equals(NameServersEnabled, other.NameServersEnabled) &&
        EqualityComparer<IPAddress>.Default.SequenceEqual(NameServers, other.NameServers) &&
        EqualityComparer<bool?>.Default.Equals(DNSSuffixEnabled, other.DNSSuffixEnabled) &&
        EqualityComparer<string>.Default.Equals(DNSSuffix, other.DNSSuffix) &&
        EqualityComparer<bool?>.Default.Equals(SearchDomainsEnabled, other.SearchDomainsEnabled) &&
        EqualityComparer<string>.Default.SequenceEqual(SearchDomains, other.SearchDomains);
}
