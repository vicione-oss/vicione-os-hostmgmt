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
    /// The <see cref="NetworkInterfacesSettings"/>.
    /// </summary>
    [JsonRequired]
    public NetworkInterfacesSettings NetworkInterfacesSettings { get; set; } = new();

    /// <summary>
    /// The <see cref="NetworkDNSSettings"/>.
    /// </summary>
    [JsonRequired]
    public NetworkDNSSettings NetworkDNSSettings { get; set; } = new();

    /// <summary>
    /// The <see cref="NetworkProxySettings"/>.
    /// </summary>
    [JsonRequired]
    public NetworkProxySettings NetworkProxySettings { get; set; } = new();

    /// <summary>
    /// The <see cref="NetworkNTPSettings"/>.
    /// </summary>
    [JsonRequired]
    public NetworkNTPSettings NetworkNTPSettings { get; set; } = new();

    /// <summary>
    /// The list of managed services.
    /// </summary>
    [JsonRequired]
    public List<ServiceDetail> Services { get; init; } = [];

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
