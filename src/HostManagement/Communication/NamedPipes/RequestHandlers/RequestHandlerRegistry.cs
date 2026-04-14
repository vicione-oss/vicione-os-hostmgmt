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
    private static readonly List<string> s_validTopics = [.. Topics.GetAllTopics()];
    private static readonly List<string> s_topicsWithRequiredContent = [.. Topics.GetTopicsWithRequiredContent()];

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
        var topic = message.Topic;
        var content = message.Content;

        if (topic is null)
            return message.CreateDefaultResponse("Error processing request. Message topic was null.");

        if (IsTopicInvalid(topic))
            return message.CreateDefaultResponse($"Error processing request. Topic '{topic}' is invalid.");

        if (!_handlers.TryGetValue(topic, out var handler))
            return message.CreateDefaultResponse($"No handler registered for topic '{topic}'.");

        try
        {
            if (!IsHandlerSupported(handler))
            {
                return message.CreateResponse(
                    JsonSerializer.Serialize(
                        handler.CreateErrorInstance($"Handler with topic '{topic}' not supported on this platform."),
                        handler.JsonTypeInfo));
            }

            if (IsRequiredContentMissing(topic, content))
            {
                return message.CreateResponse(
                    JsonSerializer.Serialize(
                        handler.CreateErrorInstance($"Handler with topic '{topic}' requires a message content."),
                        handler.JsonTypeInfo));
            }

            var response = await handler.HandleAsync(content, cancellationToken).ConfigureAwait(false);

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

    private static bool IsRequiredContentMissing(string topic, string content)
        => s_topicsWithRequiredContent.Contains(topic) && string.IsNullOrWhiteSpace(content);

    private static bool IsTopicInvalid(string topic) => !s_validTopics.Contains(topic);

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
