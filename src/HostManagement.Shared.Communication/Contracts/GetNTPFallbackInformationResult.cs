using HostManagement.Shared.Communication.Enums;

namespace HostManagement.Shared.Communication.Contracts;

/// <summary>
/// Represents the result of getting the NTP fallback servers from the system.
/// </summary>
public record GetNTPFallbackInformationResult
    : OperationResult, IOperationResultFactory<GetNTPFallbackInformationResult>
{
    /// <summary>
    /// Initializes a new <see cref="GetNTPFallbackInformationResult"/>.
    /// </summary>
    public GetNTPFallbackInformationResult()
        : base(nameof(GetNTPFallbackInformationResult))
    {
    }

    /// <summary>
    /// The list of NTP fallback servers retrieved from the system.
    /// </summary>
    public required List<string> FallbackNTPServers { get; init; } = [];

    /// <inheritdoc/>
    protected override string ExpectedType => nameof(GetNTPFallbackInformationResult);

    /// <inheritdoc/>
    public static GetNTPFallbackInformationResult CreateErrorResult(string message) => new()
    {
        Status = OperationStatus.Error,
        Message = message,
        FallbackNTPServers = []
    };
}
