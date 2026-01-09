using HostManagement.Shared.Communication.Enums;

namespace HostManagement.Shared.Communication.Contracts;

/// <summary>
/// Represents the result of installing a ViciOne Suite version.
/// </summary>
[Obsolete("Use InstallSignedDebianPackageResult instead. This will be removed in a future version.", false)]
public record InstallViciOneSuiteVersionResult
    : OperationResult, IOperationResultFactory<InstallViciOneSuiteVersionResult>
{
    /// <summary>
    /// Initializes a new <see cref="InstallViciOneSuiteVersionResult"/>.
    /// </summary>
    public InstallViciOneSuiteVersionResult()
        : base(nameof(InstallViciOneSuiteVersionResult))
    {
    }

    /// <inheritdoc/>
    protected override string ExpectedType => nameof(InstallViciOneSuiteVersionResult);

    /// <inheritdoc/>
    public static InstallViciOneSuiteVersionResult CreateErrorResult(string message) => new()
    {
        Status = OperationStatus.Error,
        Message = message
    };
}
