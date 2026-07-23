using HostManagement.Shared.Extensions;

namespace HostManagement.Shared.Contracts.Network;

/// <summary>
/// Represents the <see cref="StaticHostsSettings"/>.
/// </summary>
public sealed record StaticHostsSettings
{
    /// <summary>
    /// Determines whether static hosts are enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// The list of static host details.
    /// </summary>
    public List<StaticHostDetail> Hosts { get; init; } = [];

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hashCode = new HashCode();

        hashCode.Add(Enabled);
        hashCode.Add(Hosts.GetSequenceHashCode());

        return hashCode.ToHashCode();
    }

    /// <inheritdoc/>
    public bool Equals(StaticHostsSettings? other)
        => other is not null
        && Enabled.Equals(other.Enabled)
        && Hosts.SequenceEqual(other.Hosts);
}
