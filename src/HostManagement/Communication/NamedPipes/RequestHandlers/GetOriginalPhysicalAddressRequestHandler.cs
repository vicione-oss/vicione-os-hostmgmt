using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.Logging;
using HostManagement.Services;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Serialization;

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
    /// <param name="messageContent">The message content.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The response.</returns>
    /// <exception cref="InvalidOperationException"/>
    public override async Task<string> Handle(string messageContent, CancellationToken cancellationToken)
    {
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(messageContent, nameof(messageContent));

            var service = new PhysicalAddressService(logger);
            var result = await service.GetOriginalPhysicalAddress(messageContent, cancellationToken);

            return SourceGeneratedTypeSerializer.Serialize(result, TypedJsonTypeInfo);
        }
        catch (JsonException)
        {
            throw new InvalidOperationException(
                $"{nameof(GetOriginalPhysicalAddressResult)} could not be serialized into a string.");
        }
    }
}
