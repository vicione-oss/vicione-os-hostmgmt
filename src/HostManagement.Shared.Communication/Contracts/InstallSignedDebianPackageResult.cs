namespace HostManagement.Shared.Communication.Contracts;

/// <summary>
/// Represents the result of installing a signed Debian package.
/// </summary>
[Obsolete("Use SystemControlResult instead.")]
public record InstallSignedDebianPackageResult : OperationResult
{
    /// <summary>
    /// Initializes a new <see cref="InstallSignedDebianPackageResult"/>.
    /// </summary>
    public InstallSignedDebianPackageResult()
        : base(nameof(SystemControlResult))
    {
    }

    /// <inheritdoc/>
    protected override string ExpectedType => nameof(SystemControlResult);
}
