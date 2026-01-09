using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.Logging;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Serialization;
using HostManagement.Services;

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
    /// <param name="messageContent">The message content.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The response.</returns>
    /// <exception cref="InvalidOperationException"/>
    public override async Task<string> Handle(string messageContent, CancellationToken cancellationToken = default)
    {
        try
        {
            var scriptResult = new Result();
            var scriptHandler = new ScriptHandler(scriptResult);
            var service = new SystemService(logger, scriptHandler, scriptResult);
            var result = await service.RestartSystem();

            return SourceGeneratedTypeSerializer.Serialize(result, TypedJsonTypeInfo);
        }
        catch (JsonException)
        {
            throw new InvalidOperationException(
                $"{nameof(SystemControlResult)} could not be serialized into a string.");
        }
    }
}
