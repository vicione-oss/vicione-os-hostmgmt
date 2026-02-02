using HostManagement.Shared.Communication.Enums;

namespace HostManagement.Shared.Communication.Contracts;

/// <summary>
/// Represents the result of getting the state of a service.
/// </summary>
public record GetServiceStateResult : OperationResult, IOperationResultFactory<GetServiceStateResult>
{
    /// <summary>
    /// Initializes a new <see cref="GetServiceStateResult"/>.
    /// </summary>
    public GetServiceStateResult()
        : base(nameof(GetServiceStateResult))
    {
    }

    /// <summary>
    /// Contains a value that indicates whether the service is currently active or not.
    /// </summary>
    /// <remarks>
    /// The following states are defined: "activating", "active", "reloading", "deactivating", "inactive" and "failed".
    /// </remarks>
    public required string ActiveState { get; init; }

    /// <inheritdoc/>
    protected override string ExpectedType => nameof(GetServiceStateResult);

    /// <inheritdoc/>
    public static GetServiceStateResult CreateErrorResult(string message) => new()
    {
        Status = OperationStatus.Error,
        Message = message,
        ActiveState = "unknown"
    };
}
