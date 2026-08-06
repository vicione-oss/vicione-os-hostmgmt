using System.Text.Json.Serialization;
using HostManagement.Shared.Extensions;
using HostManagement.SystemConfig.Migrations;

namespace HostManagement.SystemConfig.Contracts;

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
internal class SystemConfiguration : IEquatable<SystemConfiguration>, ISystemConfigurationContract
{
    /// <summary>
    /// The current version of the system configuration contract.
    /// </summary>
    internal const int CurrentVersion = 2;

    [JsonRequired]
    public int Version { get; init; } = CurrentVersion;
    public List<NetworkInterfaceDetail>? NetworkInterfaces { get; set; }
    public NetworkDNSSettings? NetworkDNSSettings { get; set; }
    public NetworkProxySettings? NetworkProxySettings { get; set; }
    public NetworkNTPSettings? NetworkNTPSettings { get; set; }
    public List<ServiceDetail>? Services { get; set; }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();

        hashCode.AddRange(NetworkInterfaces);
        hashCode.Add(NetworkDNSSettings);
        hashCode.Add(NetworkProxySettings);
        hashCode.Add(NetworkNTPSettings);
        hashCode.AddRange(Services, order: service => service.Name);

        return hashCode.ToHashCode();
    }

    public override bool Equals(object? obj) => obj is SystemConfiguration other && Equals(other);

    public bool Equals(SystemConfiguration? other)
        => other is not null
        && EqualityComparer<NetworkInterfaceDetail>.Default.SequenceEqual(NetworkInterfaces, other.NetworkInterfaces)
        && EqualityComparer<NetworkDNSSettings>.Default.Equals(NetworkDNSSettings, other.NetworkDNSSettings)
        && EqualityComparer<NetworkProxySettings>.Default.Equals(NetworkProxySettings, other.NetworkProxySettings)
        && EqualityComparer<NetworkNTPSettings>.Default.Equals(NetworkNTPSettings, other.NetworkNTPSettings)
        && EqualityComparer<ServiceDetail>.Default.SequenceEqual(Services, other.Services, service => service.Name);
}
