using System.Net;

namespace HostManagement.SystemConfig.Contracts;

internal sealed record IPv4Settings
{
    public IPAddress? Gateway { get; set; }
    public bool? DHCPEnabled { get; set; }
    public List<IPv4Detail>? IPv4Details { get; set; }
    public bool? VLANEnabled { get; set; }
    public int? VLANID { get; set; }

    public override int GetHashCode()
    {
        HashCode hashCode = new();

        hashCode.Add(Gateway);
        hashCode.Add(DHCPEnabled);
        hashCode.AddRange(IPv4Details, EqualityComparer<IPv4Detail>.Default);
        hashCode.Add(VLANEnabled);
        hashCode.Add(VLANID);

        return hashCode.ToHashCode();
    }

    public bool Equals(IPv4Settings? other) =>
        other is not null &&
        EqualityComparer<IPAddress>.Default.Equals(Gateway, other.Gateway) &&
        EqualityComparer<bool?>.Default.Equals(DHCPEnabled, other.DHCPEnabled) &&
        EqualityComparer<IPv4Detail>.Default.SequenceEqual(IPv4Details, other.IPv4Details) &&
        EqualityComparer<bool?>.Default.Equals(VLANEnabled, other.VLANEnabled) &&
        EqualityComparer<int?>.Default.Equals(VLANID, other.VLANID);
}
