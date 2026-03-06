using HostManagement.Shared.Communication.Contracts;
using Microsoft.Extensions.Logging;

namespace PipeCommunicationSample;

/// <summary>
/// Registers and dispatches event callbacks for pipe messages.
/// </summary>
/// <param name="logger">An <see cref="ILogger"/> implementation.</param>
internal sealed partial class EventCallbackRegistry(ILogger logger)
{
    private readonly Dictionary<string, ICallbackHandler> _handlers = [];

    /// <summary>
    /// Handles the specified pipe message.
    /// </summary>
    /// <param name="message">The message to handle.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    public async Task Handle(NamedPipeMessage message, CancellationToken cancellationToken)
    {
        if (!_handlers.TryGetValue(message.Topic, out var handler))
        {
            NoHandlerRegistered(logger, message.Topic);
            return;
        }

        await handler.Handle(message.Content, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Registers the specified callback handler.
    /// </summary>
    /// <param name="callbackHandler">The handler to register.</param>
    public void RegisterHandler(ICallbackHandler callbackHandler)
        => _handlers.Add(callbackHandler.Topic, callbackHandler);

    [LoggerMessage(LogLevel.Information, "No Handler registered for Topic {Topic}.")]
    private static partial void NoHandlerRegistered(ILogger logger, string topic);
}
