namespace HostManagement.Communication.NamedPipes.RequestHandlers;

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
    /// The method handling a message.
    /// </summary>
    /// <param name="messageContent">The JSON serialized message content.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The result to be sent as a response to the caller.</returns>
    Task<string> Handle(string messageContent, CancellationToken cancellationToken);
}
