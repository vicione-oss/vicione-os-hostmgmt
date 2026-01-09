using System.IO.Pipes;
using System.Text.Json;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Extensions;
using Microsoft.Extensions.Logging;

namespace HostManagement.Shared.Communication;

/// <summary>
/// Wraps the raw pipe stream with messaging and request/response capability.
/// </summary>
public sealed class PipeStreamWrapper(PipeStream stream, ILogger logger) : IDisposable
{
    // Prevents more than one thread from writing to the pipe stream at once
    private readonly SemaphoreSlim _writeLock = new(1, 1);

    /// <summary>
    /// Logs that the pipe has closed and throws exception to trigger graceful closure.
    /// </summary>
    private void ClosePipe()
    {
        const string Message = "Pipe has closed.";
        logger.LogInformation(Message);

        // OperationCanceledException is handled as pipe closing gracefully.
        throw new OperationCanceledException(Message);
    }

    /// <summary>
    /// Releases all resources used by the current instance of the <see cref="PipeStreamWrapper"/> class.
    /// </summary>
    public void Dispose() => _writeLock.Dispose();

    /// <summary>
    /// Reads the message off the input stream.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The read message type and payload.</returns>
    public async Task<NamedPipeMessage> ReadMessage(CancellationToken cancellationToken)
    {
        // Read the length to see how long the message is
        var messagePayloadLength = await stream.ReadVarInt(cancellationToken).ConfigureAwait(false);
        if (messagePayloadLength == 0)
            ClosePipe();

        var messagePayloadBytes = new byte[messagePayloadLength];

        var payloadBytesRead = 0;
        while (payloadBytesRead < messagePayloadLength)
        {
            var readBytes = await stream.ReadAsync(messagePayloadBytes.AsMemory(payloadBytesRead, messagePayloadLength - payloadBytesRead), cancellationToken).ConfigureAwait(false);
            if (readBytes == 0)
                ClosePipe();

            payloadBytesRead += readBytes;
        }

        // We've read in the whole message. Now parse it out into a message object.
        using var messageStream = new MemoryStream(messagePayloadBytes);
        var namedPipeMessage = await JsonSerializer.DeserializeAsync(messageStream, SourceGenerationContext.Default.NamedPipeMessage, cancellationToken).ConfigureAwait(false)
                               ?? throw new InvalidOperationException("Could not read message from Stream");

        logger.LogInformation("Message with topic '{Topic}' and ConversationID '{ConversationId}' received.", namedPipeMessage.Topic, namedPipeMessage.ConversationId);
        logger.LogDebug("Message content was: {Content}", namedPipeMessage.Content);

        return namedPipeMessage;
    }

    /// <summary>
    /// Sends a message.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    public async Task SendMessage(NamedPipeMessage message, CancellationToken cancellationToken)
    {
        using var messageStream = new MemoryStream(128);

        await JsonSerializer.SerializeAsync(messageStream, message, SourceGenerationContext.Default.NamedPipeMessage, cancellationToken)
            .ConfigureAwait(false);

        var messageBytes = messageStream.ToArray();

        var messageLengthBytes = Utilities.GetVarInt(messageBytes.Length);

        await _writeLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            // Write out the message length
            await stream.WriteAsync(messageLengthBytes, cancellationToken).ConfigureAwait(false);

            // Write the message payload
            await stream.WriteAsync(messageBytes, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _writeLock.Release();
        }

        logger.LogInformation("Message with topic '{Topic}' and ConversationID '{ConversationId}' sent.", message.Topic, message.ConversationId);
        logger.LogDebug("Message content was: {Content}", message.Content);
    }
}
