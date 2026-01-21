using System.IO.Pipes;
using System.Text.Json;
using HostManagement.Shared.Communication.Contracts;

namespace HostManagement.Shared.Communication;

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
    /// Reads a message from the input stream.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The message read from the stream.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the message could not be read from stream.</exception>
    /// <exception cref="JsonException">Thrown when the JSON deserialization of the message fails.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the pipe stream wrapper has been disposed.</exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown when the operation is canceled via <paramref name="cancellationToken"/>, or when the pipe has closed.
    /// </exception>
    Task<NamedPipeMessage> ReadMessageAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a message to the output stream.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
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
