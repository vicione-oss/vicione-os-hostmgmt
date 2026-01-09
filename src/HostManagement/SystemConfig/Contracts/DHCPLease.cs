using System.Net;

namespace HostManagement.SystemConfig.Contracts;

internal sealed record DHCPLease
{
    public DateTimeOffset? LeaseObtained { get; set; }
    public DateTimeOffset? LeaseExpires { get; set; }
    public IPAddress? Gateway { get; set; }
    public NetworkDNSSettings? NetworkDNSSettings { get; set; }
    public IPv4Detail? IPv4Detail { get; set; }
}
