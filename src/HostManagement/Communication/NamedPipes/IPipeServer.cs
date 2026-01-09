using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Enums;

namespace HostManagement.Communication.NamedPipes;

/// <summary>
/// Represents a pipe server.
/// </summary>
internal interface IPipeServer : IDisposable
{
    /// <summary>
    /// The state of the pipe connection.
    /// </summary>
    PipeState State { get; }

    /// <summary>
    /// Publishes an event with the specified <paramref name="topic"/> and <paramref name="content"/>.
    /// </summary>
    /// <param name="topic">The topic of the message used to identify a message handler 
    /// (see <see cref="Topics"/>).</param>
    /// <param name="content">The JSON serialized message content.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task PublishEvent(string topic, string content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Waits for a client to connect to the pipe.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task WaitForConnection(CancellationToken cancellationToken = default);

    /// <summary>
    /// Waits for the remote end of the pipe to close.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns></returns>
    Task WaitForRemotePipeClose(CancellationToken cancellationToken = default);
}
