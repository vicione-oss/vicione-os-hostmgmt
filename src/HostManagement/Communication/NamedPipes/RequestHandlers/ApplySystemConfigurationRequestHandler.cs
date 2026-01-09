using System.Text.Json;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Contracts;
using HostManagement.SystemConfig;
using SourceGenerationContext = HostManagement.Shared.Contracts.SourceGenerationContext;
using CommunicationJsonContext = HostManagement.Shared.Communication.Contracts.SourceGenerationContext;
using HostManagement.Shared.Communication.Serialization;

namespace HostManagement.Communication.NamedPipes.RequestHandlers;

/// <summary>
/// Represents the <see cref="ApplySystemConfigurationRequestHandler"/>
/// </summary>
internal sealed class ApplySystemConfigurationRequestHandler : IRequestHandler
{
    /// <summary>
    /// The topic of the <see cref="ApplySystemConfigurationRequestHandler"/>.
    /// </summary>
    public string Topic => Topics.ApplySystemConfiguration;

    /// <summary>
    /// Handles the apply system configuration request.
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

            var configToApply = JsonSerializer
                .Deserialize(
                    messageContent,
                    SourceGenerationContext.Default.SystemConfiguration)
                ?? throw new InvalidOperationException(
                    $"Message content could not be deserialized to {nameof(SystemConfiguration)}" +
                    Environment.NewLine +
                    messageContent);

            var manager = new SystemConfigurationManager();
            var result = await manager.ApplyConfiguration(configToApply, cancellationToken);

            return SourceGeneratedTypeSerializer
                .Serialize(result, CommunicationJsonContext.Default.ApplySystemConfigurationResult);
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
