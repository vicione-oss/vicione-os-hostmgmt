namespace HostManagement.SystemConfig.Contracts;

internal sealed record NetworkDNSSettings
{
    public string? Hostname { get; set; }
    public bool? MulticastDNSEnabled { get; set; }
    public StaticHostsSettings? StaticHosts { get; set; }
    public NameServersSettings? NameServers { get; set; }
    public PrimaryDNSSuffixSettings? PrimaryDNSSuffix { get; set; }
    public SearchDomainsSettings? SearchDomains { get; set; }
}
