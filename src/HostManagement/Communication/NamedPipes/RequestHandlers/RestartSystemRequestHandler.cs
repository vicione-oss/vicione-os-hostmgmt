using System.Text.Json.Serialization.Metadata;
using HostManagement.Services;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Serialization;
using Microsoft.Extensions.Logging;

namespace HostManagement.Communication.NamedPipes.RequestHandlers;

/// <summary>
/// Represents the <see cref="RestartSystemRequestHandler"/>
/// </summary>
/// <param name="logger">An <see cref="ILogger"/> implementation.</param>
internal sealed class RestartSystemRequestHandler(ILogger logger) : RequestHandler<SystemControlResult>
{
    /// <summary>
    /// The topic of the <see cref="RestartSystemRequestHandler"/>.
    /// </summary>
    public override string Topic => Topics.RestartSystem;

    public override JsonTypeInfo<SystemControlResult> TypedJsonTypeInfo
        => SourceGenerationContext.Default.SystemControlResult;

    /// <summary>
    /// Handles the restart system request.
    /// </summary>
    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException"/>
    public override async Task<string> HandleAsync(string messageContent, CancellationToken cancellationToken = default)
    {
        var scriptResult = new Result();
        var scriptHandler = new ScriptHandler(scriptResult);
        var signatureValidator = new MinisignSignedDebianPackageSignatureValidator();
        var service = new SystemService(logger, scriptHandler, scriptResult, signatureValidator);

        var result = await service.RestartSystem(messageContent).ConfigureAwait(false);

        return SourceGeneratedTypeSerializer.Serialize(result, TypedJsonTypeInfo);
    }
}
