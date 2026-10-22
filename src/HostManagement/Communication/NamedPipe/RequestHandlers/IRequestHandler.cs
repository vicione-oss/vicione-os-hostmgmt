namespace HostManagement.Communication.NamedPipe.RequestHandlers;

/// <summary>
/// Represents a request handler.
/// </summary>
internal interface IRequestHandler
{
    /// <summary>
    /// The unique message topic to be handled.
    /// </summary>
    string Topic { get; }

    /// <summary>
    /// Creates the response content reporting that the request could not be handled.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <returns>
    /// The serialized error result, or the raw error message for topics that exchange raw text.
    /// </returns>
    string CreateErrorResponseContent(string message);

    /// <summary>
    /// The method handling a message.
    /// </summary>
    /// <param name="messageContent">The message content (can be JSON serialized).</param>
    /// <param name="correlationId">
    /// The conversation ID of the request being handled.
    /// Event messages caused by this request can use this value as their correlation ID.
    /// </param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The result to be sent as a response to the caller.</returns>
    Task<string> HandleAsync(string messageContent, Guid correlationId, CancellationToken cancellationToken);
}
