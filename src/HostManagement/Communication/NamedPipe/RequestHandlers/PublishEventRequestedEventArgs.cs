namespace HostManagement.Communication.NamedPipe.RequestHandlers;

/// <summary>
/// Represents the arguments for the <see cref="INotifyPublishEventRequested.PublishEventRequested"/> event.
/// </summary>
/// <param name="Topic">The topic used to identify a message callback handler.</param>
/// <param name="Content">The message content (can be JSON serialized).</param>
/// <param name="CorrelationId">
/// The correlation ID associated with the event.
/// Use the originating request's correlation ID when the event was caused by a request.
/// <see cref="Guid.Empty"/> indicates an uncorrelated event.
/// </param>
/// <param name="ExcludeRequestingClient">
/// If set to <see langword="true"/> excludes the client currently executing the request from receiving this event.
/// </param>
internal readonly record struct PublishEventRequestedEventArgs(
    string Topic,
    string Content,
    Guid CorrelationId = default,
    bool ExcludeRequestingClient = false);
