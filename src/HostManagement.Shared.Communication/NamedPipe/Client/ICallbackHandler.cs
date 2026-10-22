using HostManagement.Shared.Serialization;

namespace HostManagement.Shared.Communication.NamedPipe.Client;

/// <summary>
/// Represents a typed callback handler for a specific message type.
/// </summary>
/// <typeparam name="TMessage">
/// The message contract type. Its JSON metadata is resolved through the <see cref="MessageTypeRegistry"/>.
/// Use <see cref="string"/> to receive the raw message content.
/// </typeparam>
public interface ICallbackHandler<TMessage> : ICallbackHandler
{
    /// <summary>
    /// Handles the deserialized event message.
    /// </summary>
    /// <param name="message">The deserialized event message.</param>
    /// <param name="correlationId">
    /// The correlation ID associated with the event.
    /// If the event was caused by a request, this can contain the request's conversation ID.
    /// <see cref="Guid.Empty"/> indicates that the event is not correlated to a request.
    /// </param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task HandleAsync(TMessage message, Guid correlationId, CancellationToken cancellationToken);

#pragma warning disable CA1033 // Raw handling is intentionally exposed only through ICallbackHandler.
    /// <inheritdoc/>
    Task ICallbackHandler.HandleRawAsync(string messageContent, Guid correlationId, CancellationToken cancellationToken)
        => HandleAsync(MessageSerializer.Deserialize<TMessage>(messageContent), correlationId, cancellationToken);
#pragma warning restore CA1033
}

/// <summary>
/// Non-generic base for a callback handler. Used internally by <see cref="CallbackHandlerRegistry"/>.
/// </summary>
/// <remarks>
/// Implement this interface directly to handle the raw message content without any deserialization.
/// </remarks>
public interface ICallbackHandler
{
    /// <summary>
    /// The unique topic to be handled.
    /// </summary>
    string Topic { get; }

    /// <summary>
    /// Handles a raw message content string.
    /// </summary>
    /// <param name="messageContent">The event content.</param>
    /// <param name="correlationId">
    /// The correlation ID associated with the event.
    /// If the event was caused by a request, this can contain the request's conversation ID.
    /// <see cref="Guid.Empty"/> indicates that the event is not correlated to a request.
    /// </param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task HandleRawAsync(string messageContent, Guid correlationId, CancellationToken cancellationToken);
}
