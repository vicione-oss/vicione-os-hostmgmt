using System.Text.Json.Serialization;

namespace HostManagement.SystemConfig.Contracts;

internal class SystemConfiguration
{
    [JsonRequired]
    public int Version { get; init; } = 1;
    public NetworkInterfacesSettings? NetworkInterfacesSettings { get; set; }
    public NetworkDNSSettings? NetworkDNSSettings { get; set; }
    public NetworkProxySettings? NetworkProxySettings { get; set; }
    public NetworkNTPSettings? NetworkNTPSettings { get; set; }
    public List<ServiceDetail>? Services { get; set; }

    public override int GetHashCode()
    {
        HashCode hashCode = new();
        hashCode.Add(NetworkInterfacesSettings);
        hashCode.Add(NetworkDNSSettings);
        hashCode.Add(NetworkProxySettings);
        hashCode.Add(NetworkNTPSettings);
        hashCode.AddRange(Services, EqualityComparer<ServiceDetail>.Default, x => x.Name);
        return hashCode.ToHashCode();
    }

    public bool Equals(SystemConfiguration? other) =>
        other is not null &&
        EqualityComparer<NetworkInterfacesSettings>.Default.Equals(NetworkInterfacesSettings, other.NetworkInterfacesSettings) &&
        EqualityComparer<NetworkDNSSettings>.Default.Equals(NetworkDNSSettings, other.NetworkDNSSettings) &&
        EqualityComparer<NetworkProxySettings>.Default.Equals(NetworkProxySettings, other.NetworkProxySettings) &&
        EqualityComparer<NetworkNTPSettings>.Default.Equals(NetworkNTPSettings, other.NetworkNTPSettings) &&
        EqualityComparer<ServiceDetail>.Default.SequenceEqual(Services, other.Services, service => service.Name);
}
