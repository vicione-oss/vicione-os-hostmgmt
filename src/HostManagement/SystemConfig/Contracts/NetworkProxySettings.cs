namespace HostManagement.SystemConfig.Contracts;

internal sealed record NetworkProxySettings
{
    public NetworkProxyDetail? FTP { get; set; }
    public NetworkProxyDetail? HTTP { get; set; }
    public NetworkProxyDetail? HTTPS { get; set; }
    public NetworkProxyDetail? SFTP { get; set; }
    public NetworkProxyDetail? SOCKS { get; set; }
    public bool? DoNotProxyListEnabled { get; set; }
    public List<string>? DoNotProxyList { get; set; }

    public override int GetHashCode()
    {
        HashCode hashCode = new();
        hashCode.Add(HTTP);
        hashCode.Add(HTTPS);
        hashCode.Add(FTP);
        hashCode.Add(SFTP);
        hashCode.Add(SOCKS);
        hashCode.Add(DoNotProxyListEnabled);
        hashCode.AddRange(DoNotProxyList, EqualityComparer<string>.Default);
        return hashCode.ToHashCode();
    }

    public bool Equals(NetworkProxySettings? other) =>
        other is not null &&
        EqualityComparer<NetworkProxyDetail>.Default.Equals(HTTP, other.HTTP) &&
        EqualityComparer<NetworkProxyDetail>.Default.Equals(HTTPS, other.HTTPS) &&
        EqualityComparer<NetworkProxyDetail>.Default.Equals(FTP, other.FTP) &&
        EqualityComparer<NetworkProxyDetail>.Default.Equals(SFTP, other.SFTP) &&
        EqualityComparer<NetworkProxyDetail>.Default.Equals(SOCKS, other.SOCKS) &&
        EqualityComparer<bool?>.Default.Equals(DoNotProxyListEnabled, other.DoNotProxyListEnabled) &&
        EqualityComparer<string>.Default.SequenceEqual(DoNotProxyList, other.DoNotProxyList);
}
