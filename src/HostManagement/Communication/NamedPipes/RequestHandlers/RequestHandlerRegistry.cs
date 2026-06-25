using System.Collections.Concurrent;
using System.Text.Json;
using HostManagement.Communication.NamedPipes.Extensions;
using HostManagement.Configuration;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Contracts;
using Microsoft.Extensions.Logging;

namespace HostManagement.Communication.NamedPipes.RequestHandlers;

/// <summary>
/// Represents an <see cref="RequestHandlerRegistry"/>.
/// </summary>
internal sealed partial class RequestHandlerRegistry
{
    private static readonly List<string> s_validTopics = [.. Topics.GetAllTopics()];
    private static readonly List<string> s_topicsWithRequiredContent = [.. Topics.GetTopicsWithRequiredContent()];

    private readonly ConcurrentDictionary<string, IRequestHandler> _handlers = [];
    private readonly TargetEnvironment _targetEnvironment;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new <see cref="RequestHandlerRegistry"/>.
    /// </summary>
    /// <param name="targetEnvironment">The target environment.</param>
    /// <param name="handlers">The request handlers to register.</param>
    /// <param name="logger">An <see cref="ILogger"/> implementation.</param>
    public RequestHandlerRegistry(
        TargetEnvironment targetEnvironment,
        IEnumerable<IRequestHandler> handlers,
        ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(handlers);
        ArgumentNullException.ThrowIfNull(logger);

        _targetEnvironment = targetEnvironment;
        _logger = logger;

        foreach (var handler in handlers)
        {
            if (!_handlers.TryAdd(handler.Topic, handler))
                RequestHandlerForTopicAlreadyRegistered(_logger, handler.Topic);
            else
                RequestHandlerForTopicRegistered(_logger, handler.Topic);
        }
    }

    /// <summary>
    /// The registered request handlers.
    /// </summary>
    public ICollection<IRequestHandler> Handlers => _handlers.Values;

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
        {
            var errorMessage = GetRequestFailedBaseMessage(message) + "Topic is null.";

            HandleRequestFailedWithMessage(_logger, errorMessage);
            return message.CreateDefaultResponse(errorMessage);
        }

        if (IsTopicInvalid(topic))
        {
            var errorMessage = GetRequestFailedBaseMessage(message) + "Topic is invalid.";

            HandleRequestFailedWithMessage(_logger, errorMessage);
            return message.CreateDefaultResponse(errorMessage);
        }

        if (!_handlers.TryGetValue(topic, out var handler))
        {
            var errorMessage = GetRequestFailedBaseMessage(message) + "No handler registered.";

            HandleRequestFailedWithMessage(_logger, errorMessage);
            return message.CreateDefaultResponse(errorMessage);
        }

        try
        {
            if (!IsHandlerSupported(handler))
            {
                var errorMessage = GetRequestFailedBaseMessage(message) + "Handler not supported on this platform.";

                HandleRequestFailedWithMessage(_logger, errorMessage);

                return message.CreateResponse(
                    JsonSerializer.Serialize(handler.CreateErrorInstance(errorMessage), handler.JsonTypeInfo));
            }

            if (IsRequiredContentMissing(topic, content))
            {
                var errorMessage = GetRequestFailedBaseMessage(message) + "Handler requires a message content.";

                HandleRequestFailedWithMessage(_logger, errorMessage);

                return message.CreateResponse(
                    JsonSerializer.Serialize(handler.CreateErrorInstance(errorMessage), handler.JsonTypeInfo));
            }

            var response = await handler.HandleAsync(content, cancellationToken).ConfigureAwait(false);

            return message.CreateResponse(response);
        }
        catch (Exception ex)
        {
            var errorMessage = GetRequestFailedBaseMessage(message) + ex.Message;

            HandleRequestFailedWithException(_logger, errorMessage, ex);
            return message.CreateDefaultResponse(errorMessage);
        }

        static string GetRequestFailedBaseMessage(NamedPipeMessage message)
            => $"Handle request for topic '{message.Topic}' failed (Conversation ID: {message.ConversationId}). ";
    }

    /// <summary>
    /// Registers an <see cref="IRequestHandler"/>.
    /// </summary>
    /// <param name="handler">The handler.</param>
    public void RegisterHandler(IRequestHandler handler)
    {
        if (!_handlers.TryAdd(handler.Topic, handler))
            RequestHandlerForTopicAlreadyRegistered(_logger, handler.Topic);
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

    [LoggerMessage(LogLevel.Error, "{ErrorMessage}")]
    private static partial void HandleRequestFailedWithMessage(ILogger logger, string errorMessage);

    [LoggerMessage(LogLevel.Error, "{ErrorMessage}")]
    private static partial void HandleRequestFailedWithException(ILogger logger, string errorMessage, Exception exception);

    [LoggerMessage(LogLevel.Error, "Request handler for topic '{Topic}' is already registered.")]
    private static partial void RequestHandlerForTopicAlreadyRegistered(ILogger logger, string topic);

    [LoggerMessage(LogLevel.Debug, "Request handler for topic '{Topic}' has been registered.")]
    private static partial void RequestHandlerForTopicRegistered(ILogger logger, string topic);
}
