using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Enums;
using Microsoft.Extensions.Logging;

namespace PipeCommunicationSample;

/// <summary>
/// Handles the pipe connection state and work processing.
/// </summary>
/// <param name="pipeStreamWrapper">The pipe stream wrapper.</param>
/// <param name="eventCallbackRegistry">The event callback registry.</param>
/// <param name="logger">An <see cref="ILogger"/> implementation.</param>
internal sealed partial class PipeMessageProcessor(
    PipeStreamWrapper pipeStreamWrapper,
    EventCallbackRegistry eventCallbackRegistry,
    ILogger logger)
    : MessageProcessorBase(pipeStreamWrapper)
{
    private readonly Dictionary<Guid, ExpectedResponse> _expectedResponses = [];

    /// <summary>
    /// Announces an expected response for the specified conversation.
    /// </summary>
    /// <param name="conversationId">The conversation identifier.</param>
    /// <returns>The expected response handle.</returns>
    public ExpectedResponse AnnounceResponse(Guid conversationId)
    {
        var response = new ExpectedResponse();
        _expectedResponses.Add(conversationId, response);
        return response;
    }

    /// <summary>
    /// Processes the next message on the input stream.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    protected override async Task ProcessMessage(CancellationToken cancellationToken)
    {
        var message = await PipeStreamWrapper.ReadMessage(cancellationToken).ConfigureAwait(false);

        switch (message.Type)
        {
            case MessageType.Request:
                throw new InvalidOperationException($"Message type '{message.Type}' may only be send by the server.");
            case MessageType.Response:
                if (!_expectedResponses.Remove(message.ConversationId, out var expectedResponse))
                {
                    UnexpectedResponse(logger, message.ConversationId, message.Topic);
                    // TODO: throw?
                    return;
                }
                expectedResponse.TaskCompletionSource.TrySetResult(message);
                break;
            case MessageType.Event:
                await eventCallbackRegistry.Handle(message, cancellationToken).ConfigureAwait(false);
                break;
            default:
                throw new InvalidOperationException($"Unrecognized message type: {message.Type}");
        }
    }

    [LoggerMessage(LogLevel.Warning, "Unexpected response on conversation {ConversationId} with topic {Topic}.")]
    private static partial void UnexpectedResponse(ILogger logger, Guid conversationId, string topic);
}
