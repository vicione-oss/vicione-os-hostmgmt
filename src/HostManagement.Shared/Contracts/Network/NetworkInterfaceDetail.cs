using HostManagement.Shared.Capabilities;

namespace HostManagement.Shared.Contracts.Network;

/// <summary>
/// Represents a <see cref="NetworkInterfaceDetail"/>.
/// </summary>
public sealed record NetworkInterfaceDetail
{
    /// <summary>
    /// The common information of the network interface.
    /// </summary>
    public required NetworkInterfaceCommonInformation CommonInformation { get; set; }

    /// <summary>
    /// The IPv4 settings of the network interface.
    /// </summary>
    [SupportedSetting]
    public required IPv4Settings IPv4 { get; set; }

    /// <summary>
    /// The VLAN settings of the network interface.
    /// </summary>
    [SupportedSetting]
    public VLANSettings VLAN { get; set; } = new();
}
