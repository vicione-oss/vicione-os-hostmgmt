using System.Text.Json;
using AwesomeAssertions;
using HostManagement.Shared.Communication.NamedPipe.Client;
using Xunit;

namespace HostManagement.Shared.Communication.Tests.NamedPipe.Client;

public class PipeRequestFailedExceptionTests
{
    private const string Topic = "GetServiceState";

    private const string Reason =
        "Handle request for topic 'GetServiceState' failed (Conversation ID: 8d0f4c6e-2b1a-4f53-9a7e-3c5d1e2f4a6b). " +
        "Service name is empty.";

    [Fact]
    public void ShouldHaveNoRequestDetails_WhenCreatedWithMessageOnly()
    {
        // Act
        var exception = new PipeRequestFailedException(Reason);

        // Assert
        exception.Message.Should().Be(Reason);
        exception.Topic.Should().BeNull();
        exception.ConversationId.Should().Be(Guid.Empty);
    }

    [Fact]
    public void ShouldCarryRequestDetails_WhenCreatedForFailedRequest()
    {
        // Arrange
        var conversationId = Guid.NewGuid();
        var innerException = new JsonException("Required property 'ResultType' is missing or empty.");

        // Act
        var exception = new PipeRequestFailedException(Topic, conversationId, Reason, innerException);

        // Assert
        exception.Message.Should().Be(Reason);
        exception.Topic.Should().Be(Topic);
        exception.ConversationId.Should().Be(conversationId);
        exception.InnerException.Should().BeSameAs(innerException);
    }
}
