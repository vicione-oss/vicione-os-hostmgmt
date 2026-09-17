using System.Net.NetworkInformation;

namespace HostManagement.SystemConfig.Contracts;

internal sealed record UserDefinedMACAddressSettings
{
    public required bool Enabled { get; set; }
    public required PhysicalAddress Address { get; set; }
}
