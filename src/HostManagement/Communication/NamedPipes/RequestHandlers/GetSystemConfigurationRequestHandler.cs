using System.IO.Abstractions;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Serialization;
using HostManagement.SystemConfig;

namespace HostManagement.Communication.NamedPipes.RequestHandlers;

/// <summary>
/// Represents the <see cref="GetSystemConfigurationRequestHandler"/>.
/// </summary>
internal sealed class GetSystemConfigurationRequestHandler : RequestHandler<GetSystemConfigurationResult>
{
    /// <summary>
    /// The topic of the <see cref="GetSystemConfigurationRequestHandler"/>.
    /// </summary>
    public override string Topic => Topics.GetSystemConfiguration;

    public override JsonTypeInfo<GetSystemConfigurationResult> TypedJsonTypeInfo
        => SourceGenerationContext.Default.GetSystemConfigurationResult;

    /// <summary>
    /// Handles the get system configuration request.
    /// </summary>
    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException"/>
    public override async Task<string> HandleAsync(string messageContent, CancellationToken cancellationToken)
    {
        try
        {
            var fileSystem = new FileSystem();
            var configurationService = new SystemConfigurationService(fileSystem, new SystemConfigurationPersistence(fileSystem), new ScriptHandler());

            var result = await configurationService.GetConfiguration(cancellationToken);

            return SourceGeneratedTypeSerializer.Serialize(result, TypedJsonTypeInfo);
        }
        catch (JsonException)
        {
            throw new InvalidOperationException(
                $"{nameof(GetSystemConfigurationResult)} could not be serialized into a string.");
        }
    }
}
