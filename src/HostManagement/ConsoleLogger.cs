using Microsoft.Extensions.Logging;

namespace HostManagement;

internal sealed class ConsoleLogger(LogLevel minimumLevel) : ILogger
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel)
        => logLevel >= minimumLevel;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var shortLogLevel = logLevel switch
        {
            LogLevel.Trace => "TRC",
            LogLevel.Debug => "DBG",
            LogLevel.Information => "INF",
            LogLevel.Warning => "WRN",
            LogLevel.Error => "ERR",
            LogLevel.Critical => "CRI",
            LogLevel.None => "NON",
            _ => "UNK"
        };

        var output = $"{shortLogLevel}: {formatter(state, exception)}";

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
            Console.ForegroundColor = logLevel switch
            {
                LogLevel.Warning => ConsoleColor.Yellow,
                LogLevel.Error => ConsoleColor.Red,
                LogLevel.Critical => ConsoleColor.DarkMagenta,
                _ => Console.ForegroundColor
            };

            Console.WriteLine(output);
            Console.ResetColor();
        }
    }
}
