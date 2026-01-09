namespace HostManagement.SystemConfig.Contracts;

internal sealed record NetworkInterfaceDetail
{
    public NetworkInterfaceCommonInformation? CommonInformation { get; set; }
    public IPv4Settings? IPv4 { get; set; }
}
