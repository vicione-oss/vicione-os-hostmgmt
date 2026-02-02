using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Contracts.Network;

namespace HostManagement.Shared.Communication.Contracts;

/// <summary>
/// Represents the result of renewing the DHCP lease.
/// </summary>
[Obsolete("Use GetDHCPLeaseInformationResult instead. This will be removed in a future version.", false)]
public record RenewDHCPLeaseResult : OperationResult, IOperationResultFactory<RenewDHCPLeaseResult>
{
    /// <summary>
    /// Initializes a new <see cref="RenewDHCPLeaseResult"/>.
    /// </summary>
    public RenewDHCPLeaseResult()
        : base(nameof(RenewDHCPLeaseResult))
    {
    }

    /// <summary>
    /// The DHCP lease information.
    /// </summary>
    public required DHCPLease? DHCPLease { get; init; }

    /// <inheritdoc/>
    protected override string ExpectedType => nameof(RenewDHCPLeaseResult);

    /// <inheritdoc/>
    public static RenewDHCPLeaseResult CreateErrorResult(string message) => new()
    {
        Status = OperationStatus.Error,
        Message = message,
        DHCPLease = null
    };
}
