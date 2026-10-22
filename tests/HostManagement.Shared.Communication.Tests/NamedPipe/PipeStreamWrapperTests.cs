using System.IO.Pipes;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using AwesomeAssertions;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Communication.NamedPipe;
using HostManagement.Shared.Serialization;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace HostManagement.Shared.Communication.Tests.NamedPipe;

public class PipeStreamWrapperTests
{
    private const int DefaultMaxMessageBytes = 2 * 1024 * 1024;
    private const string TestTopic = "test";
    private const string TestEventContent = "Test event content";

    public class IsConnectedProperty
    {
        [Fact]
        public void ShouldReturnFalse_WhenCalledAfterDispose()
        {
            // Arrange
            var loggerSubstitute = CreateLoggerSubstitute();
            using var fakeStream = new FakePipeStream([0x00]);
            using var wrapper = new PipeStreamWrapper(fakeStream, loggerSubstitute);

            // Act + Assert
            wrapper.Dispose();
            wrapper.IsConnected.Should().BeFalse();
        }
    }

    public class MaxMessageBytesProperty
    {
        [Fact]
        public void ShouldReturnDefaultValue_WhenNotConfigured()
        {
            // Arrange
            var loggerSubstitute = CreateLoggerSubstitute();
            using var fakeStream = new FakePipeStream([0x00]);
            using var wrapper = new PipeStreamWrapper(fakeStream, loggerSubstitute);

            // Act + Assert
            wrapper.MaxMessageBytes.Should().Be(DefaultMaxMessageBytes);
        }

        [Fact]
        public void ShouldReturnValue_WhenConfigured()
        {
            // Arrange
            var loggerSubstitute = CreateLoggerSubstitute();
            using var fakeStream = new FakePipeStream([0x00]);
            var maxMessageBytes = 512;
            using var wrapper = new PipeStreamWrapper(fakeStream, loggerSubstitute, maxMessageBytes);

            // Act + Assert
            wrapper.MaxMessageBytes.Should().Be(maxMessageBytes);
        }
    }

    public class Dispose
    {
        [Fact]
        public void ShouldNotThrow_WhenDisposedMultipleTimes()
        {
            // Arrange
            var loggerSubstitute = CreateLoggerSubstitute();
            using var fakeStream = new FakePipeStream([0x00]);
            using var wrapper = new PipeStreamWrapper(fakeStream, loggerSubstitute);

            // Act / Assert
            wrapper.Invoking(w => w.Dispose()).Should().NotThrow();
            wrapper.Invoking(w => w.Dispose()).Should().NotThrow();
        }
    }

    public class ReadMessageAsync
    {
        private const string PipeClosed = "Pipe has closed.";

        [Fact]
        public async Task ShouldThrow_WhenReadCalledAfterDispose()
        {
            // Arrange
            var loggerSubstitute = CreateLoggerSubstitute();

            // Length 5 + 5 bytes dummy data ('A')
            using var fakeStream = new FakePipeStream([0x05, 0x41, 0x41, 0x41, 0x41, 0x41]);
            using var wrapper = new PipeStreamWrapper(fakeStream, loggerSubstitute);

            wrapper.Dispose();

            // Act
            var func = async () => await wrapper.ReadMessageAsync();

            // Assert
            await func.Should().ThrowAsync<ObjectDisposedException>();
        }

        [Fact]
        public async Task ShouldThrow_WhenMessagePayloadLengthIsZero()
        {
            // Arrange
            var loggerSubstitute = CreateLoggerSubstitute();
            var inbound = new byte[] { 0x00 };
            using var fakeStream = new FakePipeStream(inbound);
            using var wrapper = new PipeStreamWrapper(fakeStream, loggerSubstitute);

            // Act
            var func = async () => await wrapper.ReadMessageAsync();

            // Assert
            await func.Should().ThrowAsync<OperationCanceledException>();

#pragma warning disable CA1873
            loggerSubstitute.Received(1).Log(
                LogLevel.Debug,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString()!.Contains(PipeClosed)),
                null,
                Arg.Any<Func<object, Exception?, string>>());
#pragma warning restore CA1873
        }

        [Fact]
        public async Task ShouldThrow_WhenInboundMessageTooLarge()
        {
            // Arrange
            var loggerSubstitute = CreateLoggerSubstitute();

            var maxMessageBytes = 1_024;
            var messagePayloadLength = 10_000;

            // A valid header followed by an oversized payload length, so the payload buffer is never allocated.
            var header = SerializeHeader(CreateEventMessage());
            var headerLengthPrefix = Utilities.GetVarInt(header.Length);
            var payloadLengthPrefix = Utilities.GetVarInt(messagePayloadLength);

            var buffer = new byte[headerLengthPrefix.Length + header.Length + payloadLengthPrefix.Length];
            var offset = 0;

            foreach (var part in new[] { headerLengthPrefix, header, payloadLengthPrefix })
            {
                Buffer.BlockCopy(part, 0, buffer, offset, part.Length);
                offset += part.Length;
            }

            using var fakeStream = new FakePipeStream(buffer);
            using var wrapper = new PipeStreamWrapper(fakeStream, loggerSubstitute, maxMessageBytes);

            var expectedErrorMessage =
                $"Inbound message exceeds maximum size ({messagePayloadLength} > {maxMessageBytes} bytes).";

            // Act + Assert
            var func = async () => await wrapper.ReadMessageAsync();
            await func.Should().ThrowAsync<InvalidOperationException>().WithMessage(expectedErrorMessage);
        }

        [Fact]
        public async Task ShouldThrow_WhenStreamEndsBeforeDeclaredPayloadIsRead()
        {
            // Arrange
            var loggerSubstitute = CreateLoggerSubstitute();

            // Declared length of 5 (VarInt 0x05) but only 2 bytes of payload ('A','B')
            // When loading the remaining 3 bytes => ReadAsync gives 0 => ClosePipe()
            var inbound = new byte[] { 0x05, 0x41, 0x42 };
            using var fakeStream = new FakePipeStream(inbound);
            using var wrapper = new PipeStreamWrapper(fakeStream, loggerSubstitute);

            // Act
            var func = async () => await wrapper.ReadMessageAsync();

            // Assert
            await func.Should().ThrowAsync<OperationCanceledException>();

#pragma warning disable CA1873
            loggerSubstitute.Received(1).Log(
                LogLevel.Debug,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString()!.Contains(PipeClosed)),
                null,
                Arg.Any<Func<object, Exception?, string>>());
#pragma warning restore CA1873
        }

        [Fact]
        public async Task ShouldThrow_WhenHeaderInvalidJson()
        {
            // Arrange
            var loggerSubstitute = CreateLoggerSubstitute();
            var buffer = BuildFramedBuffer(Encoding.UTF8.GetBytes("not-json"), Encoding.UTF8.GetBytes("content"));

            using var fakeStream = new FakePipeStream(buffer);
            using var wrapper = new PipeStreamWrapper(fakeStream, loggerSubstitute, maxMessageBytes: 256);

            // Act
            var func = async () => await wrapper.ReadMessageAsync();

            // Assert
            await func.Should().ThrowAsync<JsonException>();
        }

        [Theory]
        [InlineData("not-json at all")]
        [InlineData("{ broken json")]
        [InlineData("pong")]
        public async Task ShouldReturnContentUnchanged_WhenPayloadIsNotJson(string content)
        {
            // The payload is carried outside the header, so the wrapper never parses it as JSON.

            // Arrange
            var loggerSubstitute = CreateLoggerSubstitute();
            var buffer = BuildFramedBuffer(CreateEventMessage(content));

            using var fakeStream = new FakePipeStream(buffer);
            using var wrapper = new PipeStreamWrapper(fakeStream, loggerSubstitute, maxMessageBytes: 256);

            // Act
            var result = await wrapper.ReadMessageAsync(TestContext.Current.CancellationToken);

            // Assert
            result.Content.Should().Be(content);
            result.Topic.Should().Be(TestTopic);
        }

        [Fact]
        public async Task ShouldReturnEmptyContent_WhenPayloadLengthIsZero()
        {
            // A payload length of 0 is a message without content; end-of-stream is signalled by a header length of 0.

            // Arrange
            var loggerSubstitute = CreateLoggerSubstitute();
            var buffer = BuildFramedBuffer(CreateEventMessage(string.Empty));

            using var fakeStream = new FakePipeStream(buffer);
            using var wrapper = new PipeStreamWrapper(fakeStream, loggerSubstitute, maxMessageBytes: 256);

            // Act
            var result = await wrapper.ReadMessageAsync(TestContext.Current.CancellationToken);

            // Assert
            result.Content.Should().BeEmpty();
            result.Topic.Should().Be(TestTopic);
        }

        [Fact]
        public async Task ShouldReturnMessage_WhenPayloadLengthEqualsMaximum()
        {
            // Arrange
            var loggerSubstitute = CreateLoggerSubstitute();
            var maxMessageBytes = 256;

            // The limit applies to the payload only, so the content itself may fill it exactly.
            var content = new string('x', maxMessageBytes);
            var message = CreateEventMessage(content);

            Encoding.UTF8.GetByteCount(content).Should().Be(maxMessageBytes);
            var buffer = BuildFramedBuffer(message);

            using var fakeStream = new FakePipeStream(buffer);
            using var wrapper = new PipeStreamWrapper(fakeStream, loggerSubstitute, maxMessageBytes);

            // Act
            var result = await wrapper.ReadMessageAsync(CancellationToken.None);

            // Assert
            result.Topic.Should().Be(TestTopic);
            result.Content.Should().Be(content);
            result.Type.Should().Be(MessageType.Event);
            result.ConversationId.Should().Be(message.ConversationId);
        }

        [Fact]
        public async Task ShouldReturnMessage_WhenPayloadLengthLessThanMaximum()
        {
            // Arrange
            var loggerSubstitute = CreateLoggerSubstitute();
            var maxMessageBytes = 256;

            var content = new string('x', maxMessageBytes - 123);
            var message = CreateEventMessage(content);
            var buffer = BuildFramedBuffer(message);

            using var fakeStream = new FakePipeStream(buffer);
            using var wrapper = new PipeStreamWrapper(fakeStream, loggerSubstitute, maxMessageBytes);

            // Act
            var result = await wrapper.ReadMessageAsync(CancellationToken.None);

            // Assert
            result.Topic.Should().Be(TestTopic);
            result.Content.Should().Be(content);
            result.Type.Should().Be(MessageType.Event);
            result.ConversationId.Should().Be(message.ConversationId);
        }
    }

    public class SendMessageAsync
    {
        private const string BrokenPipe = "Broken pipe";
        private const string PipeIsBroken = "Pipe is broken.";
        private const string PipeClosedBeforeWriteComplete = "Pipe closed before write could complete.";
        private const string SendingMessageFailed = "Sending named pipe message failed.";

        [Fact]
        public async Task ShouldThrow_WhenSendCalledAfterDispose()
        {
            // Arrange
            var loggerSubstitute = CreateLoggerSubstitute();
            using var fakeStream = new FakePipeStream([0x00]);
            using var wrapper = new PipeStreamWrapper(fakeStream, loggerSubstitute);
            wrapper.Dispose();

            var message = CreateEventMessage();

            // Act + Assert
            var func = async () => await wrapper.SendMessageAsync(message);
            await func.Should().ThrowAsync<ObjectDisposedException>();
        }

        [Fact]
        public async Task ShouldThrow_WhenOutboundMessageTooLarge()
        {
            // Arrange
            var loggerSubstitute = CreateLoggerSubstitute();
            using var fakeStream = new FakePipeStream([0x00]);
            var maxMessageBytes = 128;
            using var wrapper = new PipeStreamWrapper(fakeStream, loggerSubstitute, maxMessageBytes);

            // The limit applies to the payload only, so the content has to exceed it on its own.
            var content = new string('x', maxMessageBytes + 1);
            var message = CreateEventMessage(content);

            var expectedErrorMessage =
                $"Outbound message exceeds maximum size ({maxMessageBytes + 1} > {maxMessageBytes} bytes).";

            // Act + Assert
            var func = async () => await wrapper.SendMessageAsync(message);
            await func.Should().ThrowAsync<InvalidOperationException>().WithMessage(expectedErrorMessage);
        }

        [Fact]
        public async Task ShouldThrow_WhenCanceledWhileWaitingForWriteLock()
        {
            // Arrange
            var loggerSubstitute = CreateLoggerSubstitute();
            using var blockingStream = new BlockingWritePipeStream();
            using var wrapper = new PipeStreamWrapper(blockingStream, loggerSubstitute);

            var message1 = CreateEventMessage("first");
            var message2 = CreateEventMessage("second");

            // Act
            // First send; holds write lock until ReleaseWrite() called
            var firstSendTask = wrapper.SendMessageAsync(message1, CancellationToken.None);
            await blockingStream.WriteEntered.Task.WaitAsync(TimeSpan.FromSeconds(1), CancellationToken.None);

            // Second send and immediate cancel
            using var cts = new CancellationTokenSource();
            var secondSendTask = wrapper.SendMessageAsync(message2, cts.Token);
            cts.Cancel();

            // Assert
            await FluentActions.Awaiting(() => secondSendTask).Should().ThrowAsync<OperationCanceledException>();

            // Logger
            loggerSubstitute.DidNotReceive().Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception?, string>>());

            // First send release
            blockingStream.ReleaseWrite();
            await firstSendTask.WaitAsync(TimeSpan.FromSeconds(1), CancellationToken.None);
        }

        [Fact]
        public async Task ShouldThrow_WhenCanceledAfterLockAcquired()
        {
            // Arrange
            var loggerSubstitute = CreateLoggerSubstitute();
            using var blockingStream = new BlockingWriteAfterLockPipeStream();
            using var wrapper = new PipeStreamWrapper(blockingStream, loggerSubstitute);

            var message = CreateEventMessage("writing");
            using var cts = new CancellationTokenSource();

            // Act
            var sendTask = wrapper.SendMessageAsync(message, cts.Token);
            await blockingStream.WriteStarted.Task.WaitAsync(TimeSpan.FromSeconds(1), CancellationToken.None);

            // Cancel during write
            cts.Cancel();

            // Assert
            await FluentActions.Awaiting(() => sendTask).Should().ThrowAsync<OperationCanceledException>();

            loggerSubstitute.DidNotReceive().Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception?, string>>());
        }

        [Fact]
        public async Task ShouldThrowAndLogDebug_WhenBrokenPipeDetectedByExceptionMessage()
        {
            // Arrange
            var loggerSubstitute = CreateLoggerSubstitute();

            // System.IO.IOException: Pipe is broken.
            var exception = new IOException(PipeIsBroken);
            using var throwingStream = new ThrowingOnWritePipeStream(() => exception);

            using var wrapper = new PipeStreamWrapper(throwingStream, loggerSubstitute);
            var message = CreateEventMessage();

            // Act
            var func = async () => await wrapper.SendMessageAsync(message);

            // Assert
            await func.Should().ThrowAsync<IOException>();

#pragma warning disable CA1873
            loggerSubstitute.Received(1).Log(
                LogLevel.Debug,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString()!.Contains(PipeClosedBeforeWriteComplete)),
                null,
                Arg.Any<Func<object, Exception?, string>>());
#pragma warning restore CA1873
        }

        [Fact]
        public async Task ShouldThrowAndLogError_WhenGenericIOExceptionWithoutInnerExceptionOccurred()
        {
            // Arrange
            var loggerSubstitute = CreateLoggerSubstitute();
            using var throwingStream = new ThrowingOnWritePipeStream(() => new IOException("Generic message."));
            using var wrapper = new PipeStreamWrapper(throwingStream, loggerSubstitute);
            var message = CreateEventMessage();

            // Act
            var func = async () => await wrapper.SendMessageAsync(message);

            // Assert
            await func.Should().ThrowAsync<IOException>();

            loggerSubstitute.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString()!.Contains(SendingMessageFailed)),
                Arg.Any<IOException>(),
                Arg.Any<Func<object, Exception?, string>>());
        }

        [Fact]
        public async Task ShouldThrowAndLogDebug_WhenBrokenPipeDetectedBySocketException()
        {
            // Arrange
            var loggerSubstitute = CreateLoggerSubstitute();

            // System.IO.IOException: Broken pipe (errno 32 (EPIPE) on POSIX system)
            var exception = new IOException(BrokenPipe, new SocketException(32));
            using var throwingStream = new ThrowingOnWritePipeStream(() => exception);

            using var wrapper = new PipeStreamWrapper(throwingStream, loggerSubstitute);
            var message = CreateEventMessage();

            // Act
            var func = async () => await wrapper.SendMessageAsync(message);

            // Assert
            await func.Should().ThrowAsync<IOException>();

#pragma warning disable CA1873
            loggerSubstitute.Received(1).Log(
                LogLevel.Debug,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString()!.Contains(PipeClosedBeforeWriteComplete)),
                null,
                Arg.Any<Func<object, Exception?, string>>());
#pragma warning restore CA1873
        }

        [Fact]
        public async Task ShouldThrowAndLogError_WhenSocketExceptionHasDifferentCode()
        {
            // Arrange
            var loggerSubstitute = CreateLoggerSubstitute();

            // System.IO.IOException: Broken pipe; but error code not 32
            var exception = new IOException(BrokenPipe, new SocketException((int)SocketError.AccessDenied));
            using var throwingStream = new ThrowingOnWritePipeStream(() => exception);

            using var wrapper = new PipeStreamWrapper(throwingStream, loggerSubstitute);
            var message = CreateEventMessage();

            // Act
            var func = async () => await wrapper.SendMessageAsync(message);

            // Assert
            await func.Should().ThrowAsync<IOException>();

            loggerSubstitute.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString()!.Contains(SendingMessageFailed)),
                Arg.Any<IOException>(),
                Arg.Any<Func<object, Exception?, string>>());
        }

        [Fact]
        public async Task ShouldThrowAndLogError_WhenGenericExceptionOccurred()
        {
            // Arrange
            var loggerSubstitute = CreateLoggerSubstitute();

            var exception = new InvalidOperationException("Generic exception");
            using var throwingStream = new ThrowingOnWritePipeStream(() => exception);

            using var wrapper = new PipeStreamWrapper(throwingStream, loggerSubstitute);
            var message = CreateEventMessage();

            // Act
            var func = async () => await wrapper.SendMessageAsync(message);

            // Assert
            await func.Should().ThrowAsync<InvalidOperationException>();

            loggerSubstitute.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString()!.Contains(SendingMessageFailed)),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception?, string>>());
        }

        [Fact]
        public async Task ShouldWriteLengthPrefixAndPayload_WhenSendSucceeds()
        {
            // Arrange
            var loggerSubstitute = CreateLoggerSubstitute();
            using var stream = new CapturingPipeStream();
            using var wrapper = new PipeStreamWrapper(stream, loggerSubstitute, 256);

            var message = CreateEventMessage("payload");

            // Act
            await wrapper.SendMessageAsync(message, CancellationToken.None);

            // Assert
            var written = stream.Written;
            written.Should().NotBeEmpty();
        }

        [Fact]
        public async Task ShouldWriteFrameSmallerThanAJsonEnvelope_WhenContentIsJson()
        {
            // The payload is carried outside the header, so it is not escaped a second time.

            // Arrange
            var loggerSubstitute = CreateLoggerSubstitute();
            using var stream = new CapturingPipeStream();
            using var wrapper = new PipeStreamWrapper(stream, loggerSubstitute);

            var content = MessageSerializer.Serialize(new Response { Message = new string('x', 2_000) });
            var message = CreateEventMessage(content);

            // Act
            await wrapper.SendMessageAsync(message, TestContext.Current.CancellationToken);

            // Assert
            var envelopeLength = Encoding.UTF8.GetByteCount(JsonSerializer.Serialize(message));

            stream.Written.Length.Should().BeLessThan(envelopeLength);
        }

        private sealed class BlockingWritePipeStream : PipeStream
        {
            private readonly TaskCompletionSource _entered = new(TaskCreationOptions.RunContinuationsAsynchronously);
            private readonly TaskCompletionSource _release = new(TaskCreationOptions.RunContinuationsAsynchronously);

            public BlockingWritePipeStream() : base(PipeDirection.InOut, PipeTransmissionMode.Byte, 0) { }

            public TaskCompletionSource WriteEntered => _entered;

            public void ReleaseWrite() => _release.TrySetResult();

            public override bool CanRead => false;
            public override bool CanWrite => true;
            public override bool CanSeek => false;
            public override bool CanTimeout => false;
            public override long Length => 0;
            public override long Position { get => 0; set => throw new NotSupportedException(); }

            public override void Flush() { }
            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
            public override void SetLength(long value) => throw new NotSupportedException();
            public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
            public override int ReadByte() => throw new NotSupportedException();
            public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
                => throw new NotSupportedException();

            public override ValueTask<int> ReadAsync(Memory<byte> destination, CancellationToken cancellationToken = default)
                => throw new NotSupportedException();

            public override void Write(byte[] buffer, int offset, int count)
            {
                _entered.TrySetResult();
                _release.Task.GetAwaiter().GetResult();
            }

            public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                _entered.TrySetResult();
                return _release.Task;
            }

            public override ValueTask WriteAsync(ReadOnlyMemory<byte> source, CancellationToken cancellationToken = default)
            {
                _entered.TrySetResult();
                return new ValueTask(_release.Task);
            }
        }

        private sealed class BlockingWriteAfterLockPipeStream : PipeStream
        {
            public TaskCompletionSource WriteStarted { get; } =
                new(TaskCreationOptions.RunContinuationsAsynchronously);

            public BlockingWriteAfterLockPipeStream() : base(PipeDirection.InOut, PipeTransmissionMode.Byte, 0) { }

            public override bool CanRead => false;
            public override bool CanWrite => true;
            public override bool CanSeek => false;
            public override bool CanTimeout => false;
            public override long Length => 0;
            public override long Position { get => 0; set => throw new NotSupportedException(); }

            public override void Flush() { }
            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
            public override void SetLength(long value) => throw new NotSupportedException();
            public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
            public override int ReadByte() => throw new NotSupportedException();
            public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
                => throw new NotSupportedException();

            public override ValueTask<int> ReadAsync(Memory<byte> destination, CancellationToken cancellationToken = default)
                => throw new NotSupportedException();

            public override void Write(byte[] buffer, int offset, int count)
                => throw new NotSupportedException();

            public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                WriteStarted.TrySetResult();
                // Blocks until cancelled
                return Task.Delay(Timeout.Infinite, cancellationToken);
            }

            public override ValueTask WriteAsync(ReadOnlyMemory<byte> source, CancellationToken cancellationToken = default)
            {
                WriteStarted.TrySetResult();
                return new ValueTask(Task.Delay(Timeout.Infinite, cancellationToken));
            }
        }

        private sealed class ThrowingOnWritePipeStream(Func<Exception> factory)
            : PipeStream(PipeDirection.InOut, PipeTransmissionMode.Byte, 0)
        {
            private readonly Func<Exception> _exceptionFactory = factory;

            public override bool CanRead => false;
            public override bool CanWrite => true;
            public override bool CanSeek => false;
            public override bool CanTimeout => false;
            public override long Length => 0;
            public override long Position { get => 0; set => throw new NotSupportedException(); }

            public override void Flush() { }
            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
            public override void SetLength(long value) => throw new NotSupportedException();
            public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
            public override int ReadByte() => throw new NotSupportedException();
            public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
                => throw new NotSupportedException();

            public override ValueTask<int> ReadAsync(Memory<byte> destination, CancellationToken cancellationToken = default)
                => throw new NotSupportedException();

            public override void Write(byte[] buffer, int offset, int count)
                => throw _exceptionFactory();

            public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
                => Task.FromException(_exceptionFactory());

            public override ValueTask WriteAsync(ReadOnlyMemory<byte> source, CancellationToken cancellationToken = default)
                => ValueTask.FromException(_exceptionFactory());
        }
    }

    public class Framing
    {
        [Theory]
        [InlineData("ping")]
        [InlineData("")]
        [InlineData("""{"looks":"like json"}""")]
        [InlineData("quotes \" and backslash \\ and newline \n")]
        [InlineData("ümlaut ✓ 日本語")]
        public async Task ShouldReadBackContentUnchanged_WhenMessageWasSent(string content)
        {
            // Arrange
            var loggerSubstitute = CreateLoggerSubstitute();
            var sent = CreateEventMessage(content);

            using var writeStream = new CapturingPipeStream();
            using var writer = new PipeStreamWrapper(writeStream, loggerSubstitute);

            // Act
            await writer.SendMessageAsync(sent, TestContext.Current.CancellationToken);

            using var readStream = new FakePipeStream(writeStream.Written);
            using var reader = new PipeStreamWrapper(readStream, loggerSubstitute);

            var received = await reader.ReadMessageAsync(TestContext.Current.CancellationToken);

            // Assert
            received.Content.Should().Be(content);
            received.Topic.Should().Be(sent.Topic);
            received.Type.Should().Be(sent.Type);
            received.ConversationId.Should().Be(sent.ConversationId);
        }


    }

    private sealed class CapturingPipeStream : PipeStream
    {
        private readonly MemoryStream _capture = new();
        public CapturingPipeStream() : base(PipeDirection.InOut, PipeTransmissionMode.Byte, 0) { }

        public byte[] Written => _capture.ToArray();
        public override bool CanRead => false;
        public override bool CanWrite => true;
        public override bool CanSeek => false;
        public override bool CanTimeout => false;
        public override long Length => _capture.Length;
        public override long Position { get => _capture.Position; set => throw new NotSupportedException(); }

        public override void Flush() { }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override int ReadByte() => throw new NotSupportedException();
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public override ValueTask<int> ReadAsync(Memory<byte> destination, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => _capture.Write(buffer, offset, count);
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            _capture.Write(buffer, offset, count);
            return Task.CompletedTask;
        }

        public override ValueTask WriteAsync(ReadOnlyMemory<byte> source, CancellationToken cancellationToken = default)
        {
            _capture.Write(source.Span);
            return ValueTask.CompletedTask;
        }
    }

    /// <summary>
    /// Builds a frame: varint header length, JSON header, varint payload length, payload bytes.
    /// </summary>
    private static byte[] BuildFramedBuffer(NamedPipeMessage message)
        => BuildFramedBuffer(SerializeHeader(message), Encoding.UTF8.GetBytes(message.Content));

    private static byte[] BuildFramedBuffer(byte[] header, byte[] payload)
    {
        var headerLengthPrefix = Utilities.GetVarInt(header.Length);
        var payloadLengthPrefix = Utilities.GetVarInt(payload.Length);

        var framed = new byte[headerLengthPrefix.Length + header.Length + payloadLengthPrefix.Length + payload.Length];
        var offset = 0;

        foreach (var part in new[] { headerLengthPrefix, header, payloadLengthPrefix, payload })
        {
            Buffer.BlockCopy(part, 0, framed, offset, part.Length);
            offset += part.Length;
        }

        return framed;
    }

    private static byte[] SerializeHeader(NamedPipeMessage message)
        => JsonSerializer.SerializeToUtf8Bytes(
            new MessageHeader
            {
                ConversationId = message.ConversationId,
                Topic = message.Topic,
                Type = message.Type
            },
            SourceGenerationContext.Default.MessageHeader);

    private static NamedPipeMessage CreateEventMessage(string content = TestEventContent) => new()
    {
        Topic = TestTopic,
        Content = content,
        Type = MessageType.Event
    };

    private static ILogger CreateLoggerSubstitute()
    {
        var logger = Substitute.For<ILogger>();
        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        return logger;
    }

    private sealed class FakePipeStream(byte[] buffer) : PipeStream(PipeDirection.InOut, PipeTransmissionMode.Byte, 0)
    {
        private readonly byte[] _buffer = buffer;
        private int _pos;

        public override bool CanRead => true;
        public override bool CanWrite => true;
        public override bool CanSeek => false;
        public override bool CanTimeout => false;
        public override long Length => _buffer.Length;
        public override long Position { get => _pos; set => throw new NotSupportedException(); }

        public override void Flush() { }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_pos >= _buffer.Length)
                return 0;

            var toCopy = Math.Min(count, _buffer.Length - _pos);
            Array.Copy(_buffer, _pos, buffer, offset, toCopy);
            _pos += toCopy;

            return toCopy;
        }

        public override int ReadByte()
        {
            if (_pos >= _buffer.Length)
                return -1;

            return _buffer[_pos++];
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => Task.FromResult(Read(buffer, offset, count));

        public override ValueTask<int> ReadAsync(Memory<byte> destination, CancellationToken cancellationToken = default)
        {
            if (_pos >= _buffer.Length)
                return ValueTask.FromResult(0);

            var toCopy = Math.Min(destination.Length, _buffer.Length - _pos);

            _buffer.AsSpan(_pos, toCopy).CopyTo(destination.Span);
            _pos += toCopy;

            return ValueTask.FromResult(toCopy);
        }

        public override void Write(byte[] buffer, int offset, int count) { }
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => Task.CompletedTask;

        public override ValueTask WriteAsync(ReadOnlyMemory<byte> source, CancellationToken cancellationToken = default)
            => ValueTask.CompletedTask;
    }
}
