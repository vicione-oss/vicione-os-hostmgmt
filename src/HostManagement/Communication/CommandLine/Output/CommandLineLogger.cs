using HostManagement.Communication.CommandLine.Output;
using Microsoft.Extensions.Logging;

namespace HostManagement;

/// <summary>
/// Provides command line logging for the application using <see cref="SimpleOutput"/>.
/// </summary>
/// <param name="minimumLevel">The minimum log level to write.</param>
internal sealed class CommandLineLogger(LogLevel minimumLevel) : ILogger
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

        switch (logLevel)
        {
            case LogLevel.Debug: SimpleOutput.WriteLineDebug(output); break;
            case LogLevel.Information: SimpleOutput.WriteLineInformation(output); break;
            case LogLevel.Warning: SimpleOutput.WriteLineWarning(output); break;
            case LogLevel.Error: SimpleOutput.WriteLineError(output); break;

            // Not used log levels and default
            case LogLevel.Trace:
            case LogLevel.Critical:
            case LogLevel.None:
            default:
                SimpleOutput.WriteLine(output);
                break;
        }
    }
}
