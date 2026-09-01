namespace HostManagement.SystemConfig.Contracts;

internal sealed record NetworkProxySettings
{
    public NetworkProxyDetail? FTP { get; set; }
    public NetworkProxyDetail? HTTP { get; set; }
    public NetworkProxyDetail? HTTPS { get; set; }
    public NetworkProxyDetail? SFTP { get; set; }
    public NoProxySettings? NoProxy { get; set; }
}
