using System.Text.Json;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Serialization;
using Microsoft.Extensions.Logging;
using HostManagement.System;

namespace HostManagement.Communication.NamedPipes.RequestHandlers;

/// <summary>
/// Represents the <see cref="UpdateSystemRequestHandler"/>
/// </summary>
/// <param name="logger">An <see cref="ILogger"/> implementation.</param>
internal sealed class UpdateSystemRequestHandler(ILogger logger) : IRequestHandler
{
    private readonly ILogger _logger = logger;

    /// <summary>
    /// The topic of the <see cref="UpdateSystemRequestHandler"/>.
    /// </summary>
    public string Topic => Topics.UpdateSystem;

    /// <summary>
    /// Handles the update system request.
    /// </summary>
    /// <param name="messageContent">The message content.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The response.</returns>
    /// <exception cref="InvalidOperationException"/>
    public async Task<string> Handle(string messageContent, CancellationToken cancellationToken = default)
    {
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(messageContent, nameof(messageContent));

            var manager = new UpdateSystemManager(_logger);
            var result = await manager.UpdateSystem(messageContent);

            return SourceGeneratedTypeSerializer
                .Serialize(result, SourceGenerationContext.Default.UpdateSystemResult);
        }
        catch (JsonException)
        {
            throw new InvalidOperationException(
                $"{nameof(UpdateSystemResult)} could not be serialized into a string.");
        }
    }
}
