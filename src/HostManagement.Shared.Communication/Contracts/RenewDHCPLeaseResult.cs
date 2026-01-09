using HostManagement.Shared.Contracts.Network;

namespace HostManagement.Shared.Communication.Contracts;

/// <summary>
/// Represents the result of renewing the DHCP lease.
/// </summary>
public record RenewDHCPLeaseResult : OperationResult
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
}
