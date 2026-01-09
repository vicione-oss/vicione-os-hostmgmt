using HostManagement.Communication.NamedPipes.RequestHandlers;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Enums;

namespace HostManagement.Communication.NamedPipes;

/// <summary>
/// Handles the pipe connection state and work processing.
/// </summary>
/// <param name="pipeStreamWrapper">A pipe stream wrapper.</param>
/// <param name="requestHandlerRegistry">A request handler registry.</param>
internal sealed class PipeMessageProcessor(
    PipeStreamWrapper pipeStreamWrapper,
    RequestHandlerRegistry requestHandlerRegistry)
    : MessageProcessorBase(pipeStreamWrapper)
{
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
                var response = await requestHandlerRegistry.HandleRequest(message, cancellationToken).ConfigureAwait(false);
                await PipeStreamWrapper.SendMessage(response, cancellationToken).ConfigureAwait(false);
                break;
            case MessageType.Response:
            case MessageType.Event:
                throw new InvalidOperationException($"Message type '{message.Type}' may only be send by the server.");
            default:
                throw new InvalidOperationException($"Unrecognized message type: {message.Type}");
        }
    }
}
