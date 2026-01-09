using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit;

namespace HostManagement.Tests;

public class ConsoleLoggerTests
{
    private readonly ILogger _logger = new ConsoleLogger(LogLevel.Trace);
    private readonly string _sampleLogMessage = "This is a test message";
    private readonly Exception _sampleLogException =
        new InvalidOperationException("This is a test exception");

    public static TheoryData<Exception?> SampleLogException => new()
    {
        null,
        new InvalidOperationException("This is a test exception")
    };

    public class Log : ConsoleLoggerTests
    {
        [Fact]
        public void ShouldNotLog_WhenNoInfoGiven()
        {
            // Arrange
            using var output = new StringWriter();
            Console.SetOut(output);

            // Act
            _logger.Log(LogLevel.None, null);

            // Assert
            var consoleOutput = output.ToString();
            consoleOutput.Should().Be($"NON: [null]{Environment.NewLine}");
        }

        [Fact]
        public void ShouldLog_WhenOnlyMessageIsGiven()
        {
            // Arrange
            using var output = new StringWriter();
            Console.SetOut(output);

            // Act
            _logger.Log(LogLevel.Information, _sampleLogMessage);

            // Assert
            var consoleOutput = output.ToString();
            consoleOutput.Should().Be($"INF: {_sampleLogMessage}{Environment.NewLine}");
        }

        [Fact]
        public void ShouldLog_WhenMessageAndExceptionIsGiven()
        {
            // Arrange
            using var output = new StringWriter();
            Console.SetOut(output);

            // Act
            _logger.LogInformation(_sampleLogException, _sampleLogMessage);

            // Assert
            var consoleOutput = output.ToString();
            consoleOutput.Should().Be(
                $"INF: {_sampleLogMessage}{Environment.NewLine}{_sampleLogException}{Environment.NewLine}");
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
        public void ShouldAddCorrectPrefix_WhenLogTypeIsGiven(LogLevel logLevel)
        {
            // Arrange
            using var output = new StringWriter();
            Console.SetOut(output);

            // Act
            _logger.Log(logLevel, _sampleLogMessage);

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

            var consoleOutput = output.ToString();
            consoleOutput.Should().Be(
                $"{shortLogLevel}: {_sampleLogMessage}{Environment.NewLine}");
        }

        [Fact]
        public void ShouldNotLog_WhenLogLevelIsLowerThanMinimum()
        {
            // Arrange
            using var output = new StringWriter();
            Console.SetOut(output);

            var logger = new ConsoleLogger(LogLevel.Error);

            // Act
            logger.Log(LogLevel.Trace, _sampleLogMessage);

            // Assert
            var consoleOutput = output.ToString();
            consoleOutput.Should().BeNullOrEmpty();
        }

        [Fact]
        public void ShouldLog_WhenLogLevelEqualsMinimum()
        {
            // Arrange
            using var output = new StringWriter();
            Console.SetOut(output);

            var logger = new ConsoleLogger(LogLevel.Error);

            // Act
            logger.Log(LogLevel.Error, _sampleLogMessage);

            // Assert
            var consoleOutput = output.ToString();
            consoleOutput.Should().Be($"ERR: {_sampleLogMessage}{Environment.NewLine}");
        }

        [Fact]
        public void ShouldLog_WhenLogLevelIsHigherThanMinimum()
        {
            // Arrange
            using var output = new StringWriter();
            Console.SetOut(output);

            var logger = new ConsoleLogger(LogLevel.Error);

            // Act
            logger.Log(LogLevel.Critical, _sampleLogMessage);

            // Assert
            var consoleOutput = output.ToString();
            consoleOutput.Should().Be($"CRI: {_sampleLogMessage}{Environment.NewLine}");
        }

        [Fact]
        public void ShouldIgnoreEventId_WhenGiven()
        {
            // Arrange
            using var output = new StringWriter();
            Console.SetOut(output);

            // Act
            _logger.Log(LogLevel.Information, 0, _sampleLogMessage);

            // Assert
            var consoleOutput = output.ToString();
            consoleOutput.Should().Be($"INF: {_sampleLogMessage}{Environment.NewLine}");
        }

        [Fact]
        public void ShouldUseGivenFormatter_WhenMessageGiven()
        {
            // Arrange
            using var output = new StringWriter();
            Console.SetOut(output);

            static string MessageFormatter(string message, Exception? exception) => "CustomFormatter: " + message;

            // Act
            _logger.Log(LogLevel.Information, 0, _sampleLogMessage, _sampleLogException, MessageFormatter);

            // Assert
            var consoleOutput = output.ToString();
            consoleOutput.Should().Be(
                $"INF: CustomFormatter: {_sampleLogMessage}{Environment.NewLine}{_sampleLogException}{Environment.NewLine}");
        }

        [Theory]
        [MemberData(nameof(SampleLogException))]
        public void ShouldUseGivenFormatter_WhenExceptionGiven(Exception? exception)
        {
            // Arrange
            using var output = new StringWriter();
            Console.SetOut(output);

            static string MessageFormatter(string message, Exception? ex)
            {
                _ = message;
                return "CustomFormatter: " + ex;
            }

            var expectedMessage = exception is null
                ? $"INF: CustomFormatter: {Environment.NewLine}"
                : $"INF: CustomFormatter: {exception}{Environment.NewLine}";

            // Act
            _logger.Log(LogLevel.Information, 0, _sampleLogMessage, exception, MessageFormatter);

            // Assert
            var consoleOutput = output.ToString();
            consoleOutput.Should().Be(expectedMessage);
        }

        [Theory]
        [MemberData(nameof(SampleLogException))]
        public void ShouldUseGivenFormatter_WhenMessageAndExceptionGiven(Exception? exception)
        {
            // Arrange
            using var output = new StringWriter();
            Console.SetOut(output);

            static string MessageFormatter(string message, Exception? ex) => message + Environment.NewLine + ex;

            var expectedMessage = exception is null
                ? $"INF: {_sampleLogMessage}{Environment.NewLine}{Environment.NewLine}"
                : $"INF: {_sampleLogMessage}{Environment.NewLine}{_sampleLogException}{Environment.NewLine}";

            // Act
            _logger.Log(LogLevel.Information, 0, _sampleLogMessage, exception, MessageFormatter);

            // Assert
            var consoleOutput = output.ToString();
            consoleOutput.Should().Be(expectedMessage);
        }
    }
}
