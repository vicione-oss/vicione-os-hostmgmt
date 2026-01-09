namespace PipeCommunicationSample;

/// <summary>
/// Represents a callback handler.
/// </summary>
internal interface ICallbackHandler
{
    /// <summary>
    /// The unique topic to be handled.
    /// </summary>
    string Topic { get; }

    /// <summary>
    /// The method handling a message.
    /// </summary>
    /// <param name="messageContent">The JSON serialized message content.</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns></returns>
    Task Handle(string messageContent, CancellationToken cancellationToken);
}
