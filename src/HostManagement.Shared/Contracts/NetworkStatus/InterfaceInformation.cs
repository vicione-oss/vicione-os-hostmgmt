namespace HostManagement.Shared.Contracts.NetworkStatus;

/// <summary>
/// Represents the <see cref="InterfaceInformation"/> containing the name and connection state of
/// an interface.
/// </summary>
public sealed record InterfaceInformation
{
    /// <summary>
    /// The name of the network interface.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// The connection state of the network interface.
    /// </summary>
    public required string State { get; init; }
}
