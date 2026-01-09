namespace HostManagement.SystemConfig.Contracts;

internal sealed record ServiceDetail
{
    public required string Name { get; set; }
    public ServiceState State { get; set; }
}
