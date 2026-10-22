using HostManagement.Services;
using HostManagement.Shared;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Serialization;
using Microsoft.Extensions.Logging;

namespace HostManagement.Communication.NamedPipe.RequestHandlers;

/// <summary>
/// Represents the <see cref="ShutdownSystemRequestHandler"/>
/// </summary>
/// <param name="logger">An <see cref="ILogger"/> implementation.</param>
internal sealed class ShutdownSystemRequestHandler(ILogger logger) : RequestHandler<SystemControlResult>
{
    /// <summary>
    /// The topic of the <see cref="ShutdownSystemRequestHandler"/>.
    /// </summary>
    public override string Topic => Topics.ShutdownSystem;

    /// <summary>
    /// Handles the shutdown system request.
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

        var result = await service.ShutdownSystem(messageContent).ConfigureAwait(false);

        return MessageSerializer.Serialize(result);
    }
}
