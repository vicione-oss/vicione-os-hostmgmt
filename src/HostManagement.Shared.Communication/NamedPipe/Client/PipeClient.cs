using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipes;
using System.Net.Sockets;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace HostManagement.Shared.Communication.NamedPipe.Client;

/// <inheritdoc cref="IPipeClient"/>
/// <remarks>
/// This client uses a fixed maximum message size for all named-pipe communication.
/// <para>
/// The limit is currently <c>256 KiB</c> and applies to the serialized JSON payload only.
/// The varint length prefix is not included in this limit.
/// </para>
/// <para>
/// The limit is enforced by <see cref="PipeStreamWrapper"/> for both outgoing and incoming messages.
/// </para>
/// </remarks>
/// <param name="pipeName">The name of the pipe.</param>
/// <param name="logger">An <see cref="ILogger{PipeClient}"/> implementation.</param>
/// <param name="callbackHandlerRegistry">A <see cref="CallbackHandlerRegistry"/>.</param>
/// <param name="options">The pipe options.</param>
public sealed partial class PipeClient(
    string pipeName,
    ILogger<PipeClient>? logger = null,
    CallbackHandlerRegistry? callbackHandlerRegistry = null,
    PipeOptions? options = null)
    : IPipeClient
{
    /// <summary>
    /// The maximum allowed size of a serialized JSON message payload in bytes for client-side pipe communication.
    /// The varint length prefix is not included in this value.
    /// <para>Roughly 64x the size of the biggest message (set/get config).</para>
    /// </summary>
    private const int MaxMessageBytes = 256 * 1024;
    private const string PipeServerName = "."; // Represents the local machine

    private readonly string _pipeName = pipeName ?? throw new ArgumentNullException(nameof(pipeName));
    private readonly ILogger<PipeClient> _logger = logger ?? NullLogger<PipeClient>.Instance;
    private readonly CallbackHandlerRegistry _callbackHandlerRegistry = callbackHandlerRegistry ?? new();

    private readonly PipeOptions _options = options == null
        ? PipeOptions.Asynchronous
        : options.Value | PipeOptions.Asynchronous;

    private readonly CancellationTokenSource _internalCancellationSource = new();
    private readonly SemaphoreSlim _connectLock = new(1, 1);

    private readonly ConcurrentDictionary<Guid, ExpectedResponse> _expectedResponses = [];

    private Task? _processingTask;
    private int _connectionGeneration;
    private int _pipeStateRaw = (int)PipeState.NotOpened;
    private int _disposeCount;

    private NamedPipeClientStream? _namedPipeClientStream;

#pragma warning disable CA1859 // Needed for unit tests to mock the pipe stream wrapper
    private IPipeStreamWrapper? _pipeStreamWrapper;
#pragma warning restore CA1859

    /// <inheritdoc/>
    public PipeState State
    {
        get => (PipeState)Volatile.Read(ref _pipeStateRaw);
        private set => Volatile.Write(ref _pipeStateRaw, (int)value);
    }

    /// <summary>
    /// Disposes the pipe client.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposeCount, 1) == 1)
        {
            State = PipeState.Closed;
            return;
        }

        try
        {
            await CancelInternalOperationsAsync().ConfigureAwait(false);

            var processingTask = await SnapshotAndClearConnectionResourcesAsync().ConfigureAwait(false);

            await AwaitProcessingTaskDuringDisposeAsync(processingTask).ConfigureAwait(false);
        }
        finally
        {
            FinalizeDispose();
        }
    }

    /// <inheritdoc/>
    public async Task<NamedPipeMessage> SendRequestAsync(
        string topic,
        string content = "",
        CancellationToken cancellationToken = default)
        => await SendRequestAsync(topic, content, Guid.NewGuid(), cancellationToken);

    /// <inheritdoc/>
    public async Task<NamedPipeMessage> SendRequestAsync(
        string topic,
        string content,
        Guid correlationId,
        CancellationToken cancellationToken = default)
    {
        if (correlationId == Guid.Empty)
            throw new ArgumentException("Correlation ID must not be empty.", nameof(correlationId));

        await EnsureConnectedForRequestAsync(cancellationToken).ConfigureAwait(false);

        var expectedResponse = new ExpectedResponse();

        var message = new NamedPipeMessage
        {
            ConversationId = correlationId,
            Topic = topic,
            Type = MessageType.Request,
            Content = content
        };

        RegisterExpectedResponse(message.ConversationId, expectedResponse);

        CancellationTokenRegistration cancellationTokenRegister = default;

        try
        {
            cancellationTokenRegister = RegisterRequestCancellation(message.ConversationId, cancellationToken);

            var processingTask = _processingTask;
            var pipeStreamWrapper = GetReadyPipeStreamWrapper(processingTask);

            cancellationToken.ThrowIfCancellationRequested();

            await pipeStreamWrapper.SendMessageAsync(message, cancellationToken).ConfigureAwait(false);

            return await WaitForResponseOrProcessingStopAsync(expectedResponse, processingTask).ConfigureAwait(false);
        }
        catch
        {
            // Avoid leaking pending responses when sending or waiting fails.
            _ = TryRemoveExpectedResponse(message.ConversationId, out _);
            throw;
        }
        finally
        {
            await cancellationTokenRegister.DisposeAsync();
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// This method does not throw on connection failures. Typical failure reasons that yield <see langword="false"/>:<br/>
    /// - The operation was canceled via <paramref name="cancellationToken"/>.<br/>
    /// - The operation timed out (no server available or not responding).<br/>
    /// - A transient or permanent error occurred (e.g. access denied, connection refused).<br/><br/>
    /// In all these cases the error is logged internally and the method returns <see langword="false"/>.
    /// </remarks>
    public async Task<bool> TryConnectAsync(
        int timeoutMs = Timeout.Infinite,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        ValidateTimeout(timeoutMs);

        if (IsCurrentStreamConnected())
            return true;

        var connectionLockTaken = false;

        try
        {
            await _connectLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            connectionLockTaken = true;

            ThrowIfDisposed();

            if (IsCurrentStreamConnected())
                return true;

            await CreateOrRecreatePipeAsync().ConfigureAwait(false);
            await ConnectAsync(timeoutMs, cancellationToken).ConfigureAwait(false);

            StartProcessingTask();
            return true;
        }
        catch (ObjectDisposedException)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            return await HandleCanceledConnectAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return await HandleFailedConnectAsync(ex).ConfigureAwait(false);
        }
        finally
        {
            if (connectionLockTaken)
                _connectLock.Release();
        }
    }

    // Connection helpers

    private static void ValidateTimeout(int timeoutMs)
    {
        if (timeoutMs is < 0 and not Timeout.Infinite)
        {
            throw new ArgumentOutOfRangeException(
                nameof(timeoutMs),
                timeoutMs,
                "Timeout must be non-negative or Timeout.Infinite.");
        }
    }

    private bool IsCurrentStreamConnected()
        => State == PipeState.Connected
        && _namedPipeClientStream is { IsConnected: true };

    private async Task CreateOrRecreatePipeAsync()
    {
        ConnectingToPipe(_logger, _pipeName);

        State = PipeState.NotOpened;

        // Bump the generation before replacing resources so an old processing task
        // cannot clean up or change state for the new connection.
        Interlocked.Increment(ref _connectionGeneration);

        DisposePipeStreamWrapper();
        await DisposeNamedPipeClientStreamAsync().ConfigureAwait(false);

        _namedPipeClientStream = new NamedPipeClientStream(PipeServerName, _pipeName, PipeDirection.InOut, _options);
        _pipeStreamWrapper = new PipeStreamWrapper(_namedPipeClientStream, _logger, MaxMessageBytes);
    }

    private async Task ConnectAsync(int timeoutMs, CancellationToken cancellationToken)
    {
        var namedPipeClientStream = _namedPipeClientStream
            ?? throw new ObjectDisposedException(nameof(NamedPipeClientStream));

        await namedPipeClientStream.ConnectAsync(timeoutMs, cancellationToken).ConfigureAwait(false);

        State = PipeState.Connected;
        ConnectedToServer(_logger);
    }

    private void StartProcessingTask()
    {
        EnsurePreviousProcessingTaskFaultIsObserved();
        _processingTask = ProcessAsync(_internalCancellationSource.Token);
    }

    private void EnsurePreviousProcessingTaskFaultIsObserved()
    {
        var previousProcessingTask = _processingTask;

        if (previousProcessingTask is null || previousProcessingTask.IsCompletedSuccessfully)
            return;

        _ = previousProcessingTask.ContinueWith(
            static (task, state) =>
            {
                var client = (PipeClient)state!;

                if (task.Exception is not null)
                    OldProcessingTaskFaulted(client._logger, client._pipeName, task.Exception);
            },
            this,
            CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);
    }

    private async Task<bool> HandleCanceledConnectAsync()
    {
        await ResetCurrentConnectionAfterFailedConnectAsync().ConfigureAwait(false);
        ConnectingToPipeCanceled(_logger, _pipeName);
        return false;
    }

    private async Task<bool> HandleFailedConnectAsync(Exception exception)
    {
        await ResetCurrentConnectionAfterFailedConnectAsync().ConfigureAwait(false);
        ConnectingToPipeNotPossible(_logger, _pipeName, GetConnectFailureMessage(exception), exception);
        return false;
    }

    private async ValueTask ResetCurrentConnectionAfterFailedConnectAsync()
    {
        State = PipeState.NotOpened;

        DisposePipeStreamWrapper();
        await DisposeNamedPipeClientStreamAsync().ConfigureAwait(false);

        _pipeStreamWrapper = null;
        _namedPipeClientStream = null;
    }

    private static string GetConnectFailureMessage(Exception exception)
    {
        if (exception is not SocketException socketException)
            return exception.Message;

        return socketException.SocketErrorCode switch
        {
            SocketError.AccessDenied => "Permission denied. Please try again with privileged rights, e.g. 'sudo'.",
            SocketError.ConnectionRefused => "Connection was refused by the server.",
            SocketError.TimedOut => "The connection timed out.",
            _ => $"Unhandled SocketError: {socketException.SocketErrorCode} (Code: {socketException.ErrorCode})"
        };
    }

    // Request helpers

    private async Task EnsureConnectedForRequestAsync(CancellationToken cancellationToken)
    {
        ThrowIfDisposed();

        if (State != PipeState.Connected &&
            !await TryConnectAsync(cancellationToken: cancellationToken).ConfigureAwait(false))
        {
            throw new InvalidOperationException(
                $"Cannot send request to pipe '{_pipeName}' because it is not connected.");
        }

        ThrowIfDisposed();
    }

    private IPipeStreamWrapper GetReadyPipeStreamWrapper([NotNull] Task? processingTask)
    {
        var pipeStreamWrapper = _pipeStreamWrapper
            ?? throw new ObjectDisposedException(nameof(IPipeStreamWrapper), "Pipe stream wrapper should not be null.");

        ThrowIfDisposed();

        if (!IsReadyToSend(pipeStreamWrapper, processingTask))
            throw new OperationCanceledException($"Pipe '{_pipeName}' closed before request could be sent.");

        return pipeStreamWrapper;
    }

    private bool IsReadyToSend(IPipeStreamWrapper pipeStreamWrapper, [NotNullWhen(true)] Task? processingTask)
        => State == PipeState.Connected
        && pipeStreamWrapper.IsConnected
        && processingTask is not null
        && !processingTask.IsCompleted;

    private async Task<NamedPipeMessage> WaitForResponseOrProcessingStopAsync(
        ExpectedResponse expectedResponse,
        Task processingTask)
    {
        var expectedResponseTask = expectedResponse.TaskCompletionSource.Task;

        if (expectedResponseTask.IsCompleted)
            return await expectedResponseTask.ConfigureAwait(false);

        var completedTask = await Task.WhenAny(expectedResponseTask, processingTask).ConfigureAwait(false);

        if (completedTask == expectedResponseTask)
            return await expectedResponseTask.ConfigureAwait(false);

        // The processing loop stopped first. Await it so its finally block can drain pending responses.
        await processingTask.ConfigureAwait(false);

        // The drain usually completes the expected response.
        if (expectedResponseTask.IsCompleted)
            return await expectedResponseTask.ConfigureAwait(false);

        throw new OperationCanceledException($"Pipe '{_pipeName}' closed before a response was received.");
    }

    // Response tracking helpers

    private void RegisterExpectedResponse(Guid conversationId, ExpectedResponse expectedResponse)
    {
        if (!_expectedResponses.TryAdd(conversationId, expectedResponse))
            throw new InvalidOperationException($"Duplicate conversation id '{conversationId}'.");
    }

    private CancellationTokenRegistration RegisterRequestCancellation(
        Guid conversationId,
        CancellationToken cancellationToken)
    {
        return cancellationToken.Register(() =>
        {
            if (!TryRemoveExpectedResponse(conversationId, out var pending))
                return;

            SendRequestCanceled(_logger);

            pending.TaskCompletionSource.TrySetException(new OperationCanceledException("Request has been canceled."));
        });
    }

    private bool TryRemoveExpectedResponse(
        Guid conversationId,
        [MaybeNullWhen(false)] out ExpectedResponse expectedResponse)
            => _expectedResponses.TryRemove(conversationId, out expectedResponse);

    private void DrainExpectedResponses(Exception exception)
    {
        foreach (var expectedResponse in _expectedResponses)
        {
            if (TryRemoveExpectedResponse(expectedResponse.Key, out var pending))
                pending.TaskCompletionSource.TrySetException(exception);
        }
    }

    // Processing helpers

    private async Task ProcessAsync(CancellationToken cancellationToken)
    {
        // Capture the resources and generation this processing task owns.
        // Reconnects may replace the fields while this task is still shutting down.
        var pipeStreamWrapper = _pipeStreamWrapper;
        var namedPipeClientStream = _namedPipeClientStream;
        var connectionGeneration = Volatile.Read(ref _connectionGeneration);

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (pipeStreamWrapper is null)
                    {
                        throw new ObjectDisposedException(
                            nameof(PipeStreamWrapper),
                            "Pipe stream wrapper should not be null here.");
                    }

                    var message = await pipeStreamWrapper.ReadMessageAsync(cancellationToken).ConfigureAwait(false);

                    if (!await ProcessMessageAsync(message, cancellationToken).ConfigureAwait(false))
                        continue;
                }
                catch (Exception ex)
                {
                    HandleProcessingException(ex, connectionGeneration);
                    break;
                }
            }
        }
        finally
        {
            // Only the processing task for the current generation may complete pending responses
            // and clear the shared fields. Older tasks only dispose their own captured resources.
            var isCurrentConnection = Volatile.Read(ref _connectionGeneration) == connectionGeneration;

            if (isCurrentConnection)
                CompletePendingResponsesForCurrentConnection();

            DisposePipeStreamWrapper(pipeStreamWrapper);
            await DisposeNamedPipeClientStreamAsync(namedPipeClientStream).ConfigureAwait(false);

            if (isCurrentConnection)
            {
                ClearCurrentConnectionFieldsIfStillOwned(pipeStreamWrapper, namedPipeClientStream);

                if (pipeStreamWrapper is not null)
                    DisconnectedFromServer(_logger);
            }
        }
    }

    private async Task<bool> ProcessMessageAsync(NamedPipeMessage message, CancellationToken cancellationToken)
    {
        switch (message.Type)
        {
            case MessageType.Response:
                return ProcessResponseMessage(message);

            case MessageType.Event:
                await ProcessEventMessageAsync(message, cancellationToken).ConfigureAwait(false);
                return true;

            case MessageType.Request:
                throw new InvalidOperationException($"Message type '{message.Type}' may only be sent by the server.");

            default:
                throw new InvalidOperationException($"Message type '{message.Type}' is not recognized.");
        }
    }

    private bool ProcessResponseMessage(NamedPipeMessage message)
    {
        if (!TryRemoveExpectedResponse(message.ConversationId, out var expectedResponse))
        {
            UnexpectedResponse(_logger, message.ConversationId, message.Topic);
            return false;
        }

        expectedResponse.TaskCompletionSource.TrySetResult(message);
        return true;
    }

    private async Task ProcessEventMessageAsync(NamedPipeMessage message, CancellationToken cancellationToken)
    {
        try
        {
            await _callbackHandlerRegistry.HandleCallbackAsync(message, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            ErrorWhileProcessing(_logger, _pipeName, ex);
        }
    }

    private void HandleProcessingException(Exception exception, int connectionGeneration)
    {
        if (exception is OperationCanceledException or ObjectDisposedException)
        {
            SetStateIfCurrentConnection(connectionGeneration, PipeState.Closed);
            return;
        }

        if (exception is IOException)
        {
            // Common when the server closes the pipe on Unix; treat as a normal close
            SetStateIfCurrentConnection(connectionGeneration, PipeState.Closed);
            return;
        }

        ErrorWhileProcessing(_logger, _pipeName, exception);
        SetStateIfCurrentConnection(connectionGeneration, PipeState.Faulted);
    }

    private void CompletePendingResponsesForCurrentConnection()
    {
        var wasFaulted = State == PipeState.Faulted;

        if (!wasFaulted)
            State = PipeState.Closed;

        Exception ex = wasFaulted
            ? new IOException($"Pipe '{_pipeName}' faulted.")
            : new OperationCanceledException($"Pipe '{_pipeName}' closed.");

        DrainExpectedResponses(ex);
    }

    private void ClearCurrentConnectionFieldsIfStillOwned(
        IPipeStreamWrapper? pipeStreamWrapper,
        NamedPipeClientStream? namedPipeClientStream)
    {
        if (ReferenceEquals(_pipeStreamWrapper, pipeStreamWrapper))
            _pipeStreamWrapper = null;

        if (ReferenceEquals(_namedPipeClientStream, namedPipeClientStream))
            _namedPipeClientStream = null;

        _processingTask = null;
    }

    private void SetStateIfCurrentConnection(int connectionGeneration, PipeState state)
    {
        // Prevent an old processing task from overwriting the state of a newer connection.
        if (Volatile.Read(ref _connectionGeneration) == connectionGeneration)
            State = state;
    }

    // Disposal helpers

    private async ValueTask CancelInternalOperationsAsync()
    {
        try
        {
            await _internalCancellationSource.CancelAsync().ConfigureAwait(false);
        }
        catch (ObjectDisposedException)
        { }
    }

    private async ValueTask<Task?> SnapshotAndClearConnectionResourcesAsync()
    {
        var connectionLockTaken = false;

        try
        {
            // Synchronize with TryConnectAsync so no connection resources are created
            // while disposal snapshots and clears the current resources.
            await _connectLock.WaitAsync(CancellationToken.None).ConfigureAwait(false);
            connectionLockTaken = true;

            var processingTask = _processingTask;

            State = PipeState.Closed;
            _processingTask = null;

            // Dispose pipe resources to unblock any pending read.
            DisposePipeStreamWrapper();
            await DisposeNamedPipeClientStreamAsync().ConfigureAwait(false);

            _pipeStreamWrapper = null;
            _namedPipeClientStream = null;

            return processingTask;
        }
        catch (ObjectDisposedException)
        {
            return null;
        }
        finally
        {
            if (connectionLockTaken)
                _connectLock.Release();
        }
    }

    private async ValueTask AwaitProcessingTaskDuringDisposeAsync(Task? processingTask)
    {
        if (processingTask is null)
            return;

        try
        {
            await processingTask.ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is ObjectDisposedException or IOException or OperationCanceledException)
        {
            // These exceptions can occur during shutdown when cancellation/disposal races with pending pipe I/O.
            // Other exceptions should still propagate because they indicate unexpected processing failures.
            ExpectedCleanupErrorWhileDisposingPipeResources(_logger, _pipeName, ex);
        }
    }

    private void FinalizeDispose()
    {
        State = PipeState.Closed;

        _processingTask = null;
        _pipeStreamWrapper = null;
        _namedPipeClientStream = null;

        try { _internalCancellationSource.Dispose(); } catch (ObjectDisposedException) { }
        try { _connectLock.Dispose(); } catch (ObjectDisposedException) { }
    }

    private void DisposePipeStreamWrapper() => DisposePipeStreamWrapper(_pipeStreamWrapper);

    private void DisposePipeStreamWrapper(IPipeStreamWrapper? pipeStreamWrapper)
    {
        if (pipeStreamWrapper is null)
            return;

        try
        {
            pipeStreamWrapper.Dispose();
        }
        catch (Exception ex)
        {
            ExpectedCleanupErrorWhileDisposingPipeResources(_logger, _pipeName, ex);
        }
    }

    private async ValueTask DisposeNamedPipeClientStreamAsync()
        => await DisposeNamedPipeClientStreamAsync(_namedPipeClientStream).ConfigureAwait(false);

    private async ValueTask DisposeNamedPipeClientStreamAsync(NamedPipeClientStream? namedPipeClientStream)
    {
        if (namedPipeClientStream is null)
            return;

        try
        {
            await namedPipeClientStream.DisposeAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            ExpectedCleanupErrorWhileDisposingPipeResources(_logger, _pipeName, ex);
        }
    }

    // Lifecycle helpers

    private void ThrowIfDisposed()
        => ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposeCount) != 0, typeof(PipeClient));

    // Connection logging

    [LoggerMessage(LogLevel.Debug, "Connecting to pipe '{PipeName}'.")]
    private static partial void ConnectingToPipe(ILogger<PipeClient> logger, string pipeName);

    [LoggerMessage(LogLevel.Information, "Connected to pipe server.")]
    private static partial void ConnectedToServer(ILogger<PipeClient> logger);

    [LoggerMessage(LogLevel.Debug, "Connecting to pipe '{PipeName}' was canceled.")]
    private static partial void ConnectingToPipeCanceled(ILogger<PipeClient> logger, string pipeName);

    [LoggerMessage(LogLevel.Error, "Connecting to pipe '{PipeName}' not possible. {ExceptionMessage}")]
    private static partial void ConnectingToPipeNotPossible(ILogger<PipeClient> logger, string pipeName, string exceptionMessage, Exception exception);

    // Processing logging

    [LoggerMessage(LogLevel.Debug, "An old processing task for pipe '{PipeName}' faulted after it was replaced.")]
    private static partial void OldProcessingTaskFaulted(ILogger<PipeClient> logger, string pipeName, Exception exception);

    [LoggerMessage(LogLevel.Information, "Disconnected from pipe server.")]
    private static partial void DisconnectedFromServer(ILogger<PipeClient> logger);

    [LoggerMessage(LogLevel.Error, "An error occurred while processing messages on the pipe '{PipeName}'.")]
    private static partial void ErrorWhileProcessing(ILogger<PipeClient> logger, string pipeName, Exception exception);

    // Request logging

    [LoggerMessage(LogLevel.Debug, "Unexpected response on conversation {ConversationId} with topic {Topic}.")]
    private static partial void UnexpectedResponse(ILogger<PipeClient> logger, Guid conversationId, string topic);

    [LoggerMessage(LogLevel.Debug, "Send request was canceled.")]
    private static partial void SendRequestCanceled(ILogger<PipeClient> logger);

    // Disposal logging

    [LoggerMessage(LogLevel.Debug, "An expected cleanup error occurred while disposing pipe resources for pipe '{PipeName}'.")]
    private static partial void ExpectedCleanupErrorWhileDisposingPipeResources(ILogger<PipeClient> logger, string pipeName, Exception exception);
}
