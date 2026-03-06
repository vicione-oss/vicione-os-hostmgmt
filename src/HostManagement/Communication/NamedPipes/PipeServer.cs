using System.IO.Pipes;
using HostManagement.Communication.NamedPipes.RequestHandlers;
using HostManagement.Configuration;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Enums;
using Microsoft.Extensions.Logging;

namespace HostManagement.Communication.NamedPipes;

/// <inheritdoc cref="IPipeServer"/>
internal sealed partial class PipeServer : IPipeServer
{
    private const int MaxNumberOfServerInstances = 1;

    private readonly string _pipeName;
    private readonly ILogger _logger;
    private readonly PipeOptions? _options;
    private readonly RequestHandlerRegistry _handlerRegistry = new(EnvironmentVariables.TargetEnvironment);
    private readonly NamedPipeServerStream _rawPipeStream;
    private readonly PipeStreamWrapper _wrappedPipe;
    private readonly PipeMessageProcessor _messageProcessor;

    /// <summary>
    /// Initializes a new <see cref="PipeServer" />.
    /// </summary>
    /// <param name="pipeName">The name of the pipe.</param>
    /// <param name="logger">An <see cref="ILogger"/> implementation.</param>
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
#if DEBUG
        // Testing purposes
        _handlerRegistry.RegisterHandler(new EchoRequestHandler(this, _logger));
#endif

        RegisterRedeployScriptsRequestHandler();
        RegisterServiceRequestHandlers();
        RegisterViciOneRequestHandlers();
        RegisterInstallDebianPackageRequestHandler();
        RegisterSystemConfigurationRequestHandler();
        RegisterStateRequestHandlers();
        RegisterSystemRequestHandlers();
        RegisterRenewDHCPLeaseRequestHandler();
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

    private void RegisterInstallDebianPackageRequestHandler()
        => _handlerRegistry.RegisterHandler(new InstallSignedDebianPackageRequestHandler(_logger));

    private void RegisterRedeployScriptsRequestHandler()
        => _handlerRegistry.RegisterHandler(new RedeployScriptsRequestHandler());

    private void RegisterRenewDHCPLeaseRequestHandler()
        => _handlerRegistry.RegisterHandler(new RenewDHCPLeaseRequestHandler(_logger));

    private void RegisterServiceRequestHandlers()
    {
        _handlerRegistry.RegisterHandler(new GetServiceStateRequestHandler(_logger));
        _handlerRegistry.RegisterHandler(new StartServiceRequestHandler(_logger));
        _handlerRegistry.RegisterHandler(new StopServiceRequestHandler(_logger));
        _handlerRegistry.RegisterHandler(new RestartServiceRequestHandler(_logger));
    }

    private void RegisterStateRequestHandlers()
    {
        _handlerRegistry.RegisterHandler(new GetOriginalPhysicalAddressRequestHandler(_logger));
        _handlerRegistry.RegisterHandler(new GetNetworkStatusInformationRequestHandler(_logger));
        _handlerRegistry.RegisterHandler(new GetDHCPLeaseInformationRequestHandler(_logger));
        _handlerRegistry.RegisterHandler(new GetNTPFallbackInformationRequestHandler(_logger));
    }

    private void RegisterSystemConfigurationRequestHandler()
    {
        _handlerRegistry.RegisterHandler(new GetSystemConfigurationRequestHandler());
        _handlerRegistry.RegisterHandler(new SetSystemConfigurationRequestHandler());
    }

    private void RegisterSystemRequestHandlers()
    {
        _handlerRegistry.RegisterHandler(new ResetSystemRequestHandler(_logger));
        _handlerRegistry.RegisterHandler(new RestartSystemRequestHandler(_logger));
        _handlerRegistry.RegisterHandler(new ShutdownSystemRequestHandler(_logger));
        _handlerRegistry.RegisterHandler(new UpdateSystemRequestHandler(_logger));
    }

    private void RegisterViciOneRequestHandlers()
    {
        _handlerRegistry.RegisterHandler(new GetViciOneSuiteVersionsRequestHandler(_logger));
        _handlerRegistry.RegisterHandler(new InstallViciOneSuiteVersionRequestHandler(_logger));
    }

    [LoggerMessage(LogLevel.Information, "Connected to client.")]
    private static partial void ConnectedToClient(ILogger logger);

    [LoggerMessage(LogLevel.Debug, "Pipe '{PipeName}' has been disposed.")]
    private static partial void PipeHasBeenDisposed(ILogger logger, string pipeName);

    [LoggerMessage(LogLevel.Information, "Set up named pipe server.")]
    private static partial void SetUpNamedPipeServer(ILogger logger);
}
