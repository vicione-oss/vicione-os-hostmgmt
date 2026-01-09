using System.Text.Json;
using Microsoft.Extensions.Logging;
using HostManagement.Services;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Serialization;

namespace HostManagement.Communication.NamedPipes.RequestHandlers;

internal sealed class GetOriginalPhysicalAddressRequestHandler(ILogger logger) : IRequestHandler
{
    /// <summary>
    /// The topic of the <see cref="GetOriginalPhysicalAddressRequestHandler"/>.
    /// </summary>
    public string Topic => Topics.GetOriginalPhysicalAddress;

    /// <summary>
    /// Handles the get original physical address request.
    /// </summary>
    /// <param name="messageContent">The message content.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The response.</returns>
    /// <exception cref="InvalidOperationException"/>
    public async Task<string> Handle(string messageContent, CancellationToken cancellationToken)
    {
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(messageContent, nameof(messageContent));

            var service = new PhysicalAddressService(logger);
            var result = await service.GetOriginalPhysicalAddress(messageContent, cancellationToken);

            return SourceGeneratedTypeSerializer
                .Serialize(result, SourceGenerationContext.Default.GetOriginalPhysicalAddressResult);
        }
        catch (JsonException)
        {
            throw new InvalidOperationException(
                $"{nameof(GetOriginalPhysicalAddressResult)} could not be serialized into a string.");
        }
    }
}
