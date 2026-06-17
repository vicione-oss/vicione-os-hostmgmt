namespace HostManagement.SystemConfig.Migrations.Contracts.V1;

internal sealed record NetworkInterfacesSettings
{
    public List<NetworkInterfaceDetail>? NetworkInterfaces { get; set; }

    public override int GetHashCode()
    {
        HashCode hashCode = new();
        hashCode.AddRange(NetworkInterfaces, EqualityComparer<NetworkInterfaceDetail>.Default);
        return hashCode.ToHashCode();
    }

    public bool Equals(NetworkInterfacesSettings? other) =>
        other is not null &&
        EqualityComparer<NetworkInterfaceDetail>.Default.SequenceEqual(NetworkInterfaces, other.NetworkInterfaces);
}
