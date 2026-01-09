using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Contracts.Network;

namespace HostManagement.Shared.Communication.Contracts;

/// <summary>
/// Represents the result of getting the DHCP lease information.
/// </summary>
public record GetDHCPLeaseInformationResult : OperationResult, IOperationResultFactory<GetDHCPLeaseInformationResult>
{
    /// <summary>
    /// Initializes a new <see cref="GetDHCPLeaseInformationResult"/>.
    /// </summary>
    public GetDHCPLeaseInformationResult()
        : base(nameof(GetDHCPLeaseInformationResult))
    {
    }

    /// <summary>
    /// The DHCP lease information.
    /// </summary>
    public required DHCPLease? DHCPLease { get; init; }

    /// <inheritdoc/>
    protected override string ExpectedType => nameof(GetDHCPLeaseInformationResult);

    /// <inheritdoc/>
    public static GetDHCPLeaseInformationResult CreateErrorResult(string message) => new()
    {
        Status = OperationStatus.Error,
        Message = message,
        DHCPLease = null
    };
}
