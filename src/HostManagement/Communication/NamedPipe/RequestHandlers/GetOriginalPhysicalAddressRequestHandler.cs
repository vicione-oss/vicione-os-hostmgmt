using HostManagement.Services;
using HostManagement.Shared;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Serialization;
using Microsoft.Extensions.Logging;

namespace HostManagement.Communication.NamedPipe.RequestHandlers;

internal sealed class GetOriginalPhysicalAddressRequestHandler(ILogger logger)
    : RequestHandler<GetOriginalPhysicalAddressResult>
{
    /// <summary>
    /// The topic of the <see cref="GetOriginalPhysicalAddressRequestHandler"/>.
    /// </summary>
    public override string Topic => Topics.GetOriginalPhysicalAddress;

    /// <summary>
    /// Handles the get original physical address request.
    /// </summary>
    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException"/>
    public override async Task<string> HandleAsync(
        string messageContent,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        var service = new PhysicalAddressService(logger, new ScriptHandler());
        var result = await service.GetOriginalPhysicalAddress(messageContent, cancellationToken).ConfigureAwait(false);

        return MessageSerializer.Serialize(result);
    }
}
