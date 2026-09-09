namespace HostManagement.Shared.Communication.NamedPipe.Client;

/// <summary>
/// Represents a callback handler.
/// </summary>
public interface ICallbackHandler
{
    /// <summary>
    /// The unique topic to be handled.
    /// </summary>
    string Topic { get; }

    /// <summary>
    /// Handles an event received from the pipe server.
    /// </summary>
    /// <param name="messageContent">The event content.</param>
    /// <param name="correlationId">
    /// The correlation ID associated with the event.
    /// If the event was caused by a request, this can contain the request's conversation ID.
    /// <see cref="Guid.Empty"/> indicates that the event is not correlated to a request.
    /// </param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task HandleAsync(string messageContent, Guid correlationId, CancellationToken cancellationToken);
}
