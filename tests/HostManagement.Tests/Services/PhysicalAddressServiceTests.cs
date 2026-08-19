using AwesomeAssertions;
using HostManagement.Services;
using HostManagement.Shared.Communication.Enums;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace HostManagement.Tests.Services;

public sealed class PhysicalAddressServiceTests
{
    public sealed class GetOriginalPhysicalAddress
    {
        private const string InterfaceName = "lan1";
        private const string GetPhysicalAddressFailed = "Getting original physical address failed.";
        private const string GetPhysicalAddressCanceled = "Getting original physical address was canceled.";

        [Fact]
        public async Task ShouldLogAndReturnErrorResult_WhenExceptionOccurs()
        {
            // Arrange
            var logger = GetLoggerSubstitute();
            var expectedException = new InvalidOperationException("Unexpected Exception");

            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler
                .ExecuteGetOriginalPhysicalAddressScript(Arg.Any<string>())
                .ThrowsAsync(expectedException);

            var sut = new PhysicalAddressService(logger, scriptHandler);

            // Act
            var result = await sut.GetOriginalPhysicalAddress(InterfaceName, CancellationToken.None);

            // Assert
            logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(state => state.ToString()!.Contains(GetPhysicalAddressFailed)),
                null,
                Arg.Any<Func<object, Exception?, string>>());

            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().Contain(GetPhysicalAddressFailed);
        }

        [Fact]
        public async Task ShouldLogAndReturnWarningResult_WhenCancelled()
        {
            // Arrange
            var logger = GetLoggerSubstitute();
            var scriptHandler = Substitute.For<IScriptHandler>();
            var sut = new PhysicalAddressService(logger, scriptHandler);

            // Act
            using var cts = new CancellationTokenSource();
            await cts.CancelAsync();
            var result = await sut.GetOriginalPhysicalAddress(InterfaceName, cts.Token);

            // Assert
            logger.Received(1).Log(
                LogLevel.Warning,
                Arg.Any<EventId>(),
                Arg.Is<object>(state => state.ToString()!.Contains(GetPhysicalAddressCanceled)),
                null,
                Arg.Any<Func<object, Exception?, string>>());

            result.Status.Should().Be(OperationStatus.Warning);
            result.Message.Should().Contain(GetPhysicalAddressCanceled);
        }

        [Fact]
        public async Task ShouldLogAndReturnErrorResult_WhenGetOriginalPhysicalAddressReturnsError()
        {
            // Arrange
            var logger = GetLoggerSubstitute();
            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler
                .ExecuteGetOriginalPhysicalAddressScript(Arg.Any<string>())
                .Returns(Task.FromResult((Result.ErrorResult, string.Empty)));

            var sut = new PhysicalAddressService(logger, scriptHandler);

            // Act
            var result = await sut.GetOriginalPhysicalAddress(InterfaceName, CancellationToken.None);

            // Assert
            var expectedLogError = Result.ErrorMessage;
            logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(state =>
                    state.ToString()!.StartsWith(GetPhysicalAddressFailed) &&
                    state.ToString()!.Contains(expectedLogError)),
                null,
                Arg.Any<Func<object, Exception?, string>>());

            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().Contain(GetPhysicalAddressFailed);
        }

        [Fact]
        public async Task ShouldReturnSuccessResult_WhenGetOriginalPhysicalAddressReturnsSuccess()
        {
            // Arrange
            var logger = GetLoggerSubstitute();
            var physicalAddress = "01:23:45:67:89:AB";

            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler
                .ExecuteGetOriginalPhysicalAddressScript(Arg.Any<string>())
                .Returns(Task.FromResult((Result.SuccessResult, physicalAddress)));

            var sut = new PhysicalAddressService(logger, scriptHandler);

            // Act
            var result = await sut.GetOriginalPhysicalAddress(InterfaceName, CancellationToken.None);

            // Assert
            logger.ReceivedCalls().Should().BeEmpty();

            result.Status.Should().Be(OperationStatus.Success);
            result.Message.Should().BeEmpty();
            result.OriginalPhysicalAddress.Should().Be(physicalAddress);
        }
    }

    private static ILogger GetLoggerSubstitute()
    {
        var logger = Substitute.For<ILogger>();
        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        return logger;
    }
}
