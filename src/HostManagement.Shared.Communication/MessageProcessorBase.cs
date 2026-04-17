using HostManagement.Shared.Communication.Enums;

namespace HostManagement.Shared.Communication;

/// <summary>
/// Represents the <see cref="MessageProcessorBase"/>.
/// </summary>
/// <param name="pipeStreamWrapper">A <see cref="PipeStreamWrapper"/>.</param>
public abstract class MessageProcessorBase(IPipeStreamWrapper pipeStreamWrapper) : IDisposable
{
    private bool _disposed;
    private TaskCompletionSource<object?>? _pipeCloseCompletionSource;
    private CancellationTokenSource? _workLoopCancellationTokenSource;

    /// <summary>
    /// Gets the pipe fault exception.
    /// </summary>
    public Exception? PipeFault { get; private set; }

    /// <summary>
    /// Gets the PipeStreamWrapper.
    /// </summary>
    protected IPipeStreamWrapper PipeStreamWrapper { get; } = pipeStreamWrapper;

    /// <summary>
    /// Gets the state of the pipe.
    /// </summary>
    public PipeState State { get; private set; }

    /// <summary>
    /// Releases all resources used by the current instance of the <see cref="MessageProcessorBase"/> class.
    /// </summary>
    /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources;
    /// <see langword="false"/> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _workLoopCancellationTokenSource?.Cancel();
                _workLoopCancellationTokenSource?.Dispose();
                PipeStreamWrapper.Dispose();
            }

            _disposed = true;
        }
    }

    /// <summary>
    /// Disposes resources associated with the instance.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Processes the message.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the processing.</param>
    protected abstract Task ProcessMessage(CancellationToken cancellationToken);

    /// <summary>
    /// Starts the processing loop on the pipe.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the processing.</param>
    public async Task StartProcessing(CancellationToken cancellationToken = default)
    {
        if (State != PipeState.NotOpened)
            throw new InvalidOperationException("Can only call connect once");
        State = PipeState.Connected;

        try
        {
            _workLoopCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            // Process messages until canceled.
            while (true)
            {
                _workLoopCancellationTokenSource.Token.ThrowIfCancellationRequested();
                await ProcessMessage(_workLoopCancellationTokenSource.Token).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            // This is a normal dispose.
            State = PipeState.Closed;
            _pipeCloseCompletionSource?.TrySetResult(null);
        }
        catch (Exception exception)
        {
            State = PipeState.Faulted;
            PipeFault = exception;
            if (_pipeCloseCompletionSource != null)
            {
                if (exception is not IOException)
                    exception = new IOException("Pipe closed with error", exception);

                _pipeCloseCompletionSource.TrySetException(exception);
            }
        }
    }

    /// <summary>
    /// Wait for the other end to close the pipe.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="IOException">Thrown when the pipe has closed due to an unknown error.</exception>
    /// <remarks>This does not throw when the other end closes the pipe.</remarks>
    public Task WaitForRemotePipeClose(CancellationToken cancellationToken = default)
    {
        if (State == PipeState.Closed)
            return Task.CompletedTask;

        if (State == PipeState.Faulted && PipeFault is not null)
            return Task.FromException(PipeFault);

        _pipeCloseCompletionSource ??= new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);

        cancellationToken.Register(() =>
        {
            _pipeCloseCompletionSource.TrySetCanceled();
        });

        return _pipeCloseCompletionSource.Task;
    }
}
