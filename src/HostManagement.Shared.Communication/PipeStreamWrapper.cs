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
    private readonly CancellationTokenSource _internalCts = new();

    private readonly PipeStream _stream = stream ?? throw new ArgumentNullException(nameof(stream));
    private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    private int _disposeCount;

    /// <summary>
    /// Disposes this <see cref="PipeStreamWrapper"/>.
    /// </summary>
    public void Dispose()
    {
        // Thread-safe disposal
        if (Interlocked.Exchange(ref _disposeCount, 1) == 1)
            return;

        // Stop send/read operations
        try { _internalCts.Cancel(); } catch { }

        try
        {
            // Block until an active writer has completed.
            // Waiting writers are canceled via _internalCts and must not enter afterwards.
            _writeLock.Wait();
            _stream.Dispose();
        }
        finally
        {
            try { _writeLock.Dispose(); } catch (ObjectDisposedException) { }
            try { _internalCts.Dispose(); } catch (ObjectDisposedException) { }
        }
    }

    /// <inheritdoc/>
    public async Task<NamedPipeMessage> ReadMessageAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _internalCts.Token);

        // Read the length to see how long the message is
        var messagePayloadLength = await _stream.ReadVarIntAsync(linkedCts.Token).ConfigureAwait(false);

        // 0 length reserved as end-of-stream;
        // Must be modified if zero-length messages shall be possible in the future
        if (messagePayloadLength == 0)
            ClosePipe();

        var messagePayloadBytes = new byte[messagePayloadLength];
        var payloadBytesRead = 0;

        while (payloadBytesRead < messagePayloadLength)
        {
            var readBytes = await _stream
                .ReadAsync(
                    messagePayloadBytes.AsMemory(payloadBytesRead, messagePayloadLength - payloadBytesRead),
                    linkedCts.Token)
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
                linkedCts.Token)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException("Could not read message from stream.");

        MessageWithTopicReceived(_logger, namedPipeMessage.Topic);
        MessageDetails(_logger, namedPipeMessage.ConversationId, namedPipeMessage.Content);

        return namedPipeMessage;
    }

    /// <inheritdoc/>
    public async Task SendMessageAsync(NamedPipeMessage message, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _internalCts.Token);

        // MemoryStream won't need to resize it's array and reallocate memory
        // for most outgoing messages (~ 240 - 1000 bytes)
        using var messageStream = new MemoryStream(512);

        await JsonSerializer
            .SerializeAsync(
                messageStream,
                message,
                SourceGenerationContext.Default.NamedPipeMessage,
                linkedCts.Token)
            .ConfigureAwait(false);

        var payloadLength = (int)messageStream.Length;
        var messageLengthBytes = Utilities.GetVarInt(payloadLength);
        var writeLockTaken = false;

        try
        {
            await _writeLock.WaitAsync(linkedCts.Token).ConfigureAwait(false);
            writeLockTaken = true;

            ThrowIfDisposed();

            // Write out the message length
            await _stream.WriteAsync(messageLengthBytes, linkedCts.Token).ConfigureAwait(false);

            // Write the message payload
            var buffer = messageStream.GetBuffer();
            await _stream.WriteAsync(buffer.AsMemory(0, payloadLength), linkedCts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Normal cancel (Dispose or external Token) > no error log
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

            throw;
        }
        catch (Exception ex)
        {
            SendPipeMessageFailed(_logger, ex);
            throw;
        }
        finally
        {
            if (writeLockTaken)
            {
                try { _writeLock.Release(); }
                catch (SemaphoreFullException) { }
                catch (ObjectDisposedException) { }
            }
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

    private void ThrowIfDisposed()
    {
#pragma warning disable CA1513 // We must check _disposeCount threadsafe
        if (Volatile.Read(ref _disposeCount) != 0)
            throw new ObjectDisposedException(nameof(PipeStreamWrapper));
#pragma warning restore CA1513
    }

    [LoggerMessage(LogLevel.Debug, "ConversationId: {ConversationId}; Content: {Content}")]
    private static partial void MessageDetails(ILogger logger, Guid conversationId, string content);

    [LoggerMessage(LogLevel.Information, "Message with topic '{Topic}' received.")]
    private static partial void MessageWithTopicReceived(ILogger logger, string topic);

    [LoggerMessage(LogLevel.Information, "Message with topic '{Topic}' sent.")]
    private static partial void MessageWithTopicSent(ILogger logger, string topic);

    [LoggerMessage(LogLevel.Debug, "Pipe closed before write could complete.")]
    private static partial void PipeClosedBeforeWriteCouldComplete(ILogger logger);

    [LoggerMessage(LogLevel.Debug, "Pipe has closed.")]
    private static partial void PipeHasClosed(ILogger logger);

    [LoggerMessage(LogLevel.Error, "Sending named pipe message failed.")]
    private static partial void SendPipeMessageFailed(ILogger logger, Exception exception);
}
