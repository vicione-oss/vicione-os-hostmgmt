namespace HostManagement.Shared.Communication.Contracts;

/// <summary>
/// Represents the result of applying a system configuration.
/// </summary>
public record ApplySystemConfigurationResult : OperationResult
{
    /// <summary>
    /// Initializes a new <see cref="ApplySystemConfigurationResult"/>.
    /// </summary>
    public ApplySystemConfigurationResult()
        : base(nameof(ApplySystemConfigurationResult))
    {
    }

    /// <inheritdoc/>
    protected override string ExpectedType => nameof(ApplySystemConfigurationResult);
}
