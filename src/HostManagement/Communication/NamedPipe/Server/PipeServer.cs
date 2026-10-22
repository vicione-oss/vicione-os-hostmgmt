using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipes;
using HostManagement.Communication.NamedPipe.Extensions;
using HostManagement.Communication.NamedPipe.RequestHandlers;
using HostManagement.Configuration;
using HostManagement.Shared;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Communication.NamedPipe;
using Microsoft.Extensions.Logging;

namespace HostManagement.Communication.NamedPipe.Server;

/// <inheritdoc cref="IPipeServer"/>
/// <remarks>
/// This server uses a fixed maximum message size for all named-pipe communication.
/// <para>
/// The limit is currently <c>256 KiB</c> and applies to the serialized JSON payload only.
/// The varint length prefix is excluded from this limit.
/// </para>
/// <para>
/// The limit is enforced by <see cref="PipeStreamWrapper"/> for both outgoing and incoming messages.
/// </para>
/// </remarks>
internal sealed partial class PipeServer : IPipeServer
{
    private const int MaxNumberOfServerInstances = 2;

    /// <summary>
    /// The maximum allowed size of a serialized JSON message payload in bytes for server-side pipe communication.
    /// The varint length prefix is not included in this value.
    /// <para>Roughly 64x the size of the biggest message (set/get config).</para>
    /// </summary>
    private const int MaxMessageBytes = 256 * 1024;

    private const int BroadcastShutdownTimeoutMs = 250;
    private const int ListenerRecoveryDelayMs = 10_000;

    // Signals the queue processor whenever a new item has been enqueued.
    // The semaphore count intentionally represents pending queue work signals.
    private readonly SemaphoreSlim _messageQueueSignal = new(0);
    private readonly ConcurrentQueue<MessageQueueItem> _messageQueue = new();

    // Multiple listener instances can create the pipe concurrently,
    // but the pipe path initialization must run only once.
    private readonly SemaphoreSlim _pipeInitializationLock = new(1, 1);

    private readonly Lock _activeConnectionsLock = new();
    private readonly List<IPipeStreamWrapper> _activeConnections = [];

    private readonly CancellationTokenSource _internalCancellationSource = new();
    private readonly List<Task> _backgroundTasks = [];

    private readonly List<INotifyPublishEventRequested> _eventCapableRequestHandlers = [];

    private readonly string _pipeName;
    private readonly string _pipePath;
    private readonly ILogger _logger;
    private readonly RequestHandlerRegistry _requestHandlerRegistry;
    private readonly PipeOptions? _options;
    private readonly IPipeServerStreamFactory _pipeServerStreamFactory;
    private readonly IPipeServerInitializer _pipeServerInitializer;

    private bool _pipeInitialized;
    private int _serverStateRaw = (int)ServerState.NotStarted;
    private int _degradedListenerCount;
    private int _disposeCount;

    private IPipeStreamWrapper? _requestingConnection;

    /// <summary>
    /// Initializes a new <see cref="PipeServer"/>.
    /// </summary>
    /// <param name="pipeName">The name of the pipe.</param>
    /// <param name="logger">An <see cref="ILogger"/> implementation.</param>
    /// <param name="requestHandlerRegistry">The <see cref="RequestHandlerRegistry"/>.</param>
    /// <param name="options">The pipe options.</param>
    /// <param name="pipeServerStreamFactory">The factory used to create pipe streams and wrappers. Defaults to
    /// <see cref="DefaultPipeServerStreamFactory"/>, if null.</param>
    /// <param name="pipeServerInitializer">The initializer used to set up the pipe after creation. Defaults to
    /// <see cref="DefaultPipeServerInitializer"/>, if null.</param>
    public PipeServer(
        string pipeName,
        ILogger logger,
        RequestHandlerRegistry requestHandlerRegistry,
        PipeOptions? options = null,
        IPipeServerStreamFactory? pipeServerStreamFactory = null,
        IPipeServerInitializer? pipeServerInitializer = null)
    {
        _pipeName = pipeName ?? throw new ArgumentNullException(nameof(pipeName));
        _pipePath = Path.Combine(AppPaths.TempPath, $"CoreFxPipe_{pipeName}");
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _requestHandlerRegistry = requestHandlerRegistry ?? throw new ArgumentNullException(nameof(requestHandlerRegistry));
        _options = options;
        _pipeServerStreamFactory = pipeServerStreamFactory ?? new DefaultPipeServerStreamFactory();
        _pipeServerInitializer = pipeServerInitializer ?? new DefaultPipeServerInitializer();

        foreach (var eventCapableHandler in _requestHandlerRegistry.Handlers.OfType<INotifyPublishEventRequested>())
        {
            _eventCapableRequestHandlers.Add(eventCapableHandler);
            eventCapableHandler.PublishEventRequested += OnHandlerPublishEventRequested;
        }
    }

    private enum EnqueueMode
    {
        Normal,
        Shutdown
    }

    private enum MessageOrigin
    {
        Client,
        Server
    }

    public string PipePath => _pipePath;

    public ServerState State
    {
        get => (ServerState)Volatile.Read(ref _serverStateRaw);
        private set => Volatile.Write(ref _serverStateRaw, (int)value);
    }

    /// <summary>
    /// Disposes this pipe server.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposeCount, 1) == 1)
            return;

        await StopAsync().ConfigureAwait(false);

        State = ServerState.Disposing;

        foreach (var eventCapableHandler in _eventCapableRequestHandlers)
        {
            try { eventCapableHandler.PublishEventRequested -= OnHandlerPublishEventRequested; } catch { }
        }

        _eventCapableRequestHandlers.Clear();

        try { _internalCancellationSource.Dispose(); } catch (ObjectDisposedException) { }

        lock (_activeConnectionsLock)
        {
            foreach (var pipeStreamWrapper in _activeConnections)
            {
                try { pipeStreamWrapper.Dispose(); } catch { }
            }

            _activeConnections.Clear();
        }

        // Dispose synchronization primitives
        try { _messageQueueSignal.Dispose(); } catch (ObjectDisposedException) { }
        try { _pipeInitializationLock.Dispose(); } catch (ObjectDisposedException) { }

        State = ServerState.Disposed;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// The published event is sent through <see cref="PipeStreamWrapper"/> and is therefore subject to the configured
    /// maximum serialized payload size.
    /// </remarks>
    public Task PublishEvent(
        string topic,
        string content,
        Guid correlationId = default,
        CancellationToken cancellationToken = default)
    {
        EnqueueEventForActiveConnections(topic, content, EnqueueMode.Normal, cancellationToken, correlationId);
        return Task.CompletedTask;
    }

    public async Task StartAsync()
    {
        if (State != ServerState.NotStarted)
            throw new InvalidOperationException("Pipe server can only be started once.");

        State = ServerState.Starting;

        _backgroundTasks.Add(ProcessQueueAsync(_internalCancellationSource.Token));

        var startupTasks = new List<Task>(capacity: MaxNumberOfServerInstances);

        for (var i = 0; i < MaxNumberOfServerInstances; i++)
        {
            var startupSignal = new ListenerStartupSignal();
            startupTasks.Add(startupSignal.Task);

            _backgroundTasks.Add(ListenForConnectionsAsync(i + 1, startupSignal, _internalCancellationSource.Token));
            await Task.Delay(100, _internalCancellationSource.Token);
        }

        // Startup completes once every listener has either created and initialized its pipe
        // or has switched to degraded retry mode.
        await Task.WhenAll(startupTasks).ConfigureAwait(false);

        SetOperationalState();
        PipeServerStarted(_logger);
    }

    public async Task StopAsync()
    {
        try
        {
            State = ServerState.Stopping;

            await BroadcastShutdownAsync("Pipe server is stopping", TimeSpan.FromMilliseconds(BroadcastShutdownTimeoutMs))
                .ConfigureAwait(false);

            // Stop listening for new connections, processing messages and wait for all tasks to finish
            await _internalCancellationSource.CancelAsync();
            await Task.WhenAll(_backgroundTasks).ConfigureAwait(false);

            _backgroundTasks.Clear();

            State = ServerState.Stopped;
            PipeServerStopped(_logger);
        }
        catch (Exception ex)
        {
            PipeServerStopFailed(_logger, ex);
        }
    }

    public async Task WaitForExitAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await Task.WhenAll(_backgroundTasks).WaitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            WaitForPipeServerExitCanceled(_logger);
        }
        catch (Exception ex)
        {
            WaitForPipeServerExitFailed(_logger, ex);
        }
    }

    private void OnHandlerPublishEventRequested(object? sender, PublishEventRequestedEventArgs args)
    {
        var excludedConnection = args.ExcludeRequestingClient
            ? _requestingConnection
            : null;

        EnqueueEventForActiveConnections(
            args.Topic,
            args.Content,
            EnqueueMode.Normal,
            _internalCancellationSource.Token,
            args.CorrelationId,
            excludedConnection);
    }

    private async Task ProcessQueueAsync(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            MessageQueueProcessingCanceled(_logger);
            return;
        }

        // A single background consumer serializes request processing and outgoing server events.
        while (true)
        {
            while (_messageQueue.TryDequeue(out var item))
            {
                if (item.Barrier is not null)
                {
                    // Barrier items are used to wait until all previously queued items have been processed.
                    item.Barrier.TrySetResult();
                    continue;
                }

                var message = item.Message;
                var wrapper = item.PipeWrapper;
                var origin = item.Origin;

                if (message is null || wrapper is null || origin is null)
                {
                    QueueItemMissingMessageOrPipeWrapperOrOrigin(_logger);
                    continue;
                }

                try
                {
                    switch (origin.Value, message.Type)
                    {
                        case (MessageOrigin.Client, MessageType.Request):
                            await ProcessRequest(message, wrapper).ConfigureAwait(false); break;
                        case (MessageOrigin.Server, MessageType.Event):
                            await SendEvent(message, wrapper).ConfigureAwait(false); break;
                        default:
                            await SendDefaultResponse(message, wrapper).ConfigureAwait(false); break;
                    }
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    // Only stop queue processing, when server cancellation token was canceled
                    MessageQueueProcessingCanceled(_logger);
                    return;
                }
                catch (OperationCanceledException ex)
                {
                    // Log and continue processing, when single pipe/client cancellation occurs 
                    MessageQueueProcessingFailed(_logger, ex);
                }
                catch (ObjectDisposedException ex)
                {
                    TriedToSendToDisposedPipe(_logger, ex);
                }
                catch (Exception ex)
                {
                    MessageQueueProcessingFailed(_logger, ex);
                }
            }

            try
            {
                // Wait until another producer enqueues work. Cancellation stops the queue processor.
                await _messageQueueSignal.WaitAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                MessageQueueProcessingCanceled(_logger);
                return;
            }
        }

        async Task ProcessRequest(NamedPipeMessage message, IPipeStreamWrapper pipeWrapper)
        {
            _requestingConnection = pipeWrapper;

            try
            {
                var pipeResponse = await _requestHandlerRegistry
                    .HandleRequestAsync(message, cancellationToken)
                    .ConfigureAwait(false);

                await pipeWrapper
                    .SendMessageAsync(pipeResponse, cancellationToken)
                    .ConfigureAwait(false);

                ResponseSent(_logger, pipeResponse.Topic, pipeResponse.ConversationId);
                ResponseContentWas(_logger, pipeResponse.ConversationId, pipeResponse.Content);
            }
            finally
            {
                _requestingConnection = null;
            }
        }

        async Task SendEvent(NamedPipeMessage pipeEvent, IPipeStreamWrapper pipeWrapper)
        {
            await pipeWrapper
                .SendMessageAsync(pipeEvent, cancellationToken)
                .ConfigureAwait(false);

            EventSent(_logger, pipeEvent.Topic, pipeEvent.ConversationId);
            EventContentWas(_logger, pipeEvent.ConversationId, pipeEvent.Content);
        }

        async Task SendDefaultResponse(NamedPipeMessage message, IPipeStreamWrapper pipeWrapper)
        {
            var defaultResponse = message.CreateDefaultResponse(
                $"Unsupported message type. Message type '{message.Type}' may only be sent by the server.");

            await pipeWrapper
                .SendMessageAsync(defaultResponse, cancellationToken)
                .ConfigureAwait(false);

            DefaultResponseSent(_logger, defaultResponse.Topic, defaultResponse.ConversationId);
            DefaultResponseContentWas(_logger, defaultResponse.ConversationId, defaultResponse.Content);
        }
    }

    private async Task ListenForConnectionsAsync(
        int instanceId,
        ListenerStartupSignal startupSignal,
        CancellationToken cancellationToken)
    {
        var listenerMarkedAsDegraded = false;
        var startupSignaled = false;

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var pipeServerStream = await CreatePipeWithRetryAsync(instanceId, cancellationToken).ConfigureAwait(false);

                if (pipeServerStream is null)
                {
                    if (!await MarkListenerDegradedIfNeededAndDelayAsync().ConfigureAwait(false))
                        break;

                    continue;
                }

                try
                {
                    if (!await TryInitializePipeIfNeeded(cancellationToken).ConfigureAwait(false))
                    {
                        if (!await MarkListenerDegradedIfNeededAndDelayAsync().ConfigureAwait(false))
                            break;

                        continue;
                    }

                    MarkListenerRecoveredIfNeeded();
                    SignalStartupCompletedIfNeeded();

                    await pipeServerStream.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);

                    var pipeStreamWrapper = _pipeServerStreamFactory.CreateWrapper(pipeServerStream, _logger, MaxMessageBytes);

                    lock (_activeConnectionsLock)
                    {
                        _activeConnections.Add(pipeStreamWrapper);
                    }

                    ClientConnected(_logger, instanceId);

                    await ReadRequestsAsync(pipeStreamWrapper, instanceId, cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    SignalStartupCompletedIfNeeded();
                    ConnectionListenerCanceled(_logger, instanceId);
                    break;
                }
                catch (Exception ex)
                {
                    SignalStartupCompletedIfNeeded();
                    ConnectionListenerFailed(_logger, instanceId, ex);

                    // Stops the listener only if the retry delay was canceled.
                    if (!await DelayAsync(1_000).ConfigureAwait(false))
                        break;
                }
                finally
                {
                    await pipeServerStream.DisposeAsync().ConfigureAwait(false);
                }
            }
        }
        finally
        {
            MarkListenerRecoveredIfNeeded();
            SignalStartupCompletedIfNeeded();
        }

        async Task<bool> DelayAsync(int delayMs)
        {
            try
            {
                await Task.Delay(delayMs, cancellationToken).ConfigureAwait(false);
                return true;
            }
            catch (OperationCanceledException)
            {
                SignalStartupCompletedIfNeeded();
                ConnectionListenerCanceled(_logger, instanceId);
                return false;
            }
        }

        async Task<bool> MarkListenerDegradedIfNeededAndDelayAsync()
        {
            // The listener is considered degraded as long as neither pipe creation nor
            // pipe initialization has been successfully completed.

            if (cancellationToken.IsCancellationRequested)
            {
                SignalStartupCompletedIfNeeded();
                ConnectionListenerCanceled(_logger, instanceId);
                return false;
            }

            MarkListenerDegradedIfNeeded();
            SignalStartupCompletedIfNeeded();
            ConnectionListenerRetryScheduled(_logger, instanceId, ListenerRecoveryDelayMs);

            return await DelayAsync(ListenerRecoveryDelayMs).ConfigureAwait(false);
        }

        void MarkListenerDegradedIfNeeded()
        {
            if (listenerMarkedAsDegraded)
                return;

            listenerMarkedAsDegraded = true;
            var degradedListenerCount = Interlocked.Increment(ref _degradedListenerCount);

            SetOperationalState();
            PipeServerDegraded(_logger, degradedListenerCount, MaxNumberOfServerInstances);
        }

        void MarkListenerRecoveredIfNeeded()
        {
            if (!listenerMarkedAsDegraded)
                return;

            listenerMarkedAsDegraded = false;
            var degradedListenerCount = Interlocked.Decrement(ref _degradedListenerCount);

            if (degradedListenerCount < 0)
            {
                Interlocked.Exchange(ref _degradedListenerCount, 0);
                return;
            }

            SetOperationalState();

            if (degradedListenerCount == 0 && State == ServerState.Started)
                PipeServerRecovered(_logger);
        }

        void SignalStartupCompletedIfNeeded()
        {
            if (startupSignaled)
                return;

            startupSignaled = true;
            startupSignal.SignalCompleted();
        }
    }

    private async Task<NamedPipeServerStream?> CreatePipeWithRetryAsync(
        int instanceId,
        CancellationToken cancellationToken)
    {
        const int MaxAttempts = 3;

        for (var attempt = 1; attempt <= MaxAttempts && !cancellationToken.IsCancellationRequested; attempt++)
        {
            if (TryCreatePipe(instanceId, out var pipeServerStream))
                return pipeServerStream;

            if (pipeServerStream is not null)
                await pipeServerStream.DisposeAsync().ConfigureAwait(false);

            if (attempt < MaxAttempts)
            {
                try
                {
                    await Task.Delay(1_000, cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    return null;
                }
            }
        }

        return null;
    }

    private bool TryCreatePipe(
        int instanceId,
        [NotNullWhen(true)] out NamedPipeServerStream? namedPipeServerStream)
    {
        namedPipeServerStream = null;

        try
        {
            SettingUpPipeServerInstance(_logger, instanceId);

            namedPipeServerStream = _pipeServerStreamFactory.Create(_pipeName, MaxNumberOfServerInstances, _options);

            return true;
        }
        catch (Exception ex)
        {
            SettingUpPipeServerInstanceFailed(_logger, instanceId, ex);
            return false;
        }
    }

    private async Task<bool> TryInitializePipeIfNeeded(CancellationToken cancellationToken)
    {
        if (_pipeInitialized)
            return true;

        await _pipeInitializationLock.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            if (_pipeInitialized)
                return true;

            SettingPipePermissionsAndOwnership(_logger);

            await _pipeServerInitializer.InitializeAsync(this, cancellationToken).ConfigureAwait(false);

            _pipeInitialized = true;
            return true;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            SettingPipePermissionsAndOwnershipFailed(_logger, _pipePath, ex);

            _pipeInitialized = false;
            return false;
        }
        finally
        {
            _pipeInitializationLock.Release();
        }
    }

    private async Task ReadRequestsAsync(
#pragma warning disable CA1859 // Needed for unit tests to mock the pipe stream wrapper
        IPipeStreamWrapper pipeStreamWrapper,
#pragma warning restore CA1859
        int instanceId,
        CancellationToken cancellationToken)
    {
        try
        {
            while (pipeStreamWrapper.IsConnected && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var request = await pipeStreamWrapper.ReadMessageAsync(cancellationToken).ConfigureAwait(false);

                    EnqueueMessage(MessageQueueItem.AsClientMessage(request, pipeStreamWrapper));

                    if (request.Type == MessageType.Request)
                    {
                        RequestReceived(_logger, request.Topic, request.ConversationId);

                        if (string.IsNullOrWhiteSpace(request.Content))
                            continue;

                        RequestContentWas(_logger, instanceId, request.ConversationId, request.Content);
                    }
                }
                catch (OperationCanceledException)
                {
                    ClientDisconnected(_logger, instanceId);
                    break;
                }
                catch (Exception ex)
                {
                    PipeRequestReadFailed(_logger, instanceId, ex);
                    break;
                }
            }
        }
        finally
        {
            lock (_activeConnectionsLock)
            {
                _activeConnections.Remove(pipeStreamWrapper);
            }

            pipeStreamWrapper.Dispose();
            PipeServerInstanceDisposed(_logger, instanceId);
        }
    }

    private async Task BroadcastShutdownAsync(string reason, TimeSpan timeout)
    {
        EnqueueEventForActiveConnections(
            topic: EventTopics.PipeServerShutdown,
            content: reason,
            enqueueMode: EnqueueMode.Shutdown,
            cancellationToken: CancellationToken.None);

        // Enqueue a barrier item and wait until it's processed (meaning all items before it were sent)
        await EnqueueShutdownBarrierAndWaitAsync(timeout).ConfigureAwait(false);
    }

    private void EnqueueEventForActiveConnections(
        string topic,
        string content,
        EnqueueMode enqueueMode,
        CancellationToken cancellationToken,
        Guid correlationId = default,
        IPipeStreamWrapper? excludedConnection = null)
    {
        var eventMessage = new NamedPipeMessage()
        {
            ConversationId = correlationId,
            Topic = topic,
            Content = content,
            Type = MessageType.Event
        };

        // Take a snapshot so events can be enqueued without holding the connection lock during sends.
        IPipeStreamWrapper[] wrappers;
        lock (_activeConnectionsLock)
        {
            wrappers = [.. _activeConnections];
        }

        // Enqueue the event for all currently connected clients
        foreach (var pipeStreamWrapper in wrappers)
        {
            if (ReferenceEquals(pipeStreamWrapper, excludedConnection) ||
                !pipeStreamWrapper.IsConnected ||
                cancellationToken.IsCancellationRequested)
            {
                continue;
            }

            EnqueueMessageCore(MessageQueueItem.AsServerMessage(eventMessage, pipeStreamWrapper), enqueueMode);
        }
    }

    private async Task EnqueueShutdownBarrierAndWaitAsync(TimeSpan timeout)
    {
        var barrier = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        EnqueueMessageCore(MessageQueueItem.AsBarrier(barrier), EnqueueMode.Shutdown);

        using var timeoutCts = new CancellationTokenSource(timeout);

        try
        {
            await barrier.Task.WaitAsync(timeoutCts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            WaitForShutdownBroadcastTimedOut(_logger);
        }
    }

    private void EnqueueMessage(MessageQueueItem item) => EnqueueMessageCore(item, EnqueueMode.Normal);

    private void EnqueueMessageCore(MessageQueueItem item, EnqueueMode enqueueMode)
    {
        var state = State;

        if (state is ServerState.Stopped or ServerState.Disposing or ServerState.Disposed)
            return;

        // During shutdown, only shutdown-specific queue items (EnqueueMode.Shutdown) are still accepted.
        if (enqueueMode == EnqueueMode.Normal && state == ServerState.Stopping)
            return;

        _messageQueue.Enqueue(item);
        _messageQueueSignal.Release();
    }

    private void SetOperationalState()
    {
        var state = State;

        if (state is ServerState.Stopping or ServerState.Stopped or ServerState.Disposing or ServerState.Disposed)
            return;

        State = Volatile.Read(ref _degradedListenerCount) > 0
            ? ServerState.Degraded
            : ServerState.Started;
    }

    // Lifecycle logging

    [LoggerMessage(LogLevel.Information, "Pipe server started.")]
    private static partial void PipeServerStarted(ILogger logger);

    [LoggerMessage(LogLevel.Information, "Pipe server stopped.")]
    private static partial void PipeServerStopped(ILogger logger);

    [LoggerMessage(LogLevel.Warning, "Pipe server degraded. {DegradedCount} of {MaxCount} instances are degraded.")]
    private static partial void PipeServerDegraded(ILogger logger, int degradedCount, int maxCount);

    [LoggerMessage(LogLevel.Information, "Pipe server recovered.")]
    private static partial void PipeServerRecovered(ILogger logger);

    // Shutdown / Exit logging

    [LoggerMessage(LogLevel.Error, "Stopping pipe server failed.")]
    private static partial void PipeServerStopFailed(ILogger logger, Exception exception);

    [LoggerMessage(LogLevel.Debug, "Wait for shutdown broadcast timed out, some clients may not have received it.")]
    private static partial void WaitForShutdownBroadcastTimedOut(ILogger logger);

    [LoggerMessage(LogLevel.Debug, "Wait for pipe server exit has been canceled.")]
    private static partial void WaitForPipeServerExitCanceled(ILogger logger);

    [LoggerMessage(LogLevel.Error, "Wait for pipe server exit failed.")]
    private static partial void WaitForPipeServerExitFailed(ILogger logger, Exception exception);

    // Listener / Connection logging

    [LoggerMessage(LogLevel.Debug, "Setting up pipe server instance {InstanceId}.")]
    private static partial void SettingUpPipeServerInstance(ILogger logger, int instanceId);

    [LoggerMessage(LogLevel.Error, "Setting up pipe server instance {InstanceId} failed.")]
    private static partial void SettingUpPipeServerInstanceFailed(ILogger logger, int instanceId, Exception exception);

    [LoggerMessage(LogLevel.Debug, "Setting pipe execute permissions and ownership.")]
    private static partial void SettingPipePermissionsAndOwnership(ILogger logger);

    [LoggerMessage(LogLevel.Error, "Setting pipe execute permissions and ownership for '{PipePath}' failed.")]
    private static partial void SettingPipePermissionsAndOwnershipFailed(ILogger logger, string pipePath, Exception exception);

    [LoggerMessage(LogLevel.Information, "Client connected to pipe server instance {InstanceId}.")]
    private static partial void ClientConnected(ILogger logger, int instanceId);

    [LoggerMessage(LogLevel.Information, "Client disconnected from pipe server instance {InstanceId}.")]
    private static partial void ClientDisconnected(ILogger logger, int instanceId);

    [LoggerMessage(LogLevel.Debug, "Disposed pipe server instance {InstanceId}.")]
    private static partial void PipeServerInstanceDisposed(ILogger logger, int instanceId);

    [LoggerMessage(LogLevel.Debug, "Listening for connections on pipe server instance {InstanceId} has been canceled.")]
    private static partial void ConnectionListenerCanceled(ILogger logger, int instanceId);

    [LoggerMessage(LogLevel.Warning, "Listening for connections on pipe server instance {InstanceId} failed. Retrying in {DelayMs} ms.")]
    private static partial void ConnectionListenerRetryScheduled(ILogger logger, int instanceId, int delayMs);

    [LoggerMessage(LogLevel.Error, "Listening for connections on pipe server instance {InstanceId} failed.")]
    private static partial void ConnectionListenerFailed(ILogger logger, int instanceId, Exception exception);

    [LoggerMessage(LogLevel.Error, "Reading pipe request on pipe server instance {InstanceId} failed.")]
    private static partial void PipeRequestReadFailed(ILogger logger, int instanceId, Exception exception);

    // Request / Response / Event logging

    [LoggerMessage(LogLevel.Information, "Request with topic '{Topic}' received (Conversation ID: {ConversationId}).")]
    private static partial void RequestReceived(ILogger logger, string topic, Guid conversationId);

    [LoggerMessage(LogLevel.Debug, "Request content on pipe server instance {InstanceId} for conversation ID '{ConversationId}' was: {Content}")]
    private static partial void RequestContentWas(ILogger logger, int instanceId, Guid conversationId, string content);

    [LoggerMessage(LogLevel.Information, "Response with topic '{Topic}' sent (Conversation ID: {ConversationId}).")]
    private static partial void ResponseSent(ILogger logger, string topic, Guid conversationId);

    [LoggerMessage(LogLevel.Debug, "Response content for conversation ID '{ConversationId}' was: {Content}")]
    private static partial void ResponseContentWas(ILogger logger, Guid conversationId, string content);

    [LoggerMessage(LogLevel.Information, "Default response with topic '{Topic}' sent (Conversation ID: {ConversationId}).")]
    private static partial void DefaultResponseSent(ILogger logger, string topic, Guid conversationId);

    [LoggerMessage(LogLevel.Debug, "Default response content for conversation ID '{ConversationId}' was: {Content}")]
    private static partial void DefaultResponseContentWas(ILogger logger, Guid conversationId, string content);

    [LoggerMessage(LogLevel.Information, "Event with topic '{Topic}' sent (Correlation ID: {CorrelationId}).")]
    private static partial void EventSent(ILogger logger, string topic, Guid correlationId);

    [LoggerMessage(LogLevel.Debug, "Event content for correlation ID '{CorrelationId}' was: {Content}")]
    private static partial void EventContentWas(ILogger logger, Guid correlationId, string content);

    [LoggerMessage(LogLevel.Error, "Tried to send response to a disposed pipe.")]
    private static partial void TriedToSendToDisposedPipe(ILogger logger, Exception exception);

    // Queue Processing logging

    [LoggerMessage(LogLevel.Debug, "Processing message queue has been canceled.")]
    private static partial void MessageQueueProcessingCanceled(ILogger logger);

    [LoggerMessage(LogLevel.Error, "Processing message queue failed.")]
    private static partial void MessageQueueProcessingFailed(ILogger logger, Exception exception);

    [LoggerMessage(LogLevel.Debug, "Queue item was missing message, pipe wrapper or origin.")]
    private static partial void QueueItemMissingMessageOrPipeWrapperOrOrigin(ILogger logger);


    private sealed class ListenerStartupSignal
    {
        private readonly TaskCompletionSource _taskCompletionSource =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task Task => _taskCompletionSource.Task;

        public void SignalCompleted() => _taskCompletionSource.TrySetResult();
    }

    private sealed class MessageQueueItem
    {
        private MessageQueueItem(
            NamedPipeMessage? message,
            IPipeStreamWrapper? pipeWrapper,
            TaskCompletionSource? barrier,
            MessageOrigin? origin)
        {
            Message = message;
            PipeWrapper = pipeWrapper;
            Barrier = barrier;
            Origin = origin;
        }

        public NamedPipeMessage? Message { get; }
        public IPipeStreamWrapper? PipeWrapper { get; }
        public TaskCompletionSource? Barrier { get; }
        public MessageOrigin? Origin { get; }

        public static MessageQueueItem AsClientMessage(NamedPipeMessage message, IPipeStreamWrapper pipeWrapper)
            => new(message, pipeWrapper, null, MessageOrigin.Client);

        public static MessageQueueItem AsServerMessage(NamedPipeMessage message, IPipeStreamWrapper pipeWrapper)
            => new(message, pipeWrapper, null, MessageOrigin.Server);

        public static MessageQueueItem AsBarrier(TaskCompletionSource barrier)
            => new(null, null, barrier, null);
    }
}
