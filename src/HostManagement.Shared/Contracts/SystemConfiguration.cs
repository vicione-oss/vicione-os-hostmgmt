using System.Text.Json.Serialization;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Contracts.Service;

namespace HostManagement.Shared.Contracts;

/// <summary>
/// Represents the <see cref="SystemConfiguration"/> of the target system.
/// </summary>
public class SystemConfiguration
{
    /// <summary>
    /// The version of the system configuration.
    /// </summary>
    [JsonRequired]
    public int Version { get; init; } = 1;

    /// <summary>
    /// Initializes new <see cref="SystemConfiguration"/>.
    /// </summary>
    [JsonConstructor]
    public SystemConfiguration(List<ServiceDetail>? services = null)
    {
        Services = services ?? [];
    }

    /// <summary>
    /// The <see cref="NetworkInterfacesSettings"/>.
    /// </summary>
    public NetworkInterfacesSettings NetworkInterfacesSettings { get; set; } = new();

    /// <summary>
    /// The <see cref="NetworkDNSSettings"/>.
    /// </summary>
    public NetworkDNSSettings NetworkDNSSettings { get; set; } = new();

    /// <summary>
    /// The <see cref="NetworkProxySettings"/>.
    /// </summary>
    public NetworkProxySettings NetworkProxySettings { get; set; } = new();

    /// <summary>
    /// The <see cref="NetworkNTPSettings"/>.
    /// </summary>
    public NetworkNTPSettings NetworkNTPSettings { get; set; } = new();

    /// <summary>
    /// The list of managed services.
    /// </summary>
    public List<ServiceDetail> Services { get; init; }

    /// <inheritdoc/>
    public override int GetHashCode() =>
        HashCode.Combine(
            NetworkInterfacesSettings,
            NetworkDNSSettings,
            NetworkProxySettings,
            NetworkNTPSettings,
            Services);

    /// <inheritdoc/>
    public bool Equals(SystemConfiguration? other) =>
        other is not null &&
        NetworkInterfacesSettings.Equals(other.NetworkInterfacesSettings) &&
        NetworkDNSSettings.Equals(other.NetworkDNSSettings) &&
        NetworkProxySettings.Equals(other.NetworkProxySettings) &&
        NetworkNTPSettings.Equals(other.NetworkNTPSettings) &&
        Services.OrderBy(x => x.Name).SequenceEqual(other.Services.OrderBy(x => x.Name));
}
