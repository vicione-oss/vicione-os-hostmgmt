namespace HostManagement.Shared.Communication.Contracts;

/// <summary>
/// Represents the result of resetting the system.
/// </summary>
[Obsolete("Use SystemControlResult instead.")]
public record ResetSystemResult : OperationResult
{
    /// <summary>
    /// Initializes a new <see cref="ResetSystemResult"/>.
    /// </summary>
    public ResetSystemResult()
        : base(nameof(SystemControlResult))
    {
    }

    /// <inheritdoc/>
    protected override string ExpectedType => nameof(SystemControlResult);
}
