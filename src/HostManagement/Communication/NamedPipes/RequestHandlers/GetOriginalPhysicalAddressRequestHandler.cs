using System.Text.Json.Serialization.Metadata;
using HostManagement.Services;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Serialization;
using Microsoft.Extensions.Logging;

namespace HostManagement.Communication.NamedPipes.RequestHandlers;

internal sealed class GetOriginalPhysicalAddressRequestHandler(ILogger logger)
    : RequestHandler<GetOriginalPhysicalAddressResult>
{
    /// <summary>
    /// The topic of the <see cref="GetOriginalPhysicalAddressRequestHandler"/>.
    /// </summary>
    public override string Topic => Topics.GetOriginalPhysicalAddress;

    public override JsonTypeInfo<GetOriginalPhysicalAddressResult> TypedJsonTypeInfo
        => SourceGenerationContext.Default.GetOriginalPhysicalAddressResult;

    /// <summary>
    /// Handles the get original physical address request.
    /// </summary>
    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException"/>
    public override async Task<string> HandleAsync(string messageContent, CancellationToken cancellationToken)
    {
        var service = new PhysicalAddressService(logger);
        var result = await service.GetOriginalPhysicalAddress(messageContent, cancellationToken);

        return SourceGeneratedTypeSerializer.Serialize(result, TypedJsonTypeInfo);
    }
}
