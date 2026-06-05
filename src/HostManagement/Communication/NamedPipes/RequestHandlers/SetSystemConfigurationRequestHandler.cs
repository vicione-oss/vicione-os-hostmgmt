using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Serialization;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Serialization;
using HostManagement.SystemConfig;

namespace HostManagement.Communication.NamedPipes.RequestHandlers;

/// <summary>
/// Represents the <see cref="SetSystemConfigurationRequestHandler"/>
/// </summary>
/// <param name="configurationService">The system configuration service.</param>
internal sealed class SetSystemConfigurationRequestHandler(SystemConfigurationService configurationService)
    : RequestHandler<SetSystemConfigurationResult>
{
    /// <summary>
    /// The topic of the <see cref="SetSystemConfigurationRequestHandler"/>.
    /// </summary>
    public override string Topic => Topics.SetSystemConfiguration;

    public override JsonTypeInfo<SetSystemConfigurationResult> TypedJsonTypeInfo
        => Shared.Communication.Contracts.SourceGenerationContext.Default.SetSystemConfigurationResult;

    /// <summary>
    /// Handles the set system configuration request.
    /// </summary>
    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException"/>
    public override async Task<string> HandleAsync(string messageContent, CancellationToken cancellationToken)
    {
        try
        {
            var configToApply = SystemConfigurationSerializer.Deserialize(messageContent);

            var result = await configurationService
                .SetConfiguration(configToApply, shouldPersistConfiguration: true, cancellationToken)
                .ConfigureAwait(false);

            return SourceGeneratedTypeSerializer.Serialize(result, TypedJsonTypeInfo);
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
