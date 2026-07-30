using HostManagement.Shared.Extensions;

namespace HostManagement.Shared.Contracts.Network;

/// <summary>
/// Represents the <see cref="NoProxySettings"/>.
/// </summary>
public sealed record NoProxySettings
{
    /// <summary>
    /// The enabled state of the NoProxy list.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// The list of IPv4/IPv6 addresses or domain names that should be ignored by the proxy.
    /// </summary>
    /// <remarks>
    /// If a <b>port</b> needs to be specified the following format is valid:
    /// <para>
    /// IPv4:PORT<br/>
    /// [IPv6]:PORT
    /// </para>
    /// <para>
    /// If a <b>link-local IPv6</b> address has to be used a <b>zone identifier</b> for the interface
    /// needs to be added, e.g. eth0. The following format is valid:
    /// <para>
    /// IPv6%ID<br/>
    /// [IPv6%ID]:PORT
    /// </para>
    /// <b>Hint:</b> If a DoNotProxy item is used in a URI '%' must be percent-encoded as '%25', e.g.:
    /// 'fe80::1%25eth0' to satisfy RFC 6874.
    /// </para>
    /// </remarks>
    public List<string> Entries { get; init; } = [];

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hashCode = new HashCode();

        hashCode.Add(Enabled);
        hashCode.Add(Entries.GetSequenceHashCode());

        return hashCode.ToHashCode();
    }

    /// <inheritdoc/>
    public bool Equals(NoProxySettings? other)
        => other is not null
        && Enabled == other.Enabled
        && Entries.SequenceEqual(other.Entries);
}
