namespace HostManagement.Shared.Communication.Contracts;

/// <summary>
/// Represents the result of updating a system.
/// </summary>
public record UpdateSystemResult : OperationResult
{
    /// <summary>
    /// Initializes a new <see cref="UpdateSystemResult"/>.
    /// </summary>
    public UpdateSystemResult()
        : base(nameof(UpdateSystemResult))
    {
    }

    /// <inheritdoc/>
    protected override string ExpectedType => nameof(UpdateSystemResult);
}
