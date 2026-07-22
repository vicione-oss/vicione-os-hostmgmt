using System.Text.Json.Serialization.Metadata;
using HostManagement.Configuration;
using HostManagement.Services;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Serialization;
using Microsoft.Extensions.Logging;

namespace HostManagement.Communication.NamedPipes.RequestHandlers;

/// <summary>
/// Represents the <see cref="GetServiceStateRequestHandler"/>.
/// </summary>
internal sealed class GetServiceStateRequestHandler(ILogger logger) : RequestHandler<GetServiceStateResult>
{
    /// <summary>
    /// The topic of the <see cref="GetServiceStateRequestHandler"/>.
    /// </summary>
    public override string Topic => Topics.GetServiceState;

    public override JsonTypeInfo<GetServiceStateResult> TypedJsonTypeInfo
        => SourceGenerationContext.Default.GetServiceStateResult;

    /// <summary>
    /// Handles the get service state request.
    /// </summary>
    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException"/>
    public override async Task<string> HandleAsync(string messageContent, CancellationToken cancellationToken)
    {
        var scriptHandler = new ScriptHandler();
        var service = new SystemdUnitService(logger, scriptHandler, EnvironmentVariables.ConfigurableServices.Value);

        var result = await service.GetSystemdUnitActiveState(messageContent, cancellationToken).ConfigureAwait(false);

        return SourceGeneratedTypeSerializer.Serialize(result, TypedJsonTypeInfo);
    }
}
