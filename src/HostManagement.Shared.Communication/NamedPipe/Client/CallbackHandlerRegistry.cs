using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Serialization;

namespace HostManagement.Shared.Communication.NamedPipe.Client;

/// <summary>
/// Represents a <see cref="CallbackHandlerRegistry"/>.
/// </summary>
public sealed class CallbackHandlerRegistry
{
    private readonly Dictionary<string, ICallbackHandler> _handlers = [];

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

        await handler.HandleRawAsync(message.Content, message.ConversationId, cancellationToken).ConfigureAwait(false);
        return true;
    }

    /// <summary>
    /// Registers an <see cref="ICallbackHandler"/>.
    /// </summary>
    /// <param name="handler">The handler.</param>
    /// <returns>
    /// <see langword="true"/> if the handler was registered,
    /// <see langword="false"/> if a handler for the same topic already exists.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="handler"/> is <see langword="null"/>.
    /// </exception>
    public bool RegisterHandler(ICallbackHandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        return _handlers.TryAdd(handler.Topic, handler);
    }

    /// <summary>
    /// Registers a callback for <paramref name="topic"/> that receives the deserialized event message.
    /// </summary>
    /// <typeparam name="TMessage">
    /// The message contract type. Its JSON metadata is resolved through the <see cref="MessageTypeRegistry"/>.
    /// Use <see cref="RegisterRawHandler"/> instead to receive the raw message content.
    /// </typeparam>
    /// <param name="topic">The unique topic to be handled.</param>
    /// <param name="handleAsync">
    /// The callback receiving the deserialized message, the correlation ID and a cancellation token.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the callback was registered,
    /// <see langword="false"/> if a handler for the same topic already exists.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="topic"/> or <paramref name="handleAsync"/> is <see langword="null"/>.
    /// </exception>
    public bool RegisterHandler<TMessage>(
        string topic,
        Func<TMessage, Guid, CancellationToken, Task> handleAsync)
    {
        ArgumentNullException.ThrowIfNull(topic);
        ArgumentNullException.ThrowIfNull(handleAsync);

        return RegisterHandler(new DelegateCallbackHandler<TMessage>(topic, handleAsync));
    }

    /// <summary>
    /// Registers a callback for <paramref name="topic"/> that receives the raw message content.
    /// </summary>
    /// <param name="topic">The unique topic to be handled.</param>
    /// <param name="handleRawAsync">
    /// The callback receiving the raw message content, the correlation ID and a cancellation token.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the callback was registered,
    /// <see langword="false"/> if a handler for the same topic already exists.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="topic"/> or <paramref name="handleRawAsync"/> is <see langword="null"/>.
    /// </exception>
    public bool RegisterRawHandler(string topic, Func<string, Guid, CancellationToken, Task> handleRawAsync)
    {
        ArgumentNullException.ThrowIfNull(topic);
        ArgumentNullException.ThrowIfNull(handleRawAsync);

        return RegisterHandler(new DelegateRawCallbackHandler(topic, handleRawAsync));
    }

    private sealed class DelegateCallbackHandler<TMessage>(
        string topic,
        Func<TMessage, Guid, CancellationToken, Task> handleAsync)
        : ICallbackHandler<TMessage>
    {
        public string Topic => topic;

        public Task HandleAsync(TMessage message, Guid correlationId, CancellationToken cancellationToken)
            => handleAsync(message, correlationId, cancellationToken);
    }

    private sealed class DelegateRawCallbackHandler(
        string topic,
        Func<string, Guid, CancellationToken, Task> handleRawAsync)
        : ICallbackHandler
    {
        public string Topic => topic;

        public Task HandleRawAsync(string messageContent, Guid correlationId, CancellationToken cancellationToken)
            => handleRawAsync(messageContent, correlationId, cancellationToken);
    }
}
