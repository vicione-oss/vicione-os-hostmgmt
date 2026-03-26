using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using HostManagement.Services;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Serialization;
using Microsoft.Extensions.Logging;

namespace HostManagement.Communication.NamedPipes.RequestHandlers;

/// <summary>
/// Represents the <see cref="GetNetworkStatusInformationRequestHandler"/>.
/// </summary>
/// <param name="logger">An <see cref="ILogger"/> implementation.</param>
internal sealed class GetNetworkStatusInformationRequestHandler(ILogger logger)
    : RequestHandler<GetNetworkStatusInformationResult>
{
    /// <summary>
    /// The topic of the <see cref="GetNetworkStatusInformationRequestHandler"/>.
    /// </summary>
    public override string Topic => Topics.GetNetworkStatusInformation;

    public override JsonTypeInfo<GetNetworkStatusInformationResult> TypedJsonTypeInfo
        => SourceGenerationContext.Default.GetNetworkStatusInformationResult;

    /// <summary>
    /// Handles the get network status information request.
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

            var scriptResult = new Result();
            var scriptHandler = new ScriptHandler(scriptResult);
            var service = new NetworkStatusService(logger, scriptHandler, scriptResult);
            var result = await service.GetNetworkStatusInformation(messageContent, cancellationToken);

            return SourceGeneratedTypeSerializer.Serialize(result, TypedJsonTypeInfo);
        }
        catch (JsonException)
        {
            throw new InvalidOperationException(
                $"{nameof(GetNetworkStatusInformationResult)} could not be serialized into a string.");
        }
    }
}
