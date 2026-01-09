using System.IO.Pipes;
using Microsoft.Extensions.Logging;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Enums;
using System.Net.Sockets;

namespace HostManagement.Communication.NamedPipes.Client;

/// <inheritdoc cref="IPipeClient"/>
internal sealed class PipeClient : IPipeClient
{
    private readonly string _pipeName;
    private readonly ILogger _logger;
    private readonly PipeOptions? _options;
    private readonly EventCallbackRegistry _eventCallbackRegistry;
    private readonly NamedPipeClientStream _rawPipeStream;
    private readonly PipeStreamWrapper _wrappedPipe;
    private readonly ClientPipeMessageProcessor _messageProcessor;

    /// <summary>
    /// Initializes a new <see cref="PipeClient"/>.
    /// </summary>
    /// <param name="pipeName">The pipe name.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="options">Extra options for the pipe.</param>
    public PipeClient(string pipeName, ILogger logger, PipeOptions? options = null)
    {
        _pipeName = pipeName;
        _logger = logger;
        _options = options;
        _eventCallbackRegistry = new(_logger);

        InitHandlerRegistry();

        _rawPipeStream = CreatePipe();
        _wrappedPipe = new PipeStreamWrapper(_rawPipeStream, _logger);
        _messageProcessor = new ClientPipeMessageProcessor(_wrappedPipe, _eventCallbackRegistry, _logger);
    }

    public PipeState State => _messageProcessor?.State ?? PipeState.NotOpened;

    /// <inheritdoc/>
    public async Task<bool> TryConnect(CancellationToken cancellationToken = default)
    {
        try
        {
            await _rawPipeStream.ConnectAsync(cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("Connected to server");

            _ = _messageProcessor.StartProcessing(cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            var message = $"Connecting to pipe '{_pipeName}' not possible. ";

            if (ex is SocketException sex)
            {
                message += sex.SocketErrorCode switch
                {
                    SocketError.AccessDenied => "Permission denied. Please try again with privileged rights, e.g. 'sudo'.",
                    SocketError.ConnectionRefused => "Connection was refused by the server.",
                    SocketError.TimedOut => "The connection timed out.",
                    _ => $"Unhandled SocketError: {sex.SocketErrorCode} (Code: {sex.ErrorCode})"
                };
            }
            else
            {
                message += ex.Message;
            }

            _logger.LogError(message);

            return false;
        }
    }

    public async Task<NamedPipeMessage?> SendRequest(
        string topic,
        string content = "",
        CancellationToken cancellationToken = default)
    {
        if (State != PipeState.Connected &&
            !await TryConnect(cancellationToken))
        {
            return null;
        }

        try
        {
            var message = new NamedPipeMessage { Topic = topic, Type = MessageType.Request, Content = content };
            var expectedResponse = _messageProcessor.AnnounceResponse(message.ConversationId);

            await _wrappedPipe.SendMessage(message, cancellationToken);

            cancellationToken.Register(() =>
            {
                expectedResponse.TaskCompletionSource.TrySetException(
                    new OperationCanceledException("Request has been canceled."));
            },
            false);

            return await expectedResponse.TaskCompletionSource.Task.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                "Sending to pipe '{PipeName}' failed. {ExceptionMessage}",
                _pipeName,
                ex.Message);

            return null;
        }
    }

    /// <summary>
    /// Closes the pipe.
    /// </summary>
    public void Dispose()
    {
        _messageProcessor?.Dispose();
        _rawPipeStream?.Dispose();
        _wrappedPipe?.Dispose();
    }

    private void InitHandlerRegistry()
    {
        //TODO: Register handlers here!
        //_eventCallbackRegistry.RegisterHandler(new TestCallbackHandler());
    }

    private NamedPipeClientStream CreatePipe()
    {
        var pipeOptionsToPass = _options == null
            ? PipeOptions.Asynchronous
            : _options.Value | PipeOptions.Asynchronous;

        _logger.LogInformation("Connecting to pipe '{PipeName}'.", _pipeName);

        return new NamedPipeClientStream(".", _pipeName, PipeDirection.InOut, pipeOptionsToPass);
    }
}
