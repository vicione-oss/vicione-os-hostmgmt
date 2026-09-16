namespace HostManagement.Communication.NamedPipe.Server;

/// <summary>
/// Represents a named pipe server.
/// </summary>
internal interface IPipeServer : IAsyncDisposable
{
    /// <summary>
    /// The path of the pipe.
    /// </summary>
    string PipePath { get; }

    /// <summary>
    /// The current state of the pipe server.
    /// </summary>
    ServerState State { get; }

    /// <summary>
    /// Publishes an event with the specified <paramref name="topic"/> and <paramref name="content"/>.
    /// </summary>
    /// <param name="topic">The topic of the message used to identify a message callback handler.</param>
    /// <param name="content">The message content (can be JSON serialized).</param>
    /// <param name="correlationId">
    /// The correlation ID associated with the event.
    /// Defaults to <see cref="Guid.Empty"/> for events that are not correlated to a request.
    /// </param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task PublishEvent(string topic, string content, Guid correlationId = default, CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts the pipe server.
    /// </summary>
    Task StartAsync();

    /// <summary>
    /// Stops the pipe server.
    /// </summary>
    Task StopAsync();

    /// <summary>
    /// Waits for the pipe server to exit.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task WaitForExitAsync(CancellationToken cancellationToken = default);
}
