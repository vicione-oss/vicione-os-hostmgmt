using Microsoft.Extensions.Logging;

namespace PipeCommunicationSample;

internal sealed class ConsoleLogger(LogLevel minimumLevel) : ILogger
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= minimumLevel;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (IsEnabled(logLevel))
            Console.WriteLine(formatter(state, exception));

        if (exception is not null)
            Console.WriteLine(exception.ToString());
    }
}

internal sealed class LoggerAdapter<T>(ILogger inner) : ILogger<T>
{
    private readonly ILogger _inner = inner;

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        => _inner.BeginScope(state);

    public bool IsEnabled(LogLevel logLevel) => _inner.IsEnabled(logLevel);

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        // Keep the log format as is
        _inner.Log(logLevel, eventId, state, exception, formatter);
    }
}

internal static class LoggerExtensions
{
    public static ILogger<T> As<T>(this ILogger logger) => new LoggerAdapter<T>(logger);
}
