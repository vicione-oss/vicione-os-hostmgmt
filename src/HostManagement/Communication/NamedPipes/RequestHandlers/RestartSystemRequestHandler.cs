using System.Text.Json;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Serialization;
using Microsoft.Extensions.Logging;
using HostManagement.System;

namespace HostManagement.Communication.NamedPipes.RequestHandlers;

/// <summary>
/// Represents the <see cref="RestartSystemRequestHandler"/>
/// </summary>
/// <param name="logger">An <see cref="ILogger"/> implementation.</param>
internal sealed class RestartSystemRequestHandler(ILogger logger) : IRequestHandler
{
    private readonly ILogger _logger = logger;

    /// <summary>
    /// The topic of the <see cref="RestartSystemRequestHandler"/>.
    /// </summary>
    public string Topic => Topics.RestartSystem;

    /// <summary>
    /// Handles the restart system request.
    /// </summary>
    /// <param name="messageContent">The message content.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The response.</returns>
    /// <exception cref="InvalidOperationException"/>
    public async Task<string> Handle(string messageContent, CancellationToken cancellationToken = default)
    {
        try
        {
            var manager = new RestartSystemManager(_logger);
            var result = await manager.RestartSystem();

            return SourceGeneratedTypeSerializer
                .Serialize(result, SourceGenerationContext.Default.RestartSystemResult);
        }
        catch (JsonException)
        {
            throw new InvalidOperationException(
                $"{nameof(RestartSystemResult)} could not be serialized into a string.");
        }
    }
}
