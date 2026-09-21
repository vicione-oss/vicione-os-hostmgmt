namespace HostManagement.Shared.Contracts.Network;

/// <summary>
/// Represents the <see cref="NetworkInterfaceCommonInformation"/>.
/// </summary>
public sealed record NetworkInterfaceCommonInformation
{
    /// <summary>
    /// The enabled state of the network interface.
    /// </summary>
    public required bool Enabled { get; set; }

    /// <summary>
    /// The name of the network interface.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The user defined MAC address of the network interface.
    /// </summary>
    public UserDefinedMACAddressSettings UserDefinedMACAddress { get; set; } = new();
}
