using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.Logging;
using HostManagement.Configuration;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Serialization;
using HostManagement.Services;

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
    /// <param name="messageContent">The message content.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The response.</returns>
    /// <exception cref="InvalidOperationException"/>
    public override async Task<string> Handle(string messageContent, CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            ArgumentException.ThrowIfNullOrWhiteSpace(messageContent, nameof(messageContent));

            var scriptResult = new Result();
            var scriptHandler = new ScriptHandler(scriptResult);

            var service = new SystemdUnitService(
                logger,
                scriptHandler,
                scriptResult,
                EnvironmentVariables.ConfigurableServices);

            var result = await service.GetSystemdUnitActiveState(messageContent);

            return SourceGeneratedTypeSerializer.Serialize(result, TypedJsonTypeInfo);
        }
        catch (JsonException)
        {
            throw new InvalidOperationException(
                $"{nameof(GetServiceStateResult)} could not be serialized into a string.");
        }
    }
}
