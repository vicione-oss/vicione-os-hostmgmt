namespace HostManagement.Shared.Communication.Contracts;

/// <summary>
/// Represents the result of updating a system.
/// </summary>
[Obsolete("Use SystemControlResult instead.")]
public record UpdateSystemResult : OperationResult
{
    /// <summary>
    /// Initializes a new <see cref="UpdateSystemResult"/>.
    /// </summary>
    public UpdateSystemResult()
        : base(nameof(SystemControlResult))
    {
    }

    /// <inheritdoc/>
    protected override string ExpectedType => nameof(SystemControlResult);
}
