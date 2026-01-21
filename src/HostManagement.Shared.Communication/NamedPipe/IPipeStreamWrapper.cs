using System.IO.Pipes;
using System.Text.Json;
using HostManagement.Shared.Communication.Contracts;

namespace HostManagement.Shared.Communication.NamedPipe;

/// <summary>
/// Represents a wrapper around a <see cref="PipeStream"/> that provides messaging capabilities.
/// </summary>
public interface IPipeStreamWrapper : IDisposable
{
    /// <summary>
    /// Gets a value indicating whether the pipe is connected.
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Gets the configured maximum message size in bytes.
    /// </summary>
    int MaxMessageBytes { get; }

    /// <summary>
    /// Reads a message from the input stream.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The message read from the stream.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the incoming payload length exceeds <see cref="MaxMessageBytes"/> or when the message payload
    /// could not be read from stream.
    /// </exception>
    /// <exception cref="JsonException">Thrown when JSON deserialization of the payload fails.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the pipe stream wrapper has been disposed.</exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown when the operation is canceled via <paramref name="cancellationToken"/>, or when the pipe has closed.
    /// A payload length of <c>0</c> is currently treated as end-of-stream.
    /// </exception>
    Task<NamedPipeMessage> ReadMessageAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a message to the output stream.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <remarks>
    /// Implementations must serialize concurrent send operations so message framing is not interleaved.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the serialized outgoing payload exceeds <see cref="MaxMessageBytes"/>.
    /// The size check is performed after JSON serialization and before writing to the pipe.
    /// </exception>
    /// <exception cref="IOException">
    /// Thrown when sending the message fails due to an I/O error.
    /// </exception>
    /// <exception cref="ObjectDisposedException">Thrown when the pipe stream wrapper has been disposed.</exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown when the operation is canceled via <paramref name="cancellationToken"/>,
    /// or when the pipe is closed while sending the message.
    /// </exception>
    Task SendMessageAsync(NamedPipeMessage message, CancellationToken cancellationToken = default);
}
