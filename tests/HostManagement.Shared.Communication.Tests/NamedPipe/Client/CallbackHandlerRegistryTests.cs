using AwesomeAssertions;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Communication.NamedPipe.Client;
using HostManagement.Shared.Communication.Serialization;
using Xunit;

namespace HostManagement.Shared.Communication.Tests.NamedPipe.Client;

public class CallbackHandlerRegistryTests
{
    private const string TestTopic = "raw-event";

    public class HandleCallbackAsync
    {
        [Fact]
        public async Task ShouldReturnFalseAndRaiseEvent_WhenHandlerIsMissing()
        {
            // Arrange
            var registry = new CallbackHandlerRegistry();
            NamedPipeMessage? missingMessage = null;
            registry.OnMissingHandler += message => missingMessage = message;
            var message = new NamedPipeMessage
            {
                Topic = TestTopic,
                Type = MessageType.Event,
                Content = "test"
            };

            // Act
            var result = await registry.HandleCallbackAsync(message, CancellationToken.None);

            // Assert
            result.Should().BeFalse();
            missingMessage.Should().BeSameAs(message);
        }
    }

    public class RegisterHandler
    {
        [Fact]
        public async Task ShouldDeserializeAndInvokeHandler_WhenTypedHandlerRegistered()
        {
            // Arrange
            var registry = new CallbackHandlerRegistry();
            var handler = new SystemConfigurationChangedHandler();
            var expectedMessage = GetSystemConfigurationResult.CreateErrorResult("Test error");
            var message = new NamedPipeMessage
            {
                Topic = EventTopics.SystemConfigurationChanged,
                Type = MessageType.Event,
                Content = SourceGeneratedTypeSerializer.Serialize(
                    expectedMessage,
                    SourceGenerationContext.Default.GetSystemConfigurationResult)
            };

            registry.RegisterHandler(PipeEvents.SystemConfigurationChanged, handler).Should().BeTrue();

            // Act
            var result = await registry.HandleCallbackAsync(message, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            handler.Message.Should().BeEquivalentTo(expectedMessage);
            handler.CorrelationId.Should().Be(message.ConversationId);
        }
    }

    public class RegisterRawHandler
    {
        [Fact]
        public async Task ShouldInvokeHandler_WhenRawHandlerRegistered()
        {
            // Arrange
            var registry = new CallbackHandlerRegistry();
            string? receivedContent = null;
            Guid receivedCorrelationId = default;
            var message = new NamedPipeMessage
            {
                Topic = TestTopic,
                Type = MessageType.Event,
                Content = "raw-content"
            };

            registry.RegisterRawHandler(
                TestTopic,
                (content, correlationId, _) =>
                {
                    receivedContent = content;
                    receivedCorrelationId = correlationId;
                    return Task.CompletedTask;
                }).Should().BeTrue();

            // Act
            var result = await registry.HandleCallbackAsync(message, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            receivedContent.Should().Be(message.Content);
            receivedCorrelationId.Should().Be(message.ConversationId);
        }
    }

    private sealed class SystemConfigurationChangedHandler : ICallbackHandler<GetSystemConfigurationResult>
    {
        public Guid CorrelationId { get; private set; }

        public GetSystemConfigurationResult? Message { get; private set; }

        public Task HandleAsync(
            GetSystemConfigurationResult message,
            Guid correlationId,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Message = message;
            CorrelationId = correlationId;
            return Task.CompletedTask;
        }
    }
}
