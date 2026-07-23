namespace HostManagement.SystemConfig.Contracts;

internal sealed record NetworkDNSSettings
{
    public string? Hostname { get; set; }
    public bool? MulticastDNSEnabled { get; set; }
    public StaticHostsSettings? StaticHosts { get; set; }
    public NameServersSettings? NameServers { get; set; }
    public PrimaryDNSSuffixSettings? PrimaryDNSSuffix { get; set; }
    public bool? SearchDomainsEnabled { get; set; }
    public List<string>? SearchDomains { get; set; }

    public override int GetHashCode()
    {
        HashCode hashCode = new();
        hashCode.Add(Hostname);
        hashCode.Add(MulticastDNSEnabled);
        hashCode.Add(StaticHosts);
        hashCode.Add(NameServers);
        hashCode.Add(PrimaryDNSSuffix);
        hashCode.Add(SearchDomainsEnabled);
        hashCode.AddRange(SearchDomains, EqualityComparer<string>.Default);
        return hashCode.ToHashCode();
    }

    public bool Equals(NetworkDNSSettings? other) =>
        other is not null &&
        EqualityComparer<string>.Default.Equals(Hostname, other.Hostname) &&
        EqualityComparer<bool?>.Default.Equals(MulticastDNSEnabled, other.MulticastDNSEnabled) &&
        EqualityComparer<StaticHostsSettings?>.Default.Equals(StaticHosts, other.StaticHosts) &&
        EqualityComparer<NameServersSettings?>.Default.Equals(NameServers, other.NameServers) &&
        EqualityComparer<PrimaryDNSSuffixSettings?>.Default.Equals(PrimaryDNSSuffix, other.PrimaryDNSSuffix) &&
        EqualityComparer<bool?>.Default.Equals(SearchDomainsEnabled, other.SearchDomainsEnabled) &&
        EqualityComparer<string>.Default.SequenceEqual(SearchDomains, other.SearchDomains);
}
