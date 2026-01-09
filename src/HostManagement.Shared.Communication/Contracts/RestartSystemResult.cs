namespace HostManagement.Shared.Communication.Contracts;

/// <summary>
/// Represents the result of restarting the system.
/// </summary>
[Obsolete("Use SystemControlResult instead.")]
public record RestartSystemResult : OperationResult
{
    /// <summary>
    /// Initializes a new <see cref="RestartSystemResult"/>.
    /// </summary>
    public RestartSystemResult()
        : base(nameof(SystemControlResult))
    {
    }

    /// <inheritdoc/>
    protected override string ExpectedType => nameof(SystemControlResult);
}
