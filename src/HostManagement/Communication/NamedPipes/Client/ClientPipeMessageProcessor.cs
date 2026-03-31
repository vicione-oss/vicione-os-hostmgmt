using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Enums;
using Microsoft.Extensions.Logging;

namespace HostManagement.Communication.NamedPipes.Client;

/// <summary>
/// Handles the pipe connection state and work processing of a client.
/// </summary>
/// <param name="pipeStreamWrapper">A pipe stream wrapper.</param>
/// <param name="eventCallbackRegistry">An event callback registry.</param>
/// <param name="logger">A logger.</param>
internal sealed partial class ClientPipeMessageProcessor(
    PipeStreamWrapper pipeStreamWrapper,
    EventCallbackRegistry eventCallbackRegistry,
    ILogger logger)
    : MessageProcessorBase(pipeStreamWrapper)
{
    private readonly Dictionary<Guid, ExpectedResponse> _expectedResponses = [];

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
        var message = await PipeStreamWrapper.ReadMessageAsync(cancellationToken).ConfigureAwait(false);

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
