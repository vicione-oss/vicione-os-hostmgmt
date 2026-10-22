using HostManagement.Shared;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Serialization;
using HostManagement.SystemConfig;

namespace HostManagement.Communication.NamedPipe.RequestHandlers;

/// <summary>
/// Represents the <see cref="ResetNetworkInterfaceRequestHandler"/>.
/// </summary>
/// <param name="configurationService">The system configuration service.</param>
internal sealed class ResetNetworkInterfaceRequestHandler(SystemConfigurationService configurationService)
    : RequestHandler<SystemControlResult>, INotifyPublishEventRequested
{
    /// <inheritdoc/>
    public event EventHandler<PublishEventRequestedEventArgs>? PublishEventRequested;

    /// <summary>
    /// The topic of the <see cref="ResetNetworkInterfaceRequestHandler"/>.
    /// </summary>
    public override string Topic => Topics.ResetNetworkInterface;

    /// <summary>
    /// Handles the reset network interface request.
    /// </summary>
    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException"/>
    public override async Task<string> HandleAsync(
        string messageContent,
        Guid correlationId,
        CancellationToken cancellationToken = default)
    {
        var result = await configurationService
            .ResetNetworkInterface(messageContent, cancellationToken)
            .ConfigureAwait(false);

        var serializedResult = MessageSerializer.Serialize(result);

        if (result.Status != OperationStatus.Error)
        {
            var getResult = await configurationService.GetConfiguration(cancellationToken).ConfigureAwait(false);

            if (getResult.Status != OperationStatus.Error)
            {
                var serializedGetResult = MessageSerializer.Serialize(getResult);

                PublishEventRequested?.Invoke(
                    this,
                    new PublishEventRequestedEventArgs(
                        EventTopics.SystemConfigurationChanged,
                        serializedGetResult,
                        correlationId,
                        ExcludeRequestingClient: true));
            }
        }

        return serializedResult;
    }
}
