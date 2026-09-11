using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Xunit;

namespace HostManagement.Tests;

public class CommandLineLoggerTests
{
    private const string SampleLogMessage = "This is a test message";
    private readonly ILogger _logger = new CommandLineLogger(LogLevel.Trace);
    private readonly Exception _sampleLogException = new InvalidOperationException("This is a test exception");

    public static TheoryData<Exception?> SampleLogException
        => [
            (Exception ?)null,
            new InvalidOperationException("This is a test exception")
        ];

    [Collection("Non Parallel Tests")]
    [global::System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1848:Use the LoggerMessage delegates")]
    public sealed class Log : CommandLineLoggerTests
    {
        [Fact]
        public void ShouldNotLog_WhenNoInfoGiven()
        {
            // Act
            var consoleOutput = CaptureConsoleOutput(
                () => _logger.Log(LogLevel.None, null));

            // Assert
            consoleOutput.Should().BeNullOrEmpty();
        }

        [Fact]
        public void ShouldLog_WhenMessageIsNullAndExceptionIsGiven()
        {
            // Act
            var consoleOutput = CaptureConsoleOutput(
                () => _logger.Log(LogLevel.Information, _sampleLogException, null));

            // Assert
            consoleOutput.Should().Contain(_sampleLogException.ToString());
        }

        [Fact]
        public void ShouldLog_WhenOnlyMessageIsGiven()
        {
            // Act
            var consoleOutput = CaptureConsoleOutput(
                () => _logger.Log(LogLevel.Information, SampleLogMessage));

            // Assert
            consoleOutput.Should().Be(
                ToExpectedOutput($"INFORMATION: {SampleLogMessage}"));
        }

        [Fact]
        public void ShouldLog_WhenMessageAndExceptionIsGiven()
        {
            // Act
            var consoleOutput = CaptureConsoleOutput(
                () => _logger.LogInformation(_sampleLogException, SampleLogMessage));

            // Assert
            consoleOutput.Should().Be(
                ToExpectedOutput(
                    $"INFORMATION: {SampleLogMessage}{Environment.NewLine}" +
                    $"{_sampleLogException}"));
        }

        [Fact]
        public void ShouldNotLog_WhenLogLevelIsLowerThanMinimum()
        {
            // Arrange
            var logger = new CommandLineLogger(LogLevel.Error);

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
            var logger = new CommandLineLogger(LogLevel.Debug);

            // Act
            var consoleOutput = CaptureConsoleOutput(
                () => logger.Log(LogLevel.Debug, SampleLogMessage));

            // Assert
            consoleOutput.Should().Be(ToExpectedOutput($"DEBUG: {SampleLogMessage}"));
        }

        [Fact]
        public void ShouldLog_WhenLogLevelIsHigherThanMinimum()
        {
            // Arrange
            var logger = new CommandLineLogger(LogLevel.Information);

            // Act
            var consoleOutput = CaptureConsoleOutput(
                () => logger.Log(LogLevel.Warning, SampleLogMessage));

            // Assert
            consoleOutput.Should().Be(ToExpectedOutput($"WARNING: {SampleLogMessage}"));
        }

        [Fact]
        public void ShouldIgnoreEventId_WhenGiven()
        {
            // Act
            var consoleOutput = CaptureConsoleOutput(
                () => _logger.Log(LogLevel.Error, 123, SampleLogMessage));

            // Assert
            consoleOutput.Should().Be(ToExpectedOutput($"ERROR: {SampleLogMessage}"));
        }

        [Fact]
        public void ShouldUseGivenFormatter_WhenMessageGiven()
        {
            // Arrange
            static string MessageFormatter(string message, Exception? exception) => "CustomFormatter: " + message;

            // Act
            var consoleOutput = CaptureConsoleOutput(
                () => _logger.Log(LogLevel.Information, 0, SampleLogMessage, _sampleLogException, MessageFormatter));

            // Assert
            consoleOutput.Should().Be(
                ToExpectedOutput(
                    $"INFORMATION: CustomFormatter: {SampleLogMessage}{Environment.NewLine}" +
                    $"{_sampleLogException}"));
        }

        [Theory]
        [MemberData(nameof(SampleLogException))]
        public void ShouldUseGivenFormatter_WhenExceptionGiven(Exception? exception)
        {
            // Arrange
            static string MessageFormatter(string message, Exception? ex) => "CustomFormatter: " + ex;

            // Act
            var consoleOutput = CaptureConsoleOutput(
                () => _logger.Log(LogLevel.Information, 0, SampleLogMessage, exception, MessageFormatter));

            // Assert
            consoleOutput.Should().Be(ToExpectedOutput($"INFORMATION: CustomFormatter: {exception}"));
        }

        [Theory]
        [MemberData(nameof(SampleLogException))]
        public void ShouldUseGivenFormatter_WhenMessageAndExceptionGiven(Exception? exception)
        {
            // Arrange
            static string MessageFormatter(string message, Exception? ex) => message + Environment.NewLine + ex;

            // Act
            var consoleOutput = CaptureConsoleOutput(
                () => _logger.Log(LogLevel.Information, 0, SampleLogMessage, exception, MessageFormatter));

            // Assert
            consoleOutput.Should().Be(
                ToExpectedOutput(
                    exception is null
                        ? $"INFORMATION: {SampleLogMessage}{Environment.NewLine}"
                        : $"INFORMATION: {SampleLogMessage}{Environment.NewLine}{exception}"));
        }

        [Theory]
        [InlineData(LogLevel.Trace)]
        [InlineData(LogLevel.Critical)]
        [InlineData(LogLevel.None)]
        public void ShouldLog_WhenDefaultLogLevelGiven(LogLevel level)
        {
            // Act
            var consoleOutput = CaptureConsoleOutput(() => _logger.Log(level, SampleLogMessage));

            // Assert
            consoleOutput.Should().Be(ToExpectedOutput($"{SampleLogMessage}"));
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

        private static string ToExpectedOutput(string line)
            => line + Environment.NewLine + Environment.NewLine;
    }
}
