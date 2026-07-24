using System.Text.Json.Serialization;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Contracts.Service;
using HostManagement.Shared.Extensions;

namespace HostManagement.Shared.Contracts;

/// <summary>
/// Represents the public shared <see cref="SystemConfiguration"/> of the target system.
/// </summary>
/// <remarks>
/// This namespace defines the public API contract for external consumers of the HostManagement system.
/// These DTOs represent the stable, publicly-exposed interface for configuration and status information.
/// </remarks>
public class SystemConfiguration
{
    /// <summary>
    /// The version of the system configuration.
    /// </summary>
    [JsonRequired]
    public int Version { get; init; } = 2;

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
    public List<ServiceDetail> Services { get; init; } = [];

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hashCode = new HashCode();

        hashCode.Add(NetworkInterfacesSettings);
        hashCode.Add(NetworkDNSSettings);
        hashCode.Add(NetworkProxySettings);
        hashCode.Add(NetworkNTPSettings);
        hashCode.Add(Services.OrderBy(x => x.Name).GetSequenceHashCode());

        return hashCode.ToHashCode();
    }

    /// <inheritdoc/>
    public bool Equals(SystemConfiguration? other)
        => other is not null
        && NetworkInterfacesSettings.Equals(other.NetworkInterfacesSettings)
        && NetworkDNSSettings.Equals(other.NetworkDNSSettings)
        && NetworkProxySettings.Equals(other.NetworkProxySettings)
        && NetworkNTPSettings.Equals(other.NetworkNTPSettings)
        && Services.OrderBy(x => x.Name).SequenceEqual(other.Services.OrderBy(x => x.Name));
}
