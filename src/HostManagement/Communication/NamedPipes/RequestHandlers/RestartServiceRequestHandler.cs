using System.Text.Json;
using Microsoft.Extensions.Logging;
using HostManagement.Configuration;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Serialization;
using HostManagement.Services;

namespace HostManagement.Communication.NamedPipes.RequestHandlers;

/// <summary>
/// Represents the <see cref="RestartServiceRequestHandler"/>
/// </summary>
/// <param name="logger">An <see cref="ILogger"/> implementation.</param>
internal sealed class RestartServiceRequestHandler(ILogger logger) : IRequestHandler
{
    private readonly ILogger _logger = logger;

    /// <summary>
    /// The topic of the <see cref="RestartServiceRequestHandler"/>.
    /// </summary>
    public string Topic => Topics.RestartService;

    /// <summary>
    /// Handles the restart service request.
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

            var scriptResult = new Result();
            var scriptHandler = new ScriptHandler(scriptResult);

            var service = new SystemdUnitService(
                _logger,
                scriptHandler,
                scriptResult,
                EnvironmentVariables.ConfigurableServices);

            var result = await service.RestartSystemdUnit(messageContent);

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
