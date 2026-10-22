using HostManagement.Services;
using HostManagement.Shared;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Serialization;
using Microsoft.Extensions.Logging;

namespace HostManagement.Communication.NamedPipe.RequestHandlers;

/// <summary>
/// Represents the <see cref="UpdateSystemRequestHandler"/>
/// </summary>
/// <param name="logger">An <see cref="ILogger"/> implementation.</param>
internal sealed class UpdateSystemRequestHandler(ILogger logger) : RequestHandler<SystemControlResult>
{
    /// <summary>
    /// The topic of the <see cref="UpdateSystemRequestHandler"/>.
    /// </summary>
    public override string Topic => Topics.UpdateSystem;

    /// <summary>
    /// Handles the update system request.
    /// </summary>
    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException"/>
    public override async Task<string> HandleAsync(
        string messageContent,
        Guid correlationId,
        CancellationToken cancellationToken = default)
    {
        var scriptHandler = new ScriptHandler();
        var signatureValidator = new MinisignSignedDebianPackageSignatureValidator();
        var service = new SystemService(logger, scriptHandler, signatureValidator);

        var result = await service.UpdateSystem(messageContent).ConfigureAwait(false);

        return MessageSerializer.Serialize(result);
    }
}
