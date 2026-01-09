namespace HostManagement.Shared.Communication.Contracts;

/// <summary>
/// Represents the result of shutting down the system.
/// </summary>
[Obsolete("Use SystemControlResult instead.")]
public record ShutdownSystemResult : OperationResult
{
    /// <summary>
    /// Initializes a new <see cref="ShutdownSystemResult"/>.
    /// </summary>
    public ShutdownSystemResult()
        : base(nameof(SystemControlResult))
    {
    }

    /// <inheritdoc/>
    protected override string ExpectedType => nameof(SystemControlResult);
}
