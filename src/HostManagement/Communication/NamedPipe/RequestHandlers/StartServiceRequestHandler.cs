using System.Text.Json.Serialization.Metadata;
using HostManagement.Configuration;
using HostManagement.Services;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Serialization;
using Microsoft.Extensions.Logging;

namespace HostManagement.Communication.NamedPipe.RequestHandlers;

/// <summary>
/// Represents the <see cref="StartServiceRequestHandler"/>
/// </summary>
/// <param name="logger">An <see cref="ILogger"/> implementation.</param>
internal sealed class StartServiceRequestHandler(ILogger logger) : RequestHandler<ServiceControlResult>
{
    /// <summary>
    /// The topic of the <see cref="StartServiceRequestHandler"/>.
    /// </summary>
    public override string Topic => Topics.StartService;

    public override JsonTypeInfo<ServiceControlResult> TypedJsonTypeInfo
        => SourceGenerationContext.Default.ServiceControlResult;

    /// <summary>
    /// Handles the start service request.
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

        var result = await service.StartSystemdUnit(messageContent).ConfigureAwait(false);

        return SourceGeneratedTypeSerializer.Serialize(result, TypedJsonTypeInfo);
    }
}
