using HostManagement.Shared.Communication.Enums;

namespace HostManagement.Shared.Communication.Contracts;

/// <summary>
/// Represents the result of redeploying the application scripts.
/// </summary>
public record RedeployScriptsResult : OperationResult, IOperationResultFactory<RedeployScriptsResult>
{
    /// <summary>
    /// Initializes a new <see cref="RedeployScriptsResult"/>.
    /// </summary>
    public RedeployScriptsResult()
        : base(nameof(RedeployScriptsResult))
    {
    }

    /// <inheritdoc/>
    protected override string ExpectedType => nameof(RedeployScriptsResult);

    /// <inheritdoc/>
    public static RedeployScriptsResult CreateErrorResult(string message) => new()
    {
        Status = OperationStatus.Error,
        Message = message
    };
}
