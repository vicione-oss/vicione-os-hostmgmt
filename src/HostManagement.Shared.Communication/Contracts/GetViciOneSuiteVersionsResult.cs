using HostManagement.Shared.Communication.Enums;

namespace HostManagement.Shared.Communication.Contracts;

/// <summary>
/// Represents the result of getting a list of compatible ViciOne Suite versions.
/// </summary>
[Obsolete("This will be removed in a future version.", false)]
public record GetViciOneSuiteVersionsResult : OperationResult, IOperationResultFactory<GetViciOneSuiteVersionsResult>
{
    /// <summary>
    /// Initializes a new <see cref="GetViciOneSuiteVersionsResult"/>.
    /// </summary>
    public GetViciOneSuiteVersionsResult()
        : base(nameof(GetViciOneSuiteVersionsResult))
    {
    }

    /// <summary>
    /// The list of compatible ViciOne Suite versions returned from the operation.
    /// </summary>
    public required List<string> SuiteVersions { get; init; } = [];

    /// <inheritdoc/>
    protected override string ExpectedType => nameof(GetViciOneSuiteVersionsResult);

    /// <inheritdoc/>
    public static GetViciOneSuiteVersionsResult CreateErrorResult(string message) => new()
    {
        Status = OperationStatus.Error,
        Message = message,
        SuiteVersions = []
    };
}
