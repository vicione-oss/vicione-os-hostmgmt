using System.IO.Pipes;
using System.Net.Sockets;
using System.Text.Json;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Extensions;
using Microsoft.Extensions.Logging;

namespace HostManagement.Shared.Communication;

/// <summary>
/// Wraps the raw pipe stream with messaging capability.
/// </summary>
/// <param name="stream">The raw pipe stream.</param>
/// <param name="logger">An <see cref="ILogger"/> implementation.</param>
public sealed partial class PipeStreamWrapper(PipeStream stream, ILogger logger) : IPipeStreamWrapper
{
    // Prevents more than one thread from writing to the pipe stream at once
    private readonly SemaphoreSlim _writeLock = new(1, 1);

    private readonly PipeStream _stream = stream ?? throw new ArgumentNullException(nameof(stream));
    private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Disposes this <see cref="PipeStreamWrapper"/>.
    /// </summary>
    public void Dispose()
    {
        _stream.Dispose();
        _writeLock.Dispose();
    }

    /// <inheritdoc/>
    public async Task<NamedPipeMessage> ReadMessageAsync(CancellationToken cancellationToken = default)
    {
        // Read the length to see how long the message is
        var messagePayloadLength = await _stream.ReadVarIntAsync(cancellationToken).ConfigureAwait(false);

        if (messagePayloadLength == 0)
            ClosePipe();

        var messagePayloadBytes = new byte[messagePayloadLength];
        var payloadBytesRead = 0;

        while (payloadBytesRead < messagePayloadLength)
        {
            var readBytes = await _stream
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

    /// <inheritdoc/>
    public async Task SendMessageAsync(NamedPipeMessage message, CancellationToken cancellationToken = default)
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
            await _stream.WriteAsync(messageLengthBytes, cancellationToken).ConfigureAwait(false);

            // Write the message payload
            await _stream.WriteAsync(messageBytes, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Normale cancel (Dispose or external Token) > no error log
            throw;
        }
        catch (IOException ex)
        {
            if (ex.InnerException is null)
            {
                if (ex.Message?.IndexOf("Pipe is broken", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    // System.IO.IOException: Pipe is broken.
                    PipeClosedBeforeWriteCouldComplete(_logger);
                }
                else
                {
                    SendPipeMessageFailed(_logger, ex);
                }
            }
            else if (ex.InnerException is SocketException socketException &&
                     socketException.NativeErrorCode == 32)
            {
                // System.IO.IOException: Broken pipe
                PipeClosedBeforeWriteCouldComplete(_logger);
            }
            else
            {
                SendPipeMessageFailed(_logger, ex);
            }

            return;
        }
        catch (Exception ex)
        {
            SendPipeMessageFailed(_logger, ex);
            return;
        }
        finally
        {
            _writeLock.Release();
        }

        MessageWithTopicSent(_logger, message.Topic);
        MessageDetails(_logger, message.ConversationId, message.Content);
    }

    private void ClosePipe()
    {
        PipeHasClosed(_logger);

        // OperationCanceledException is handled as pipe closing gracefully.
        throw new OperationCanceledException();
    }

    [LoggerMessage(LogLevel.Debug, "ConversationId: {ConversationId}; Content: {Content}")]
    private static partial void MessageDetails(ILogger logger, Guid conversationId, string content);

    [LoggerMessage(LogLevel.Information, "Message with topic '{Topic}' received.")]
    private static partial void MessageWithTopicReceived(ILogger logger, string topic);

    [LoggerMessage(LogLevel.Information, "Message with topic '{Topic}' sent.")]
    private static partial void MessageWithTopicSent(ILogger logger, string topic);

    [LoggerMessage(LogLevel.Information, "Pipe closed before write could complete.")]
    private static partial void PipeClosedBeforeWriteCouldComplete(ILogger logger);

    [LoggerMessage(LogLevel.Debug, "Pipe has closed.")]
    private static partial void PipeHasClosed(ILogger logger);

    [LoggerMessage(LogLevel.Error, "Sending named pipe message failed.")]
    private static partial void SendPipeMessageFailed(ILogger logger, Exception exception);
}
