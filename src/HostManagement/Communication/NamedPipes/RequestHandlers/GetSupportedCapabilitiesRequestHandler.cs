using System.Text.Json.Serialization.Metadata;
using HostManagement.Services;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Serialization;

namespace HostManagement.Communication.NamedPipes.RequestHandlers;

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

    public override JsonTypeInfo<GetSupportedCapabilitiesResult> TypedJsonTypeInfo
        => CapabilitySourceGenerationContext.Default.GetSupportedCapabilitiesResult;

    /// <summary>
    /// Handles the get supported capabilities request.
    /// </summary>
    /// <param name="messageContent">The message content.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The response.</returns>
    /// <exception cref="InvalidOperationException"/>
    public override async Task<string> HandleAsync(string messageContent, CancellationToken cancellationToken)
    {
        var result = await capabilitiesService.GetSupportedCapabilities(cancellationToken);
        return SourceGeneratedTypeSerializer.Serialize(result, TypedJsonTypeInfo);
    }
}
