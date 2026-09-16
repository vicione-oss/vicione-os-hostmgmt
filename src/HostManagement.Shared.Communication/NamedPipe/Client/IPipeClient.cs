using System.Text.Json;
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
    /// <param name="correlationId">
    /// The optional correlation ID to use as the request's conversation ID.
    /// When omitted, a new ID is generated. A provided value must not be <see cref="Guid.Empty"/>.
    /// </param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The response message.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="correlationId"/> is <see cref="Guid.Empty"/>.
    /// </exception>
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
    Task<NamedPipeMessage> SendRequestAsync(
        string topic,
        string content = "",
        Guid? correlationId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a request using a strongly typed request descriptor and deserializes the response.
    /// </summary>
    /// <typeparam name="TResponse">The response type.</typeparam>
    /// <param name="request">The request descriptor containing the topic and source-generated response metadata.</param>
    /// <param name="content">The JSON serialized request content.</param>
    /// <param name="correlationId">
    /// The optional correlation ID to use as the request's conversation ID.
    /// When omitted, a new ID is generated. A provided value must not be <see cref="Guid.Empty"/>.
    /// </param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The deserialized response content.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is null.</exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="correlationId"/> is <see cref="Guid.Empty"/>.
    /// </exception>
    /// <exception cref="JsonException">Thrown when the response cannot be deserialized.</exception>
    Task<TResponse> SendRequestAsync<TResponse>(
        PipeRequest<TResponse> request,
        string content = "",
        Guid? correlationId = null,
        CancellationToken cancellationToken = default);

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
