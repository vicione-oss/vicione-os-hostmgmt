namespace HostManagement.SystemConfig.Contracts;

internal sealed record NetworkInterfaceCommonInformation
{
    public required string Name { get; set; }
    public bool? Enabled { get; set; }
    public UserDefinedMACAddressSettings? UserDefinedMACAddress { get; set; }
}
