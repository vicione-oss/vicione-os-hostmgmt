using AwesomeAssertions;
using HostManagement.Services;
using HostManagement.Shared.Communication.Enums;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace HostManagement.Tests.Services;

public sealed class NTPServiceTests
{
    public sealed class GetNTPFallbackInformation
    {
        private const string GetNTPFallbackInformationFailed = "Getting fallback NTP servers failed.";
        private const string GetNTPFallbackInformationCanceled = "Getting fallback NTP servers was canceled.";

        [Fact]
        public async Task ShouldLogAndReturnErrorResult_WhenExceptionOccurs()
        {
            // Arrange
            var logger = GetLoggerSubstitute();
            var expectedException = new InvalidOperationException("Unexpected Exception");

            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler.ExecuteGetNetworkNTPFallbackInformationScript().ThrowsAsync(expectedException);

            var sut = new NTPService(logger, scriptHandler);

            // Act
            var result = await sut.GetNTPFallbackInformation(CancellationToken.None);

            // Assert
            logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(state => state.ToString()!.Contains(GetNTPFallbackInformationFailed)),
                Arg.Is<Exception>(ex => ReferenceEquals(ex, expectedException)),
                Arg.Any<Func<object, Exception?, string>>());

            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().Contain(GetNTPFallbackInformationFailed);
        }

        [Fact]
        public async Task ShouldLogAndReturnWarningResult_WhenCancelled()
        {
            // Arrange
            var logger = GetLoggerSubstitute();
            var scriptHandler = Substitute.For<IScriptHandler>();
            var sut = new NTPService(logger, scriptHandler);

            // Act
            using var cts = new CancellationTokenSource();
            await cts.CancelAsync();
            var result = await sut.GetNTPFallbackInformation(cts.Token);

            // Assert
            logger.Received(1).Log(
                LogLevel.Warning,
                Arg.Any<EventId>(),
                Arg.Is<object>(state => state.ToString()!.Contains(GetNTPFallbackInformationCanceled)),
                null,
                Arg.Any<Func<object, Exception?, string>>());

            result.Status.Should().Be(OperationStatus.Warning);
            result.Message.Should().Contain(GetNTPFallbackInformationCanceled);
        }

        [Fact]
        public async Task ShouldLogAndReturnErrorResult_WhenGetNetworkNTPFallbackInformationReturnsError()
        {
            // Arrange
            var logger = GetLoggerSubstitute();

            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler
                .ExecuteGetNetworkNTPFallbackInformationScript()
                .Returns(Task.FromResult<(Result, IEnumerable<string>)>((Result.ErrorResult, [])));

            var sut = new NTPService(logger, scriptHandler);

            // Act
            var result = await sut.GetNTPFallbackInformation(CancellationToken.None);

            // Assert
            var expectedLogError = Result.ErrorMessage;
            logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(state =>
                    state.ToString()!.StartsWith(GetNTPFallbackInformationFailed) &&
                    state.ToString()!.Contains(expectedLogError)),
                null,
                Arg.Any<Func<object, Exception?, string>>());

            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().Contain(GetNTPFallbackInformationFailed);
            result.FallbackNTPServers.Should().BeEmpty();

        }

        [Fact]
        public async Task ShouldReturnSuccessResult_WhenGetNetworkNTPFallbackInformationReturnsSuccess()
        {
            // Arrange
            var logger = GetLoggerSubstitute();
            IEnumerable<string> fallbackNTPServers =
            [
                "0.debian.pool.ntp.org",
                "1.debian.pool.ntp.org",
                "2.debian.pool.ntp.org",
                "3.debian.pool.ntp.org"
            ];

            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler
                .ExecuteGetNetworkNTPFallbackInformationScript()
                .Returns(Task.FromResult((Result.SuccessResult, fallbackNTPServers)));

            var sut = new NTPService(logger, scriptHandler);

            // Act
            var result = await sut.GetNTPFallbackInformation(CancellationToken.None);

            // Assert
            logger.ReceivedCalls().Should().BeEmpty();

            result.Status.Should().Be(OperationStatus.Success);
            result.Message.Should().BeEmpty();
            result.FallbackNTPServers.Should().Contain(fallbackNTPServers);
        }
    }

    private static ILogger GetLoggerSubstitute()
    {
        var logger = Substitute.For<ILogger>();
        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        return logger;
    }
}
