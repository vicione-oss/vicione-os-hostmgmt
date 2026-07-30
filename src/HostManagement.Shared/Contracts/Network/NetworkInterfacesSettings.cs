using HostManagement.Shared.Extensions;

namespace HostManagement.Shared.Contracts.Network;

/// <summary>
/// Represents the <see cref="NetworkInterfacesSettings"/>.
/// </summary>
public sealed record NetworkInterfacesSettings
{
    /// <summary>
    /// The list of network interfaces.
    /// </summary>
    public List<NetworkInterfaceDetail> NetworkInterfaces { get; init; } = [];

    /// <inheritdoc/>
    public override int GetHashCode() => NetworkInterfaces.GetSequenceHashCode();

    /// <inheritdoc/>
    public bool Equals(NetworkInterfacesSettings? other)
        => other is not null
        && NetworkInterfaces.SequenceEqual(other.NetworkInterfaces);
}
