using HostManagement.Shared.Communication.Contracts;
using Microsoft.Extensions.Logging;

namespace PipeCommunicationSample;

internal sealed partial class EventCallbackRegistry(ILogger logger)
{
    private readonly Dictionary<string, ICallbackHandler> _handlers = [];

    public async Task Handle(NamedPipeMessage message, CancellationToken cancellationToken)
    {
        if (!_handlers.TryGetValue(message.Topic, out var handler))
        {
            NoHandlerRegistered(logger, message.Topic);
            return;
        }

        await handler.Handle(message.Content, cancellationToken).ConfigureAwait(false);
    }

    public void RegisterHandler(ICallbackHandler callbackHandler)
        => _handlers.Add(callbackHandler.Topic, callbackHandler);

    [LoggerMessage(LogLevel.Information, "No Handler registered for Topic {Topic}.")]
    private static partial void NoHandlerRegistered(ILogger logger, string topic);
}
