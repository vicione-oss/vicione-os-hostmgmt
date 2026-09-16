using System.Text.Json;
using HostManagement.Shared.Communication.Contracts;

namespace HostManagement.Shared.Communication.NamedPipe.Client;

/// <summary>
/// Represents a registry of named pipe callback handlers.
/// </summary>
public sealed class CallbackHandlerRegistry
{
    private readonly Dictionary<string, Func<NamedPipeMessage, CancellationToken, Task>> _handlers = [];

    /// <summary>
    /// Raised when a message arrives with no registered handler.
    /// </summary>
    public event Action<NamedPipeMessage>? OnMissingHandler;

    /// <summary>
    /// Handles a named pipe event message callback.
    /// </summary>
    /// <param name="message">The named pipe message.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// Returns <see langword="true"/> if a handler existed and the callback was handled,
    /// otherwise <see langword="false"/>.
    /// </returns>
    public async Task<bool> HandleCallbackAsync(
        NamedPipeMessage message,
        CancellationToken cancellationToken = default)
    {
        if (!_handlers.TryGetValue(message.Topic, out var handler))
        {
            OnMissingHandler?.Invoke(message);
            return false;
        }

        await handler(message, cancellationToken).ConfigureAwait(false);
        return true;
    }

    /// <summary>
    /// Registers a callback handler that consumes raw message content.
    /// </summary>
    /// <param name="topic">The event topic handled by <paramref name="handler"/>.</param>
    /// <param name="handler">The callback handler.</param>
    /// <returns><see langword="true"/> if the handler was registered; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="topic"/> is empty or whitespace.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="topic"/> or <paramref name="handler"/> is null.</exception>
    public bool RegisterRawHandler(
        string topic,
        Func<string, Guid, CancellationToken, Task> handler)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(topic);
        ArgumentNullException.ThrowIfNull(handler);

        return _handlers.TryAdd(
            topic,
            (message, cancellationToken) => handler(message.Content, message.ConversationId, cancellationToken));
    }

    /// <summary>
    /// Registers a typed callback handler using the source-generated JSON metadata carried by
    /// <paramref name="pipeEvent"/>.
    /// </summary>
    /// <typeparam name="TMessage">The callback message type.</typeparam>
    /// <param name="pipeEvent">The event descriptor.</param>
    /// <param name="handler">The callback handler.</param>
    /// <returns><see langword="true"/> if the handler was registered; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="pipeEvent"/> or <paramref name="handler"/> is null.
    /// </exception>
    public bool RegisterHandler<TMessage>(PipeEvent<TMessage> pipeEvent, ICallbackHandler<TMessage> handler)
    {
        ArgumentNullException.ThrowIfNull(pipeEvent);
        ArgumentNullException.ThrowIfNull(handler);

        return _handlers.TryAdd(
            pipeEvent.Topic,
            (message, cancellationToken) =>
            {
                var typedMessage = JsonSerializer
                    .Deserialize(message.Content, pipeEvent.MessageTypeInfo)
                    ?? throw new JsonException($"Deserialization of topic '{message.Topic}' returned null.");

                return handler.HandleAsync(typedMessage, message.ConversationId, cancellationToken);
            });
    }
}
