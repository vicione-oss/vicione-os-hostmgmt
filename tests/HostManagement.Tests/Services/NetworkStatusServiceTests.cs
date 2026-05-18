using AwesomeAssertions;
using HostManagement.Scripts.Enums;
using HostManagement.Services;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Contracts.NetworkStatus;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace HostManagement.Tests.Services;

public class NetworkStatusServiceTests
{
    public class GetNetworkStatusInformation
    {
        private const string InterfaceName = "lan1";
        private const string GetNetworkStatusInformationFailed = "Getting network status information failed.";
        private const string GetNetworkStatusInformationCanceled = "Getting network status information was canceled.";
        private const string ExpectedScriptSampleMessage = "A script error.";

        [Fact]
        public async Task ShouldLogAndReturnErrorResult_WhenExceptionOccurs()
        {
            // Arrange
            var logger = GetLoggerSubstitute();
            var expectedException = new InvalidOperationException("Unexpected Exception");

            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler
                .ExecuteIsNetworkInterfaceAvailableScript(Arg.Any<string>())
                .ThrowsAsync(expectedException);

            var sut = new NetworkStatusService(logger, scriptHandler, new Result());

            // Act
            var result = await sut.GetNetworkStatusInformation(InterfaceName, CancellationToken.None);

            // Assert
            logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(state => state.ToString()!.Contains(GetNetworkStatusInformationFailed)),
                Arg.Is<Exception>(ex => ReferenceEquals(ex, expectedException)),
                Arg.Any<Func<object, Exception?, string>>());

            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().Contain(GetNetworkStatusInformationFailed);
        }

        [Fact]
        public async Task ShouldLogAndReturnWarningResult_WhenCancelled()
        {
            // Arrange
            var logger = GetLoggerSubstitute();
            var scriptHandler = Substitute.For<IScriptHandler>();
            var sut = new NetworkStatusService(logger, scriptHandler, new Result());

            // Act
            using var cts = new CancellationTokenSource();
            await cts.CancelAsync();
            var result = await sut.GetNetworkStatusInformation(InterfaceName, cts.Token);

            // Assert
            logger.Received(1).Log(
                LogLevel.Warning,
                Arg.Any<EventId>(),
                Arg.Is<object>(state => state.ToString()!.Contains(GetNetworkStatusInformationCanceled)),
                null,
                Arg.Any<Func<object, Exception?, string>>());

            result.Status.Should().Be(OperationStatus.Warning);
            result.Message.Should().Contain(GetNetworkStatusInformationCanceled);
        }

        [Fact]
        public async Task ShouldLogAndReturnErrorResult_WhenIsNetworkInterfaceAvailableReturnsError()
        {
            // Arrange
            var logger = GetLoggerSubstitute();

            var result = new Result();
            result.AddScriptError(ScriptIdentifier.IsNetworkInterfaceAvailable, ExpectedScriptSampleMessage);

            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler
                .ExecuteIsNetworkInterfaceAvailableScript(Arg.Any<string>())
                .Returns(Task.FromResult(OperationStatus.Error));

            var sut = new NetworkStatusService(logger, scriptHandler, result);

            // Act
            var networkStatusResult = await sut.GetNetworkStatusInformation(InterfaceName, CancellationToken.None);

            // Assert
            logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(state =>
                    state.ToString()!.StartsWith(GetNetworkStatusInformationFailed) &&
                    state.ToString()!.Contains(ScriptIdentifier.IsNetworkInterfaceAvailable.ToString()) &&
                    state.ToString()!.Contains(ExpectedScriptSampleMessage)),
                null,
                Arg.Any<Func<object, Exception?, string>>());

            networkStatusResult.Status.Should().Be(OperationStatus.Error);
            networkStatusResult.Message.Should().Contain(GetNetworkStatusInformationFailed);
        }

        [Fact]
        public async Task ShouldLogAndReturnErrorResult_WhenGetNetworkStatusInformationReturnsError()
        {
            // Arrange
            var logger = GetLoggerSubstitute();

            var result = new Result();
            result.AddScriptError(ScriptIdentifier.NetworkStatusInformationGet, ExpectedScriptSampleMessage);

            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler
                .ExecuteIsNetworkInterfaceAvailableScript(Arg.Any<string>())
                .Returns(Task.FromResult(OperationStatus.Success));

            scriptHandler
                .ExecuteGetNetworkInterfacesScript()
                .Returns(Task.FromResult((OperationStatus.Success, "")));

            scriptHandler
                .ExecuteGetNetworkStatusInformationScript(Arg.Any<ScriptOptions>())
                .Returns(Task.FromResult((OperationStatus.Error, NetworkStatusInformation.Empty)));

            var sut = new NetworkStatusService(logger, scriptHandler, result);

            // Act
            var networkStatusResult = await sut.GetNetworkStatusInformation(InterfaceName, CancellationToken.None);

            // Assert
            logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(state =>
                    state.ToString()!.StartsWith(GetNetworkStatusInformationFailed) &&
                    state.ToString()!.Contains(ScriptIdentifier.NetworkStatusInformationGet.ToString()) &&
                    state.ToString()!.Contains(ExpectedScriptSampleMessage)),
                null,
                Arg.Any<Func<object, Exception?, string>>());

            networkStatusResult.Status.Should().Be(OperationStatus.Error);
            networkStatusResult.Message.Should().Contain(GetNetworkStatusInformationFailed);
        }

        [Fact]
        public async Task ShouldReturnSuccessResult_WhenGetNetworkStatusInformationWasSuccessful()
        {
            // Arrange
            var logger = GetLoggerSubstitute();
            var networkStatusInformation = new NetworkStatusInformation
            {
                IsDefaultRouteConfigured = true,
                IsDefaultGatewayAvailable = true,
                IsInternetAvailable = true,
                IsDNSFunctional = true,
                InterfaceState = "UP"
            };

            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler
                .ExecuteIsNetworkInterfaceAvailableScript(Arg.Any<string>())
                .Returns(Task.FromResult(OperationStatus.Success));

            scriptHandler
                .ExecuteGetNetworkInterfacesScript()
                .Returns(Task.FromResult((OperationStatus.Success, "")));

            scriptHandler
                .ExecuteGetNetworkStatusInformationScript(Arg.Any<ScriptOptions>())
                .Returns(Task.FromResult((OperationStatus.Success, networkStatusInformation)));

            var sut = new NetworkStatusService(logger, scriptHandler, new Result());

            // Act
            var networkStatusResult = await sut.GetNetworkStatusInformation(InterfaceName, CancellationToken.None);

            // Assert
            logger.ReceivedCalls().Should().BeEmpty();

            networkStatusResult.Status.Should().Be(OperationStatus.Success);
            networkStatusResult.Message.Should().BeEmpty();
            networkStatusResult.NetworkStatusInformation.Should().Be(networkStatusInformation);
        }
    }

    private static ILogger GetLoggerSubstitute()
    {
        var logger = Substitute.For<ILogger>();
        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        return logger;
    }
}
