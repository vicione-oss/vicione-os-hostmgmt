using System.Net;

namespace HostManagement.SystemConfig.Migrations.Contracts.V1;

internal sealed record StaticHostDetail
{
    public required IPAddress IPAddress { get; set; }
    public required string Hostname { get; set; }
}
