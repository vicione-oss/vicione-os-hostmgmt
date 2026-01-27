using System.IO.Abstractions;
using System.Text.Json;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Serialization;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Serialization;
using HostManagement.SystemConfig;
using CommunicationJsonContext = HostManagement.Shared.Communication.Contracts.SourceGenerationContext;

namespace HostManagement.Communication.NamedPipes.RequestHandlers;

/// <summary>
/// Represents the <see cref="SetSystemConfigurationRequestHandler"/>
/// </summary>
internal sealed class SetSystemConfigurationRequestHandler : IRequestHandler
{
    /// <summary>
    /// The topic of the <see cref="SetSystemConfigurationRequestHandler"/>.
    /// </summary>
    public string Topic => Topics.SetSystemConfiguration;

    /// <summary>
    /// Handles the set system configuration request.
    /// </summary>
    /// <param name="messageContent">The message content.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The response.</returns>
    /// <exception cref="InvalidOperationException"/>
    public async Task<string> Handle(string messageContent, CancellationToken cancellationToken)
    {
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(messageContent, nameof(messageContent));

            var configToApply = SystemConfigurationSerializer.Deserialize(messageContent);
            var fileSystem = new FileSystem();
            var configurationService = new SystemConfigurationService(fileSystem, new SystemConfigurationPersistence(fileSystem), new ScriptHandler());

            var result = await configurationService
                .SetConfiguration(configToApply, shouldPersistConfiguration: true, cancellationToken);

            return SourceGeneratedTypeSerializer
                .Serialize(result, CommunicationJsonContext.Default.SetSystemConfigurationResult);
        }
        catch (JsonException)
        {
            throw new InvalidOperationException(
                $"Message content could not be deserialized to {nameof(SystemConfiguration)}" +
                Environment.NewLine +
                messageContent);
        }
    }
}
