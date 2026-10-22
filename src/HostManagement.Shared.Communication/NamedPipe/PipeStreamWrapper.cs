using System.Buffers;
using System.IO.Pipes;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Extensions;
using Microsoft.Extensions.Logging;

namespace HostManagement.Shared.Communication.NamedPipe;

/// <inheritdoc cref="IPipeStreamWrapper"/>
/// <remarks>
/// A message is transferred as a frame of four parts: the varint-encoded header length, the JSON-serialized
/// <see cref="MessageHeader"/>, the varint-encoded payload length and the payload bytes.
/// <para>
/// The payload is the message content encoded as UTF-8 and is carried outside the header, so it is written verbatim.
/// A single JSON envelope holding the content as a string would escape it a second time, which inflates the frame by
/// roughly 70 % for JSON content and costs an additional escape and unescape pass over the whole payload.
/// </para>
/// <paramref name="maxMessageBytes"/> applies only to the payload.
/// <list type="bullet">
/// <item>
/// If the payload length is greater than <paramref name="maxMessageBytes"/>, inbound messages are rejected before the
/// payload buffer is allocated and outbound messages are rejected before anything is written to the pipe.
/// </item>
/// <item>
/// A header length of <c>0</c> indicates end-of-stream. A payload length of <c>0</c> is a message without content.
/// </item>
/// </list>
/// </remarks>
/// <param name="stream">The raw pipe stream.</param>
/// <param name="logger">An <see cref="ILogger"/> implementation.</param>
/// <param name="maxMessageBytes">The maximum allowed size of a message payload in bytes (excluding the header and the
/// varint length prefixes). Must be greater than zero. Defaults to <see cref="DefaultMaxMessageBytes"/>.
/// </param>
public sealed partial class PipeStreamWrapper(
    PipeStream stream,
    ILogger logger,
    int maxMessageBytes = PipeStreamWrapper.DefaultMaxMessageBytes)
    : IPipeStreamWrapper
{
    /// <summary>
    /// The default maximum message size in bytes for the message payload (2 MiB).
    /// The header and the varint length prefixes are not included in this value.
    /// </summary>
    public const int DefaultMaxMessageBytes = 2 * 1024 * 1024;

    /// <summary>
    /// The maximum allowed size of a serialized message header in bytes.
    /// A header carries only the conversation ID, the topic and the message type.
    /// </summary>
    private const int MaxHeaderBytes = 4 * 1024;

    /// <summary>
    /// The initial size of the buffers used to assemble a frame header, chosen to fit a header without resizing.
    /// </summary>
    private const int InitialHeaderBufferBytes = 256;

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
    /// Gets the maximum allowed size of the message payload in bytes.
    /// The header and the varint length prefixes are not included in this limit.
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

        var header = await ReadHeaderAsync(linkedCts.Token).ConfigureAwait(false);
        var content = await ReadPayloadAsync(linkedCts.Token).ConfigureAwait(false);

        return new NamedPipeMessage
        {
            ConversationId = header.ConversationId,
            Topic = header.Topic,
            Type = header.Type,
            Content = content
        };
    }

    private async Task<MessageHeader> ReadHeaderAsync(CancellationToken cancellationToken)
    {
        var headerLength = await _stream.ReadVarIntAsync(cancellationToken).ConfigureAwait(false);

        // A header length of 0 signals end-of-stream / closed pipe. A serialized header is never empty,
        // which leaves a payload length of 0 free to mean a message without content.
        if (headerLength == 0)
            ClosePipe();

        if (headerLength > MaxHeaderBytes)
        {
            throw new InvalidOperationException(
                $"Inbound message header exceeds maximum size ({headerLength} > {MaxHeaderBytes} bytes).");
        }

        var headerBytes = ArrayPool<byte>.Shared.Rent(headerLength);

        try
        {
            await ReadExactlyAsync(headerBytes.AsMemory(0, headerLength), cancellationToken).ConfigureAwait(false);

            return JsonSerializer.Deserialize(
                headerBytes.AsSpan(0, headerLength),
                SourceGenerationContext.Default.MessageHeader)
                    ?? throw new InvalidOperationException("Could not read message header from stream.");
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(headerBytes);
        }
    }

    private async Task<string> ReadPayloadAsync(CancellationToken cancellationToken)
    {
        // The payload length is read before the buffer is allocated so oversized messages are rejected early.
        var payloadLength = await _stream.ReadVarIntAsync(cancellationToken).ConfigureAwait(false);

        if (payloadLength > _maxMessageBytes)
        {
            throw new InvalidOperationException(
                $"Inbound message exceeds maximum size ({payloadLength} > {_maxMessageBytes} bytes).");
        }

        if (payloadLength == 0)
            return string.Empty;

        var payloadBytes = ArrayPool<byte>.Shared.Rent(payloadLength);

        try
        {
            await ReadExactlyAsync(payloadBytes.AsMemory(0, payloadLength), cancellationToken).ConfigureAwait(false);

            // The payload is the content verbatim, so it is decoded instead of being parsed out of a JSON envelope.
            return Encoding.UTF8.GetString(payloadBytes.AsSpan(0, payloadLength));
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(payloadBytes);
        }
    }

    private async Task ReadExactlyAsync(Memory<byte> buffer, CancellationToken cancellationToken)
    {
        var bytesRead = 0;

        while (bytesRead < buffer.Length)
        {
            var readBytes = await _stream
                .ReadAsync(buffer[bytesRead..], cancellationToken)
                .ConfigureAwait(false);

            if (readBytes == 0)
                ClosePipe();

            bytesRead += readBytes;
        }
    }

    /// <inheritdoc/>
    public async Task SendMessageAsync(NamedPipeMessage message, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        ArgumentNullException.ThrowIfNull(message);

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _internalCts.Token);

        var payloadLength = Encoding.UTF8.GetByteCount(message.Content);

        if (payloadLength > _maxMessageBytes)
        {
            throw new InvalidOperationException(
                $"Outbound message exceeds maximum size ({payloadLength} > {_maxMessageBytes} bytes).");
        }

        // The frame prefix (header length, header, payload length) is small and is assembled in one buffer, so the
        // payload itself is written straight from its own buffer instead of being copied into the frame.
        var prefixWriter = new ArrayBufferWriter<byte>(InitialHeaderBufferBytes);

        WriteFramePrefix(prefixWriter, message, payloadLength);

        var payloadBytes = ArrayPool<byte>.Shared.Rent(payloadLength);
        var writeLockTaken = false;

        try
        {
            Encoding.UTF8.GetBytes(message.Content, payloadBytes);

            await _writeLock.WaitAsync(linkedCts.Token).ConfigureAwait(false);
            writeLockTaken = true;

            ThrowIfDisposed();

            await _stream.WriteAsync(prefixWriter.WrittenMemory, linkedCts.Token).ConfigureAwait(false);

            if (payloadLength > 0)
                await _stream.WriteAsync(payloadBytes.AsMemory(0, payloadLength), linkedCts.Token).ConfigureAwait(false);
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
            ArrayPool<byte>.Shared.Return(payloadBytes);

            if (writeLockTaken)
            {
                try { _writeLock.Release(); }
                catch (SemaphoreFullException) { }
                catch (ObjectDisposedException) { }
            }
        }
    }

    private static void WriteFramePrefix(ArrayBufferWriter<byte> writer, NamedPipeMessage message, int payloadLength)
    {
        var header = new MessageHeader
        {
            ConversationId = message.ConversationId,
            Topic = message.Topic,
            Type = message.Type
        };

        // The header is serialized first so its exact length is known before the length prefix is written.
        var headerWriter = new ArrayBufferWriter<byte>(InitialHeaderBufferBytes);

        using (var jsonWriter = new Utf8JsonWriter(headerWriter))
            JsonSerializer.Serialize(jsonWriter, header, SourceGenerationContext.Default.MessageHeader);

        writer.Write(Utilities.GetVarInt(headerWriter.WrittenCount));
        writer.Write(headerWriter.WrittenSpan);
        writer.Write(Utilities.GetVarInt(payloadLength));
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
