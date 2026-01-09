namespace HostManagement.Shared.Communication.Contracts;

/// <summary>
/// Represents the result of installing a ViciOne Suite version.
/// </summary>
[Obsolete("Use InstallSignedDebianPackageResult instead. This will be removed in a future version.", false)]
public record InstallViciOneSuiteVersionResult : OperationResult
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
}
