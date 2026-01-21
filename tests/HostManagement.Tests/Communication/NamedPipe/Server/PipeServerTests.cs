using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipes;
using System.Text.Json.Serialization.Metadata;
using AwesomeAssertions;
using HostManagement.Communication.NamedPipe.RequestHandlers;
using HostManagement.Communication.NamedPipe.Server;
using HostManagement.Configuration;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Capabilities;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Communication.NamedPipe;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace HostManagement.Tests.Communication.NamedPipe.Server;

public class PipeServerTests
{
    private const string TestPipeName = "test-pipe";

    private const string EventTopic = EventTopics.PipeServerShutdown;
    private const string EventContent = "Test event content";

    private const string RequestTopic = Topics.GetNetworkStatusInformation;
    private const string RequestContent = "Test request content";

    private static readonly TimeSpan s_defaultBarrierTimeout = TimeSpan.FromSeconds(1);

    public class Constructor
    {
        [Fact]
        public void ShouldThrow_WhenPipeNameIsNull()
        {
            var supportedTopics = new SupportedTopics();
            var loggerSubstitute = GetLoggerSubstitute();

            var registry = new RequestHandlerRegistry(
                TargetEnvironment.EdgeSViciOne,
                supportedTopics,
                [],
                loggerSubstitute);

            var func = () => new PipeServer(null!, loggerSubstitute, registry);
            func.Should().Throw<ArgumentNullException>().WithParameterName("pipeName");
        }

        [Fact]
        public void ShouldThrow_WhenLoggerIsNull()
        {
            var supportedTopics = new SupportedTopics();
            var loggerSubstitute = GetLoggerSubstitute();

            var registry = new RequestHandlerRegistry(
                TargetEnvironment.EdgeSViciOne,
                supportedTopics,
                [],
                loggerSubstitute);

            var func = () => new PipeServer(TestPipeName, null!, registry);
            func.Should().Throw<ArgumentNullException>().WithParameterName("logger");
        }

        [Fact]
        public void ShouldThrow_WhenRequestHandlerRegistryIsNull()
        {
            var loggerSubstitute = GetLoggerSubstitute();

            var func = () => new PipeServer(TestPipeName, loggerSubstitute, null!);
            func.Should().Throw<ArgumentNullException>().WithParameterName("requestHandlerRegistry");
        }

        [Fact]
        public async Task ShouldCreatePipeServer_WhenParamsGiven()
        {
            var expectedPipePath = Path.Combine(AppPaths.TempPath, $"CoreFxPipe_{TestPipeName}");

            await using var context = new PipeServerContext(new TestNotifyHandler());
            context.Server.PipePath.Should().Be(expectedPipePath);
        }
    }

    public class StateProperty
    {
        [Fact]
        public async Task ShouldSetStateToDegraded_WhenListenerBecomesDegraded()
        {
            await using var context = new PipeServerContext();
            PipeServerTestAccessors.SetServerState(context.Server, nameof(ServerState.Started));

            PipeServerTestAccessors.IncrementDegradedListenerCount(context.Server);
            PipeServerTestAccessors.InvokeSetOperationalState(context.Server);
            context.Server.State.Should().Be(ServerState.Degraded);
        }

        [Fact]
        public async Task ShouldRemainDegraded_WhenAnotherListenerIsStillDegraded()
        {
            await using var context = new PipeServerContext();
            PipeServerTestAccessors.SetServerState(context.Server, nameof(ServerState.Started));

            PipeServerTestAccessors.IncrementDegradedListenerCount(context.Server);
            PipeServerTestAccessors.IncrementDegradedListenerCount(context.Server);
            PipeServerTestAccessors.InvokeSetOperationalState(context.Server);

            PipeServerTestAccessors.DecrementDegradedListenerCount(context.Server);
            PipeServerTestAccessors.InvokeSetOperationalState(context.Server);
            context.Server.State.Should().Be(ServerState.Degraded);
        }

        [Fact]
        public async Task ShouldRecoverToStarted_WhenLastDegradedListenerRecovers()
        {
            await using var context = new PipeServerContext();
            PipeServerTestAccessors.SetServerState(context.Server, nameof(ServerState.Started));

            PipeServerTestAccessors.IncrementDegradedListenerCount(context.Server);
            PipeServerTestAccessors.InvokeSetOperationalState(context.Server);
            context.Server.State.Should().Be(ServerState.Degraded);

            PipeServerTestAccessors.DecrementDegradedListenerCount(context.Server);
            PipeServerTestAccessors.InvokeSetOperationalState(context.Server);
            context.Server.State.Should().Be(ServerState.Started);
        }

        [Theory]
        [InlineData(nameof(ServerState.Stopping))]
        [InlineData(nameof(ServerState.Stopped))]
        [InlineData(nameof(ServerState.Disposing))]
        [InlineData(nameof(ServerState.Disposed))]
        public async Task ShouldNotChangeState_WhenListenerHealthChangesInTerminalState(string serverState)
        {
            await using var context = new PipeServerContext();
            PipeServerTestAccessors.SetServerState(context.Server, serverState);

            PipeServerTestAccessors.IncrementDegradedListenerCount(context.Server);
            PipeServerTestAccessors.InvokeSetOperationalState(context.Server);

            PipeServerTestAccessors.DecrementDegradedListenerCount(context.Server);
            PipeServerTestAccessors.InvokeSetOperationalState(context.Server);
            context.Server.State.Should().Be(Enum.Parse<ServerState>(serverState));
        }
    }

    public class DisposeAsync
    {
        [Fact]
        public async Task ShouldSetStateToDisposedAndNotThrow_WhenDisposedMultipleTimes()
        {
            await using var context = new PipeServerContext(new TestHandler());

            await context.Server.DisposeAsync();
            context.Server.State.Should().Be(ServerState.Disposed);

            var secondDispose = () => context.Server.DisposeAsync().AsTask();
            await secondDispose.Should().NotThrowAsync();
            context.Server.State.Should().Be(ServerState.Disposed);
        }
    }

    public class PublishEvent
    {
        [Fact]
        public async Task ShouldNotEnqueueEvent_WhenCancelled()
        {
            await using var context = new ConnectedPipeServerContext();
            using var cts = new CancellationTokenSource();
            await cts.CancelAsync();

            await using (var scope = new QueueProcessingScope(context.Server))
            {
                await PublishEventAndDrainAsync(context.Server, cts.Token);
            }

            context.Client.TryDequeueSent(out _).Should().BeFalse();
        }

        [Fact]
        public async Task ShouldNotEnqueueEvent_WhenServerIsStopping()
        {
            await using var context = new ConnectedPipeServerContext();
            PipeServerTestAccessors.SetServerState(context.Server, nameof(ServerState.Stopping));

            await using (var scope = new QueueProcessingScope(context.Server))
            {
                await PublishEventAndDrainAsync(context.Server, CancellationToken.None);
            }

            context.Client.TryDequeueSent(out _).Should().BeFalse();
        }

        [Theory]
        [InlineData(nameof(ServerState.Stopped))]
        [InlineData(nameof(ServerState.Disposing))]
        [InlineData(nameof(ServerState.Disposed))]
        public async Task ShouldNotEnqueueEvent_WhenServerIsInTerminalState(string serverState)
        {
            await using var context = new ConnectedPipeServerContext();
            PipeServerTestAccessors.SetServerState(context.Server, serverState);

            await using (var scope = new QueueProcessingScope(context.Server))
            {
                await PublishEventAndDrainAsync(context.Server, CancellationToken.None);
            }

            context.Client.TryDequeueSent(out _).Should().BeFalse();
        }

        [Fact]
        public async Task ShouldEnqueueAndSendEvent_WhenClientConnected()
        {
            // Arrange
            await using var context = new ConnectedPipeServerContext();

            // Act
            await using (var scope = new QueueProcessingScope(context.Server))
            {
                await PublishEventAndDrainAsync(context.Server, CancellationToken.None);
            }

            // Assert
            context.Client.TryDequeueSent(out var namedPipeMessage).Should().BeTrue();
            AssertExpectedNamedPipeEvent(namedPipeMessage, EventTopic, Guid.Empty, EventContent);
        }

        [Fact]
        public async Task ShouldSendEventToExistingClient_WhenAllListenersAreDegraded()
        {
            // Arrange
            await using var context = new ConnectedPipeServerContext();
            PipeServerTestAccessors.IncrementDegradedListenerCount(context.Server);
            PipeServerTestAccessors.IncrementDegradedListenerCount(context.Server);
            PipeServerTestAccessors.InvokeSetOperationalState(context.Server);

            context.Server.State.Should().Be(ServerState.Degraded);

            // Act
            await using (var scope = new QueueProcessingScope(context.Server))
            {
                await PublishEventAndDrainAsync(context.Server, CancellationToken.None);
            }

            // Assert
            context.Client.TryDequeueSent(out var namedPipeMessage).Should().BeTrue();
            AssertExpectedNamedPipeEvent(namedPipeMessage, EventTopic, Guid.Empty, EventContent);
        }

        private static async Task PublishEventAndDrainAsync(PipeServer server, CancellationToken cancellationToken)
        {
            await server.PublishEvent(EventTopic, EventContent, cancellationToken);
            await PipeServerTestAccessors.InvokeEnqueueShutdownBarrierAndWaitAsync(server, s_defaultBarrierTimeout);
        }
    }

    public class StartAsync
    {
        private const int MaxInstances = 2;
        private const string PipeServerCanOnlyStartOnce = "Pipe server can only be started once.";
        private const string PipeServerDegraded = "Pipe server degraded.";
        private const string PipeServerStarted = "Pipe server started.";

        [Fact]
        public async Task ShouldThrow_WhenStartedMoreThanOnce()
        {
            // Arrange
            await using var context = new PipeServerContext(
                new SuccessfulPipeServerStreamFactory(),
                new SuccessfulPipeServerInitializer());

            await context.Server.StartAsync();

            // Act + Assert
            var secondStart = () => context.Server.StartAsync();
            await secondStart.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage(PipeServerCanOnlyStartOnce);
        }

        [Fact]
        public async Task ShouldThrow_WhenStartedAfterStop()
        {
            // Arrange
            await using var context = new PipeServerContext(
                new SuccessfulPipeServerStreamFactory(),
                new SuccessfulPipeServerInitializer());

            await context.Server.StartAsync();
            await context.Server.StopAsync();

            // Act + Assert
            var restart = () => context.Server.StartAsync();
            await restart.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage(PipeServerCanOnlyStartOnce);
        }

        [Fact]
        public async Task ShouldThrow_WhenStartedAfterDispose()
        {
            // Arrange
            await using var context = new PipeServerContext(
                new SuccessfulPipeServerStreamFactory(),
                new SuccessfulPipeServerInitializer());

            await context.Server.StartAsync();
            await context.Server.DisposeAsync();

            // Act + Assert
            var restart = () => context.Server.StartAsync();
            await restart.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage(PipeServerCanOnlyStartOnce);
        }

        [Fact]
        public async Task ShouldSetStateToDegraded_WhenPipeCreationFails()
        {
            // Arrange
            await using var context = new PipeServerContext(
                new ThrowingPipeServerStreamFactory(),
                new SuccessfulPipeServerInitializer());

            // Act + Assert
            await context.Server.StartAsync();
            context.Server.State.Should().Be(ServerState.Degraded);
            AssertLogReceived(context.Logger, LogLevel.Warning, PipeServerDegraded, MaxInstances);
        }

        [Fact]
        public async Task ShouldSetStateToDegraded_WhenPipeInitializationFails()
        {
            // Arrange
            await using var context = new PipeServerContext(
                new SuccessfulPipeServerStreamFactory(),
                new ThrowingPipeServerInitializer());

            // Act + Assert
            await context.Server.StartAsync();
            context.Server.State.Should().Be(ServerState.Degraded);
            AssertLogReceived(context.Logger, LogLevel.Warning, PipeServerDegraded, MaxInstances);
        }

        [Fact]
        public async Task ShouldSetStateToStarted_WhenPipeCreationSucceedsAndInitializationCompletes()
        {
            // Arrange
            await using var context = new PipeServerContext(
                new SuccessfulPipeServerStreamFactory(),
                new SuccessfulPipeServerInitializer());

            // Act + Assert
            await context.Server.StartAsync();
            context.Server.State.Should().Be(ServerState.Started);
            AssertLogReceived(context.Logger, LogLevel.Information, PipeServerStarted);
        }

        private sealed class ThrowingPipeServerInitializer : IPipeServerInitializer
        {
            public Task InitializeAsync(PipeServer pipeServer, CancellationToken cancellationToken)
                => throw new IOException("Initialization failed.");
        }

        private sealed class ThrowingPipeServerStreamFactory : IPipeServerStreamFactory
        {
            public NamedPipeServerStream Create(string pipeName, int maxNumberOfServerInstances, PipeOptions? options)
                => throw new IOException("Pipe creation failed.");

            public IPipeStreamWrapper CreateWrapper(NamedPipeServerStream pipeServerStream, ILogger logger)
                => throw new NotSupportedException("Not used in this test.");
        }
    }

    public class StopAsync
    {
        [Fact]
        public async Task ShouldBroadcastShutdownEvent_WhenStopAsyncCalled()
        {
            // Arrange
            await using var context = new ConnectedPipeServerContext();

            var expectedContent = "Pipe server is stopping";
            var expectedServerLog = "Pipe server stopped.";

            // Act
            await using (var scope = new QueueProcessingScope(context.Server))
            {
                await context.Server.StopAsync();
            }

            // Assert
            context.Client.TryDequeueSent(out var namedPipeMessage).Should().BeTrue();

            AssertExpectedNamedPipeEvent(namedPipeMessage, EventTopics.PipeServerShutdown, Guid.Empty, expectedContent);
            AssertLogReceived(context.Logger, LogLevel.Information, expectedServerLog);
        }

        [Fact]
        public async Task ShouldLogTimeout_WhenShutdownBroadcastCannotDrain()
        {
            // Arrange
            await using var context = new ConnectedPipeServerContext();

            var expectedServerLog = "Wait for shutdown broadcast timed out, some clients may not have received it.";

            // Act + Assert
            // ProcessQueueAsync was not started, so no process of barrier item -> timeout expected
            await context.Server.StopAsync();
            context.Client.TryDequeueSent(out _).Should().BeFalse();

            AssertLogReceived(context.Logger, LogLevel.Debug, expectedServerLog);
        }

        [Fact]
        public async Task ShouldLogError_WhenStopAsyncFails()
        {
            // Arrange
            await using var context = new PipeServerContext();
            var expectedServerLog = "Stopping pipe server failed.";

            AddFaultedBackgroundTask(context.Server, new InvalidOperationException("boom"));

            // Act + Assert
            await context.Server.StopAsync();

            AssertLogReceived<Exception>(context.Logger, LogLevel.Error, expectedServerLog);
        }
    }

    public class WaitForExitAsync
    {
        [Fact]
        public async Task ShouldLogCancel_WhenWaitForExitIsCancelled()
        {
            // Arrange
            await using var context = new PipeServerContext();
            using var cts = new CancellationTokenSource();

            var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            PipeServerTestAccessors.AddBackgroundTask(context.Server, tcs.Task); // Add dummy task

            var expectedServerLog = "Wait for pipe server exit has been canceled.";

            // Act
            await cts.CancelAsync();
            var func = () => context.Server.WaitForExitAsync(cts.Token);

            // Assert
            await func.Should().NotThrowAsync();

            AssertLogReceived(context.Logger, LogLevel.Debug, expectedServerLog);

            // Cleanup
            tcs.TrySetResult();
        }

        [Fact]
        public async Task ShouldLogError_WhenWaitForExitThrows()
        {
            // Arrange
            await using var context = new PipeServerContext();
            var expectedServerLog = "Wait for pipe server exit failed.";

            AddFaultedBackgroundTask(context.Server, new InvalidOperationException("boom"));

            // Act + Assert
            var func = () => context.Server.WaitForExitAsync();
            await func.Should().NotThrowAsync();

            AssertLogReceived<Exception>(context.Logger, LogLevel.Error, expectedServerLog);
        }
    }

    public class ProcessQueueAsync
    {
        private const string QueueMissingMessage = "Queue item was missing message, pipe wrapper or origin.";
        private const string QueueProcessingFailed = "Processing message queue failed.";

        [Fact]
        public async Task ShouldLogError_WhenSendingToDisposedPipe()
        {
            // Arrange
            await using var context = new ConnectedPipeServerContext(new TestHandler());

            var pipeMessageRequest = CreatePipeMessageRequest(RequestTopic, RequestContent);
            var expectedServerLog = "Tried to send response to a disposed pipe.";

            // Act
            await using (var scope = new QueueProcessingScope(context.Server))
            {
                context.Client.Dispose();
                await EnqueueAndDrain(context.Server, pipeMessageRequest, context.Client);
            }

            // Assert
            // Client did not receive response
            context.Client.TryDequeueSent(out _).Should().BeFalse();

            // Server has logged disposed pipe
            AssertLogReceived<ObjectDisposedException>(context.Logger, LogLevel.Error, expectedServerLog);
        }

        [Fact]
        public async Task ShouldLogError_WhenStreamWrapperThrows()
        {
            // Arrange
            await using var context = new PipeServerContext();
            using var throwingClient = new ThrowingOnSendPipeStreamWrapper();

            var request = CreatePipeMessageRequest(RequestTopic, RequestContent);

            // Act + Assert
            await using (var scope = new QueueProcessingScope(context.Server))
            {
                await EnqueueAndDrain(context.Server, request, throwingClient, QueueMessageOrigin.Server);
            }

            AssertLogReceived<Exception>(context.Logger, LogLevel.Error, QueueProcessingFailed);
        }

        [Fact]
        public async Task ShouldContinueProcessingAndLogError_WhenSinglePipeSendCancellationOccurs()
        {
            // Arrange
            await using var context = new PipeServerContext();
            using var throwingClient = new OperationCanceledOnSendPipeStreamWrapper();
            using var receivingClient = new FakePipeStreamWrapper();

            var firstEvent = CreatePipeMessageEvent(EventTopic, "first");
            var secondEvent = CreatePipeMessageEvent(EventTopic, "second");

            // Act
            await using (var scope = new QueueProcessingScope(context.Server))
            {
                PipeServerTestAccessors.InvokeEnqueueServerMessage(context.Server, firstEvent, throwingClient);
                PipeServerTestAccessors.InvokeEnqueueServerMessage(context.Server, secondEvent, receivingClient);

                await PipeServerTestAccessors
                    .InvokeEnqueueShutdownBarrierAndWaitAsync(context.Server, s_defaultBarrierTimeout);
            }

            // Assert
            receivingClient.TryDequeueSent(out var message).Should().BeTrue();
            AssertExpectedNamedPipeEvent(message, secondEvent.Topic, secondEvent.ConversationId, secondEvent.Content);

            throwingClient.SendAttempts.Should().Be(1);
            AssertLogReceived<OperationCanceledException>(context.Logger, LogLevel.Error, QueueProcessingFailed);
        }

        [Fact]
        public async Task ShouldNotThrow_WhenCancelled()
        {
            // Arrange
            await using var context = new PipeServerContext();
            using var cts = new CancellationTokenSource();

            var expectedServerLog = "Processing message queue has been canceled.";

            await cts.CancelAsync();

            // Act + Assert
            var func = () => PipeServerTestAccessors.InvokeProcessQueueAsync(context.Server, cts.Token);
            await func.Should().NotThrowAsync();

            AssertLogReceived(context.Logger, LogLevel.Debug, expectedServerLog);
        }

        [Fact]
        public async Task ShouldLogDebug_WhenQueueItemMissingMessage()
        {
            // Arrange
            await using var context = new ConnectedPipeServerContext();

            // Act + Assert
            await using (var scope = new QueueProcessingScope(context.Server))
            {
                await EnqueueAndDrain(context.Server, message: null!, context.Client);
            }

            context.Client.TryDequeueSent(out _).Should().BeFalse();
            AssertLogReceived(context.Logger, LogLevel.Debug, QueueMissingMessage);
        }

        [Fact]
        public async Task ShouldLogDebug_WhenQueueItemMissingWrapper()
        {
            // Arrange
            await using var context = new PipeServerContext();

            var pipeMessageRequest = CreatePipeMessageRequest(RequestTopic, RequestContent);

            // Act + Assert
            await using (var scope = new QueueProcessingScope(context.Server))
            {
                await EnqueueAndDrain(context.Server, pipeMessageRequest, wrapper: null!);
            }

            AssertLogReceived(context.Logger, LogLevel.Debug, QueueMissingMessage);
        }

        [Fact]
        public async Task ShouldNotSendResponse_WhenNormalMessageIsEnqueuedWhileStopping()
        {
            // Arrange
            await using var context = new ConnectedPipeServerContext(new TestHandler());

            var pipeMessageRequest = CreatePipeMessageRequest(RequestTopic, RequestContent);

            PipeServerTestAccessors.SetServerState(context.Server, nameof(ServerState.Stopping));

            // Act + Assert
            await using (var scope = new QueueProcessingScope(context.Server))
            {
                await EnqueueAndDrain(context.Server, pipeMessageRequest, context.Client);
            }

            context.Client.TryDequeueSent(out _).Should().BeFalse();
        }

        [Fact]
        public async Task ShouldSendResponse_WhenRequestEnqueued()
        {
            // Arrange
            await using var context = new ConnectedPipeServerContext(new TestHandler());

            var request = CreatePipeMessageRequest(RequestTopic, RequestContent);
            var responseContent = CreateEchoResponseContent(RequestContent);

            var expectedServerLog = CreateResponseSentLog(request.Topic, request.ConversationId);
            var expectedServerDebugLog = CreateResponseContentLog(request.ConversationId, responseContent);

            // Act
            await using (var scope = new QueueProcessingScope(context.Server))
            {
                await EnqueueAndDrain(context.Server, request, context.Client);
            }

            // Assert
            // Client received response
            context.Client.TryDequeueSent(out var namedPipeMessage).Should().BeTrue();

            AssertExpectedNamedPipeResponse(namedPipeMessage, request.Topic, request.ConversationId, responseContent);

            // Server has logged response
            AssertLogReceived(context.Logger, LogLevel.Information, expectedServerLog);
            AssertLogReceived(context.Logger, LogLevel.Debug, expectedServerDebugLog);
        }

        [Fact]
        public async Task ShouldSendResponseAndEvent_WhenRequestTriggersPublishEventRequested()
        {
            // Arrange
            await using var context = new ConnectedPipeServerContext(new TestNotifyHandler());

            var request = CreatePipeMessageRequest(RequestTopic, RequestContent);
            var responseContent = CreateEchoResponseContent(RequestContent);

            var expectedServerResponseLog = CreateResponseSentLog(request.Topic, request.ConversationId);
            var expectedServerResponseDebugLog = CreateResponseContentLog(request.ConversationId, responseContent);

            var expectedServerEventLog = $"Event with topic '{EventTopic}' sent.";
            var expectedServerEventDebugLog = $"Event content was: {EventContent}";

            // Act
            await using (var scope = new QueueProcessingScope(context.Server))
            {
                await EnqueueAndDrain(context.Server, request, context.Client);
            }

            // Assert
            var responseAndEventMessages = new List<NamedPipeMessage>(capacity: 2);

            if (context.Client.TryDequeueSent(out var message1))
                responseAndEventMessages.Add(message1);

            if (context.Client.TryDequeueSent(out var message2))
                responseAndEventMessages.Add(message2);

            responseAndEventMessages.Should().HaveCount(2);

            // Client received response and event
            responseAndEventMessages.Should().ContainSingle(namedPipeMessage =>
                namedPipeMessage.Type == MessageType.Response &&
                namedPipeMessage.Topic == request.Topic &&
                namedPipeMessage.ConversationId == request.ConversationId &&
                namedPipeMessage.Content == responseContent);

            responseAndEventMessages.Should().ContainSingle(namedPipeMessage =>
                namedPipeMessage.Type == MessageType.Event &&
                namedPipeMessage.Topic == EventTopic &&
                namedPipeMessage.ConversationId == Guid.Empty &&
                namedPipeMessage.Content == EventContent);

            // Server has logged response and event
            AssertLogReceived(context.Logger, LogLevel.Information, expectedServerResponseLog);
            AssertLogReceived(context.Logger, LogLevel.Debug, expectedServerResponseDebugLog);

            AssertLogReceived(context.Logger, LogLevel.Information, expectedServerEventLog);
            AssertLogReceived(context.Logger, LogLevel.Debug, expectedServerEventDebugLog);
        }

        [Theory]
        [InlineData(RequestTopic, MessageType.Event, "{\"Message\":\"Unsupported message type. Message type \\u0027Event\\u0027 may only be sent by the server.\"}")]
        [InlineData(RequestTopic, MessageType.Response, "{\"Message\":\"Unsupported message type. Message type \\u0027Response\\u0027 may only be sent by the server.\"}")]
        public async Task ShouldSendDefaultResponse_WhenRequestHasInvalidMessageType(string topic, MessageType type, string expectedMessage)
        {
            // Arrange
            await using var context = new ConnectedPipeServerContext();

            var request = new NamedPipeMessage
            {
                Topic = topic,
                Content = RequestContent,
                Type = type
            };

            var expectedServerLog =
                $"Default response with topic '{request.Topic}' sent " +
                $"(Conversation ID: {request.ConversationId}).";
            var expectedServerDebugLog =
                $"Default response content for conversation ID '{request.ConversationId}' " +
                $"was: {expectedMessage}";

            // Act
            await using (var scope = new QueueProcessingScope(context.Server))
            {
                await EnqueueAndDrain(context.Server, request, context.Client);
            }

            // Client received default response
            context.Client.TryDequeueSent(out var namedPipeMessage).Should().BeTrue();

            AssertExpectedNamedPipeResponse(namedPipeMessage, request.Topic, request.ConversationId, expectedMessage);

            // Server has logged default response
            AssertLogReceived(context.Logger, LogLevel.Information, expectedServerLog);
            AssertLogReceived(context.Logger, LogLevel.Debug, expectedServerDebugLog);
        }

        [Fact]
        public async Task ShouldSendErrorResponse_WhenRequestHasInvalidTopic()
        {
            // Arrange
            await using var context = new ConnectedPipeServerContext();

            var request = new NamedPipeMessage
            {
                Topic = EventTopics.PipeServerShutdown,
                Content = RequestContent,
                Type = MessageType.Request
            };

            var expectedMessage =
                $"{{\"Message\":\"Handle request for topic \\u0027{request.Topic}\\u0027 failed " +
                $"(Conversation ID: {request.ConversationId}). Topic is invalid.\"}}";

            var expectedServerLog = CreateResponseSentLog(request.Topic, request.ConversationId);
            var expectedServerDebugLog = CreateResponseContentLog(request.ConversationId, expectedMessage);

            // Act
            await using (var scope = new QueueProcessingScope(context.Server))
            {
                await EnqueueAndDrain(context.Server, request, context.Client);
            }

            // Client received default response
            context.Client.TryDequeueSent(out var namedPipeMessage).Should().BeTrue();

            AssertExpectedNamedPipeResponse(namedPipeMessage, request.Topic, request.ConversationId, expectedMessage);

            // Server has logged default response
            AssertLogReceived(context.Logger, LogLevel.Information, expectedServerLog);
            AssertLogReceived(context.Logger, LogLevel.Debug, expectedServerDebugLog);
        }

        [Fact]
        public async Task ShouldSendDefaultResponse_WhenHandlerThrowsInProcessRequest()
        {
            // Arrange
            await using var context = new ConnectedPipeServerContext(new ThrowingHandler());

            var request = CreatePipeMessageRequest(RequestTopic, RequestContent);
            var responseContent =
                $"{{\"Message\":\"Handle request for topic \\u0027{request.Topic}\\u0027 failed " +
                $"(Conversation ID: {request.ConversationId}). Handler failed.\"}}";

            var expectedServerLog = CreateResponseSentLog(request.Topic, request.ConversationId);
            var expectedServerDebugLog = CreateResponseContentLog(request.ConversationId, responseContent);

            // Act
            await using (var scope = new QueueProcessingScope(context.Server))
            {
                await EnqueueAndDrain(context.Server, request, context.Client);
            }

            // Assert
            // Client received default response
            context.Client.TryDequeueSent(out var namedPipeMessage).Should().BeTrue();

            AssertExpectedNamedPipeResponse(namedPipeMessage, request.Topic, request.ConversationId, responseContent);

            // Server has logged response
            AssertLogReceived(context.Logger, LogLevel.Information, expectedServerLog);
            AssertLogReceived(context.Logger, LogLevel.Debug, expectedServerDebugLog);
        }

        [Fact]
        public async Task ShouldCompleteBarrier_WhenQueueDrained()
        {
            // Arrange
            await using var context = new ConnectedPipeServerContext();

            var pipeEvent = CreatePipeMessageEvent(EventTopic, EventContent);

            // Act
            await using (var scope = new QueueProcessingScope(context.Server))
            {
                await EnqueueAndDrain(context.Server, pipeEvent, context.Client, QueueMessageOrigin.Server);
            }

            // Assert
            context.Client.TryDequeueSent(out var namedPipeMessage).Should().BeTrue();

            AssertExpectedNamedPipeEvent(namedPipeMessage, pipeEvent.Topic, pipeEvent.ConversationId, EventContent);
        }

        private enum QueueMessageOrigin
        {
            Client,
            Server
        }

        private static async Task EnqueueAndDrain(
            PipeServer server,
            NamedPipeMessage message,
            IPipeStreamWrapper wrapper,
            QueueMessageOrigin origin = QueueMessageOrigin.Client)
        {
            switch (origin)
            {
                case QueueMessageOrigin.Client:
                    PipeServerTestAccessors.InvokeEnqueueClientMessage(server, message, wrapper);
                    break;

                case QueueMessageOrigin.Server:
                    PipeServerTestAccessors.InvokeEnqueueServerMessage(server, message, wrapper);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(origin), origin, null);
            }

            await PipeServerTestAccessors.InvokeEnqueueShutdownBarrierAndWaitAsync(server, s_defaultBarrierTimeout);
        }

        private sealed class OperationCanceledOnSendPipeStreamWrapper : IPipeStreamWrapper
        {
            public bool IsConnected { get; private set; } = true;

            public int SendAttempts { get; private set; }

            public Task<NamedPipeMessage> ReadMessageAsync(CancellationToken cancellationToken = default)
                => throw new NotSupportedException("Not used in these tests.");

            public Task SendMessageAsync(NamedPipeMessage message, CancellationToken cancellationToken = default)
            {
                SendAttempts++;
                throw new OperationCanceledException("Client pipe closed.");
            }

            public void Dispose() => IsConnected = false;
        }

        private sealed class ThrowingOnSendPipeStreamWrapper : IPipeStreamWrapper
        {
            public bool IsConnected { get; set; } = true;

            public Task<NamedPipeMessage> ReadMessageAsync(CancellationToken cancellationToken = default)
                => throw new NotSupportedException("Not used in these tests.");

            public Task SendMessageAsync(NamedPipeMessage message, CancellationToken cancellationToken = default)
                => throw new InvalidOperationException("Send failed.");

            public void Dispose() => IsConnected = false;
        }

        private sealed class ThrowingHandler : IRequestHandler
        {
            // Must be a valid topic, so the handler is supported, otherwise 'CreateErrorInstance' will throw here
            public string Topic => RequestTopic;

            public JsonTypeInfo JsonTypeInfo => throw new NotImplementedException();

            public OperationResult CreateErrorInstance(string message) => throw new NotImplementedException();

            public Task<string> HandleAsync(string messageContent, CancellationToken cancellationToken)
                => throw new InvalidOperationException("Handler failed.");
        }
    }

    public class ReadRequestsAsync
    {
        [SuppressMessage(
            "Reliability",
            "CA2025:Ensure tasks using 'IDisposable' instances are complete before disposing the resources",
            Justification = "False positive in test code. The task is awaited before the test wrapper is disposed.")]
        [Fact]
        public async Task ShouldLogClientDisconnected_WhenCancellationOccursDuringRead()
        {
            // Arrange
            await using var context = new PipeServerContext();
            var readableFake = new ReadableFakePipeStreamWrapper();
            using var cts = new CancellationTokenSource();

            var instanceId = 1;
            var expectedServerLog = $"Client disconnected from pipe server instance {instanceId}.";
            var expectedServerDebugLog = $"Disposed pipe server instance {instanceId}.";

            var readTask = Task.CompletedTask;

            try
            {
                // Act
                readTask = PipeServerTestAccessors
                    .InvokeReadRequestsAsync(context.Server, readableFake, instanceId, cts.Token);

                await readableFake.WaitForReadStartedAsync(TimeSpan.FromSeconds(1));
                await cts.CancelAsync();
                await readTask;

                // Assert
                AssertLogReceived(context.Logger, LogLevel.Information, expectedServerLog);
                AssertLogReceived(context.Logger, LogLevel.Debug, expectedServerDebugLog);
            }
            finally
            {
                await cts.CancelAsync();
                await readTask;
                readableFake.Dispose();
            }
        }

        [SuppressMessage(
            "Reliability",
            "CA2025:Ensure tasks using 'IDisposable' instances are complete before disposing the resources",
            Justification = "False positive in test code. The background tasks are awaited before the test wrapper is disposed.")]
        [Fact]
        public async Task ShouldSendResponse_WhenClientSendsRequest()
        {
            // Arrange
            await using var context = new PipeServerContext(new TestHandler());
            var readableFake = new ReadableFakePipeStreamWrapper();

            var request = CreatePipeMessageRequest(RequestTopic, RequestContent);
            var responseContent = CreateEchoResponseContent(RequestContent);

            var instanceId = 1;
            var expectedServerLog =
                $"Request with topic '{request.Topic}' received (Conversation ID: {request.ConversationId}).";
            var expectedServerDebugLog =
                $"Request content on pipe server instance {instanceId} for conversation ID " +
                $"'{request.ConversationId}' was: {RequestContent}";

            using var cts = new CancellationTokenSource();
            var queueTask = Task.CompletedTask;
            var readTask = Task.CompletedTask;

            try
            {
                queueTask = PipeServerTestAccessors
                    .InvokeProcessQueueAsync(context.Server, cts.Token);

                readTask = PipeServerTestAccessors
                    .InvokeReadRequestsAsync(context.Server, readableFake, instanceId, cts.Token);

                // Act
                readableFake.EnqueueIncoming(request);

                var message = await readableFake.WaitForSentAsync(TimeSpan.FromSeconds(2));
                message.Should().NotBeNull();

                // Assert
                AssertExpectedNamedPipeResponse(message, request.Topic, request.ConversationId, responseContent);

                AssertLogReceived(context.Logger, LogLevel.Information, expectedServerLog);
                AssertLogReceived(context.Logger, LogLevel.Debug, expectedServerDebugLog);
            }
            finally
            {
                await cts.CancelAsync();
                await Task.WhenAll(queueTask, readTask);
                readableFake.Dispose();
            }
        }

        [Fact]
        public async Task ShouldLogError_WhenReadThrows()
        {
            // Arrange
            await using var context = new PipeServerContext();
            using var throwingOnRead = new ThrowingOnReadPipeStreamWrapper();
            using var cts = new CancellationTokenSource();

            var expectedServerErrorLog = "Reading pipe request on pipe server instance 42 failed.";

            // Act + Assert
            await PipeServerTestAccessors.InvokeReadRequestsAsync(context.Server, throwingOnRead, 42, cts.Token);

            AssertLogReceived<Exception>(context.Logger, LogLevel.Error, expectedServerErrorLog);
        }

        private sealed class ReadableFakePipeStreamWrapper : IPipeStreamWrapper
        {
            private readonly BlockingCollection<NamedPipeMessage> _incoming = new(new ConcurrentQueue<NamedPipeMessage>());
            private readonly TaskCompletionSource _readStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
            private readonly ConcurrentQueue<NamedPipeMessage> _sent = new();

            public bool IsConnected { get; private set; } = true;

            public void EnqueueIncoming(NamedPipeMessage message) => _incoming.Add(message);

            public bool TryDequeueSent(out NamedPipeMessage message) => _sent.TryDequeue(out message!);

            public Task WaitForReadStartedAsync(TimeSpan timeout)
                => _readStarted.Task.WaitAsync(timeout);

            public Task<NamedPipeMessage> ReadMessageAsync(CancellationToken cancellationToken = default)
                => Task.Run(
                    () =>
                    {
                        _readStarted.TrySetResult();
                        return _incoming.Take(cancellationToken);
                    },
                    CancellationToken.None);

            public Task SendMessageAsync(NamedPipeMessage message, CancellationToken cancellationToken = default)
            {
                ObjectDisposedException.ThrowIf(!IsConnected, this);
                _sent.Enqueue(message);
                return Task.CompletedTask;
            }

            public async Task<NamedPipeMessage?> WaitForSentAsync(TimeSpan timeout)
            {
                var stopwatch = Stopwatch.StartNew();

                while (stopwatch.Elapsed < timeout)
                {
                    if (_sent.TryDequeue(out var message))
                        return message;

                    await Task.Delay(10);
                }

                return null;
            }

            public void Dispose()
            {
                IsConnected = false;
                _incoming.Dispose();
            }
        }

        private sealed class ThrowingOnReadPipeStreamWrapper : IPipeStreamWrapper
        {
            public bool IsConnected { get; private set; } = true;

            public Task<NamedPipeMessage> ReadMessageAsync(CancellationToken cancellationToken = default)
                => throw new InvalidOperationException("Read failed.");

            public Task SendMessageAsync(NamedPipeMessage message, CancellationToken cancellationToken = default)
                => Task.CompletedTask;

            public void Dispose() => IsConnected = false;
        }
    }

    private static void AddFaultedBackgroundTask(PipeServer server, Exception exception)
    {
        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        PipeServerTestAccessors.AddBackgroundTask(server, tcs.Task);
        tcs.TrySetException(exception);
    }

    private static void AssertExpectedNamedPipeEvent(
        NamedPipeMessage message,
        string topic,
        Guid conversationId,
        string content)
        => AssertExpectedNamedPipeMessage(message, MessageType.Event, topic, conversationId, content);

    private static void AssertExpectedNamedPipeResponse(
        NamedPipeMessage message,
        string topic,
        Guid conversationId,
        string content)
        => AssertExpectedNamedPipeMessage(message, MessageType.Response, topic, conversationId, content);

    private static void AssertExpectedNamedPipeMessage(
        NamedPipeMessage message,
        MessageType type,
        string topic,
        Guid conversationId,
        string content)
    {
        message.Type.Should().Be(type);
        message.Topic.Should().Be(topic);
        message.ConversationId.Should().Be(conversationId);
        message.Content.Should().Be(content);
    }

#pragma warning disable CA1873 // Avoid potentially expensive logging
    private static void AssertLogReceived(
        ILogger logger,
        LogLevel logLevel,
        string expectedMessage,
        int expectedCount = 1)
    {
        logger.Received(expectedCount).Log(
            logLevel,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains(expectedMessage)),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    private static void AssertLogReceived<TException>(ILogger logger, LogLevel logLevel, string expectedMessage)
        where TException : Exception
    {
        logger.Received(1).Log(
            logLevel,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains(expectedMessage)),
            Arg.Any<TException>(),
            Arg.Any<Func<object, Exception?, string>>());
    }
#pragma warning restore CA1873

    private static string CreateEchoResponseContent(string content)
        => $"{{\"Message\":\"echo:{content}\"}}";

    private static NamedPipeMessage CreatePipeMessageEvent(string topic, string content)
        => new() { Topic = topic, Content = content, Type = MessageType.Event };

    private static NamedPipeMessage CreatePipeMessageRequest(string topic, string content)
        => new() { Topic = topic, Content = content, Type = MessageType.Request };

    private static string CreateResponseSentLog(string topic, Guid conversationId)
        => $"Response with topic '{topic}' sent (Conversation ID: {conversationId}).";

    private static string CreateResponseContentLog(Guid conversationId, string content)
        => $"Response content for conversation ID '{conversationId}' was: {content}";

    private static ILogger GetLoggerSubstitute()
    {
        var loggerSubstitute = Substitute.For<ILogger>();
        loggerSubstitute.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        return loggerSubstitute;
    }

    private sealed class ConnectedPipeServerContext : PipeServerContext
    {
        public ConnectedPipeServerContext(params IRequestHandler[] handlers)
            : base(handlers)
        {
            Client = new FakePipeStreamWrapper();
            PipeServerTestAccessors.AddActiveConnection(Server, Client);
        }

        public FakePipeStreamWrapper Client { get; }

        public override async ValueTask DisposeAsync()
        {
            Client.Dispose();
            await base.DisposeAsync();
        }
    }

    private sealed class FakePipeStreamWrapper : IPipeStreamWrapper
    {
        private readonly ConcurrentQueue<NamedPipeMessage> _sent = new();

        public bool IsConnected { get; private set; } = true;

        public bool TryDequeueSent(out NamedPipeMessage message) => _sent.TryDequeue(out message!);

        public Task<NamedPipeMessage> ReadMessageAsync(CancellationToken cancellationToken = default)
            => throw new NotSupportedException("Not used in these tests.");

        public Task SendMessageAsync(NamedPipeMessage message, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(!IsConnected, this);
            _sent.Enqueue(message);
            return Task.CompletedTask;
        }

        public void Dispose() => IsConnected = false;
    }

    private class PipeServerContext : IAsyncDisposable
    {
        public PipeServerContext(params IRequestHandler[] handlers)
        {
            Logger = GetLoggerSubstitute();
            Server = CreatePipeServer(Logger, null, null, handlers);
        }

        public PipeServerContext(
            IPipeServerStreamFactory pipeServerStreamFactory,
            IPipeServerInitializer pipeServerInitializer,
            params IRequestHandler[] handlers)
        {
            Logger = GetLoggerSubstitute();
            Server = CreatePipeServer(Logger, pipeServerStreamFactory, pipeServerInitializer, handlers);
        }

        public ILogger Logger { get; }
        public PipeServer Server { get; }

        public virtual async ValueTask DisposeAsync() => await Server.DisposeAsync();

        private static PipeServer CreatePipeServer(
            ILogger logger,
            IPipeServerStreamFactory? pipeServerStreamFactory,
            IPipeServerInitializer? pipeServerInitializer,
            params IRequestHandler[] handlers)
                => new(
                    TestPipeName,
                    logger,
                    new RequestHandlerRegistry(TargetEnvironment.EdgeSViciOne, new SupportedTopics(), handlers, logger),
                    options: null,
                    pipeServerStreamFactory: pipeServerStreamFactory,
                    pipeServerInitializer: pipeServerInitializer);
    }

    private sealed class SuccessfulPipeServerInitializer : IPipeServerInitializer
    {
        public Task InitializeAsync(PipeServer pipeServer, CancellationToken cancellationToken)
            => Task.CompletedTask;
    }

    private sealed class SuccessfulPipeServerStreamFactory : IPipeServerStreamFactory
    {
        public NamedPipeServerStream Create(string pipeName, int maxNumberOfServerInstances, PipeOptions? options)
        {
            var effectiveOptions = options == null
                ? PipeOptions.Asynchronous
                : options.Value | PipeOptions.Asynchronous;

            return new NamedPipeServerStream(
                pipeName,
                PipeDirection.InOut,
                maxNumberOfServerInstances,
                PipeTransmissionMode.Byte,
                effectiveOptions);
        }

        public IPipeStreamWrapper CreateWrapper(NamedPipeServerStream pipeServerStream, ILogger logger)
            => throw new NotSupportedException("Not used in this test.");
    }

    private sealed class TestHandler : IRequestHandler
    {
        public string Topic => RequestTopic;

        public JsonTypeInfo JsonTypeInfo => throw new NotImplementedException();

        public OperationResult CreateErrorInstance(string message) => throw new NotImplementedException();

        public Task<string> HandleAsync(string messageContent, CancellationToken cancellationToken)
            => Task.FromResult(CreateEchoResponseContent(messageContent));
    }

    private sealed class TestNotifyHandler : IRequestHandler, INotifyPublishEventRequested
    {
        public event EventHandler<PublishEventRequestedEventArgs>? PublishEventRequested;

        public string Topic => RequestTopic;

        public JsonTypeInfo JsonTypeInfo => throw new NotImplementedException();

        public Task<string> HandleAsync(string messageContent, CancellationToken cancellationToken)
        {
            PublishEventRequested?.Invoke(
                this,
                new PublishEventRequestedEventArgs(EventTopic, EventContent));

            return Task.FromResult(CreateEchoResponseContent(messageContent));
        }

        public OperationResult CreateErrorInstance(string message) => throw new NotImplementedException();
    }

    private sealed class QueueProcessingScope : IAsyncDisposable
    {
        private readonly CancellationTokenSource _cts = new();
        private readonly Task _queueTask;

        public QueueProcessingScope(PipeServer server)
            => _queueTask = PipeServerTestAccessors.InvokeProcessQueueAsync(server, _cts.Token);

        public async ValueTask DisposeAsync()
        {
            await _cts.CancelAsync();
            await _queueTask;
            _cts.Dispose();
        }
    }
}
