using Microsoft.Extensions.Logging;

namespace PipeCommunicationSample;

internal sealed class ConsoleLogger(LogLevel minimumLevel) : ILogger
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= minimumLevel;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (IsEnabled(logLevel))
            Console.WriteLine(formatter(state, exception));

        if (exception is not null)
            Console.WriteLine(exception.ToString());
    }
}
