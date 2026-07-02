namespace HostManagement.Shared.Contracts.Network;

/// <summary>
/// Represents the <see cref="VLANSettings"/>.
/// </summary>
public sealed record VLANSettings
{
    /// <summary>
    /// Determines if VLAN should be enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// The VLAN ID.
    /// </summary>
    /// <remarks>
    /// <b>0:</b> indicates that the frame does not carry a VLAN ID<br/>
    /// <b>1-4094:</b> usable VLAN IDs<br/>
    /// <b>4095:</b> reserved for implementation use and can not be configured
    /// </remarks>
    public int ID { get; set; }
}
