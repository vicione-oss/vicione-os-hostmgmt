using System.IO.Pipes;
using System.Net.Sockets;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Enums;
using Microsoft.Extensions.Logging;

namespace HostManagement.Communication.NamedPipes.Client;

/// <inheritdoc cref="IPipeClient"/>
internal sealed partial class PipeClient : IPipeClient
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
    /// <param name="logger">An <see cref="ILogger"/> implementation.</param>
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

            ConnectedToServer(_logger);

            _ = _messageProcessor.StartProcessing(cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            string? exceptionMessage;

            if (ex is SocketException sex)
            {
                exceptionMessage = sex.SocketErrorCode switch
                {
                    SocketError.AccessDenied => "Permission denied. Please try again with privileged rights, e.g. 'sudo'.",
                    SocketError.ConnectionRefused => "Connection was refused by the server.",
                    SocketError.TimedOut => "The connection timed out.",
                    _ => $"Unhandled SocketError: {sex.SocketErrorCode} (Code: {sex.ErrorCode})"
                };
            }
            else
            {
                exceptionMessage = ex.Message;
            }

            ConnectingToPipeNotPossible(_logger, _pipeName, exceptionMessage);

            return false;
        }
    }

    public async Task<NamedPipeMessage> SendRequest(
        string topic,
        string content = "",
        CancellationToken cancellationToken = default)
    {
        if (State != PipeState.Connected &&
            !await TryConnect(cancellationToken))
        {
            throw new InvalidOperationException(
                $"Cannot send request to pipe '{_pipeName}' because it is not connected.");
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
            SendingToPipeFailed(_logger, _pipeName, ex);
            throw;
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

        ConnectingToPipe(_logger, _pipeName);

        return new NamedPipeClientStream(".", _pipeName, PipeDirection.InOut, pipeOptionsToPass);
    }

    [LoggerMessage(LogLevel.Information, "Connected to server.")]
    private static partial void ConnectedToServer(ILogger logger);

    [LoggerMessage(LogLevel.Error, "Connecting to pipe '{PipeName}' not possible. Exception: {ExceptionMessage}")]
    private static partial void ConnectingToPipeNotPossible(ILogger logger, string pipeName, string exceptionMessage);

    [LoggerMessage(LogLevel.Error, "Sending to pipe '{PipeName}' failed.")]
    private static partial void SendingToPipeFailed(ILogger logger, string pipeName, Exception exception);

    [LoggerMessage(LogLevel.Information, "Connecting to pipe '{PipeName}'.")]
    private static partial void ConnectingToPipe(ILogger logger, string pipeName);
}
