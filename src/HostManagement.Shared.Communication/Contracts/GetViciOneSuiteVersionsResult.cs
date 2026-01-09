using HostManagement.Shared.Communication.Enums;

namespace HostManagement.Shared.Communication.Contracts;

/// <summary>
/// Represents the result of getting a list of compatible ViciOne Suite versions.
/// </summary>
public record GetViciOneSuiteVersionsResult
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
    /// The list of compatible ViciOne Suite versions returned from the operation.
    /// </summary>
    public required List<string> SuiteVersions { get; init; } = [];
}
