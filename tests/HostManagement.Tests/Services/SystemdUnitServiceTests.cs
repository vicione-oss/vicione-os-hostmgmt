using System.Text.Json;
using AwesomeAssertions;
using HostManagement.Services;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Contracts.Service;
using HostManagement.Shared.Enums;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using SharedJsonContext = HostManagement.Shared.Contracts.SourceGenerationContext;

namespace HostManagement.Tests.Services;

public class SystemdUnitServiceTests
{
    private const string NotAllowedUnitName = "not-allowed.service";
    private const string UnknownSystemdUnit = "unknown";
    private const string ValidUnitName = "ssh.service";

    private static readonly List<string> s_configurableServices =
    [
        ValidUnitName,
        "remoteconnect.service",
        "enginehost@*.service",
        "enginehost-privileged@*.service"
    ];

    public static TheoryData<ServiceControlAction> ServiceControlActions =>
    [
        ServiceControlAction.Start,
        ServiceControlAction.Stop,
        ServiceControlAction.Restart
    ];

    public class GetSystemdUnitActiveState
    {
        private const string GetSystemdUnitActiveStateCanceled =
            $"Getting active state of systemd unit '{ValidUnitName}' was canceled.";

        [Fact]
        public async Task ShouldAddError_WhenExceptionOccurs()
        {
            // Arrange
            var expectedException = new InvalidOperationException("Test exception");
            var expectedMessage = $"Getting active state of systemd unit '{ValidUnitName}' failed.";

            var logger = Substitute.For<ILogger>();
            logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);

            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler
                .ExecuteGetSystemdUnitActiveStateScript(Arg.Any<ScriptOptions>())
                .ThrowsAsync(expectedException);

            var sut = new SystemdUnitService(logger, scriptHandler, s_configurableServices);

            // Act
            var result = await sut.GetSystemdUnitActiveState(ValidUnitName, CancellationToken.None);

            // Assert
            logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString()!.Contains(expectedMessage)),
                Arg.Is<Exception>(expectedException),
                Arg.Any<Func<object, Exception?, string>>());

            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().Contain(expectedMessage);
        }

        [Fact]
        public async Task ShouldAddError_WhenScriptReturnsError()
        {
            // Arrange
            var expectedMessage = $"Getting active state of systemd unit '{ValidUnitName}' failed.";

            var logger = Substitute.For<ILogger>();
            var scriptHandler = Substitute.For<IScriptHandler>();

            scriptHandler
                .ExecuteGetSystemdUnitActiveStateScript(Arg.Any<ScriptOptions>())
                .Returns((Result.TestErrorResult, "unknown"));

            var sut = new SystemdUnitService(logger, scriptHandler, s_configurableServices);

            // Act
            var result = await sut.GetSystemdUnitActiveState(ValidUnitName, CancellationToken.None);

            // Assert
            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().Contain(expectedMessage);
        }

        [Fact]
        public async Task ShouldLogAddReturnWarningResult_WhenCanceled()
        {
            // Arrange
            var logger = Substitute.For<ILogger>();
            logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);

            var scriptHandler = Substitute.For<IScriptHandler>();

            var sut = new SystemdUnitService(logger, scriptHandler, s_configurableServices);

            // Act
            using var cts = new CancellationTokenSource();
            await cts.CancelAsync();
            var result = await sut.GetSystemdUnitActiveState(ValidUnitName, cts.Token);

            // Assert
            logger.Received(1).Log(
                LogLevel.Warning,
                Arg.Any<EventId>(),
                Arg.Is<object>(state => state.ToString()!.Contains(GetSystemdUnitActiveStateCanceled)),
                null,
                Arg.Any<Func<object, Exception?, string>>());

            result.Status.Should().Be(OperationStatus.Warning);
            result.Message.Should().Contain(GetSystemdUnitActiveStateCanceled);
        }

        [Fact]
        public async Task ShouldReturnSuccess_WhenScriptReturnsSuccess()
        {
            // Arrange
            var logger = Substitute.For<ILogger>();
            var scriptHandler = Substitute.For<IScriptHandler>();

            scriptHandler
                .ExecuteGetSystemdUnitActiveStateScript(Arg.Any<ScriptOptions>())
                .Returns((Result.SuccessResult, "active"));

            var sut = new SystemdUnitService(logger, scriptHandler, s_configurableServices);

            // Act
            var result = await sut.GetSystemdUnitActiveState(ValidUnitName, CancellationToken.None);

            // Assert
            result.Status.Should().Be(OperationStatus.Success);
            result.Message.Should().BeEmpty();
        }
    }

    public class ControlSystemdUnit : SystemdUnitServiceTests
    {
        [Theory]
        [MemberData(nameof(ServiceControlActions))]
        public async Task ShouldAddError_WhenServiceControlRequestDeserializationThrows(ServiceControlAction action)
        {
            // Arrange
            var invalidMessageContent = "No valid ServiceControlRequest type";

            var expectedMessage = action switch
            {
                ServiceControlAction.Start => $"Starting systemd unit '{UnknownSystemdUnit}' failed.",
                ServiceControlAction.Stop => $"Stopping systemd unit '{UnknownSystemdUnit}' failed.",
                ServiceControlAction.Restart => $"Restarting systemd unit '{UnknownSystemdUnit}' failed.",
                _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
            };

            var logger = GetLoggerSubstitute();
            var scriptHandler = Substitute.For<IScriptHandler>();
            var sut = new SystemdUnitService(logger, scriptHandler, s_configurableServices);

            // Act
            var result = action switch
            {
                ServiceControlAction.Start => await sut.StartSystemdUnit(invalidMessageContent),
                ServiceControlAction.Stop => await sut.StopSystemdUnit(invalidMessageContent),
                ServiceControlAction.Restart => await sut.RestartSystemdUnit(invalidMessageContent),
                _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
            };

            // Assert
            logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString()!.Contains(expectedMessage)),
                Arg.Any<JsonException>(),
                Arg.Any<Func<object, Exception?, string>>());

            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().Contain(expectedMessage);

            _ = scriptHandler.DidNotReceive().ExecuteSetSystemdUnitStateScript(Arg.Any<ScriptOptions>());
        }

        [Fact]
        public async Task ShouldAddError_WhenSystemdUnitIsNotAllowed()
        {
            // Arrange
            var context = new ServiceControlRequestContext(NotAllowedUnitName);
            var expectedMessage = $"Systemd unit '{NotAllowedUnitName}' is not allowed.";

            var logger = GetLoggerSubstitute();
            var scriptHandler = Substitute.For<IScriptHandler>();

            var sut = new SystemdUnitService(logger, scriptHandler, s_configurableServices);

            // Act
            var result = await sut.StopSystemdUnit(context.SerializedRequest);

            // Assert
            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().Contain(expectedMessage);
        }

        [Theory]
        [MemberData(nameof(ServiceControlActions))]
        public async Task ShouldAddError_WhenSetSystemdUnitStateScriptThrows(ServiceControlAction action)
        {
            // Arrange
            var context = new ServiceControlRequestContext(ValidUnitName);

            var expectedException = new InvalidOperationException("Test exception");
            var expectedMessage = action switch
            {
                ServiceControlAction.Start => $"Starting systemd unit '{ValidUnitName}' failed.",
                ServiceControlAction.Stop => $"Stopping systemd unit '{ValidUnitName}' failed.",
                ServiceControlAction.Restart => $"Restarting systemd unit '{ValidUnitName}' failed.",
                _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
            };

            var logger = GetLoggerSubstitute();

            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler.ExecuteSetSystemdUnitStateScript(Arg.Any<ScriptOptions>()).ThrowsAsync(expectedException);

            var sut = new SystemdUnitService(logger, scriptHandler, s_configurableServices);

            // Act
            var result = action switch
            {
                ServiceControlAction.Start => await sut.StartSystemdUnit(context.SerializedRequest),
                ServiceControlAction.Stop => await sut.StopSystemdUnit(context.SerializedRequest),
                ServiceControlAction.Restart => await sut.RestartSystemdUnit(context.SerializedRequest),
                _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
            };

            // Assert
            logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString()!.Contains(expectedMessage)),
                Arg.Is<Exception>(expectedException),
                Arg.Any<Func<object, Exception?, string>>());

            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().Contain(expectedMessage);
        }

        [Theory]
        [MemberData(nameof(ServiceControlActions))]
        public async Task ShouldAddError_WhenScriptReturnsError(ServiceControlAction action)
        {
            // Arrange
            var context = new ServiceControlRequestContext(ValidUnitName);

            var expectedScriptMessage = Result.TestErrorMessage;
            var expectedMessage = action switch
            {
                ServiceControlAction.Start => $"Starting systemd unit '{ValidUnitName}' failed.",
                ServiceControlAction.Stop => $"Stopping systemd unit '{ValidUnitName}' failed.",
                ServiceControlAction.Restart => $"Restarting systemd unit '{ValidUnitName}' failed.",
                _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
            };

            var logger = GetLoggerSubstitute();

            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler.ExecuteSetSystemdUnitStateScript(Arg.Any<ScriptOptions>()).Returns(Result.TestErrorResult);

            var sut = new SystemdUnitService(logger, scriptHandler, s_configurableServices);

            // Act
            var result = action switch
            {
                ServiceControlAction.Start => await sut.StartSystemdUnit(context.SerializedRequest),
                ServiceControlAction.Stop => await sut.StopSystemdUnit(context.SerializedRequest),
                ServiceControlAction.Restart => await sut.RestartSystemdUnit(context.SerializedRequest),
                _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
            };

            // Assert
            logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(o =>
                    o.ToString()!.Contains(expectedMessage) &&
                    o.ToString()!.Contains(expectedScriptMessage)),
                null,
                Arg.Any<Func<object, Exception?, string>>());

            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().Contain(expectedMessage);
        }

        [Theory]
        [MemberData(nameof(ServiceControlActions))]
        public async Task ShouldReturnSuccess_WhenScriptCalledWithNoDelay(ServiceControlAction action)
        {
            // Arrange
            var context = new ServiceControlRequestContext(ValidUnitName);
            var logger = GetLoggerSubstitute();

            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler.ExecuteSetSystemdUnitStateScript(Arg.Any<ScriptOptions>()).Returns(Result.SuccessResult);

            var sut = new SystemdUnitService(logger, scriptHandler, s_configurableServices);

            // Act
            var result = action switch
            {
                ServiceControlAction.Start => await sut.StartSystemdUnit(context.SerializedRequest),
                ServiceControlAction.Stop => await sut.StopSystemdUnit(context.SerializedRequest),
                ServiceControlAction.Restart => await sut.RestartSystemdUnit(context.SerializedRequest),
                _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
            };

            // Assert
            result.Status.Should().Be(OperationStatus.Success);
            result.Message.Should().Be(
                $"Systemd unit '{ValidUnitName}' has been {action.ToString().ToLowerInvariant()}ed.");
        }

        [Theory]
        [MemberData(nameof(ServiceControlActions))]
        public async Task ShouldReturnSuccess_WhenScriptCalledWithDelay(ServiceControlAction action)
        {
            // Arrange
            var delay = 42;
            var context = new ServiceControlRequestContext(ValidUnitName, delay);
            var logger = GetLoggerSubstitute();

            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler.ExecuteSetSystemdUnitStateScript(Arg.Any<ScriptOptions>()).Returns(Result.SuccessResult);

            var sut = new SystemdUnitService(logger, scriptHandler, s_configurableServices);

            // Act
            var result = action switch
            {
                ServiceControlAction.Start => await sut.StartSystemdUnit(context.SerializedRequest),
                ServiceControlAction.Stop => await sut.StopSystemdUnit(context.SerializedRequest),
                ServiceControlAction.Restart => await sut.RestartSystemdUnit(context.SerializedRequest),
                _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
            };

            // Assert
            result.Status.Should().Be(OperationStatus.Success);
            result.Message.Should().Be(
                $"Systemd unit '{ValidUnitName}' will be {action.ToString().ToLowerInvariant()}ed in {delay} seconds.");
        }

        private sealed class ServiceControlRequestContext
        {
            internal ServiceControlRequest Request { get; }
            internal string SerializedRequest { get; }

            public ServiceControlRequestContext(string name, int delay = 0)
            {
                Request = new ServiceControlRequest() { Name = name, DelayInSeconds = delay };
                SerializedRequest = JsonSerializer.Serialize(Request, SharedJsonContext.Default.ServiceControlRequest);
            }
        }
    }

    private static ILogger GetLoggerSubstitute()
    {
        var logger = Substitute.For<ILogger>();
        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        return logger;
    }
}
