namespace HostManagement.SystemConfig.Migrations.Contracts.V1;

internal sealed record ServiceDetail
{
    public required string Name { get; set; }
    public ServiceState State { get; set; }
}
