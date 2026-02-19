namespace HostManagement.SystemConfig.Contracts;

internal sealed record NetworkProxyDetail
{
    public bool Enabled { get; set; }
    public string? Server { get; set; }
    public int? Port { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
}
