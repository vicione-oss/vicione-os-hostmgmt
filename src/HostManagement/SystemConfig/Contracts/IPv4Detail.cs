using System.Net;

namespace HostManagement.SystemConfig.Contracts;

internal sealed record IPv4Detail
{
    public required IPAddress IPAddress { get; set; }
    public required IPAddress Netmask { get; set; }
}
