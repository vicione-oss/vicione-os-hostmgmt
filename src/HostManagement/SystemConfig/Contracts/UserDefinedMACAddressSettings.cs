namespace HostManagement.SystemConfig.Contracts;

internal sealed record UserDefinedMACAddressSettings
{
    public required bool Enabled { get; set; }
    public required string Address { get; set; }
}
