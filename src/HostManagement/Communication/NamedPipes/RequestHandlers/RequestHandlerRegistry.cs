using System.Text.Json;
using HostManagement.Communication.NamedPipes.Extensions;
using HostManagement.Configuration;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Contracts;

namespace HostManagement.Communication.NamedPipes.RequestHandlers;

/// <summary>
/// Represents an <see cref="RequestHandlerRegistry"/>.
/// </summary>
/// <param name="targetEnvironment">The target environment.</param>
internal sealed class RequestHandlerRegistry(TargetEnvironment targetEnvironment)
{
    private readonly Dictionary<string, IRequestHandler> _handlers = [];
    private readonly TargetEnvironment _targetEnvironment = targetEnvironment;

    /// <summary>
    /// Handles a named pipe message request.
    /// </summary>
    /// <param name="message">The named pipe message.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see cref="NamedPipeMessage"/></returns>
    public async Task<NamedPipeMessage> HandleRequestAsync(
        NamedPipeMessage message,
        CancellationToken cancellationToken)
    {
        if (!_handlers.TryGetValue(message.Topic, out var handler))
            return message.CreateDefaultResponse($"No handler registered for topic '{message.Topic}'.");

        try
        {
            string response;

            if (IsHandlerSupported(handler))
            {
                response = await handler.HandleAsync(message.Content, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                response = JsonSerializer.Serialize(
                    handler.CreateErrorInstance($"Handler with topic '{message.Topic}' not supported on this platform."),
                    handler.JsonTypeInfo);
            }

            return message.CreateResponse(response);
        }
        catch (ArgumentException ex) when (ex.Message.Contains("messageContent"))
        {
            var response = JsonSerializer.Serialize(
                handler.CreateErrorInstance("Error processing request. Message content was missing or empty."),
                handler.JsonTypeInfo);

            return message.CreateResponse(response);
        }
        catch (Exception ex)
        {
            return message.CreateDefaultResponse($"Error processing request: {ex.Message}");
        }
    }

    /// <summary>
    /// Registers an <see cref="IRequestHandler"/>.
    /// </summary>
    /// <param name="handler">The handler.</param>
    /// <exception cref="InvalidOperationException"></exception>
    public void RegisterHandler(IRequestHandler handler)
    {
        if (!_handlers.TryAdd(handler.Topic, handler))
            throw new InvalidOperationException($"A handler for topic '{handler.Topic}' is already registered.");
    }

    private bool IsHandlerSupported(IRequestHandler handler)
    {
        var topic = handler.Topic;

#if DEBUG
        // Testing purposes
        if (topic is Topics.Echo)
            return true;
#endif

        return _targetEnvironment switch
        {
            TargetEnvironment.Debian => topic switch
            {
                Topics.RedeployScripts => true,

                Topics.GetServiceState => true,
                Topics.StartService => true,
                Topics.StopService => true,
                Topics.RestartService => true,

                Topics.InstallSignedDebianPackage => true,

                _ => false
            },
            TargetEnvironment.EdgeSViciOne => topic switch
            {
                Topics.RedeployScripts => true,

                Topics.GetServiceState => true,
                Topics.StartService => true,
                Topics.StopService => true,
                Topics.RestartService => true,

                Topics.GetSystemConfiguration => true,
                Topics.SetSystemConfiguration => true,

                Topics.GetOriginalPhysicalAddress => true,
                Topics.GetNetworkStatusInformation => true,
                Topics.GetDHCPLeaseInformation => true,
                Topics.GetNTPFallbackInformation => true,

                Topics.InstallSignedDebianPackage => true,
                Topics.ResetSystem => true,
                Topics.RestartSystem => true,
                Topics.ShutdownSystem => true,
                Topics.UpdateSystem => true,

                Topics.RenewDHCPLease => true,
                Topics.ResetNetworkInterface => true,

                _ => false
            },
            TargetEnvironment.Unknown => false,
            _ => false
        };
    }
}
