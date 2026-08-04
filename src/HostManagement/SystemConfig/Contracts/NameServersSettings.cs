using System.Net;
using HostManagement.Shared.Extensions;

namespace HostManagement.SystemConfig.Contracts;

internal sealed record NameServersSettings
{
    public bool? Enabled { get; set; }
    public List<IPAddress>? Addresses { get; set; }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();

        hashCode.Add(Enabled);
        hashCode.AddRange(Addresses);

        return hashCode.ToHashCode();
    }

    public bool Equals(NameServersSettings? other)
        => other is not null
        && EqualityComparer<bool?>.Default.Equals(Enabled, other.Enabled)
        && EqualityComparer<IPAddress>.Default.SequenceEqual(Addresses, other.Addresses);
}
