using HostManagement.Shared.Communication.Contracts;
using Microsoft.Extensions.Logging;

namespace HostManagement.Communication.NamedPipes.Client;

/// <summary>
/// Represents an <see cref="EventCallbackRegistry"/>.
/// </summary>
/// <param name="logger">A logger.</param>
internal sealed partial class EventCallbackRegistry(ILogger logger)
{
    private readonly Dictionary<string, ICallbackHandler> _handlers = [];

    /// <summary>
    /// Handles a named pipe message event.
    /// </summary>
    /// <param name="message">The named pipe message.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    public async Task Handle(NamedPipeMessage message, CancellationToken cancellationToken)
    {
        if (!_handlers.TryGetValue(message.Topic, out var handler))
        {
            NoHandlerRegisteredForTopic(logger, message.Topic);
            return;
        }

        await handler.Handle(message.Content, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Registers an <see cref="ICallbackHandler"/>.
    /// </summary>
    /// <param name="handler">The handler.</param>
    /// <exception cref="InvalidOperationException"></exception>
    public void RegisterHandler(ICallbackHandler handler)
    {
        if (!_handlers.TryAdd(handler.Topic, handler))
            throw new InvalidOperationException($"A handler for topic '{handler.Topic}' is already registered.");
    }

    [LoggerMessage(LogLevel.Information, "No handler registered for topic {MessageTopic}.")]
    private static partial void NoHandlerRegisteredForTopic(ILogger logger, string messageTopic);
}
