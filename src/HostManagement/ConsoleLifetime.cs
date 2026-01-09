using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

namespace HostManagement;

internal class ConsoleLifetime : IDisposable
{
    private readonly Action<ILogger, string, Exception?> _logReceivedSignal = LoggerMessage.Define<string>(LogLevel.Debug, 0, "Received signal: {Signal}");
    private readonly ILogger _logger;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private PosixSignalRegistration? _sigIntRegistration;
    private PosixSignalRegistration? _sigQuitRegistration;
    private PosixSignalRegistration? _sigTermRegistration;

    internal ConsoleLifetime(ILogger logger)
    {
        _logger = logger;
        RegisterShutdownHandlers();
    }

    internal CancellationToken Token => _cancellationTokenSource.Token;

    public void Dispose()
    {
        UnregisterShutdownHandlers();
        _cancellationTokenSource.Dispose();
    }

    private void RegisterShutdownHandlers()
    {
        if (!OperatingSystem.IsWasi())
        {
            Action<PosixSignalContext> handler = HandlePosixSignal;
            _sigIntRegistration = PosixSignalRegistration.Create(PosixSignal.SIGINT, handler);
            _sigQuitRegistration = PosixSignalRegistration.Create(PosixSignal.SIGQUIT, handler);
            _sigTermRegistration = PosixSignalRegistration.Create(PosixSignal.SIGTERM, handler);
        }
    }

    private void HandlePosixSignal(PosixSignalContext context)
    {
        _logReceivedSignal(_logger, context.Signal.ToString(), null);

        context.Cancel = true;
        _cancellationTokenSource.Cancel();
    }

    private void UnregisterShutdownHandlers()
    {
        _sigIntRegistration?.Dispose();
        _sigQuitRegistration?.Dispose();
        _sigTermRegistration?.Dispose();
    }
}
