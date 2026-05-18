using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using HostManagement.Configuration;
using HostManagement.Services;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Serialization;
using Microsoft.Extensions.Logging;

namespace HostManagement.Communication.NamedPipes.RequestHandlers;

/// <summary>
/// Represents the <see cref="StopServiceRequestHandler"/>
/// </summary>
/// <param name="logger">An <see cref="ILogger"/> implementation.</param>
internal sealed class StopServiceRequestHandler(ILogger logger) : RequestHandler<ServiceControlResult>
{
    /// <summary>
    /// The topic of the <see cref="StopServiceRequestHandler"/>.
    /// </summary>
    public override string Topic => Topics.StopService;

    public override JsonTypeInfo<ServiceControlResult> TypedJsonTypeInfo
        => SourceGenerationContext.Default.ServiceControlResult;

    /// <summary>
    /// Handles the stop service request.
    /// </summary>
    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException"/>
    public override async Task<string> HandleAsync(string messageContent, CancellationToken cancellationToken = default)
    {
        try
        {
            var scriptResult = new Result();
            var scriptHandler = new ScriptHandler(scriptResult);

            var service = new SystemdUnitService(
                logger,
                scriptHandler,
                scriptResult,
                EnvironmentVariables.ConfigurableServices.Value);

            var result = await service.StopSystemdUnit(messageContent);

            return SourceGeneratedTypeSerializer.Serialize(result, TypedJsonTypeInfo);
        }
        catch (JsonException)
        {
            throw new InvalidOperationException(
                $"{nameof(ServiceControlResult)} could not be serialized into a string.");
        }
    }
}
