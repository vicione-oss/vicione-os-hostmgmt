using System.Text.Json.Serialization.Metadata;
using HostManagement.Shared;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Communication.Serialization;
using HostManagement.Shared.Serialization;
using HostManagement.SystemConfig;

namespace HostManagement.Communication.NamedPipe.RequestHandlers;

/// <summary>
/// Represents the <see cref="SetSystemConfigurationRequestHandler"/>
/// </summary>
/// <param name="configurationService">The system configuration service.</param>
internal sealed class SetSystemConfigurationRequestHandler(SystemConfigurationService configurationService)
    : RequestHandler<SetSystemConfigurationResult>, INotifyPublishEventRequested
{
    /// <inheritdoc/>
    public event EventHandler<PublishEventRequestedEventArgs>? PublishEventRequested;

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
    public override async Task<string> HandleAsync(
        string messageContent,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        var configToApply = SystemConfigurationSerializer.Deserialize(messageContent);

        var result = await configurationService
            .SetConfiguration(configToApply, shouldPersistConfiguration: true, cancellationToken)
            .ConfigureAwait(false);

        var serializedResult = SourceGeneratedTypeSerializer.Serialize(result, TypedJsonTypeInfo);

        if (result.Status != OperationStatus.Error)
        {
            var serializedGetResult = SourceGeneratedTypeSerializer.Serialize(
                GetSystemConfigurationResult.CreateSuccessResult("System configuration changed", configToApply),
                SourceGenerationContext.Default.GetSystemConfigurationResult);

            PublishEventRequested?.Invoke(
                this,
                new PublishEventRequestedEventArgs(
                    EventTopics.SystemConfigurationChanged,
                    serializedGetResult,
                    correlationId,
                    ExcludeRequestingClient: true));
        }

        return serializedResult;
    }
}
