namespace HostManagement.SystemConfig.Contracts;

internal sealed record PrimaryDNSSuffixSettings
{
    public bool? Enabled { get; set; }
    public string? Suffix { get; set; }
}
