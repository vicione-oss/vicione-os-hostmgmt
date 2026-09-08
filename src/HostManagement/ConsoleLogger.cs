using Microsoft.Extensions.Logging;

namespace HostManagement;

/// <summary>
/// Provides logging for the application.
/// <list type="bullet">
/// <item>If the application is started as a service, logs are send to the journal.</item>
/// <item>If the application is started via console, logs are printed to it directly.</item>
/// </list>
/// </summary>
/// <param name="minimumLevel">The minimum log level to write.</param>
/// <param name="useShortLogLevels">Whether to use short log level names.</param>
internal sealed class ConsoleLogger(LogLevel minimumLevel, bool useShortLogLevels = true) : ILogger
{
    /// <inheritdoc/>
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    /// <inheritdoc/>
    public bool IsEnabled(LogLevel logLevel) => logLevel >= minimumLevel;

    /// <inheritdoc/>
    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var formattedMessage = formatter(state, exception);

        if (exception is null && (state is null || formattedMessage == "[null]"))
            return;

        var output = formattedMessage;

        if (exception is not null &&
            !output.Contains(exception.ToString()))
        {
            output += Environment.NewLine + exception;
        }

        output = output.Replace("\\u0027", "'");

        if (Console.IsOutputRedirected)
        {
            var journalPriority = logLevel switch
            {
                LogLevel.Trace => 7,       // debug
                LogLevel.Debug => 7,       // debug
                LogLevel.Information => 6, // info
                LogLevel.Warning => 4,     // warning
                LogLevel.Error => 3,       // err
                LogLevel.Critical => 2,    // crit
                _ => 6                     // info
            };

            foreach (var line in output.ReplaceLineEndings("\n").Split('\n'))
            {
                Console.WriteLine($"<{journalPriority}>{line}");
            }
        }
        else
        {
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

            output = $"{logLevelString}: {output}";

            var indicatorLength = logLevelString.Length + 1;
            var indicator = output[..indicatorLength];
            var text = output[indicatorLength..];

            Console.ForegroundColor = logLevel switch
            {
                LogLevel.Debug => ConsoleColor.Cyan,
                LogLevel.Information => ConsoleColor.Blue,
                LogLevel.Warning => ConsoleColor.Yellow,
                LogLevel.Error => ConsoleColor.Red,
                _ => Console.ForegroundColor
            };

            Console.Write(indicator);
            Console.ResetColor();
            Console.WriteLine(text);
        }
    }
}
