using HostManagement.Shared.Communication.Contracts;
using Microsoft.Extensions.Logging;

namespace PipeCommunicationSample;

internal sealed class EventCallbackRegistry(ILogger logger)
{
    private readonly Dictionary<string, ICallbackHandler> _handlers = [];

    public async Task Handle(NamedPipeMessage message, CancellationToken cancellationToken)
    {
        if (!_handlers.TryGetValue(message.Topic, out var handler))
        {
            logger.LogInformation("No Handler registered for Topic {MessageTopic}", message.Topic);
            return;
        }

        await handler.Handle(message.Content, cancellationToken).ConfigureAwait(false);
    }

    public void RegisterHandler(ICallbackHandler callbackHandler)
        => _handlers.Add(callbackHandler.Topic, callbackHandler);
}
