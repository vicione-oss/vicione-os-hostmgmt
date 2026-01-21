using HostManagement.Shared.Communication.Contracts;

namespace HostManagement.Shared.Communication.NamedPipe.Client;

/// <summary>
/// Represents a <see cref="CallbackHandlerRegistry"/>.
/// </summary>
public sealed class CallbackHandlerRegistry
{
    private readonly Dictionary<string, ICallbackHandler> _handlers = [];

    /// <summary>
    /// Handles a named pipe event message callback.
    /// </summary>
    /// <param name="message">The named pipe message.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// Returns <see langword="true"/> if a handler existed and the callback was handled,
    /// otherwise <see langword="false"/>.
    /// </returns>
    public async Task<bool> HandleCallbackAsync(
        NamedPipeMessage message,
        CancellationToken cancellationToken = default)
    {
        if (!_handlers.TryGetValue(message.Topic, out var handler))
        {
            OnMissingHandler?.Invoke(message);
            return false;
        }

        await handler.HandleAsync(message.Content, cancellationToken).ConfigureAwait(false);
        return true;
    }

    /// <summary>
    /// Raised when a message arrives with no registered handler.
    /// </summary>
    public event Action<NamedPipeMessage>? OnMissingHandler;

    /// <summary>
    /// Registers an <see cref="ICallbackHandler"/>.
    /// </summary>
    /// <param name="handler">The handler.</param>
    public bool RegisterHandler(ICallbackHandler handler) => _handlers.TryAdd(handler.Topic, handler);
}
