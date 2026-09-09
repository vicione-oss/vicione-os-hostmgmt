using System.Text.Json.Serialization.Metadata;
using HostManagement.Services;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Serialization;
using Microsoft.Extensions.Logging;

namespace HostManagement.Communication.NamedPipe.RequestHandlers;

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
    /// Handles the get NTP fallback information request.
    /// </summary>
    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException"/>
    public override async Task<string> HandleAsync(
        string messageContent,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        var service = new NTPService(logger, new ScriptHandler());
        var result = await service.GetNTPFallbackInformation(cancellationToken).ConfigureAwait(false);

        return SourceGeneratedTypeSerializer.Serialize(result, TypedJsonTypeInfo);
    }
}
