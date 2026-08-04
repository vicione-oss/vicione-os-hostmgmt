using HostManagement.Shared.Extensions;

namespace HostManagement.SystemConfig.Contracts;

internal sealed record StaticHostsSettings
{
    public bool? Enabled { get; set; }
    public List<StaticHostDetail>? Hosts { get; set; }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();

        hashCode.Add(Enabled);
        hashCode.AddRange(Hosts);

        return hashCode.ToHashCode();
    }

    public bool Equals(StaticHostsSettings? other)
        => other is not null
        && EqualityComparer<bool?>.Default.Equals(Enabled, other.Enabled)
        && EqualityComparer<StaticHostDetail>.Default.SequenceEqual(Hosts, other.Hosts);
}
