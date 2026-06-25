using System.Text.Json.Serialization.Metadata;
using HostManagement.Services;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Serialization;
using Microsoft.Extensions.Logging;

namespace HostManagement.Communication.NamedPipes.RequestHandlers;

/// <summary>
/// Represents the <see cref="ResetSystemRequestHandler"/>
/// </summary>
/// <param name="logger">An <see cref="ILogger"/> implementation.</param>
internal sealed class ResetSystemRequestHandler(ILogger logger) : RequestHandler<SystemControlResult>
{
    /// <summary>
    /// The topic of the <see cref="ResetSystemRequestHandler"/>.
    /// </summary>
    public override string Topic => Topics.ResetSystem;

    public override JsonTypeInfo<SystemControlResult> TypedJsonTypeInfo
        => SourceGenerationContext.Default.SystemControlResult;

    /// <summary>
    /// Handles the reset system request.
    /// </summary>
    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException"/>
    public override async Task<string> HandleAsync(string messageContent, CancellationToken cancellationToken = default)
    {
        var scriptResult = new Result();
        var scriptHandler = new ScriptHandler(scriptResult);
        var signatureValidator = new MinisignSignedDebianPackageSignatureValidator();
        var service = new SystemService(logger, scriptHandler, scriptResult, signatureValidator);

        var result = await service.ResetSystem();

        return SourceGeneratedTypeSerializer.Serialize(result, TypedJsonTypeInfo);
    }
}
