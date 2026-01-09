using System.IO.Pipes;
using HostManagement.Communication.NamedPipes.RequestHandlers;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Enums;
using Microsoft.Extensions.Logging;

namespace HostManagement.Communication.NamedPipes;

/// <inheritdoc cref="IPipeServer"/>
internal sealed class PipeServer : IPipeServer
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

        _logger.LogInformation("Connected to client");

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

        _logger.LogInformation("Pipe '{PipeName}' has been closed.", _pipeName);
    }

    private void InitHandlerRegistry()
    {
        _handlerRegistry.RegisterHandler(new GetSystemConfigurationRequestHandler());
        _handlerRegistry.RegisterHandler(new SetSystemConfigurationRequestHandler());
        _handlerRegistry.RegisterHandler(new GetViciOneSuiteVersionsRequestHandler(_logger));
        _handlerRegistry.RegisterHandler(new InstallViciOneSuiteVersionRequestHandler(_logger));
        _handlerRegistry.RegisterHandler(new RedeployScriptsRequestHandler());
        _handlerRegistry.RegisterHandler(new GetNetworkStatusInformationRequestHandler(_logger));
        _handlerRegistry.RegisterHandler(new RenewDHCPLeaseRequestHandler(_logger));
        _handlerRegistry.RegisterHandler(new StartServiceRequestHandler(_logger));
        _handlerRegistry.RegisterHandler(new StopServiceRequestHandler(_logger));
        _handlerRegistry.RegisterHandler(new RestartServiceRequestHandler(_logger));
        _handlerRegistry.RegisterHandler(new ResetSystemRequestHandler(_logger));
        _handlerRegistry.RegisterHandler(new RestartSystemRequestHandler(_logger));
        _handlerRegistry.RegisterHandler(new EchoRequestHandler(this, _logger)); //For testing purposes
    }

    private NamedPipeServerStream CreatePipe()
    {
        var pipeOptionsToPass = _options == null
            ? PipeOptions.Asynchronous
            : _options.Value | PipeOptions.Asynchronous;

        _logger.LogInformation("Set up named pipe server '{PipeName}'.", _pipeName);

        return new NamedPipeServerStream(
            _pipeName,
            PipeDirection.InOut,
            MaxNumberOfServerInstances,
            PipeTransmissionMode.Byte,
            pipeOptionsToPass);
    }
}
