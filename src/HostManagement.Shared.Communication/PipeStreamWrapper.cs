using System.IO.Pipes;
using System.Text.Json;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Extensions;
using Microsoft.Extensions.Logging;

namespace HostManagement.Shared.Communication;

/// <summary>
/// Wraps the raw pipe stream with messaging and request/response capability.
/// </summary>
public sealed partial class PipeStreamWrapper(PipeStream stream, ILogger logger) : IDisposable
{
    private readonly ILogger _logger = logger;

    // Prevents more than one thread from writing to the pipe stream at once
    private readonly SemaphoreSlim _writeLock = new(1, 1);

    /// <summary>
    /// Logs that the pipe has closed and throws exception to trigger graceful closure.
    /// </summary>
    private void ClosePipe()
    {
        PipeHasClosed(_logger);

        // OperationCanceledException is handled as pipe closing gracefully.
        throw new OperationCanceledException();
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
            var readBytes = await stream
                .ReadAsync(
                    messagePayloadBytes.AsMemory(payloadBytesRead, messagePayloadLength - payloadBytesRead),
                    cancellationToken)
                .ConfigureAwait(false);

            if (readBytes == 0)
                ClosePipe();

            payloadBytesRead += readBytes;
        }

        // We've read in the whole message. Now parse it out into a message object.
        using var messageStream = new MemoryStream(messagePayloadBytes);

        var namedPipeMessage = await JsonSerializer
            .DeserializeAsync(
                messageStream,
                SourceGenerationContext.Default.NamedPipeMessage,
                cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException("Could not read message from Stream.");

        MessageWithTopicReceived(_logger, namedPipeMessage.Topic);
        MessageDetails(_logger, namedPipeMessage.ConversationId, namedPipeMessage.Content);

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

        await JsonSerializer
            .SerializeAsync(
                messageStream,
                message,
                SourceGenerationContext.Default.NamedPipeMessage,
                cancellationToken)
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
        catch (IOException ex) when (IsBrokenPipe(ex))
        {
            // the other end of the pipe has gone away
            PipeClosedBeforeWriteCouldComplete(_logger);
            return;
        }
        finally
        {
            _writeLock.Release();
        }

        MessageWithTopicSent(_logger, message.Topic);
        MessageDetails(_logger, message.ConversationId, message.Content);
    }

    private static bool IsBrokenPipe(IOException ex)
    {
        // On Linux, EPipe (errno 32) becomes HRESULT 0x80070020
        const int EPIPE_HRESULT = unchecked((int)0x80070020);

        if (ex.HResult == EPIPE_HRESULT)
            return true;

        // fallback
        return ex.Message?.IndexOf("Broken pipe", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    [LoggerMessage(LogLevel.Debug, "ConversationId: {ConversationId}; Content: {Content}")]
    private static partial void MessageDetails(ILogger logger, Guid conversationId, string content);

    [LoggerMessage(LogLevel.Information, "Message with topic '{Topic}' received.")]
    private static partial void MessageWithTopicReceived(ILogger logger, string topic);

    [LoggerMessage(LogLevel.Information, "Message with topic '{Topic}' sent.")]
    private static partial void MessageWithTopicSent(ILogger logger, string topic);

    [LoggerMessage(LogLevel.Information, "Pipe closed before write could complete.")]
    private static partial void PipeClosedBeforeWriteCouldComplete(ILogger logger);

    [LoggerMessage(LogLevel.Information, "Pipe has closed.")]
    private static partial void PipeHasClosed(ILogger logger);
}
