using System.Text.Json.Serialization.Metadata;
using HostManagement.Configuration;
using HostManagement.Services;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Serialization;
using Microsoft.Extensions.Logging;

namespace HostManagement.Communication.NamedPipes.RequestHandlers;

/// <summary>
/// Represents the <see cref="RestartServiceRequestHandler"/>
/// </summary>
/// <param name="logger">An <see cref="ILogger"/> implementation.</param>
internal sealed class RestartServiceRequestHandler(ILogger logger) : RequestHandler<ServiceControlResult>
{
    /// <summary>
    /// The topic of the <see cref="RestartServiceRequestHandler"/>.
    /// </summary>
    public override string Topic => Topics.RestartService;

    public override JsonTypeInfo<ServiceControlResult> TypedJsonTypeInfo
        => SourceGenerationContext.Default.ServiceControlResult;

    /// <summary>
    /// Handles the restart service request.
    /// </summary>
    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException"/>
    public override async Task<string> HandleAsync(string messageContent, CancellationToken cancellationToken = default)
    {
        var scriptHandler = new ScriptHandler();
        var service = new SystemdUnitService(logger, scriptHandler, EnvironmentVariables.ConfigurableServices.Value);

        var result = await service.RestartSystemdUnit(messageContent).ConfigureAwait(false);

        return SourceGeneratedTypeSerializer.Serialize(result, TypedJsonTypeInfo);
    }
}
