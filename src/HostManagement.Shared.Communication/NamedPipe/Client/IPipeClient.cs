using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Enums;

namespace HostManagement.Shared.Communication.NamedPipe.Client;

/// <summary>
/// Represents a named pipe client.
/// </summary>
public interface IPipeClient : IAsyncDisposable
{
    /// <summary>
    /// The state of the pipe connection.
    /// </summary>
    PipeState State { get; }

    /// <summary>
    /// Sends a request with the specified <paramref name="topic"/> and <paramref name="content"/>.
    /// </summary>
    /// <param name="topic">
    /// The topic of the request used to identify a request handler (see <see cref="Topics"/>).
    /// </param>
    /// <param name="content">The JSON serialized message content.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The response message.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the client is not connected and connecting fails, when a duplicate conversation identifier is
    /// detected, or when the serialized request payload exceeds the configured pipe message size limit.
    /// </exception>
    /// <exception cref="IOException">
    /// Thrown when the pipe faults while awaiting the response,
    /// or when sending the request fails due to an I/O error.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when the client or the underlying connection has been disposed.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown when the operation is canceled via <paramref name="cancellationToken"/>,
    /// or when the pipe is closed while awaiting the response.
    /// </exception>
    Task<NamedPipeMessage> SendRequestAsync(string topic, string content = "", CancellationToken cancellationToken = default);

    /// <summary>
    /// Tries to connect to a pipe.
    /// </summary>
    /// <param name="timeoutMs">
    /// The number of milliseconds to wait for the server to respond before the connection times out.
    /// The value must be non-negative or <see cref="Timeout.Infinite"/>.
    /// </param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><see langword="true"/> if the connection was established, otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="timeoutMs"/> is negative and not <see cref="Timeout.Infinite"/>.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when the client has been disposed.
    /// </exception>
    Task<bool> TryConnectAsync(int timeoutMs = Timeout.Infinite, CancellationToken cancellationToken = default);
}
