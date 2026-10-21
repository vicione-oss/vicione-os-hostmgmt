namespace HostManagement.Shared.Contracts.Service;

/// <summary>
/// Represents a service control request with a name and optional control delay.
/// </summary>
public sealed record ServiceControlRequest
{
    /// <summary>
    /// The service name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// An optional delay in seconds. [default: 0]
    /// </summary>
    public int DelayInSeconds { get; init; }
}
