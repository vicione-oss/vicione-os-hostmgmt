using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Contracts;

namespace HostManagement.Shared.Communication.Contracts;

/// <summary>
/// Represents the result of loading a system configuration.
/// </summary>
public record LoadSystemConfigurationResult
{
    /// <summary>
    /// The status returned from the operation.
    /// </summary>
    public required OperationStatus Status { get; init; }

    /// <summary>
    /// The message returned from the operation.
    /// </summary>
    public required string? Message { get; init; }

    /// <summary>
    /// The <see cref="SystemConfiguration"/> returned from the operation.
    /// </summary>
    public required SystemConfiguration? Configuration { get; init; }
}
