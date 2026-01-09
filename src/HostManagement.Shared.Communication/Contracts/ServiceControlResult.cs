namespace HostManagement.Shared.Communication.Contracts;

/// <summary>
/// Represents the result of starting, stopping or restarting a service.
/// </summary>
public record ServiceControlResult : OperationResult
{
    /// <summary>
    /// Initializes a new <see cref="ServiceControlResult"/>.
    /// </summary>
    public ServiceControlResult()
        : base(nameof(ServiceControlResult))
    {
    }

    /// <inheritdoc/>
    protected override string ExpectedType => nameof(ServiceControlResult);
}
