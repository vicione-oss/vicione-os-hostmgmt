using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using AwesomeAssertions;
using HostManagement.Communication.NamedPipe.RequestHandlers;
using HostManagement.Configuration;
using HostManagement.Shared.Capabilities;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Capabilities;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Enums;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace HostManagement.Tests.Communication.NamedPipe.RequestHandlers;

public sealed class RequestHandlerRegistryTests
{
    private const string ExceptionMessage = "Unit-Test-Error";
    private static readonly JsonTypeInfo<GetNetworkStatusInformationResult> s_networkStatusJsonTypeInfo =
        SourceGenerationContext.Default.GetNetworkStatusInformationResult;

    private static RequestHandlerRegistry CreateSut(
        TargetEnvironment targetEnvironment,
        IEnumerable<IRequestHandler> handlers,
        ILogger logger,
        CapabilityStatus capability = CapabilityStatus.Enabled,
        Action<SupportedTopics>? configureSupport = null)
    {
        var supportedTopics = new SupportedTopics
        {
            InstallSignedDebianPackage = capability,
            ResetNetworkInterface = capability,
            RenewDHCPLease = capability,

            StartService = capability,
            StopService = capability,
            RestartService = capability,

            ShutdownSystem = capability,
            RestartSystem = capability,
            ResetSystem = capability,
            UpdateSystem = capability
        };

        configureSupport?.Invoke(supportedTopics);

        return new(targetEnvironment, supportedTopics, handlers, logger);
    }

    public class Constructor
    {
        [Fact]
        public void ShouldLogErrorAndNotAddDuplicateHandler_WhenRegisteringDuplicateTopic()
        {
            // Arrange
            var logger = GetLoggerSubstitute();
            var duplicateTopic = Topics.GetNTPFallbackInformation;
            var handler1 = new FakeHandler(duplicateTopic, handleResult: "h1");
            var handler2 = new FakeHandler(duplicateTopic, handleResult: "h2");

            var expectedLogMessage = $"Request handler for topic '{duplicateTopic}' is already registered.";

            // Act
            var registry = CreateSut(TargetEnvironment.EdgeSViciOne, [handler1, handler2], logger);

            // Assert
            logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString()!.Contains(expectedLogMessage)),
                null,
                Arg.Any<Func<object, Exception?, string>>());

            registry.Handlers.Should().OnlyHaveUniqueItems();
        }
    }

    public class HandleRequest
    {
        [Fact]
        public async Task ShouldReturnDefaultResponse_WhenTopicNull()
        {
            // Arrange
            var logger = GetLoggerSubstitute();

            var request = new NamedPipeMessage
            {
                Topic = null!,
                Type = MessageType.Request,
                Content = "lan1"
            };

            var registry = CreateSut(TargetEnvironment.EdgeSViciOne, [], logger);
            var expectedMessage = GetRequestFailedMessage(request, "Topic is null.");

            // Act
            var response = await registry.HandleRequestAsync(request, CancellationToken.None);

            // Assert
            AssertLogReceived(logger, LogLevel.Error, expectedMessage);

            response.Should().NotBeNull();
            response.Content.Should().Contain(expectedMessage.Replace("'", "\\u0027"));
        }

        [Fact]
        public async Task ShouldReturnDefaultResponse_WhenTopicInvalid()
        {
            // Arrange
            var logger = GetLoggerSubstitute();
            var invalidTopic = "invalidTopic";

            var request = new NamedPipeMessage
            {
                Topic = invalidTopic,
                Type = MessageType.Request,
                Content = "lan1"
            };

            var registry = CreateSut(TargetEnvironment.EdgeSViciOne, [], logger);
            var expectedMessage = GetRequestFailedMessage(request, "Topic is invalid.");

            // Act
            var response = await registry.HandleRequestAsync(request, CancellationToken.None);

            // Assert
            AssertLogReceived(logger, LogLevel.Error, expectedMessage);

            response.Should().NotBeNull();
            response.Content.Should().Contain(expectedMessage.Replace("'", "\\u0027"));
        }

        [Fact]
        public async Task ShouldReturnDefaultResponse_WhenNoHandlerRegistered()
        {
            // Arrange
            var logger = GetLoggerSubstitute();
            var validButUnknownTopic = Topics.GetNetworkStatusInformation;

            var request = new NamedPipeMessage
            {
                Topic = validButUnknownTopic,
                Type = MessageType.Request,
                Content = "lan1"
            };

            var registry = CreateSut(TargetEnvironment.EdgeSViciOne, [], logger);
            var expectedMessage = GetRequestFailedMessage(request, "No handler registered.");

            // Act
            var response = await registry.HandleRequestAsync(request, CancellationToken.None);

            // Assert
            AssertLogReceived(logger, LogLevel.Error, expectedMessage);

            response.Should().NotBeNull();
            response.Content.Should().Contain(expectedMessage.Replace("'", "\\u0027"));
        }

        [Fact]
        public async Task ShouldReturnDefaultResponse_WhenHandlerThrows()
        {
            // Arrange
            var logger = GetLoggerSubstitute();
            var validTopic = Topics.GetNetworkStatusInformation;
            var handler = new FakeHandler(validTopic, handleThrows: true);

            var request = new NamedPipeMessage
            {
                Topic = validTopic,
                Type = MessageType.Request,
                Content = "lan1"
            };

            var registry = CreateSut(TargetEnvironment.EdgeSViciOne, [handler], logger);
            var expectedMessage = GetRequestFailedMessage(request, ExceptionMessage);

            // Act
            var response = await registry.HandleRequestAsync(request, CancellationToken.None);

            // Assert
            AssertLogReceived<InvalidOperationException>(logger, LogLevel.Error, expectedMessage);

            response.Should().NotBeNull();
            response.Content.Should().Contain(expectedMessage.Replace("'", "\\u0027"));
            handler.WasHandleCalled.Should().BeTrue();
        }

        [Fact]
        public async Task ShouldReturnSerializedError_WhenTopicNotAvailableOnPlatform()
        {
            // Arrange
            var logger = GetLoggerSubstitute();
            var unsupportedTopic = Topics.GetNetworkStatusInformation; // Environment Debian does not support this topic
            var handler = new FakeHandler(unsupportedTopic, handleResult: "handled");

            var request = new NamedPipeMessage
            {
                Topic = unsupportedTopic,
                Type = MessageType.Request,
                Content = "lan1"
            };

            var registry = CreateSut(TargetEnvironment.Debian, [handler], logger);
            var expectedMessage = GetRequestFailedMessage(request, "Topic not available on this platform.");

            // Act
            var response = await registry.HandleRequestAsync(request, CancellationToken.None);

            // Assert
            AssertLogReceived(logger, LogLevel.Error, expectedMessage);

            response.Should().NotBeNull();
            handler.WasHandleCalled.Should().BeFalse();

            var result = JsonSerializer.Deserialize(response.Content, s_networkStatusJsonTypeInfo);

            result.Should().NotBeNull();
            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().Contain(expectedMessage);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task ShouldReturnSerializedError_WhenRequiredContentMissing(string? content)
        {
            // Arrange
            var logger = GetLoggerSubstitute();
            var topic = Topics.GetNetworkStatusInformation;
            var handler = new FakeHandler(topic, handleResult: "handled");

            var request = new NamedPipeMessage
            {
                Topic = topic,
                Type = MessageType.Request,
                Content = content!
            };

            var registry = CreateSut(TargetEnvironment.EdgeSViciOne, [handler], logger);
            var expectedMessage = GetRequestFailedMessage(request, "Request requires a message content.");

            // Act
            var response = await registry.HandleRequestAsync(request, CancellationToken.None);

            // Assert
            AssertLogReceived(logger, LogLevel.Error, expectedMessage);

            response.Should().NotBeNull();
            handler.WasHandleCalled.Should().BeFalse();

            var result = JsonSerializer.Deserialize(response.Content, s_networkStatusJsonTypeInfo);

            result.Should().NotBeNull();
            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().Contain(expectedMessage);
        }

        [Theory]
        [InlineData(Topics.InstallSignedDebianPackage)]
        [InlineData(Topics.ResetNetworkInterface)]
        [InlineData(Topics.RenewDHCPLease)]
        [InlineData(Topics.StartService)]
        [InlineData(Topics.StopService)]
        [InlineData(Topics.RestartService)]
        [InlineData(Topics.ShutdownSystem)]
        [InlineData(Topics.RestartSystem)]
        [InlineData(Topics.ResetSystem)]
        [InlineData(Topics.UpdateSystem)]
        public async Task ShouldReturnSerializedError_WhenTopicCapabilityDisabled(string topic)
        {
            // Arrange
            var logger = GetLoggerSubstitute();
            var handler = new FakeHandler(topic, handleResult: "handled");

            var request = new NamedPipeMessage
            {
                Topic = topic,
                Type = MessageType.Request,
                Content = "lan1"
            };

            var registry = CreateSut(
                TargetEnvironment.EdgeSViciOne,
                [handler],
                logger,
                configureSupport: support => DisableTopicCapability(support, topic));

            var expectedMessage = GetRequestFailedMessage(
                request,
                "Topic cannot be used because its capability is disabled.");

            // Act
            var response = await registry.HandleRequestAsync(request, CancellationToken.None);

            // Assert
            AssertLogReceived(logger, LogLevel.Error, expectedMessage);

            response.Should().NotBeNull();
            response.Content.Should().Contain(expectedMessage.Replace("'", "\\u0027"));

            handler.WasHandleCalled.Should().BeFalse();
        }

        private static void DisableTopicCapability(SupportedTopics supportedTopics, string topic)
        {
            switch (topic)
            {
                case Topics.InstallSignedDebianPackage: supportedTopics.InstallSignedDebianPackage = CapabilityStatus.Disabled; break;
                case Topics.ResetNetworkInterface: supportedTopics.ResetNetworkInterface = CapabilityStatus.Disabled; break;
                case Topics.RenewDHCPLease: supportedTopics.RenewDHCPLease = CapabilityStatus.Disabled; break;

                case Topics.StartService: supportedTopics.StartService = CapabilityStatus.Disabled; break;
                case Topics.StopService: supportedTopics.StopService = CapabilityStatus.Disabled; break;
                case Topics.RestartService: supportedTopics.RestartService = CapabilityStatus.Disabled; break;

                case Topics.ShutdownSystem: supportedTopics.ShutdownSystem = CapabilityStatus.Disabled; break;
                case Topics.RestartSystem: supportedTopics.RestartSystem = CapabilityStatus.Disabled; break;
                case Topics.ResetSystem: supportedTopics.ResetSystem = CapabilityStatus.Disabled; break;
                case Topics.UpdateSystem: supportedTopics.UpdateSystem = CapabilityStatus.Disabled; break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(topic),
                        topic,
                        "No capability mapping is defined for the topic.");
            }
        }

        [Fact]
        public async Task ShouldReturnDefaultResponse_WhenCanceled()
        {
            // Arrange
            var logger = GetLoggerSubstitute();
            var supportedTopic = Topics.GetNetworkStatusInformation;
            var handler = new FakeHandler(supportedTopic, handleResult: "handled");

            var request = new NamedPipeMessage
            {
                Topic = supportedTopic,
                Type = MessageType.Request,
                Content = "lan1"
            };

            var registry = CreateSut(TargetEnvironment.EdgeSViciOne, [handler], logger);
            var expectedMessage =
                $"Handle request for topic '{request.Topic}' canceled (Conversation ID: {request.ConversationId}).";

            using var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act
            var response = await registry.HandleRequestAsync(request, cts.Token);

            // Assert
            AssertLogReceived(logger, LogLevel.Warning, expectedMessage);

            response.Should().NotBeNull();
            response.Content.Should().Contain(expectedMessage.Replace("'", "\\u0027"));
        }

        [Fact]
        public async Task ShouldDispatchToHandler_WhenHandlerSupported()
        {
            // Arrange
            var logger = GetLoggerSubstitute();
            var supportedTopic = Topics.GetNetworkStatusInformation;
            var handler = new FakeHandler(supportedTopic, handleResult: "handled");

            var request = new NamedPipeMessage
            {
                Topic = supportedTopic,
                Type = MessageType.Request,
                Content = "lan1"
            };

            var registry = CreateSut(TargetEnvironment.EdgeSViciOne, [handler], logger);

            // Act
            var response = await registry.HandleRequestAsync(request, CancellationToken.None);

            // Assert
            response.Should().NotBeNull();
            response.Content.Should().Be("handled");
            handler.WasHandleCalled.Should().BeTrue();
            handler.CorrelationId.Should().Be(request.ConversationId);
        }

        private static string GetRequestFailedMessage(NamedPipeMessage message, string error)
            => $"Handle request for topic '{message.Topic}' failed (Conversation ID: {message.ConversationId}). {error}";
    }

    private sealed class FakeHandler(
        string topic,
        string? handleResult = null,
        bool handleThrows = false,
        Exception? handleException = null) : RequestHandler<GetNetworkStatusInformationResult>
    {
        private readonly bool _handleThrows = handleThrows;
        private readonly Exception? _handleException = handleException;

        public Guid CorrelationId { get; private set; }
        public string HandleResult { get; } = handleResult ?? string.Empty;
        public bool WasHandleCalled { get; private set; }
        public override string Topic { get; } = topic;

        public override JsonTypeInfo<GetNetworkStatusInformationResult> TypedJsonTypeInfo
            => s_networkStatusJsonTypeInfo;

        public override GetNetworkStatusInformationResult CreateTypedErrorInstance(string message)
            => GetNetworkStatusInformationResult.CreateErrorResult(message);

        public override Task<string> HandleAsync(
            string messageContent,
            Guid correlationId,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            CorrelationId = correlationId;
            WasHandleCalled = true;

            if (_handleException is not null)
                throw _handleException;

            if (_handleThrows)
                throw new InvalidOperationException(ExceptionMessage);

            return Task.FromResult(HandleResult);
        }
    }

    private static ILogger GetLoggerSubstitute()
    {
        var logger = Substitute.For<ILogger>();
        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        return logger;
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
}
