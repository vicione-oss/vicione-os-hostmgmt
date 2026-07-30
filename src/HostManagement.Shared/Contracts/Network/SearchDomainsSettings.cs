using HostManagement.Shared.Extensions;

namespace HostManagement.Shared.Contracts.Network;

/// <summary>
/// Represents the <see cref="SearchDomainsSettings"/>.
/// </summary>
public sealed record SearchDomainsSettings
{
    /// <summary>
    /// Determines whether search domains are enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// The list of DNS search domains.
    /// </summary>
    /// <remarks>
    /// DNS suffixes excluding the primary DNS suffix (<see cref="NetworkDNSSettings.PrimaryDNSSuffix"/>).
    /// </remarks>
    public List<string> Domains { get; init; } = [];

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hashCode = new HashCode();

        hashCode.Add(Enabled);
        hashCode.Add(Domains.GetSequenceHashCode());

        return hashCode.ToHashCode();
    }

    /// <inheritdoc/>
    public bool Equals(SearchDomainsSettings? other)
        => other is not null
        && Enabled.Equals(other.Enabled)
        && Domains.SequenceEqual(other.Domains);
}
