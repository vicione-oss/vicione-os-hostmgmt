namespace HostManagement.Shared.Contracts.Network;

/// <summary>
/// Represents the <see cref="UserDefinedMACAddressSettings"/>.
/// </summary>
public sealed record UserDefinedMACAddressSettings
{
    /// <summary>
    /// Determines whether the user defined MAC address is enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// The user defined MAC address.
    /// </summary>
    public string Address { get; set; } = string.Empty;
}
