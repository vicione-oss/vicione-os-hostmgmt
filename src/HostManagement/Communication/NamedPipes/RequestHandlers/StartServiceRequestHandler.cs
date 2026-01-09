using System.Text.Json;
using Microsoft.Extensions.Logging;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Serialization;
using HostManagement.Service;

namespace HostManagement.Communication.NamedPipes.RequestHandlers;

/// <summary>
/// Represents the <see cref="StartServiceRequestHandler"/>
/// </summary>
/// <param name="logger">An <see cref="ILogger"/> implementation.</param>
internal sealed class StartServiceRequestHandler(ILogger logger) : IRequestHandler
{
    private readonly ILogger _logger = logger;

    /// <summary>
    /// The topic of the <see cref="StartServiceRequestHandler"/>.
    /// </summary>
    public string Topic => Topics.StartService;

    /// <summary>
    /// Handles the start service request.
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

            var manager = new ServiceControlManager(_logger);
            var result = await manager.StartService(messageContent);

            return SourceGeneratedTypeSerializer
                .Serialize(result, SourceGenerationContext.Default.ServiceControlResult);
        }
        catch (JsonException)
        {
            throw new InvalidOperationException(
                $"{nameof(ServiceControlResult)} could not be serialized into a string.");
        }
    }
}
