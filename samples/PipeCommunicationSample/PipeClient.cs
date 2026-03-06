using System.IO.Pipes;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Enums;
using Microsoft.Extensions.Logging;

namespace PipeCommunicationSample;

/// <inheritdoc cref="IPipeClient"/>
internal sealed partial class PipeClient : IPipeClient
{
    private readonly string _pipeName;
    private readonly ILogger _logger;
    private readonly PipeOptions? _options;
    private readonly EventCallbackRegistry _eventCallbackRegistry;
    private readonly NamedPipeClientStream _rawPipeStream;
    private readonly PipeStreamWrapper _wrappedPipe;
    private readonly PipeMessageProcessor _messageProcessor;

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
        _eventCallbackRegistry = new EventCallbackRegistry(_logger);

        InitHandlerRegistry();

        _rawPipeStream = CreatePipe();
        _wrappedPipe = new PipeStreamWrapper(_rawPipeStream, _logger);
        _messageProcessor = new PipeMessageProcessor(_wrappedPipe, _eventCallbackRegistry, _logger);
    }

    public PipeState State => _messageProcessor?.State ?? PipeState.NotOpened;

    /// <inheritdoc/>
    /// <exception cref="IOException">Thrown when the connection fails.</exception>
    public async Task Connect(CancellationToken cancellationToken = default)
    {
        await _rawPipeStream.ConnectAsync(cancellationToken).ConfigureAwait(false);

        ConnectedToServer(_logger);

        _ = _messageProcessor.StartProcessing(cancellationToken);
    }

    public async Task<NamedPipeMessage> SendRequest(
        string topic,
        string content = "",
        CancellationToken cancellationToken = default)
    {
        if (State != PipeState.Connected)
            await Connect(cancellationToken);

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
        _eventCallbackRegistry.RegisterHandler(new TestCallbackHandler());
        //TODO: Register handlers here!
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

    [LoggerMessage(LogLevel.Information, "Connecting to pipe '{PipeName}'.")]
    private static partial void ConnectingToPipe(ILogger logger, string pipeName);
}
