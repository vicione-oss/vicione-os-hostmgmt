using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Contracts.Network;

namespace HostManagement.Shared.Communication.Contracts;

/// <summary>
/// Represents the result of getting the NTP synchronization status.
/// </summary>
public record GetNTPSyncStatusResult : OperationResult, IOperationResultFactory<GetNTPSyncStatusResult>
{
    /// <summary>
    /// Initializes a new <see cref="GetNTPSyncStatusResult"/>.
    /// </summary>
    public GetNTPSyncStatusResult()
        : base(nameof(GetNTPSyncStatusResult))
    {
    }

    /// <summary>
    /// The NTP synchronization status or <see langword="null"/> if <see cref="OperationResult.Status"/> is not <see cref="OperationStatus.Success"/>.
    /// </summary>
    public required NTPSyncStatus? NTPSyncStatus { get; init; }

    /// <inheritdoc/>
    protected override string ExpectedType => nameof(GetNTPSyncStatusResult);

    /// <inheritdoc/>
    public static GetNTPSyncStatusResult CreateErrorResult(string message) => new()
    {
        Status = OperationStatus.Error,
        Message = message,
        NTPSyncStatus = null
    };
}
