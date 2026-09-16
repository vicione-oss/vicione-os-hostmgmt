namespace HostManagement.Shared.Communication.NamedPipe.Client;

/// <summary>
/// Represents a typed callback handler for a specific message type.
/// </summary>
/// <typeparam name="TMessage">The callback message type.</typeparam>
public interface ICallbackHandler<in TMessage>
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
}
