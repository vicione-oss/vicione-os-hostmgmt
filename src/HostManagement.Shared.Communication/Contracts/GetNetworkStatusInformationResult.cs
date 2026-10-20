using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Contracts.NetworkStatus;

namespace HostManagement.Shared.Communication.Contracts;

/// <summary>
/// Represents the result of getting network status information.
/// </summary>
public record GetNetworkStatusInformationResult
    : OperationResult, IOperationResultFactory<GetNetworkStatusInformationResult>
{
    /// <summary>
    /// Initializes a new <see cref="GetNetworkStatusInformationResult"/>.
    /// </summary>
    public GetNetworkStatusInformationResult()
        : base(nameof(GetNetworkStatusInformationResult))
    {
    }

    /// <summary>
    /// The network status information, e.g. whether default gateway or internet are available.
    /// </summary>
    public required NetworkStatusInformation NetworkStatusInformation { get; init; }

    /// <inheritdoc/>
    protected override string ExpectedType => nameof(GetNetworkStatusInformationResult);

    /// <inheritdoc/>
    public static GetNetworkStatusInformationResult CreateErrorResult(string message) => new()
    {
        Status = OperationStatus.Error,
        Message = message,
        NetworkStatusInformation = NetworkStatusInformation.Empty
    };
}
