namespace HostManagement.SystemConfig.Contracts;

internal sealed record NetworkProxyDetail
{
    public bool Enabled { get; set; }
    public required string Server { get; set; }
    public required int Port { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
}
