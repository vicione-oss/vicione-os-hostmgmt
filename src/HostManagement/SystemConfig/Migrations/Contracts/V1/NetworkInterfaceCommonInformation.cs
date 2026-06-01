namespace HostManagement.SystemConfig.Migrations.Contracts.V1;

internal sealed record NetworkInterfaceCommonInformation
{
    public required string Name { get; set; }
    public bool? Enabled { get; set; }
    public string? PhysicalAddress { get; set; }
}
