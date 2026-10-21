using HostManagement.Services;
using HostManagement.Shared;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Serialization;

namespace HostManagement.Communication.NamedPipe.RequestHandlers;

/// <summary>
/// Represents the <see cref="GetSupportedCapabilitiesRequestHandler"/>.
/// </summary>
/// <param name="capabilitiesService">The capabilities service.</param>
internal sealed class GetSupportedCapabilitiesRequestHandler(SupportedCapabilitiesService capabilitiesService)
    : RequestHandler<GetSupportedCapabilitiesResult>
{
    /// <summary>
    /// The topic of the <see cref="GetSupportedCapabilitiesRequestHandler"/>.
    /// </summary>
    public override string Topic => Topics.GetSupportedCapabilities;

    /// <summary>
    /// Handles the get supported capabilities request.
    /// </summary>
    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException"/>
    public override async Task<string> HandleAsync(
        string messageContent,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        var result = await capabilitiesService.GetSupportedCapabilities(cancellationToken);
        return MessageSerializer.Serialize(result);
    }
}
