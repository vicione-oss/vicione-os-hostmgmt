namespace HostManagement.Shared.Contracts.Network;

/// <summary>
/// Represents the <see cref="PrimaryDNSSuffixSettings"/>.
/// </summary>
public sealed record PrimaryDNSSuffixSettings
{
    /// <summary>
    /// Determines whether the primary DNS suffix is enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// The primary DNS suffix.
    /// </summary>
    public string Suffix { get; set; } = string.Empty;
}
