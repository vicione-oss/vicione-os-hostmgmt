using System.IO.Pipes;
using System.Net.Sockets;
using System.Text.Json;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Extensions;
using Microsoft.Extensions.Logging;

namespace HostManagement.Shared.Communication.NamedPipe;

/// <inheritdoc cref="IPipeStreamWrapper"/>
/// <remarks>
/// Messages are transferred as a varint-encoded payload length, followed by the serialized JSON payload.
/// <paramref name="maxMessageBytes"/> applies only to the serialized JSON payload.
/// <list type="bullet">
/// <item>
/// If the payload length is greater than <paramref name="maxMessageBytes"/>, inbound messages are rejected before the
/// payload buffer is allocated and outbound messages are rejected after serialization and before writing to the pipe.
/// </item>
/// <item>
/// A payload length of <c>0</c> is currently reserved to indicate end-of-stream. Empty payload messages are therefore
/// not part of the protocol at the moment.
/// </item>
/// </list>
/// </remarks>
/// <param name="stream">The raw pipe stream.</param>
/// <param name="logger">An <see cref="ILogger"/> implementation.</param>
/// <param name="maxMessageBytes">The maximum allowed size of a JSON-serialized message payload in bytes (excluding the
/// varint length prefix). Must be greater than zero. Defaults to <see cref="DefaultMaxMessageBytes"/>.
/// </param>
public sealed partial class PipeStreamWrapper(
    PipeStream stream,
    ILogger logger,
    int maxMessageBytes = PipeStreamWrapper.DefaultMaxMessageBytes)
    : IPipeStreamWrapper
{
    /// <summary>
    /// The default maximum message size in bytes for the serialized JSON payload (2 MiB).
    /// The varint length prefix is not included in this value.
    /// </summary>
    public const int DefaultMaxMessageBytes = 2 * 1024 * 1024;

    // Prevents more than one thread from writing to the pipe stream at once
    private readonly SemaphoreSlim _writeLock = new(1, 1);
    private readonly CancellationTokenSource _internalCts = new();

    private readonly PipeStream _stream = stream ?? throw new ArgumentNullException(nameof(stream));
    private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly int _maxMessageBytes = maxMessageBytes > 0
        ? maxMessageBytes
        : throw new ArgumentOutOfRangeException(nameof(maxMessageBytes), "Value must be greater than zero.");

    private int _disposeCount;

    /// <inheritdoc/>
    public bool IsConnected
    {
        get
        {
            if (Volatile.Read(ref _disposeCount) != 0)
                return false;

            try
            {
                return _stream.IsConnected;
            }
            catch (ObjectDisposedException)
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Gets the maximum allowed size of the serialized JSON payload in bytes.
    /// The varint length prefix is not included in this limit.
    /// </summary>
    public int MaxMessageBytes => _maxMessageBytes;

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

        // Read the payload length prefix first so oversized inbound messages can be rejected
        // before allocating the payload buffer.
        var messagePayloadLength = await _stream.ReadVarIntAsync(linkedCts.Token).ConfigureAwait(false);

        // A payload length of 0 is currently reserved to signal end-of-stream / closed pipe.
        // Empty payload messages are therefore not supported by the protocol at the moment.
        // If empty messages are introduced in the future, this behavior must be changed.
        if (messagePayloadLength == 0)
            ClosePipe();

        if (messagePayloadLength > _maxMessageBytes)
        {
            throw new InvalidOperationException(
                $"Inbound message exceeds maximum size ({messagePayloadLength} > {_maxMessageBytes} bytes).");
        }

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

        return namedPipeMessage;
    }

    /// <inheritdoc/>
    public async Task SendMessageAsync(NamedPipeMessage message, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _internalCts.Token);

        // MemoryStream won't need to resize it's array and reallocate memory
        // for most outgoing messages (~ 240 - 1000 bytes).
        using var messageStream = new MemoryStream(512);

        // Serialize to an in-memory buffer first so the exact payload size is known before anything
        // is written to the pipe.
        await JsonSerializer
            .SerializeAsync(
                messageStream,
                message,
                SourceGenerationContext.Default.NamedPipeMessage,
                linkedCts.Token)
            .ConfigureAwait(false);

        var payloadLength = (int)messageStream.Length;

        if (payloadLength > _maxMessageBytes)
        {
            throw new InvalidOperationException(
                $"Outbound message exceeds maximum size ({payloadLength} > {_maxMessageBytes} bytes).");
        }

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
    }

    private void ClosePipe()
    {
        PipeHasClosed(_logger);

        // OperationCanceledException is handled as pipe closing gracefully.
        throw new OperationCanceledException();
    }

    private void ThrowIfDisposed()
    {
#pragma warning disable CA1513 // We must check _disposeCount thread-safe
        if (Volatile.Read(ref _disposeCount) != 0)
            throw new ObjectDisposedException(nameof(PipeStreamWrapper));
#pragma warning restore CA1513
    }

    [LoggerMessage(LogLevel.Debug, "Pipe closed before write could complete.")]
    private static partial void PipeClosedBeforeWriteCouldComplete(ILogger logger);

    [LoggerMessage(LogLevel.Debug, "Pipe has closed.")]
    private static partial void PipeHasClosed(ILogger logger);

    [LoggerMessage(LogLevel.Error, "Sending named pipe message failed.")]
    private static partial void SendPipeMessageFailed(ILogger logger, Exception exception);
}
