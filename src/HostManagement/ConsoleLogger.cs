using Microsoft.Extensions.Logging;

namespace HostManagement;

internal sealed class ConsoleLogger(
    LogLevel minimumLevel,
    bool useShortLogLevels = true,
    bool addNewLine = false) : ILogger
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel)
        => logLevel >= minimumLevel;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var logLevelString = logLevel switch
        {
            LogLevel.Trace => useShortLogLevels ? "TRC" : "TRACE",
            LogLevel.Debug => useShortLogLevels ? "DBG" : "DEBUG",
            LogLevel.Information => useShortLogLevels ? "INF" : "INFORMATION",
            LogLevel.Warning => useShortLogLevels ? "WRN" : "WARNING",
            LogLevel.Error => useShortLogLevels ? "ERR" : "ERROR",
            LogLevel.Critical => useShortLogLevels ? "CRI" : "CRITICAL",
            LogLevel.None => useShortLogLevels ? "NON" : "NONE",
            _ => useShortLogLevels ? "UNK" : "UNKNOWN"
        };

        var output = $"{logLevelString}: {formatter(state, exception)}";

        if (exception is not null &&
            !output.Contains(exception.ToString()))
        {
            output += Environment.NewLine + exception;
        }

        if (Console.IsOutputRedirected)
        {
            Console.WriteLine(output);
        }
        else
        {
            var indicatorLength = logLevelString.Length + 1;
            var indicator = output[..indicatorLength];
            var text = output[indicatorLength..];

            Console.ForegroundColor = logLevel switch
            {
                LogLevel.Warning => ConsoleColor.Yellow,
                LogLevel.Error => ConsoleColor.Red,
                LogLevel.Critical => ConsoleColor.DarkMagenta,
                _ => Console.ForegroundColor
            };

            Console.Write(indicator);
            Console.ResetColor();
            Console.WriteLine(text);

            if (addNewLine)
                Console.WriteLine();
        }
    }
}
