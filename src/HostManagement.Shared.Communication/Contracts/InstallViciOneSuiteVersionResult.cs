using HostManagement.Shared.Communication.Enums;

namespace HostManagement.Shared.Communication.Contracts;

/// <summary>
/// Represents the result of installing a ViciOne Suite version.
/// </summary>
public record InstallViciOneSuiteVersionResult
{
    /// <summary>
    /// The status returned from the operation.
    /// </summary>
    public required OperationStatus Status { get; init; }

    /// <summary>
    /// The message returned from the operation.
    /// </summary>
    public required string? Message { get; init; }
}
