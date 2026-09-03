using System.Net;
using System.Text.Json.Serialization;
using HostManagement.Shared.Extensions;

namespace HostManagement.Shared.Contracts.Network;

/// <summary>
/// Represents the <see cref="IPv4Settings"/>.
/// </summary>
public sealed record IPv4Settings
{
    /// <summary>
    /// The IP address of the default gateway.
    /// </summary>
    [JsonConverter(typeof(IPAddressConverter))]
    public IPAddress? Gateway { get; set; }

    /// <summary>
    /// Determines if DHCP should be enabled.
    /// </summary>
    public bool DHCPEnabled { get; set; }

    /// <summary>
    /// The list of IPv4 details.
    /// </summary>
    public List<IPv4Detail> IPv4Details { get; init; } = [];

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hashCode = new HashCode();

        hashCode.Add(Gateway);
        hashCode.Add(DHCPEnabled);
        hashCode.AddRange(IPv4Details);

        return hashCode.ToHashCode();
    }

    /// <inheritdoc/>
    public bool Equals(IPv4Settings? other)
        => other is not null
        && EqualityComparer<IPAddress>.Default.Equals(Gateway, other.Gateway)
        && DHCPEnabled == other.DHCPEnabled
        && IPv4Details.SequenceEqual(other.IPv4Details);
}
