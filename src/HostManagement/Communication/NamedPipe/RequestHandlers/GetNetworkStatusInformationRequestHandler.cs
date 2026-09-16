using System.Text.Json.Serialization.Metadata;
using HostManagement.Services;
using HostManagement.Shared;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Serialization;
using Microsoft.Extensions.Logging;

namespace HostManagement.Communication.NamedPipe.RequestHandlers;

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
    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException"/>
    public override async Task<string> HandleAsync(
        string messageContent,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        var scriptHandler = new ScriptHandler();
        var service = new NetworkStatusService(logger, scriptHandler);
        var result = await service.GetNetworkStatusInformation(messageContent, cancellationToken).ConfigureAwait(false);

        return SourceGeneratedTypeSerializer.Serialize(result, TypedJsonTypeInfo);
    }
}
