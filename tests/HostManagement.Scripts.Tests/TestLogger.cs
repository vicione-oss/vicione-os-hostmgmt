using Microsoft.Extensions.Logging;

namespace HostManagement.Scripts.Tests;

internal sealed class TestLogger(LogLevel minimumLevel) : ILogger
{
    private readonly List<string?> _logs = [];

    public IReadOnlyCollection<string?> Logs => _logs.AsReadOnly();

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel)
        => logLevel >= minimumLevel;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (IsEnabled(logLevel))
            _logs.Add(formatter(state, exception));

        if (exception is not null)
            _logs.Add(exception.ToString());
    }
}
