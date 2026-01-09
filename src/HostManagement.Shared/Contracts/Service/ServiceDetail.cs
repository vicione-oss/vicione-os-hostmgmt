using HostManagement.Shared.Enums;

namespace HostManagement.Shared.Contracts.Service;

/// <summary>
/// Represents the details of a Linux service (systemd unit).
/// </summary>
public sealed record ServiceDetail
{
    /// <summary>
    /// Represents the empty or unknown ServiceDetail.
    /// </summary>
    public static ServiceDetail Empty { get; } =
        new ServiceDetail() { Name = "Empty", State = ServiceState.Unknown };

    /// <summary>
    /// The service name.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The service state.
    /// </summary>
    public required ServiceState State { get; set; }
}
