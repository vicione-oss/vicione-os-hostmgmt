using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.Logging;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Serialization;
using HostManagement.Services;

namespace HostManagement.Communication.NamedPipes.RequestHandlers;

/// <summary>
/// Represents the <see cref="GetNTPFallbackInformationRequestHandler"/>.
/// </summary>
internal sealed class GetNTPFallbackInformationRequestHandler(ILogger logger)
    : RequestHandler<GetNTPFallbackInformationResult>
{
    /// <summary>
    /// The topic of the <see cref="GetNTPFallbackInformationRequestHandler"/>.
    /// </summary>
    public override string Topic => Topics.GetNTPFallbackInformation;

    public override JsonTypeInfo<GetNTPFallbackInformationResult> TypedJsonTypeInfo
        => SourceGenerationContext.Default.GetNTPFallbackInformationResult;

    /// <summary>
    /// Handles the get NTPFallback information request.
    /// </summary>
    /// <param name="messageContent">The message content.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The response.</returns>
    /// <exception cref="InvalidOperationException"/>
    public override async Task<string> Handle(string messageContent, CancellationToken cancellationToken)
    {
        try
        {
            var service = new NTPService(logger);
            var result = await service.GetNTPFallbackInformation(cancellationToken);

            return SourceGeneratedTypeSerializer.Serialize(result, TypedJsonTypeInfo);
        }
        catch (JsonException)
        {
            throw new InvalidOperationException(
                $"{nameof(GetNTPFallbackInformationResult)} could not be serialized into a string.");
        }
    }
}
