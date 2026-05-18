using System.IO.Abstractions;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Serialization;
using HostManagement.SystemConfig;
using Microsoft.Extensions.Logging;

namespace HostManagement.Communication.NamedPipes.RequestHandlers;

/// <summary>
/// Represents the <see cref="ResetNetworkInterfaceRequestHandler"/>.
/// </summary>
internal sealed class ResetNetworkInterfaceRequestHandler(ILogger logger) : RequestHandler<SystemControlResult>
{
    /// <summary>
    /// The topic of the <see cref="ResetNetworkInterfaceRequestHandler"/>.
    /// </summary>
    public override string Topic => Topics.ResetNetworkInterface;

    public override JsonTypeInfo<SystemControlResult> TypedJsonTypeInfo
        => SourceGenerationContext.Default.SystemControlResult;

    /// <summary>
    /// Handles the reset network interface request.
    /// </summary>
    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException"/>
    public override async Task<string> HandleAsync(string messageContent, CancellationToken cancellationToken = default)
    {
        try
        {
            var fileSystem = new FileSystem();
            var configurationService = new SystemConfigurationService(
                fileSystem,
                new SystemConfigurationPersistence(fileSystem),
                new ScriptHandler(),
                logger);

            var result = await configurationService
                .ResetNetworkInterface(messageContent, cancellationToken)
                .ConfigureAwait(false);

            return SourceGeneratedTypeSerializer.Serialize(result, TypedJsonTypeInfo);
        }
        catch (JsonException)
        {
            throw new InvalidOperationException(
                $"{nameof(SystemControlResult)} could not be serialized into a string.");
        }
    }
}
