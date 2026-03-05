using Microsoft.Extensions.Logging;

namespace HostManagement;

/// <summary>
/// Provides console logging for the application.
/// </summary>
/// <param name="minimumLevel">The minimum log level to write.</param>
/// <param name="useShortLogLevels">Whether to use short log level names.</param>
/// <param name="addNewLine">Whether to add an extra newline after each entry.</param>
internal sealed class ConsoleLogger(
    LogLevel minimumLevel,
    bool useShortLogLevels = true,
    bool addNewLine = false) : ILogger
{
    /// <inheritdoc/>
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    /// <inheritdoc/>
    public bool IsEnabled(LogLevel logLevel)
        => logLevel >= minimumLevel;

    /// <inheritdoc/>
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var formattedMessage = formatter(state, exception);

        if (exception is null && (state is null || formattedMessage == "[null]"))
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

        var output = $"{logLevelString}: {formattedMessage}";

        if (exception is not null &&
            !output.Contains(exception.ToString()))
        {
            output += Environment.NewLine + exception;
        }

        output = output.Replace("\\u0027", "'");

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
