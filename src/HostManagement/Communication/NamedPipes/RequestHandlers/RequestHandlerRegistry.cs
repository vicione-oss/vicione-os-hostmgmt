using HostManagement.Communication.NamedPipes.Extensions;
using HostManagement.Shared.Communication.Contracts;

namespace HostManagement.Communication.NamedPipes.RequestHandlers;

/// <summary>
/// Represents an <see cref="RequestHandlerRegistry"/>.
/// </summary>
internal sealed class RequestHandlerRegistry
{
    private readonly Dictionary<string, IRequestHandler> _handlers = [];

    /// <summary>
    /// Handles a named pipe message request.
    /// </summary>
    /// <param name="message">The named pipe message.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see cref="NamedPipeMessage"/></returns>
    public async Task<NamedPipeMessage> HandleRequest(NamedPipeMessage message, CancellationToken cancellationToken)
    {
        if (!_handlers.TryGetValue(message.Topic, out var handler))
            return message.CreateDefaultResponse($"No handler registered for topic {message.Topic}.");

        try
        {
            var response = await handler.Handle(message.Content, cancellationToken).ConfigureAwait(false);
            return message.CreateResponse(response);
        }
        catch (Exception ex)
        {
            return message.CreateDefaultResponse($"Error processing request: {ex.Message}");
        }
    }

    /// <summary>
    /// Registers an <see cref="IRequestHandler"/>.
    /// </summary>
    /// <param name="handler">The handler.</param>
    /// <exception cref="InvalidOperationException"></exception>
    public void RegisterHandler(IRequestHandler handler)
    {
        if (!_handlers.TryAdd(handler.Topic, handler))
            throw new InvalidOperationException($"A handler for topic '{handler.Topic}' is already registered.");
    }
}
