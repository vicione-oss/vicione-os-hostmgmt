namespace HostManagement.Shared.Communication.Contracts;

/// <summary>
/// Represents the result of a system operation (e.g., shutdown, restart, reset,
/// or installing a signed Debian package).
/// </summary>
public record SystemControlResult : OperationResult
{
    /// <summary>
    /// Initializes a new <see cref="SystemControlResult"/>.
    /// </summary>
    public SystemControlResult()
        : base(nameof(SystemControlResult))
    {
    }

    /// <inheritdoc/>
    protected override string ExpectedType => nameof(SystemControlResult);
}
