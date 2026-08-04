using System.Net;
using HostManagement.Shared.Extensions;

namespace HostManagement.SystemConfig.Contracts;

internal sealed record IPv4Settings
{
    public IPAddress? Gateway { get; set; }
    public bool? DHCPEnabled { get; set; }
    public List<IPv4Detail>? IPv4Details { get; set; }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();

        hashCode.Add(Gateway);
        hashCode.Add(DHCPEnabled);
        hashCode.AddRange(IPv4Details);

        return hashCode.ToHashCode();
    }

    public bool Equals(IPv4Settings? other)
        => other is not null
        && EqualityComparer<IPAddress>.Default.Equals(Gateway, other.Gateway)
        && EqualityComparer<bool?>.Default.Equals(DHCPEnabled, other.DHCPEnabled)
        && EqualityComparer<IPv4Detail>.Default.SequenceEqual(IPv4Details, other.IPv4Details);
}
