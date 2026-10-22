using HostManagement.Shared;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Enums;
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
        var (result, appliedConfiguration) = await configurationService
            .SetConfigurationFromJson(messageContent, cancellationToken)
            .ConfigureAwait(false);

        var serializedResult = MessageSerializer.Serialize(result);

        if (result.Status != OperationStatus.Error && appliedConfiguration is not null)
        {
            var serializedGetResult = MessageSerializer.Serialize(
                GetSystemConfigurationResult.CreateSuccessResult("System configuration changed", appliedConfiguration));

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
