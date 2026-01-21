using System.IO.Pipes;
using AwesomeAssertions;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Communication.NamedPipe;
using HostManagement.Shared.Communication.NamedPipe.Client;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace HostManagement.Shared.Communication.Tests.NamedPipe.Client;

public class PipeClientTests
{
    private const string TestPipeName = "test-pipe";
    private const string TestTopic = "test";
    private const string TestEventContent = "Test event content";
    private const string TestRequestContent = "Test request content";

    public class Constructor
    {
        [Fact]
        public void ShouldThrow_WhenPipeNameIsNull()
        {
            var loggerSubstitute = GetLoggerSubstitute();
            var func = () => new PipeClient(null!, loggerSubstitute, new CallbackHandlerRegistry());
            func.Should().Throw<ArgumentNullException>().WithParameterName("pipeName");
        }

        [Fact]
        public async Task ShouldCreatePipeClient_WhenParamsGiven()
        {
            var loggerSubstitute = GetLoggerSubstitute();
            await using var pipeClient = new PipeClient(TestPipeName, loggerSubstitute, new CallbackHandlerRegistry());
            pipeClient.Should().NotBeNull();
            pipeClient.State.Should().Be(PipeState.NotOpened);
        }
    }

    public class DisposeAsync
    {
        [Fact]
        public async Task ShouldNotThrow_WhenDisposedMultipleTimes()
        {
            var loggerSubstitute = GetLoggerSubstitute();
            await using var client = new PipeClient(TestPipeName, loggerSubstitute, new CallbackHandlerRegistry());

            client.Invoking(c => c.DisposeAsync()).Should().NotThrow();
            client.State.Should().Be(PipeState.Closed);

            client.Invoking(c => c.DisposeAsync()).Should().NotThrow();
            client.State.Should().Be(PipeState.Closed);
        }

        [Fact]
        public async Task ShouldNotThrow_WhenWrapperDisposeThrows()
        {
            // Arrange
            var loggerSubstitute = GetLoggerSubstitute();
            await using var client = new PipeClient(TestPipeName, loggerSubstitute, new CallbackHandlerRegistry());
            using var throwingOnDisposeWrapper = new ThrowingOnDisposeWrapper();

            PipeClientTestAccessors.SetWrapper(client, throwingOnDisposeWrapper);

            // Act
            var act = async () => await client.DisposeAsync();

            // Assert
            await act.Should().NotThrowAsync();
            client.State.Should().Be(PipeState.Closed);
        }

        private sealed class ThrowingOnDisposeWrapper : IPipeStreamWrapper
        {
            private int _disposeCount;

            public bool IsConnected => true;

            public Task<NamedPipeMessage> ReadMessageAsync(CancellationToken cancellationToken = default)
                => throw new NotSupportedException("Not used in this test.");

            public Task SendMessageAsync(NamedPipeMessage message, CancellationToken cancellationToken = default)
                => throw new NotSupportedException("Not used in this test.");

            public void Dispose()
            {
                if (Interlocked.Exchange(ref _disposeCount, 1) == 0)
                    throw new IOException("Dispose failed.");
            }
        }
    }

    public class TryConnectAsync
    {
        [Fact]
        public async Task ShouldThrow_WhenClientIsDisposed()
        {
            // Arrange
            var loggerSubstitute = GetLoggerSubstitute();
            await using var client = new PipeClient(TestPipeName, loggerSubstitute, new CallbackHandlerRegistry());

            await client.DisposeAsync();

            // Act
            var act = () => client.TryConnectAsync();

            // Assert
            await act.Should()
                .ThrowAsync<ObjectDisposedException>()
                .WithMessage($"*{nameof(PipeClient)}*");
        }

        [Fact]
        public async Task ShouldThrow_WhenTimeoutIsInvalid()
        {
            // Arrange
            var loggerSubstitute = GetLoggerSubstitute();
            await using var client = new PipeClient(TestPipeName, loggerSubstitute, new CallbackHandlerRegistry());

            // Act
            var act = () => client.TryConnectAsync(timeoutMs: -2);

            // Assert
            await act.Should()
                .ThrowAsync<ArgumentOutOfRangeException>()
                .WithParameterName("timeoutMs")
                .WithMessage("*non-negative*");
        }

        [Fact]
        public async Task ShouldReturnFalse_WhenCancellationRequestedImmediately()
        {
            // Arrange
            var loggerSubstitute = GetLoggerSubstitute();
            await using var client = new PipeClient(TestPipeName, loggerSubstitute, new CallbackHandlerRegistry());
            using var cts = new CancellationTokenSource();

            var expectedClientDebugLog = $"Connecting to pipe '{TestPipeName}' was canceled.";

            // Act
            await cts.CancelAsync();
            var result = await client.TryConnectAsync(timeoutMs: 10_000, cancellationToken: cts.Token);

            // Assert
            result.Should().BeFalse();
            client.State.Should().Be(PipeState.NotOpened);

            // Client has logged cancellation
            loggerSubstitute.Received(1).Log(
                Arg.Is(LogLevel.Debug),
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString()!.Contains(expectedClientDebugLog)),
                null,
                Arg.Any<Func<object, Exception?, string>>());

            // Client should not have error logs
            loggerSubstitute.DidNotReceive().Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception?, string>>());
        }

        [Fact]
        public async Task ShouldReturnFalse_WhenTimeoutOccurs()
        {
            // Arrange
            var loggerSubstitute = GetLoggerSubstitute();
            var pipeName = CreateUniquePipeName();
            await using var client = new PipeClient(pipeName, loggerSubstitute, new CallbackHandlerRegistry());

            // Act
            // No server is running, so connect fails after timeout
            var result = await client.TryConnectAsync(timeoutMs: 10, CancellationToken.None);

            // Assert
            result.Should().BeFalse();
            client.State.Should().Be(PipeState.NotOpened);
        }

        [Fact]
        public async Task ShouldResetStateToNotOpened_WhenReconnectFailsAfterStaleConnectedState()
        {
            // Arrange
            var loggerSubstitute = GetLoggerSubstitute();
            var pipeName = CreateUniquePipeName();
            await using var client = new PipeClient(pipeName, loggerSubstitute, new CallbackHandlerRegistry());

            // Simulate a stale connected state without an active pipe stream
            PipeClientTestAccessors.SetStateConnected(client);

            // Act
            // No server is running, so connect fails after timeout
            var result = await client.TryConnectAsync(timeoutMs: 10, CancellationToken.None);

            // Assert
            result.Should().BeFalse();
            client.State.Should().Be(PipeState.NotOpened);
        }

        private static string CreateUniquePipeName() => $"{TestPipeName}-{Guid.NewGuid():N}";
    }

    public class SendRequest
    {
        [Fact]
        public async Task ShouldThrow_WhenClientIsDisposed()
        {
            // Arrange
            var loggerSubstitute = GetLoggerSubstitute();
            await using var client = new PipeClient(TestPipeName, loggerSubstitute, new CallbackHandlerRegistry());

            await client.DisposeAsync();

            // Act
            var act = () => client.SendRequestAsync(Topics.Echo, TestRequestContent);

            // Assert
            await act.Should()
                .ThrowAsync<ObjectDisposedException>()
                .WithMessage($"*{nameof(PipeClient)}*");
        }

        [Fact]
        public async Task ShouldThrow_WhenNotConnectedAndConnectFails()
        {
            // Arrange
            var loggerSubstitute = GetLoggerSubstitute();
            await using var client = new PipeClient(TestPipeName, loggerSubstitute, new CallbackHandlerRegistry());
            using var cts = new CancellationTokenSource();

            // Act
            await cts.CancelAsync();
            var func = () => client.SendRequestAsync(Topics.Echo, TestRequestContent, cts.Token);

            // Assert
            await func.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage($"Cannot send request to pipe '{TestPipeName}' because it is not connected.");
        }

        [Fact]
        public async Task ShouldCancelAndCleanup_WhenCancellationRequestedImmediately()
        {
            // Arrange
            var loggerSubstitute = GetLoggerSubstitute();

            await using var client = new PipeClient(TestPipeName, loggerSubstitute, new CallbackHandlerRegistry());
            using var wrapper = new TrackingWrapper();
            using var cts = new CancellationTokenSource();

            PipeClientTestAccessors.SetStateConnected(client);
            PipeClientTestAccessors.SetWrapper(client, wrapper);

            var expectedClientDebugLog = "Send request was canceled.";

            // Act
            await cts.CancelAsync();
            var func = () => client.SendRequestAsync(Topics.Echo, TestRequestContent, cts.Token);

            // Assert
            await func.Should().ThrowAsync<OperationCanceledException>();
            wrapper.SendCalled.Should().BeFalse();
            PipeClientTestAccessors.GetExpectedResponses(client).Should().BeEmpty();

            // Client has logged cancellation
            loggerSubstitute.Received(1).Log(
                LogLevel.Debug,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString()!.Contains(expectedClientDebugLog)),
                null,
                Arg.Any<Func<object, Exception?, string>>());
        }

        [Fact]
        public async Task ShouldNotSend_WhenCancellationRequestedImmediately()
        {
            // Arrange
            var loggerSubstitute = GetLoggerSubstitute();

            await using var client = new PipeClient(TestPipeName, loggerSubstitute, new CallbackHandlerRegistry());
            using var wrapper = new TrackingWrapper();
            using var cts = new CancellationTokenSource();

            PipeClientTestAccessors.SetStateConnected(client);
            PipeClientTestAccessors.SetWrapper(client, wrapper);

            await cts.CancelAsync();

            // Act
            var act = () => client.SendRequestAsync(Topics.Echo, TestRequestContent, cts.Token);

            // Assert
            await act.Should().ThrowAsync<OperationCanceledException>();
            wrapper.SendCalled.Should().BeFalse();
            PipeClientTestAccessors.GetExpectedResponses(client).Should().BeEmpty();
        }

        [Fact]
        public async Task ShouldCancelAndCleanup_WhenWrapperIsNotConnectedBeforeSend()
        {
            // Arrange
            var loggerSubstitute = GetLoggerSubstitute();
            await using var client = new PipeClient(TestPipeName, loggerSubstitute, new CallbackHandlerRegistry());
            using var wrapper = new DisconnectedWrapper();

            PipeClientTestAccessors.SetStateConnected(client);
            PipeClientTestAccessors.SetWrapper(client, wrapper);

            // Act
            var act = () => client.SendRequestAsync(Topics.Echo, TestRequestContent);

            // Assert
            await act.Should()
                .ThrowAsync<OperationCanceledException>()
                .WithMessage($"Pipe '{TestPipeName}' closed before request could be sent.");

            PipeClientTestAccessors.GetExpectedResponses(client).Should().BeEmpty();
        }

        [Fact]
        public async Task ShouldThrow_WhenWrapperMissingButStateConnected()
        {
            // Arrange
            var loggerSubstitute = GetLoggerSubstitute();
            await using var client = new PipeClient(TestPipeName, loggerSubstitute, new CallbackHandlerRegistry());

            PipeClientTestAccessors.SetStateConnected(client);
            // Don't set a Wrapper

            var expectedExceptionMessage = "*Pipe stream wrapper should not be null.*";

            // Act
            var func = () => client.SendRequestAsync(Topics.Echo, TestRequestContent);

            // Assert
            await func.Should().ThrowAsync<ObjectDisposedException>()
                .WithMessage(expectedExceptionMessage);
        }

        [Fact]
        public async Task ShouldRemovePendingEntry_WhenSendMessageThrows()
        {
            // Arrange
            var loggerSubstitute = GetLoggerSubstitute();
            await using var client = new PipeClient(TestPipeName, loggerSubstitute, new CallbackHandlerRegistry());

            // Throws IOException on SendMessageAsync, e.g. when pipe broken
            using var wrapper = new ThrowingOnSendWrapper();

            var processingTaskCompletionSource =
                new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

            PipeClientTestAccessors.SetStateConnected(client);
            PipeClientTestAccessors.SetWrapper(client, wrapper);
            PipeClientTestAccessors.SetProcessingTask(client, processingTaskCompletionSource.Task);

            try
            {
                // Act
                var func = () => client.SendRequestAsync(Topics.Echo, TestRequestContent);

                // Assert
                await func.Should().ThrowAsync<IOException>();
                PipeClientTestAccessors.GetExpectedResponses(client).Should().BeEmpty();
            }
            finally
            {
                // Cleanup
                processingTaskCompletionSource.TrySetResult();
            }
        }

        [Fact]
        public async Task ShouldCancelPendingRequest_WhenCanceledAfterSend()
        {
            // Arrange
            var loggerSubstitute = GetLoggerSubstitute();
            await using var client = new PipeClient(TestPipeName, loggerSubstitute, new CallbackHandlerRegistry());
            using var cts = new CancellationTokenSource();

            var processingTaskCompletionSource =
                new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

            using var wrapper = new PassiveWrapper();
            PipeClientTestAccessors.SetStateConnected(client);
            PipeClientTestAccessors.SetWrapper(client, wrapper);
            PipeClientTestAccessors.SetProcessingTask(client, processingTaskCompletionSource.Task);

            try
            {
                // Act
                var sendTask = client.SendRequestAsync(Topics.Echo, TestRequestContent, cts.Token);

                // Make sure pending entry is created before cancellation
                var pending = await WaitForSingleExpectedResponseAsync(client, TimeSpan.FromSeconds(2));
                pending.Should().NotBeNull();

                await cts.CancelAsync();
                var func = async () => await sendTask;

                // Assert
                await func.Should().ThrowAsync<OperationCanceledException>();
                PipeClientTestAccessors.GetExpectedResponses(client).Should().BeEmpty();
            }
            finally
            {
                // Cleanup
                processingTaskCompletionSource.TrySetResult();
            }
        }

        [Fact]
        public async Task ShouldReceiveResponse_WhenRequestIsSend()
        {
            // Arrange
            var loggerSubstitute = GetLoggerSubstitute();
            var untypedLogger = GetUntypedLoggerSubstitute();

            await using var client = new PipeClient(TestPipeName, loggerSubstitute, new CallbackHandlerRegistry());
            using var serverPipe = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.None);
            using var _ = new AnonymousPipeClientStream(PipeDirection.In, serverPipe.ClientSafePipeHandle);
            using var wrapper = new PipeStreamWrapper(serverPipe, untypedLogger);

            var processingTaskCompletionSource =
                new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

            PipeClientTestAccessors.SetStateConnected(client);
            PipeClientTestAccessors.SetWrapper(client, wrapper);
            PipeClientTestAccessors.SetProcessingTask(client, processingTaskCompletionSource.Task);

            try
            {
                // Act
                var sendTask = client.SendRequestAsync(Topics.Echo, TestRequestContent, CancellationToken.None);
                var pendingResponse = await WaitForSingleExpectedResponseAsync(client, TimeSpan.FromSeconds(2));

                // Assert
                pendingResponse.Should().NotBeNull();

                var (conversationId, expectedResponse) = pendingResponse!.Value;
                var response = new NamedPipeMessage
                {
                    ConversationId = conversationId,
                    Topic = Topics.Echo,
                    Type = MessageType.Response,
                    Content = $"{{\"Message\":\"echo:{TestRequestContent}\"}}"
                };
                expectedResponse.TaskCompletionSource.TrySetResult(response);

                var result = await sendTask;
                result.Type.Should().Be(response.Type);
                result.Topic.Should().Be(response.Topic);
                result.ConversationId.Should().Be(response.ConversationId);
                result.Content.Should().Be(response.Content);
            }
            finally
            {
                // Cleanup
                processingTaskCompletionSource.TrySetResult();
            }
        }

        private static async Task<(Guid, ExpectedResponse)?> WaitForSingleExpectedResponseAsync(
            PipeClient client,
            TimeSpan timeout)
        {
            var expectedResponses = PipeClientTestAccessors.GetExpectedResponses(client);
            var end = DateTime.UtcNow + timeout;

            while (DateTime.UtcNow < end)
            {
                if (expectedResponses.Count == 1)
                {
                    foreach (var keyValuePair in expectedResponses)
                        return (keyValuePair.Key, keyValuePair.Value);
                }

                await Task.Delay(10);
            }

            return null;
        }

        private sealed class DisconnectedWrapper : IPipeStreamWrapper
        {
            public bool IsConnected => false;

            public Task<NamedPipeMessage> ReadMessageAsync(CancellationToken cancellationToken = default)
                => throw new NotSupportedException("Not used in this test.");

            public Task SendMessageAsync(NamedPipeMessage message, CancellationToken cancellationToken = default)
                => throw new InvalidOperationException("Send should not be called.");

            public void Dispose() { }
        }

        private sealed class PassiveWrapper : IPipeStreamWrapper
        {
            public bool IsConnected => true;

            public Task<NamedPipeMessage> ReadMessageAsync(CancellationToken cancellationToken = default)
                => throw new NotSupportedException("Not used in these tests.");

            public Task SendMessageAsync(NamedPipeMessage message, CancellationToken cancellationToken = default)
                => Task.CompletedTask;

            public void Dispose() { }
        }

        private sealed class ThrowingOnSendWrapper : IPipeStreamWrapper
        {
            public bool IsConnected => true;

            public Task<NamedPipeMessage> ReadMessageAsync(CancellationToken cancellationToken = default)
                => throw new NotSupportedException("Not used in these tests.");

            public Task SendMessageAsync(NamedPipeMessage message, CancellationToken cancellationToken = default)
                => throw new IOException("Send failed.");

            public void Dispose() { }
        }

        private sealed class TrackingWrapper : IPipeStreamWrapper
        {
            public bool IsConnected => true;

            public bool SendCalled { get; private set; }

            public Task<NamedPipeMessage> ReadMessageAsync(CancellationToken cancellationToken = default)
                => throw new NotSupportedException("Not used in this test.");

            public Task SendMessageAsync(NamedPipeMessage message, CancellationToken cancellationToken = default)
            {
                SendCalled = true;
                return Task.CompletedTask;
            }

            public void Dispose() { }
        }
    }

    public class ProcessAsync
    {
        [Fact]
        public async Task ShouldClose_WhenCancellationAlreadyRequestedBeforeProcessingStarts()
        {
            // Arrange
            var loggerSubstitute = GetLoggerSubstitute();
            await using var client = new PipeClient(TestPipeName, loggerSubstitute, new CallbackHandlerRegistry());
            using var cts = new CancellationTokenSource();

            PipeClientTestAccessors.SetStateConnected(client);

            await cts.CancelAsync();

            // Act
            await PipeClientTestAccessors.InvokeProcessAsync(client, cts.Token);

            // Assert
            client.State.Should().Be(PipeState.Closed);
        }

        [Fact]
        public async Task ShouldCloseAndDrain_WhenPipeWrapperIsNull()
        {
            // Arrange
            var loggerSubstitute = GetLoggerSubstitute();
            await using var client = new PipeClient(TestPipeName, loggerSubstitute, new CallbackHandlerRegistry());
            using var cts = new CancellationTokenSource();

            // Pending-Response vorbereiten, um Drain im Closed-Zweig zu verifizieren
            var conversationId = Guid.NewGuid();
            var pendingResponse = new ExpectedResponse();
            PipeClientTestAccessors.GetExpectedResponses(client).TryAdd(conversationId, pendingResponse);

            // Connected but no wrapper set
            PipeClientTestAccessors.SetStateConnected(client);

            // Act
            var processing = PipeClientTestAccessors.InvokeProcessAsync(client, cts.Token);
            await processing; // exits after ObjectDisposedException because wrapper is null

            // Assert
            client.State.Should().Be(PipeState.Closed);

            var func = async () => await pendingResponse.TaskCompletionSource.Task;
            await func.Should().ThrowAsync<OperationCanceledException>();

            PipeClientTestAccessors.GetExpectedResponses(client).Should().BeEmpty();

            // Client did not log disconnect because no wrapper was captured
            loggerSubstitute.DidNotReceive().Log(
                LogLevel.Information,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                null,
                Arg.Any<Func<object, Exception?, string>>());

            // Client did not log error
            loggerSubstitute.DidNotReceive().Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception?, string>>());
        }

        [Fact]
        public async Task ShouldCloseAndDrain_WhenReadThrowsIOException()
        {
            // Arrange
            var loggerSubstitute = GetLoggerSubstitute();
            await using var client = new PipeClient(TestPipeName, loggerSubstitute, new CallbackHandlerRegistry());
            using var cts = new CancellationTokenSource();

            // Manually enter a pending response
            var conversationId = Guid.NewGuid();
            var pendingResponse = new ExpectedResponse();
            PipeClientTestAccessors.GetExpectedResponses(client).TryAdd(conversationId, pendingResponse);

            // Throws IOException on ReadMessageAsync
            using var wrapper = new ThrowingOnReadWrapper();
            PipeClientTestAccessors.SetWrapper(client, wrapper);
            PipeClientTestAccessors.SetStateConnected(client);

            var expectedClientLog = "Disconnected from pipe server.";

            // Act
            var processing = PipeClientTestAccessors.InvokeProcessAsync(client, cts.Token);
            await processing; // exits with IOException

            // Assert
            client.State.Should().Be(PipeState.Closed);

            var func = async () => await pendingResponse.TaskCompletionSource.Task;
            await func.Should().ThrowAsync<OperationCanceledException>();

            PipeClientTestAccessors.GetExpectedResponses(client).Should().BeEmpty();

            // Client has logged disconnect
            loggerSubstitute.Received(1).Log(
                LogLevel.Information,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString()!.Contains(expectedClientLog)),
                null,
                Arg.Any<Func<object, Exception?, string>>());

            // Client should not have error logs
            loggerSubstitute.DidNotReceive().Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception?, string>>());
        }

        [Fact]
        public async Task ShouldLogUnexpectedResponse_WhenResponseHasNoPendingWaiter()
        {
            // Arrange
            var loggerSubstitute = GetLoggerSubstitute();
            await using var client = new PipeClient(TestPipeName, loggerSubstitute, new CallbackHandlerRegistry());
            using var cts = new CancellationTokenSource();

            // Unexpected response (not in our '_expectedResponses' to be answered)
            var unexpectedResponse = new NamedPipeMessage
            {
                Topic = TestTopic,
                Type = MessageType.Response,
                Content = "Unexpected response"
            };

            using var wrapper = new SingleMessageWrapper(unexpectedResponse);
            PipeClientTestAccessors.SetWrapper(client, wrapper);
            PipeClientTestAccessors.SetStateConnected(client);

            var expectedClientLog =
                $"Unexpected response on conversation {unexpectedResponse.ConversationId} " +
                $"with topic {unexpectedResponse.Topic}.";

            // Act
            var processing = PipeClientTestAccessors.InvokeProcessAsync(client, cts.Token);
            await cts.CancelAsync();
            await processing;

            // Assert
            // We have no pending response, so response is unexpected
            PipeClientTestAccessors.GetExpectedResponses(client).Should().BeEmpty();

            // Client has logged unexpected response
            loggerSubstitute.Received(1).Log(
                Arg.Is(LogLevel.Debug),
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString()!.Contains(expectedClientLog)),
                null,
                Arg.Any<Func<object, Exception?, string>>());
        }

        [Fact]
        public async Task ShouldLogErrorAndContinue_WhenCallbackHandlerThrows()
        {
            // Arrange
            var loggerSubstitute = GetLoggerSubstitute();

            var registry = new CallbackHandlerRegistry();
            registry.RegisterHandler(new ThrowingCallbackHandler());

            await using var client = new PipeClient(TestPipeName, loggerSubstitute, registry);
            using var cts = new CancellationTokenSource();

            var namedPipeEvent = new NamedPipeMessage
            {
                Topic = TestTopic,
                Content = TestEventContent,
                Type = MessageType.Event
            };

            using var wrapper = new SingleMessageWrapper(namedPipeEvent);
            PipeClientTestAccessors.SetWrapper(client, wrapper);
            PipeClientTestAccessors.SetStateConnected(client);

            var expectedClientErrorLog = $"An error occurred while processing messages on the pipe '{TestPipeName}'.";
            var expectedClientErrorExceptionMessage = "Callback handler failed.";

            // Act
            var processing = PipeClientTestAccessors.InvokeProcessAsync(client, cts.Token);
            await Task.Delay(50, CancellationToken.None);
            await cts.CancelAsync();
            await processing;

            // Assert
            client.State.Should().Be(PipeState.Closed);
            wrapper.IsConnected.Should().BeFalse();

            // Client has logged error 
            loggerSubstitute.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString()!.Contains(expectedClientErrorLog)),
                Arg.Is<Exception>(ex =>
                    ex is InvalidOperationException &&
                    ex.Message.Contains(expectedClientErrorExceptionMessage)),
                Arg.Any<Func<object, Exception?, string>>());
        }

        [Fact]
        public async Task ShouldFaultAndDrainWithIOException_WhenRequestMessageReceived()
        {
            // Arrange
            var loggerSubstitute = GetLoggerSubstitute();
            await using var client = new PipeClient(TestPipeName, loggerSubstitute, new CallbackHandlerRegistry());
            using var cts = new CancellationTokenSource();

            // Manually enter a pending response
            var conversationId = Guid.NewGuid();
            var pendingResponse = new ExpectedResponse();
            PipeClientTestAccessors.GetExpectedResponses(client).TryAdd(conversationId, pendingResponse);

            // Not allowed request message from server
            var notAllowedRequest = new NamedPipeMessage
            {
                Topic = TestTopic,
                Type = MessageType.Request,
                Content = "Not allowed!"
            };

            using var wrapper = new SingleMessageWrapper(notAllowedRequest);
            PipeClientTestAccessors.SetWrapper(client, wrapper);
            PipeClientTestAccessors.SetStateConnected(client);

            var expectedClientErrorLog = $"An error occurred while processing messages on the pipe '{TestPipeName}'.";
            var expectedClientErrorLogExceptionMessage =
                $"Message type '{notAllowedRequest.Type}' may only be sent by the server.";

            // Act
            var processing = PipeClientTestAccessors.InvokeProcessAsync(client, cts.Token);
            await processing;

            // Assert
            client.State.Should().Be(PipeState.Faulted);

            // Pending exits with IOException
            var func = async () => await pendingResponse.TaskCompletionSource.Task;
            await func.Should().ThrowAsync<IOException>().WithMessage($"Pipe '{TestPipeName}' faulted.");
            PipeClientTestAccessors.GetExpectedResponses(client).Should().BeEmpty();

            // Client has logged error while processing
            loggerSubstitute.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString()!.Contains(expectedClientErrorLog)),
                Arg.Is<Exception>(ex =>
                    ex is InvalidOperationException && ex.Message.Contains(expectedClientErrorLogExceptionMessage)),
                Arg.Any<Func<object, Exception?, string>>());
        }

        [Fact]
        public async Task ShouldInvokeCallbackHandler_WhenEventReceived()
        {
            // Arrange
            var loggerSubstitute = GetLoggerSubstitute();
            var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            using var cts = new CancellationTokenSource();

            var registry = new CallbackHandlerRegistry();
            registry.RegisterHandler(new TestCallbackHandler(tcs));

            await using var client = new PipeClient(TestPipeName, loggerSubstitute, registry);

            var namedPipeEvent = new NamedPipeMessage
            {
                Topic = TestTopic,
                Content = TestEventContent,
                Type = MessageType.Event
            };

            // Fake wrapper delivers a one-time event and then blocks until cancel
            using var wrapper = new SingleMessageWrapper(namedPipeEvent);
            PipeClientTestAccessors.SetWrapper(client, wrapper);
            PipeClientTestAccessors.SetStateConnected(client);

            var expectedClientLog = "Disconnected from pipe server.";

            // Act
            var processing = PipeClientTestAccessors.InvokeProcessAsync(client, cts.Token);

            // Assert
            // Wait until callback is executed
            await tcs.Task.WaitAsync(TimeSpan.FromSeconds(2), CancellationToken.None);

            // Cleanup/Assert final state after cancel
            await cts.CancelAsync();
            await processing;
            client.State.Should().Be(PipeState.Closed);

            // Client has logged disconnect
            loggerSubstitute.Received(1).Log(
                LogLevel.Information,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString()!.Contains(expectedClientLog)),
                null,
                Arg.Any<Func<object, Exception?, string>>());
        }

        [Fact]
        public async Task ShouldDrainPendingResponses_WhenProcessingStops()
        {
            // Arrange
            var loggerSubstitute = GetLoggerSubstitute();
            await using var client = new PipeClient(TestPipeName, loggerSubstitute, new CallbackHandlerRegistry());
            using var cts = new CancellationTokenSource();

            // Wrapper that immediately waits for Cancel
            using var wrapper = new SingleMessageWrapper(null);
            PipeClientTestAccessors.SetWrapper(client, wrapper);
            PipeClientTestAccessors.SetStateConnected(client);

            // Manually enter a pending response
            var conversationId = Guid.NewGuid();
            var pendingResponse = new ExpectedResponse();
            PipeClientTestAccessors.GetExpectedResponses(client).TryAdd(conversationId, pendingResponse);

            var expectedClientLog = "Disconnected from pipe server.";

            // Act
            var processing = PipeClientTestAccessors.InvokeProcessAsync(client, cts.Token);
            await cts.CancelAsync();

            // Awaiting the task should throw OperationCanceledException
            var func = async () => await pendingResponse.TaskCompletionSource.Task;

            // Assert
            await func.Should().ThrowAsync<OperationCanceledException>();

            await processing;
            PipeClientTestAccessors.GetExpectedResponses(client).Should().BeEmpty();
            client.State.Should().Be(PipeState.Closed);

            // Client has logged disconnect
            loggerSubstitute.Received(1).Log(
                LogLevel.Information,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString()!.Contains(expectedClientLog)),
                null,
                Arg.Any<Func<object, Exception?, string>>());
        }

        private sealed class TestCallbackHandler(TaskCompletionSource tcs) : ICallbackHandler
        {
            public string Topic => TestTopic;

            public Task HandleAsync(string messageContent, CancellationToken cancellationToken)
            {
                tcs.TrySetResult();
                return Task.CompletedTask;
            }
        }

        private sealed class ThrowingCallbackHandler : ICallbackHandler
        {
            public string Topic => TestTopic;

            public Task HandleAsync(string messageContent, CancellationToken cancellationToken)
                => throw new InvalidOperationException("Callback handler failed.");
        }

        private sealed class ThrowingOnReadWrapper : IPipeStreamWrapper
        {
            public bool IsConnected { get; private set; } = true;

            public Task<NamedPipeMessage> ReadMessageAsync(CancellationToken cancellationToken = default)
                => throw new IOException("Read failed.");

            public Task SendMessageAsync(NamedPipeMessage message, CancellationToken cancellationToken = default)
                => Task.CompletedTask;

            public void Dispose() => IsConnected = false;
        }

        private sealed class SingleMessageWrapper(NamedPipeMessage? message) : IPipeStreamWrapper
        {
            private bool _readOnce;

            public bool IsConnected { get; private set; } = true;

            public Task<NamedPipeMessage> ReadMessageAsync(CancellationToken cancellationToken = default)
            {
                if (!_readOnce && message is not null)
                {
                    _readOnce = true;
                    return Task.FromResult(message);
                }

                return Task.Run(async () =>
                {
                    await Task.Delay(Timeout.Infinite, cancellationToken);
                    return default(NamedPipeMessage)!;
                },
                cancellationToken);
            }

            public Task SendMessageAsync(NamedPipeMessage message, CancellationToken cancellationToken = default)
                => Task.CompletedTask;

            public void Dispose() => IsConnected = false;
        }
    }

    private static ILogger<PipeClient> GetLoggerSubstitute()
    {
        var logger = Substitute.For<ILogger<PipeClient>>();
        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        return logger;
    }

    private static ILogger GetUntypedLoggerSubstitute()
    {
        var logger = Substitute.For<ILogger>();
        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        return logger;
    }
}
