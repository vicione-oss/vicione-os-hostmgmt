namespace HostManagement.Shared.Communication.Contracts;

/// <summary>
/// Represents the result of resetting the system.
/// </summary>
public record ResetSystemResult : OperationResult
{
    /// <summary>
    /// Initializes a new <see cref="ResetSystemResult"/>.
    /// </summary>
    public ResetSystemResult()
        : base(nameof(ResetSystemResult))
    {
    }

    /// <inheritdoc/>
    protected override string ExpectedType => nameof(ResetSystemResult);
}
