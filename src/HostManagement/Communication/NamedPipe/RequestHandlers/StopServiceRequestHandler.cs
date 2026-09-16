using System.Text.Json.Serialization.Metadata;
using HostManagement.Configuration;
using HostManagement.Services;
using HostManagement.Shared;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Serialization;
using Microsoft.Extensions.Logging;

namespace HostManagement.Communication.NamedPipe.RequestHandlers;

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
    public override async Task<string> HandleAsync(
        string messageContent,
        Guid correlationId,
        CancellationToken cancellationToken = default)
    {
        var scriptHandler = new ScriptHandler();
        var service = new SystemdUnitService(logger, scriptHandler, EnvironmentVariables.ConfigurableServices.Value);

        var result = await service.StopSystemdUnit(messageContent).ConfigureAwait(false);

        return SourceGeneratedTypeSerializer.Serialize(result, TypedJsonTypeInfo);
    }
}
