using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using AwesomeAssertions;
using HostManagement.Communication.NamedPipes.RequestHandlers;
using HostManagement.Configuration;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Enums;
using Xunit;

namespace HostManagement.Tests.Communication.NamedPipes.RequestHandlers;

public sealed class RequestHandlerRegistryTests
{
    private const string ErrorMessage = "Unit-Test-Error";
    private static readonly JsonTypeInfo<GetNetworkStatusInformationResult> s_networkStatusJsonTypeInfo =
        SourceGenerationContext.Default.GetNetworkStatusInformationResult;

    public class RegisterHandler
    {
        [Fact]
        public void ShouldThrow_WhenRegisteringDuplicateTopic()
        {
            // Arrange
            var duplicateTopic = Topics.GetNTPFallbackInformation;
            var handler1 = new FakeHandler(duplicateTopic, s_networkStatusJsonTypeInfo, handleResult: "h1");
            var handler2 = new FakeHandler(duplicateTopic, s_networkStatusJsonTypeInfo, handleResult: "h2");

            var registry = new RequestHandlerRegistry(TargetEnvironment.EdgeSViciOne);
            registry.RegisterHandler(handler1);

            // Act
            var act = () => registry.RegisterHandler(handler2);

            // Assert
            act.Should()
                .Throw<InvalidOperationException>()
                .WithMessage($"A handler for topic '{duplicateTopic}' is already registered.");
        }
    }
    public class HandleRequest
    {
        [Fact]
        public async Task ShouldReturnDefaultResponse_WhenTopicNull()
        {
            // Arrange
            var request = new NamedPipeMessage
            {
                Topic = null!,
                Type = MessageType.Request,
                Content = "lan1"
            };

            var registry = new RequestHandlerRegistry(TargetEnvironment.EdgeSViciOne);

            // Act
            var response = await registry.HandleRequestAsync(request, CancellationToken.None);

            // Assert
            response.Should().NotBeNull();
            response.Content.Should().Contain("Error processing request. Message topic was null.");
        }

        [Fact]
        public async Task ShouldReturnDefaultResponse_WhenTopicInvalid()
        {
            // Arrange
            var invalidTopic = "invalidTopic";

            var request = new NamedPipeMessage
            {
                Topic = invalidTopic,
                Type = MessageType.Request,
                Content = "lan1"
            };

            var registry = new RequestHandlerRegistry(TargetEnvironment.EdgeSViciOne);

            // Act
            var response = await registry.HandleRequestAsync(request, CancellationToken.None);

            // Assert
            response.Should().NotBeNull();
            response.Content.Should().Contain($"Error processing request. Topic \\u0027{invalidTopic}\\u0027 is invalid.");
        }

        [Fact]
        public async Task ShouldReturnDefaultResponse_WhenNoHandlerRegistered()
        {
            // Arrange
            var validButUnknownTopic = Topics.GetNetworkStatusInformation;

            var request = new NamedPipeMessage
            {
                Topic = validButUnknownTopic,
                Type = MessageType.Request,
                Content = "lan1"
            };

            var registry = new RequestHandlerRegistry(TargetEnvironment.EdgeSViciOne);

            // Act
            var response = await registry.HandleRequestAsync(request, CancellationToken.None);

            // Assert
            response.Should().NotBeNull();
            response.Content.Should().Contain($"No handler registered for topic \\u0027{validButUnknownTopic}\\u0027.");
        }

        [Fact]
        public async Task ShouldReturnDefaultResponse_WhenHandlerThrows()
        {
            // Arrange
            var validTopic = Topics.GetNetworkStatusInformation;
            var handler = new FakeHandler(validTopic, s_networkStatusJsonTypeInfo, handleThrows: true);

            var request = new NamedPipeMessage
            {
                Topic = validTopic,
                Type = MessageType.Request,
                Content = "lan1"
            };

            var registry = new RequestHandlerRegistry(TargetEnvironment.EdgeSViciOne);
            registry.RegisterHandler(handler);

            // Act
            var response = await registry.HandleRequestAsync(request, CancellationToken.None);

            // Assert
            response.Should().NotBeNull();
            response.Content.Should().Contain("Error processing request:");
            handler.WasHandleCalled.Should().BeTrue();
        }

        [Fact]
        public async Task ShouldReturnSerializedError_WhenHandlerNotSupportedOnEnvironment()
        {
            // Arrange
            var unsupportedTopic = Topics.GetNetworkStatusInformation; // Environment Debian does not support this topic
            var handler = new FakeHandler(unsupportedTopic, s_networkStatusJsonTypeInfo, handleResult: "handled");

            var request = new NamedPipeMessage
            {
                Topic = unsupportedTopic,
                Type = MessageType.Request,
                Content = "lan1"
            };

            var registry = new RequestHandlerRegistry(TargetEnvironment.Debian);
            registry.RegisterHandler(handler);

            // Act
            var response = await registry.HandleRequestAsync(request, CancellationToken.None);

            // Assert
            response.Should().NotBeNull();
            handler.WasHandleCalled.Should().BeFalse();

            var result = JsonSerializer.Deserialize(response.Content, s_networkStatusJsonTypeInfo);

            result.Should().NotBeNull();
            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().Contain($"Handler with topic '{unsupportedTopic}' not supported on this platform.");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task ShouldReturnSerializedError_WhenRequiredContentMissing(string? content)
        {
            // Arrange
            var topic = Topics.GetNetworkStatusInformation;
            var handler = new FakeHandler(topic, s_networkStatusJsonTypeInfo, handleResult: "handled");

            var request = new NamedPipeMessage
            {
                Topic = topic,
                Type = MessageType.Request,
                Content = content!
            };

            var registry = new RequestHandlerRegistry(TargetEnvironment.EdgeSViciOne);
            registry.RegisterHandler(handler);

            // Act
            var response = await registry.HandleRequestAsync(request, CancellationToken.None);

            // Assert
            response.Should().NotBeNull();
            handler.WasHandleCalled.Should().BeFalse();

            var result = JsonSerializer.Deserialize(response.Content, s_networkStatusJsonTypeInfo);

            result.Should().NotBeNull();
            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().Contain($"Handler with topic '{topic}' requires a message content.");
        }

        [Fact]
        public async Task ShouldDispatchToHandler_WhenHandlerSupported()
        {
            // Arrange
            var supportedTopic = Topics.GetNetworkStatusInformation;
            var handler = new FakeHandler(supportedTopic, s_networkStatusJsonTypeInfo, handleResult: "handled");

            var request = new NamedPipeMessage
            {
                Topic = supportedTopic,
                Type = MessageType.Request,
                Content = "lan1"
            };

            var registry = new RequestHandlerRegistry(TargetEnvironment.EdgeSViciOne);
            registry.RegisterHandler(handler);

            // Act
            var response = await registry.HandleRequestAsync(request, CancellationToken.None);

            // Assert
            response.Should().NotBeNull();
            response.Content.Should().Be("handled");
            handler.WasHandleCalled.Should().BeTrue();
        }
    }

    private sealed class FakeHandler(
        string topic,
        JsonTypeInfo typeInfo,
        string? handleResult = null,
        bool handleThrows = false,
        Exception? handleException = null) : IRequestHandler
    {
        private readonly bool _handleThrows = handleThrows;
        private readonly Exception? _handleException = handleException;

        public string Topic { get; } = topic;
        public JsonTypeInfo JsonTypeInfo { get; } = typeInfo;
        public string HandleResult { get; } = handleResult ?? string.Empty;
        public bool WasHandleCalled { get; private set; }

        public OperationResult CreateErrorInstance(string message)
            => GetNetworkStatusInformationResult.CreateErrorResult(message);

        public Task<string> HandleAsync(string messageContent, CancellationToken cancellationToken)
        {
            WasHandleCalled = true;

            if (_handleException is not null)
                throw _handleException;

            if (_handleThrows)
                throw new InvalidOperationException(ErrorMessage);

            return Task.FromResult(HandleResult);
        }
    }
}
