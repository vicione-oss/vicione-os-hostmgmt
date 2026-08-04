using System.Text.Json.Serialization;
using HostManagement.Shared.Extensions;

namespace HostManagement.SystemConfig.Migrations.Contracts.V1;

/// <summary>
/// Represents the internally used configuration of the system.
/// </summary>
/// <remarks>
/// <para>
/// This defines internal DTOs that are used exclusively within the <see cref="HostManagement"/> project.
/// </para>
/// <para>
/// <strong>Key characteristics:</strong>
/// </para>
/// <list type="bullet">
/// <item><description>Use nullable reference types to represent optional fields</description></item>
/// <item><description>Are not exposed directly to external consumers</description></item>
/// <item><description>Require mapping via <see cref="SystemConfig.ContractsTranslator"/> before being exposed to the public API</description></item>
/// </list>
/// </remarks>
internal class SystemConfiguration : ISystemConfigurationContract
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
