namespace HostManagement.Shared.Communication.Contracts;

/// <summary>
/// Represents the result of restarting the system.
/// </summary>
public record RestartSystemResult : OperationResult
{
    /// <summary>
    /// Initializes a new <see cref="RestartSystemResult"/>.
    /// </summary>
    public RestartSystemResult()
        : base(nameof(RestartSystemResult))
    {
    }

    /// <inheritdoc/>
    protected override string ExpectedType => nameof(RestartSystemResult);
}
