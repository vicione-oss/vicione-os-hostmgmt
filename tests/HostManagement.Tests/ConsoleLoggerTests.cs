using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Xunit;

namespace HostManagement.Tests;

public class ConsoleLoggerTests
{
    private const string SampleLogMessage = "This is a test message";

    private readonly ILogger _loggerWithShortLogLevels = new ConsoleLogger(LogLevel.Trace);
    private readonly ILogger _loggerWithLongLogLevels = new ConsoleLogger(LogLevel.Trace, false);
    private readonly Exception _sampleLogException = new InvalidOperationException("This is a test exception");

    public static TheoryData<Exception?> SampleLogException =>
    [
        null,
        new InvalidOperationException("This is a test exception")
    ];

    [Collection("Non Parallel Tests")]
    [global::System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1848:Use the LoggerMessage delegates")]
    public class Log : ConsoleLoggerTests
    {
        [Fact]
        public void ShouldNotLog_WhenNoInfoGiven()
        {
            // Act
            var consoleOutput = CaptureConsoleOutput(() =>
                _loggerWithShortLogLevels.Log(LogLevel.None, null));

            // Assert
            consoleOutput.Should().Be($"NON: [null]{Environment.NewLine}");
        }

        [Fact]
        public void ShouldLog_WhenOnlyMessageIsGiven()
        {
            // Act
            var consoleOutput = CaptureConsoleOutput(
                () => _loggerWithShortLogLevels.Log(LogLevel.Information, SampleLogMessage));

            // Assert
            consoleOutput.Should().Be($"INF: {SampleLogMessage}{Environment.NewLine}");
        }

        [Fact]
        public void ShouldLog_WhenMessageAndExceptionIsGiven()
        {
            // Act
            var consoleOutput = CaptureConsoleOutput(
                () => _loggerWithShortLogLevels.LogInformation(_sampleLogException, SampleLogMessage));

            // Assert
            consoleOutput.Should().Be($"INF: {SampleLogMessage}{Environment.NewLine}{_sampleLogException}{Environment.NewLine}");
        }

        [Theory]
        [InlineData(LogLevel.Trace)]
        [InlineData(LogLevel.Debug)]
        [InlineData(LogLevel.Information)]
        [InlineData(LogLevel.Warning)]
        [InlineData(LogLevel.Error)]
        [InlineData(LogLevel.Critical)]
        [InlineData(LogLevel.None)]
        [InlineData((LogLevel)42)]
        public void ShouldAddCorrectShortLogLevel_WhenLogTypeIsGiven(LogLevel logLevel)
        {
            // Act
            var consoleOutput = CaptureConsoleOutput(
                () => _loggerWithShortLogLevels.Log(logLevel, SampleLogMessage));

            // Assert
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
            consoleOutput.Should().Be($"{shortLogLevel}: {SampleLogMessage}{Environment.NewLine}");
        }

        [Theory]
        [InlineData(LogLevel.Trace)]
        [InlineData(LogLevel.Debug)]
        [InlineData(LogLevel.Information)]
        [InlineData(LogLevel.Warning)]
        [InlineData(LogLevel.Error)]
        [InlineData(LogLevel.Critical)]
        [InlineData(LogLevel.None)]
        [InlineData((LogLevel)42)]
        public void ShouldAddCorrectLongLogLevel_WhenLogTypeIsGiven(LogLevel logLevel)
        {
            // Act
            var consoleOutput = CaptureConsoleOutput(
                () => _loggerWithLongLogLevels.Log(logLevel, SampleLogMessage));

            // Assert
            var longLogLevel = logLevel switch
            {
                LogLevel.Trace => "TRACE",
                LogLevel.Debug => "DEBUG",
                LogLevel.Information => "INFORMATION",
                LogLevel.Warning => "WARNING",
                LogLevel.Error => "ERROR",
                LogLevel.Critical => "CRITICAL",
                LogLevel.None => "NONE",
                _ => "UNKNOWN"
            };
            consoleOutput.Should().Be($"{longLogLevel}: {SampleLogMessage}{Environment.NewLine}");
        }

        [Fact]
        public void ShouldNotLog_WhenLogLevelIsLowerThanMinimum()
        {
            // Arrange
            var logger = new ConsoleLogger(LogLevel.Error);

            // Act
            var consoleOutput = CaptureConsoleOutput(
                () => logger.Log(LogLevel.Trace, SampleLogMessage));

            // Assert
            consoleOutput.Should().BeNullOrEmpty();
        }

        [Fact]
        public void ShouldLog_WhenLogLevelEqualsMinimum()
        {
            // Arrange
            var logger = new ConsoleLogger(LogLevel.Error);

            // Act
            var consoleOutput = CaptureConsoleOutput(
                () => logger.Log(LogLevel.Error, SampleLogMessage));

            // Assert
            consoleOutput.Should().Be($"ERR: {SampleLogMessage}{Environment.NewLine}");
        }

        [Fact]
        public void ShouldLog_WhenLogLevelIsHigherThanMinimum()
        {
            // Arrange
            var logger = new ConsoleLogger(LogLevel.Error);

            // Act
            var consoleOutput = CaptureConsoleOutput(
                () => logger.Log(LogLevel.Critical, SampleLogMessage));

            // Assert
            consoleOutput.Should().Be($"CRI: {SampleLogMessage}{Environment.NewLine}");
        }

        [Fact]
        public void ShouldIgnoreEventId_WhenGiven()
        {
            // Act
            var consoleOutput = CaptureConsoleOutput(
                () => _loggerWithShortLogLevels.Log(LogLevel.Information, 0, SampleLogMessage));

            // Assert
            consoleOutput.Should().Be($"INF: {SampleLogMessage}{Environment.NewLine}");
        }

        [Fact]
        public void ShouldUseGivenFormatter_WhenMessageGiven()
        {
            // Arrange
            static string MessageFormatter(string message, Exception? exception) => "CustomFormatter: " + message;

            // Act
            var consoleOutput = CaptureConsoleOutput(
                () => _loggerWithShortLogLevels.Log(
                    LogLevel.Information,
                    0,
                    SampleLogMessage,
                    _sampleLogException,
                    MessageFormatter));

            // Assert
            consoleOutput.Should().Be($"INF: CustomFormatter: {SampleLogMessage}{Environment.NewLine}{_sampleLogException}{Environment.NewLine}");
        }

        [Theory]
        [MemberData(nameof(SampleLogException))]
        public void ShouldUseGivenFormatter_WhenExceptionGiven(Exception? exception)
        {
            // Arrange
            static string MessageFormatter(string message, Exception? ex) => "CustomFormatter: " + ex;

            // Act
            var consoleOutput = CaptureConsoleOutput(
                () => _loggerWithShortLogLevels.Log(
                    LogLevel.Information,
                    0,
                    SampleLogMessage,
                    exception,
                    MessageFormatter));

            // Assert
            consoleOutput.Should().Be(exception is null
                ? $"INF: CustomFormatter: {Environment.NewLine}"
                : $"INF: CustomFormatter: {exception}{Environment.NewLine}");
        }

        [Theory]
        [MemberData(nameof(SampleLogException))]
        public void ShouldUseGivenFormatter_WhenMessageAndExceptionGiven(Exception? exception)
        {
            // Arrange
            static string MessageFormatter(string message, Exception? ex) => message + Environment.NewLine + ex;

            // Act
            var consoleOutput = CaptureConsoleOutput(
                () => _loggerWithShortLogLevels.Log(
                    LogLevel.Information,
                    0,
                    SampleLogMessage,
                    exception,
                    MessageFormatter));

            // Assert
            consoleOutput.Should().Be(exception is null
                ? $"INF: {SampleLogMessage}{Environment.NewLine}{Environment.NewLine}"
                : $"INF: {SampleLogMessage}{Environment.NewLine}{_sampleLogException}{Environment.NewLine}");
        }

        private static string CaptureConsoleOutput(Action action)
        {
            using StringWriter stringWriter = new();
            var originalConsoleOut = Console.Out;
            try
            {
                Console.SetOut(stringWriter);
                action();
            }
            finally
            {
                Console.SetOut(originalConsoleOut);
            }
            return stringWriter.ToString();
        }
    }
}
