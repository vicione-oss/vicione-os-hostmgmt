using HostManagement.Shared.Enums;

namespace HostManagement.Shared.Contracts.Service;

/// <summary>
/// Represents the details of a Linux service (systemd unit).
/// </summary>
public sealed record ServiceDetail
{
    /// <summary>
    /// The service name.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The service state.
    /// </summary>
    public required ServiceState State { get; set; }
}
