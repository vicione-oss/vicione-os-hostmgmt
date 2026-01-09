using System.Text.Json.Serialization;
using HostManagement.Shared.Extensions;

namespace HostManagement.Shared.Contracts.Network;

/// <summary>
/// Represents the <see cref="NetworkInterfacesSettings"/>.
/// </summary>
public sealed record NetworkInterfacesSettings
{
    /// <summary>
    /// Initializes new <see cref="NetworkInterfacesSettings"/>.
    /// </summary>
    [JsonConstructor]
    public NetworkInterfacesSettings(List<NetworkInterfaceDetail>? networkInterfaces = null)
    {
        NetworkInterfaces = networkInterfaces ?? [];
    }

    /// <summary>
    /// The list of network interfaces.
    /// </summary>
    public List<NetworkInterfaceDetail> NetworkInterfaces { get; init; }

    /// <inheritdoc/>
    public override int GetHashCode() => NetworkInterfaces.GetSequenceHashCode();

    /// <inheritdoc/>
    public bool Equals(NetworkInterfacesSettings? other) =>
        other is not null &&
        NetworkInterfaces.SequenceEqual(other.NetworkInterfaces);
}
