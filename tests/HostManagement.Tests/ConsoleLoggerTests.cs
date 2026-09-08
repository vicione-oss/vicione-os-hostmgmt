using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Xunit;

namespace HostManagement.Tests;

public class ConsoleLoggerTests
{
    private const string SampleLogMessage = "This is a test message";

    private readonly ILogger _sut = new ConsoleLogger(LogLevel.Trace);
    private readonly Exception _sampleLogException = new InvalidOperationException("This is a test exception");

    public static TheoryData<Exception?> SampleLogException =>
    [
        (Exception?)null,
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
            var consoleOutput = CaptureConsoleOutput(
                () => _sut.Log(LogLevel.None, null));

            // Assert
            consoleOutput.Should().BeNullOrEmpty();
        }

        [Fact]
        public void ShouldLog_WhenMessageIsNullAndExceptionIsGiven()
        {
            // Act
            var consoleOutput = CaptureConsoleOutput(
                () => _sut.Log(LogLevel.Information, _sampleLogException, null));

            // Assert
            consoleOutput.Should().StartWith("<6>");
            consoleOutput.Should().Contain(_sampleLogException.ToString());
        }

        [Fact]
        public void ShouldLog_WhenOnlyMessageIsGiven()
        {
            // Act
            var consoleOutput = CaptureConsoleOutput(
                () => _sut.Log(LogLevel.Information, SampleLogMessage));

            // Assert
            consoleOutput.Should().Be($"<6>{SampleLogMessage}{Environment.NewLine}");
        }

        [Fact]
        public void ShouldLog_WhenMessageAndExceptionIsGiven()
        {
            // Act
            var consoleOutput = CaptureConsoleOutput(
                () => _sut.LogInformation(_sampleLogException, SampleLogMessage));

            // Assert
            consoleOutput.Should().Be(
                $"<6>{SampleLogMessage}{Environment.NewLine}" +
                $"<6>{_sampleLogException}{Environment.NewLine}");
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
        public void ShouldAddCorrectJournalPriority_WhenOutputIsRedirected(LogLevel logLevel)
        {
            // Arrange
            var expectedPriority = GetExpectedJournalPriority(logLevel);
            var message = $"First line{Environment.NewLine}Second line";

            var expectedOutput =
                $"<{expectedPriority}>First line{Environment.NewLine}" +
                $"<{expectedPriority}>Second line{Environment.NewLine}";

            // Act
            var consoleOutput = CaptureConsoleOutput(() => _sut.Log(logLevel, message));

            // Assert
            consoleOutput.Should().Be(expectedOutput);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldNotAddTextualLogLevel_WhenOutputIsRedirected(bool useShortLogLevels)
        {
            // Arrange
            var logger = new ConsoleLogger(LogLevel.Trace, useShortLogLevels);

            // Act
            var consoleOutput = CaptureConsoleOutput(
                () => logger.Log(LogLevel.Information, SampleLogMessage));

            // Assert
            consoleOutput.Should().Be($"<6>{SampleLogMessage}{Environment.NewLine}");
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
            consoleOutput.Should().Be($"<3>{SampleLogMessage}{Environment.NewLine}");
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
            consoleOutput.Should().Be($"<2>{SampleLogMessage}{Environment.NewLine}");
        }

        [Fact]
        public void ShouldIgnoreEventId_WhenGiven()
        {
            // Act
            var consoleOutput = CaptureConsoleOutput(
                () => _sut.Log(LogLevel.Information, 0, SampleLogMessage));

            // Assert
            consoleOutput.Should().Be($"<6>{SampleLogMessage}{Environment.NewLine}");
        }

        [Fact]
        public void ShouldUseGivenFormatter_WhenMessageGiven()
        {
            // Arrange
            static string MessageFormatter(string message, Exception? exception) => "CustomFormatter: " + message;

            // Act
            var consoleOutput = CaptureConsoleOutput(
                () => _sut.Log(
                    LogLevel.Information,
                    0,
                    SampleLogMessage,
                    _sampleLogException,
                    MessageFormatter));

            // Assert
            consoleOutput.Should().Be(
                $"<6>CustomFormatter: {SampleLogMessage}{Environment.NewLine}" +
                $"<6>{_sampleLogException}{Environment.NewLine}");
        }

        [Theory]
        [MemberData(nameof(SampleLogException))]
        public void ShouldUseGivenFormatter_WhenExceptionGiven(Exception? exception)
        {
            // Arrange
            static string MessageFormatter(string message, Exception? ex) => "CustomFormatter: " + ex;

            // Act
            var consoleOutput = CaptureConsoleOutput(
                () => _sut.Log(
                    LogLevel.Information,
                    0,
                    SampleLogMessage,
                    exception,
                    MessageFormatter));

            // Assert
            consoleOutput.Should().Be(
                exception is null
                ? $"<6>CustomFormatter: {Environment.NewLine}"
                : $"<6>CustomFormatter: {exception}{Environment.NewLine}");
        }

        [Theory]
        [MemberData(nameof(SampleLogException))]
        public void ShouldUseGivenFormatter_WhenMessageAndExceptionGiven(Exception? exception)
        {
            // Arrange
            static string MessageFormatter(string message, Exception? ex) => message + Environment.NewLine + ex;

            // Act
            var consoleOutput = CaptureConsoleOutput(
                () => _sut.Log(
                    LogLevel.Information,
                    0,
                    SampleLogMessage,
                    exception,
                    MessageFormatter));

            // Assert
            consoleOutput.Should().Be(
                exception is null
                ? $"<6>{SampleLogMessage}{Environment.NewLine}" +
                  $"<6>{Environment.NewLine}"
                : $"<6>{SampleLogMessage}{Environment.NewLine}" +
                  $"<6>{_sampleLogException}{Environment.NewLine}");
        }

        private static int GetExpectedJournalPriority(LogLevel logLevel) => logLevel switch
        {
            LogLevel.Trace => 7,       // debug
            LogLevel.Debug => 7,       // debug
            LogLevel.Information => 6, // info
            LogLevel.Warning => 4,     // warning
            LogLevel.Error => 3,       // err
            LogLevel.Critical => 2,    // crit
            _ => 6                     // info
        };

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
