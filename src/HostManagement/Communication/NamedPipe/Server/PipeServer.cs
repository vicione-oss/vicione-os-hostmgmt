using System.IO.Pipes;
using HostManagement.Communication.NamedPipe.RequestHandlers;
using HostManagement.Configuration;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Enums;
using Microsoft.Extensions.Logging;

namespace HostManagement.Communication.NamedPipe.Server;

/// <inheritdoc cref="IPipeServer"/>
internal sealed partial class PipeServer : IPipeServer
{
    private const int MaxNumberOfServerInstances = 1;

    private readonly string _pipeName;
    private readonly string _pipePath;
    private readonly ILogger _logger;
    private readonly RequestHandlerRegistry _requestHandlerRegistry;
    private readonly PipeOptions? _options;
    private readonly NamedPipeServerStream _rawPipeStream;
    private readonly PipeStreamWrapper _wrappedPipe;
    private readonly PipeMessageProcessor _messageProcessor;

    /// <summary>
    /// Initializes a new <see cref="PipeServer" />.
    /// </summary>
    /// <param name="pipeName">The name of the pipe.</param>
    /// <param name="logger">An <see cref="ILogger"/> implementation.</param>
    /// <param name="requestHandlerRegistry">The <see cref="RequestHandlerRegistry"/>.</param>
    /// <param name="options">Extra options for the pipe.</param>
    public PipeServer(
        string pipeName,
        ILogger logger,
        RequestHandlerRegistry requestHandlerRegistry,
        PipeOptions? options = null)
    {
        _pipeName = pipeName;
        _pipePath = Path.Combine(AppPaths.TempPath, $"CoreFxPipe_{pipeName}");
        _logger = logger;
        _requestHandlerRegistry = requestHandlerRegistry ?? throw new ArgumentNullException(nameof(requestHandlerRegistry));
        _options = options;

        _rawPipeStream = CreatePipe();
        _wrappedPipe = new PipeStreamWrapper(_rawPipeStream, _logger);
        _messageProcessor = new PipeMessageProcessor(_wrappedPipe, _requestHandlerRegistry);
    }

    public string PipePath => _pipePath;

    public PipeState State => _messageProcessor?.State ?? PipeState.NotOpened;

    public Task PublishEvent(string topic, string content, CancellationToken cancellationToken = default)
        => _wrappedPipe.SendMessageAsync(
            new NamedPipeMessage
            {
                Topic = topic,
                Type = MessageType.Event,
                Content = content
            },
            cancellationToken);

    /// <inheritdoc/>
    /// <exception cref="IOException">Thrown when the connection fails.</exception>
    public async Task WaitForConnection(CancellationToken cancellationToken = default)
    {
        await _rawPipeStream.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);

        ConnectedToClient(_logger);

        _ = _messageProcessor.StartProcessing(cancellationToken);
    }

    /// <inheritdoc/>
    /// <exception cref="IOException">Thrown when the pipe has closed due to an unknown error.</exception>
    /// <remarks>This does not throw when the other end closes the pipe.</remarks>
    public Task WaitForRemotePipeClose(CancellationToken cancellationToken = default)
        => _messageProcessor.WaitForRemotePipeClose(cancellationToken);

    /// <summary>
    /// Closes the pipe.
    /// </summary>
    public void Dispose()
    {
        _rawPipeStream?.Dispose();
        _wrappedPipe?.Dispose();
        _messageProcessor?.Dispose();

        PipeHasBeenDisposed(_logger, _pipeName);
    }

    private NamedPipeServerStream CreatePipe()
    {
        var pipeOptionsToPass = _options == null
            ? PipeOptions.Asynchronous
            : _options.Value | PipeOptions.Asynchronous;

        SetUpNamedPipeServer(_logger);

        return new NamedPipeServerStream(
            _pipeName,
            PipeDirection.InOut,
            MaxNumberOfServerInstances,
            PipeTransmissionMode.Byte,
            pipeOptionsToPass);
    }

    [LoggerMessage(LogLevel.Information, "Connected to client.")]
    private static partial void ConnectedToClient(ILogger logger);

    [LoggerMessage(LogLevel.Debug, "Pipe '{PipeName}' has been disposed.")]
    private static partial void PipeHasBeenDisposed(ILogger logger, string pipeName);

    [LoggerMessage(LogLevel.Information, "Set up named pipe server.")]
    private static partial void SetUpNamedPipeServer(ILogger logger);
}
