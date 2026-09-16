using System.Text.Json;
using AwesomeAssertions;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Communication.NamedPipe.Client;
using HostManagement.Shared.Serialization;
using Xunit;

namespace HostManagement.Shared.Communication.Tests.NamedPipe.Client;

public class CallbackHandlerRegistryTests
{
    private const string TestTopic = "test";
    private const string ActiveState = "active";

    public class RegisterHandler
    {
        [Fact]
        public void ShouldThrow_WhenHandlerNull()
        {
            // Arrange
            var registry = new CallbackHandlerRegistry();

            // Act
            var act = () => registry.RegisterHandler(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("handler");
        }

        [Fact]
        public void ShouldReturnFalse_WhenTopicAlreadyRegistered()
        {
            // Arrange
            var registry = new CallbackHandlerRegistry();
            registry.RegisterHandler(new RawHandler(TestTopic));

            // Act
            var result = registry.RegisterHandler(new RawHandler(TestTopic));

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ShouldDeserializeMessage_WhenTypedHandlerRegistered()
        {
            // Arrange
            var registry = new CallbackHandlerRegistry();
            var handler = new TypedHandler(TestTopic);

            registry.RegisterHandler(handler);

            // Act
            var result = await registry.HandleCallbackAsync(
                CreateEvent(SerializedServiceStateResult()),
                TestContext.Current.CancellationToken);

            // Assert
            result.Should().BeTrue();
            handler.Received!.ActiveState.Should().Be(ActiveState);
        }
    }

    public class RegisterHandlerWithCallback
    {
        [Fact]
        public void ShouldThrow_WhenTopicNull()
        {
            // Arrange
            var registry = new CallbackHandlerRegistry();

            // Act
            var act = () => registry.RegisterHandler<GetServiceStateResult>(null!, (_, _, _) => Task.CompletedTask);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("topic");
        }

        [Fact]
        public void ShouldThrow_WhenCallbackNull()
        {
            // Arrange
            var registry = new CallbackHandlerRegistry();

            // Act
            var act = () => registry.RegisterHandler<GetServiceStateResult>(TestTopic, null!);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("handleAsync");
        }

        [Fact]
        public async Task ShouldThrow_WhenMessageContentMalformed()
        {
            // Arrange
            var registry = new CallbackHandlerRegistry();
            registry.RegisterHandler<GetServiceStateResult>(TestTopic, (_, _, _) => Task.CompletedTask);

            // Act
            var act = async () => await registry.HandleCallbackAsync(CreateEvent("not json"), TestContext.Current.CancellationToken);

            // Assert
            await act.Should().ThrowAsync<JsonException>();
        }

        [Fact]
        public async Task ShouldInvokeCallbackWithDeserializedMessage_WhenCallbackRegistered()
        {
            // Arrange
            var registry = new CallbackHandlerRegistry();
            var correlationId = Guid.NewGuid();

            GetServiceStateResult? received = null;
            Guid receivedCorrelationId = default;

            registry.RegisterHandler<GetServiceStateResult>(TestTopic, (message, id, _) =>
            {
                received = message;
                receivedCorrelationId = id;
                return Task.CompletedTask;
            });

            // Act
            var result = await registry
                .HandleCallbackAsync(
                    CreateEvent(SerializedServiceStateResult(), correlationId),
                    TestContext.Current.CancellationToken);

            // Assert
            result.Should().BeTrue();
            received!.ActiveState.Should().Be(ActiveState);
            receivedCorrelationId.Should().Be(correlationId);
        }
    }

    public class RegisterRawHandler
    {
        [Fact]
        public void ShouldThrow_WhenTopicNull()
        {
            // Arrange
            var registry = new CallbackHandlerRegistry();

            // Act
            var act = () => registry.RegisterRawHandler(null!, (_, _, _) => Task.CompletedTask);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("topic");
        }

        [Fact]
        public void ShouldThrow_WhenCallbackNull()
        {
            // Arrange
            var registry = new CallbackHandlerRegistry();

            // Act
            var act = () => registry.RegisterRawHandler(TestTopic, null!);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("handleRawAsync");
        }

        [Theory]
        [InlineData("tadaaaaa!")]
        [InlineData("")]
        [InlineData("{ not json")]
        public async Task ShouldInvokeCallbackWithUnchangedContent_WhenCallbackRegistered(string content)
        {
            // Arrange
            var registry = new CallbackHandlerRegistry();
            string? received = null;

            registry.RegisterRawHandler(TestTopic, (messageContent, _, _) =>
            {
                received = messageContent;
                return Task.CompletedTask;
            });

            // Act
            var result = await registry.HandleCallbackAsync(CreateEvent(content), TestContext.Current.CancellationToken);

            // Assert
            result.Should().BeTrue();
            received.Should().Be(content);
        }
    }

    public class HandleCallbackAsync
    {
        [Fact]
        public async Task ShouldReturnFalseAndRaiseEvent_WhenNoHandlerRegistered()
        {
            // Arrange
            var registry = new CallbackHandlerRegistry();
            NamedPipeMessage? missingHandlerMessage = null;

            registry.OnMissingHandler += message => missingHandlerMessage = message;

            // Act
            var result = await registry.HandleCallbackAsync(CreateEvent("content"), TestContext.Current.CancellationToken);

            // Assert
            result.Should().BeFalse();
            missingHandlerMessage!.Topic.Should().Be(TestTopic);
        }
    }

    private static string SerializedServiceStateResult() => MessageSerializer.Serialize(new GetServiceStateResult
    {
        Status = OperationStatus.Success,
        Message = "Service state read.",
        ActiveState = ActiveState
    });

    private static NamedPipeMessage CreateEvent(string content, Guid correlationId = default) => new()
    {
        ConversationId = correlationId,
        Topic = TestTopic,
        Type = MessageType.Event,
        Content = content
    };

    private sealed class RawHandler(string topic) : ICallbackHandler
    {
        public string Topic => topic;

        public string? Received { get; private set; }

        public Task HandleRawAsync(string messageContent, Guid correlationId, CancellationToken cancellationToken)
        {
            Received = messageContent;
            return Task.CompletedTask;
        }
    }

    private sealed class TypedHandler(string topic) : ICallbackHandler<GetServiceStateResult>
    {
        public string Topic => topic;

        public GetServiceStateResult? Received { get; private set; }

        public Task HandleAsync(
            GetServiceStateResult message,
            Guid correlationId,
            CancellationToken cancellationToken)
        {
            Received = message;
            return Task.CompletedTask;
        }
    }
}
