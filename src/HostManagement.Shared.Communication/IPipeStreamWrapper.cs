using System.IO.Pipes;
using HostManagement.Shared.Communication.Contracts;

namespace HostManagement.Shared.Communication;

/// <summary>
/// Represents a wrapper around a <see cref="PipeStream"/> that provides messaging capabilities.
/// </summary>
public interface IPipeStreamWrapper : IDisposable
{
    /// <summary>
    /// Reads the message off the input stream.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The read message type and payload.</returns>
    Task<NamedPipeMessage> ReadMessageAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a message.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task SendMessageAsync(NamedPipeMessage message, CancellationToken cancellationToken = default);
}
