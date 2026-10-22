using HostManagement.Shared.Capabilities;
using HostManagement.Shared.Communication.Enums;

namespace HostManagement.Shared.Communication.Contracts;

/// <summary>
/// Represents the result of getting the supported capabilities.
/// </summary>
public record GetSupportedCapabilitiesResult : OperationResult, IOperationResultFactory<GetSupportedCapabilitiesResult>
{
    /// <summary>
    /// Initializes a new <see cref="GetSupportedCapabilitiesResult"/>.
    /// </summary>
    public GetSupportedCapabilitiesResult()
        : base(nameof(GetSupportedCapabilitiesResult))
    {
    }

    /// <summary>
    /// The <see cref="SupportedCapabilities"/> returned from the operation.
    /// </summary>
    public required SupportedCapabilities SupportedCapabilities { get; init; }

    /// <inheritdoc/>
    protected override string ExpectedType => nameof(GetSupportedCapabilitiesResult);

    /// <inheritdoc/>
    public static GetSupportedCapabilitiesResult CreateErrorResult(string message) => new()
    {
        Status = OperationStatus.Error,
        Message = message,
        SupportedCapabilities = new()
    };
}
