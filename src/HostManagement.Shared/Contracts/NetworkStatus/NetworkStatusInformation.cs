using System.Text.Json.Serialization;

namespace HostManagement.Shared.Contracts.NetworkStatus;

/// <summary>
/// Represents the <see cref="NetworkStatusInformation"/>.
/// </summary>
public sealed record NetworkStatusInformation
{
    /// <summary>
    /// Represents the empty <see cref="NetworkStatusInformation"/>.
    /// </summary>
    [JsonIgnore]
    public static NetworkStatusInformation Empty { get; } = new NetworkStatusInformation()
    {
        IsDefaultRouteConfigured = false,
        IsDefaultGatewayAvailable = false,
        IsInternetAvailable = false,
        IsDNSFunctional = false,
        Interfaces = []
    };

    /// <summary>
    /// Determines whether a default route is configured.
    /// </summary>
    public required bool IsDefaultRouteConfigured { get; init; }

    /// <summary>
    /// Determines whether a default gateway is available.
    /// </summary>
    public required bool IsDefaultGatewayAvailable { get; init; }

    /// <summary>
    /// Determines whether an internet connection is available.
    /// </summary>
    public required bool IsInternetAvailable { get; init; }

    /// <summary>
    /// Determines whether domain name resolution is functional.
    /// </summary>
    public required bool IsDNSFunctional { get; init; }

    /// <summary>
    /// A list containing the name and connection state of all interfaces.
    /// </summary>
    public required IReadOnlyCollection<InterfaceInformation> Interfaces { get; init; }
}
