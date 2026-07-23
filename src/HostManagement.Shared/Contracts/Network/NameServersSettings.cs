using System.Net;
using System.Text.Json.Serialization;
using HostManagement.Shared.Extensions;

namespace HostManagement.Shared.Contracts.Network;

/// <summary>
/// Represents the <see cref="NameServersSettings"/>.
/// </summary>
public sealed record NameServersSettings
{
    /// <summary>
    /// Determines whether the DNS name servers are enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// The list of DNS name servers (IPv4 and IPv6 addresses).
    /// </summary>
    [JsonConverter(typeof(IPAddressListConverter))]
    public List<IPAddress> Addresses { get; init; } = [];

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hashCode = new HashCode();

        hashCode.Add(Enabled);
        hashCode.Add(Addresses.GetSequenceHashCode());

        return hashCode.ToHashCode();
    }

    /// <inheritdoc/>
    public bool Equals(NameServersSettings? other)
        => other is not null
        && Enabled.Equals(other.Enabled)
        && Addresses.SequenceEqual(other.Addresses);
}
