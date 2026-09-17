using System.Net.NetworkInformation;
using System.Text.Json.Serialization;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Contracts.Network;

namespace HostManagement.Shared.Communication.Contracts;

/// <summary>
/// Represents the result of getting the original physical address (initial factory setting).
/// </summary>
public record GetOriginalPhysicalAddressResult
    : OperationResult, IOperationResultFactory<GetOriginalPhysicalAddressResult>
{
    /// <summary>
    /// Initializes a new <see cref="GetOriginalPhysicalAddressResult"/>.
    /// </summary>
    public GetOriginalPhysicalAddressResult()
        : base(nameof(GetOriginalPhysicalAddressResult))
    {
    }

    /// <summary>
    /// The original physical address (initial factory setting).
    /// </summary>
    [JsonConverter(typeof(PhysicalAddressConverter))]
    public required PhysicalAddress? OriginalPhysicalAddress { get; init; }

    /// <inheritdoc/>
    protected override string ExpectedType => nameof(GetOriginalPhysicalAddressResult);

    /// <inheritdoc/>
    public static GetOriginalPhysicalAddressResult CreateErrorResult(string message) => new()
    {
        Status = OperationStatus.Error,
        Message = message,
        OriginalPhysicalAddress = null
    };
}
