namespace HostManagement.Shared.Communication.Contracts;

/// <summary>
/// Represents the result of shutting down the system.
/// </summary>
public record ShutdownSystemResult : OperationResult
{
    /// <summary>
    /// Initializes a new <see cref="ShutdownSystemResult"/>.
    /// </summary>
    public ShutdownSystemResult()
        : base(nameof(ShutdownSystemResult))
    {
    }

    /// <inheritdoc/>
    protected override string ExpectedType => nameof(ShutdownSystemResult);
}
