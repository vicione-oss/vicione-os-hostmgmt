using HostManagement.Services;
using HostManagement.Shared;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Serialization;
using Microsoft.Extensions.Logging;

namespace HostManagement.Communication.NamedPipe.RequestHandlers;

/// <summary>
/// Represents the <see cref="GetNTPSyncStatusRequestHandler"/>.
/// </summary>
internal sealed class GetNTPSyncStatusRequestHandler(ILogger logger) : RequestHandler<GetNTPSyncStatusResult>
{
    /// <summary>
    /// The topic of the <see cref="GetNTPSyncStatusRequestHandler"/>.
    /// </summary>
    public override string Topic => Topics.GetNTPSyncStatus;

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
        var result = await service.GetNTPSyncStatus(cancellationToken).ConfigureAwait(false);

        return MessageSerializer.Serialize(result);
    }
}
