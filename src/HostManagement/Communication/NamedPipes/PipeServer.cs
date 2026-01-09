using System.IO.Pipes;
using Microsoft.Extensions.Logging;
using HostManagement.Communication.NamedPipes.RequestHandlers;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Enums;

namespace HostManagement.Communication.NamedPipes;

/// <inheritdoc cref="IPipeServer"/>
internal sealed partial class PipeServer : IPipeServer
{
    private const int MaxNumberOfServerInstances = 1;

    private readonly string _pipeName;
    private readonly ILogger _logger;
    private readonly PipeOptions? _options;
    private readonly RequestHandlerRegistry _handlerRegistry = new();
    private readonly NamedPipeServerStream _rawPipeStream;
    private readonly PipeStreamWrapper _wrappedPipe;
    private readonly PipeMessageProcessor _messageProcessor;

    /// <summary>
    /// Initializes a new <see cref="PipeServer" />.
    /// </summary>
    /// <param name="pipeName">The name of the pipe.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="options">Extra options for the pipe.</param>
    public PipeServer(string pipeName, ILogger logger, PipeOptions? options = null)
    {
        _pipeName = pipeName;
        _logger = logger;
        _options = options;

        InitHandlerRegistry();

        _rawPipeStream = CreatePipe();
        _wrappedPipe = new PipeStreamWrapper(_rawPipeStream, _logger);
        _messageProcessor = new PipeMessageProcessor(_wrappedPipe, _handlerRegistry);
    }

    public string PipeName => _pipeName;

    public ILogger Logger => _logger;

    public PipeState State => _messageProcessor?.State ?? PipeState.NotOpened;

    public Task PublishEvent(string topic, string content, CancellationToken cancellationToken = default)
        => _wrappedPipe.SendMessage(
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

    private void InitHandlerRegistry()
    {
        // Complete config
        _handlerRegistry.RegisterHandler(new GetSystemConfigurationRequestHandler());
        _handlerRegistry.RegisterHandler(new SetSystemConfigurationRequestHandler());

        // State (not part of config)
        _handlerRegistry.RegisterHandler(new GetOriginalPhysicalAddressRequestHandler(_logger));
        _handlerRegistry.RegisterHandler(new GetNetworkStatusInformationRequestHandler(_logger));
        _handlerRegistry.RegisterHandler(new GetDHCPLeaseInformationRequestHandler(_logger));
        _handlerRegistry.RegisterHandler(new GetNTPFallbackInformationRequestHandler(_logger));

        // Action (not part of config; may result in a new state and/or return data)
        _handlerRegistry.RegisterHandler(new GetViciOneSuiteVersionsRequestHandler(_logger));
        _handlerRegistry.RegisterHandler(new InstallViciOneSuiteVersionRequestHandler(_logger));

        _handlerRegistry.RegisterHandler(new RedeployScriptsRequestHandler());
        _handlerRegistry.RegisterHandler(new RenewDHCPLeaseRequestHandler(_logger));

        _handlerRegistry.RegisterHandler(new StartServiceRequestHandler(_logger));
        _handlerRegistry.RegisterHandler(new StopServiceRequestHandler(_logger));
        _handlerRegistry.RegisterHandler(new RestartServiceRequestHandler(_logger));

        _handlerRegistry.RegisterHandler(new ResetSystemRequestHandler(_logger));
        _handlerRegistry.RegisterHandler(new RestartSystemRequestHandler(_logger));
        _handlerRegistry.RegisterHandler(new ShutdownSystemRequestHandler(_logger));
        _handlerRegistry.RegisterHandler(new UpdateSystemRequestHandler(_logger));

        // Testing purposes
        _handlerRegistry.RegisterHandler(new EchoRequestHandler(this, _logger));
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
