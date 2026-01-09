using System.Text.Json;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Serialization;
using Microsoft.Extensions.Logging;
using HostManagement.Services;

namespace HostManagement.Communication.NamedPipes.RequestHandlers;

/// <summary>
/// Represents the <see cref="ShutdownSystemRequestHandler"/>
/// </summary>
/// <param name="logger">An <see cref="ILogger"/> implementation.</param>
internal sealed class ShutdownSystemRequestHandler(ILogger logger) : IRequestHandler
{
    private readonly ILogger _logger = logger;

    /// <summary>
    /// The topic of the <see cref="ShutdownSystemRequestHandler"/>.
    /// </summary>
    public string Topic => Topics.ShutdownSystem;

    /// <summary>
    /// Handles the shutdown system request.
    /// </summary>
    /// <param name="messageContent">The message content.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The response.</returns>
    /// <exception cref="InvalidOperationException"/>
    public async Task<string> Handle(string messageContent, CancellationToken cancellationToken = default)
    {
        try
        {
            var scriptResult = new Result();
            var scriptHandler = new ScriptHandler(scriptResult);
            var service = new SystemService(_logger, scriptHandler, scriptResult);
            var result = await service.ShutdownSystem();

            return SourceGeneratedTypeSerializer
                .Serialize(result, SourceGenerationContext.Default.SystemControlResult);
        }
        catch (JsonException)
        {
            throw new InvalidOperationException(
                $"{nameof(SystemControlResult)} could not be serialized into a string.");
        }
    }
}
