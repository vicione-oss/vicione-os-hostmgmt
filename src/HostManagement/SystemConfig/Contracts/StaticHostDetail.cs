using System.Net;

namespace HostManagement.SystemConfig.Contracts;

internal sealed record StaticHostDetail
{
    public required IPAddress IPAddress { get; set; }
    public required string Hostname { get; set; }
}
