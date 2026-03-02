using Microsoft.Extensions.Logging;
using AwesomeAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Enums;
using HostManagement.Services;

namespace HostManagement.Tests.Services;

public class SystemdUnitServiceTests
{
    private const string ValidUnitName = "ssh.service";
    private const string NotAllowedUnitName = "not-allowed.service";

    private static readonly List<string> s_configurableServices =
    [
        "ssh.service",
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

            var sut = new SystemdUnitService(logger, scriptHandler, new Result(), s_configurableServices);

            // Act
            var result = await sut.GetSystemdUnitActiveState(ValidUnitName);

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
                .Returns((OperationStatus.Error, "unknown"));

            var sut = new SystemdUnitService(logger, scriptHandler, new Result(), s_configurableServices);

            // Act
            var result = await sut.GetSystemdUnitActiveState(ValidUnitName);

            // Assert
            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().Contain(expectedMessage);
        }

        [Fact]
        public async Task ShouldReturnSuccess_WhenScriptReturnsSuccess()
        {
            // Arrange
            var logger = Substitute.For<ILogger>();
            var scriptHandler = Substitute.For<IScriptHandler>();

            scriptHandler
                .ExecuteSetSystemdUnitStateScript(Arg.Any<ScriptOptions>())
                .Returns(OperationStatus.Success);

            var sut = new SystemdUnitService(logger, scriptHandler, new Result(), s_configurableServices);

            // Act
            var result = await sut.GetSystemdUnitActiveState(ValidUnitName);

            // Assert
            result.Status.Should().Be(OperationStatus.Success);
            result.Message.Should().BeEmpty();
        }
    }

    public class ControlSystemdUnit : SystemdUnitServiceTests
    {
        [Theory]
        [MemberData(nameof(ServiceControlActions))]
        public async Task ShouldAddError_WhenExceptionOccurs(ServiceControlAction action)
        {
            // Arrange
            var expectedException = new InvalidOperationException("Test exception");
            var expectedMessage = action switch
            {
                ServiceControlAction.Start => $"Starting systemd unit '{ValidUnitName}' failed.",
                ServiceControlAction.Stop => $"Stopping systemd unit '{ValidUnitName}' failed.",
                ServiceControlAction.Restart => $"Restarting systemd unit '{ValidUnitName}' failed.",
                _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
            };

            var logger = Substitute.For<ILogger>();
            logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);

            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler
                .ExecuteSetSystemdUnitStateScript(Arg.Any<ScriptOptions>())
                .ThrowsAsync(expectedException);

            var sut = new SystemdUnitService(logger, scriptHandler, new Result(), s_configurableServices);

            // Act
            var result = action switch
            {
                ServiceControlAction.Start => await sut.StartSystemdUnit(ValidUnitName),
                ServiceControlAction.Stop => await sut.StopSystemdUnit(ValidUnitName),
                ServiceControlAction.Restart => await sut.RestartSystemdUnit(ValidUnitName),
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

        [Fact]
        public async Task ShouldAddError_WhenSystemdUnitIsNotAllowed()
        {
            // Arrange
            var expectedMessage = $"Systemd unit '{NotAllowedUnitName}' is not allowed.";

            var logger = Substitute.For<ILogger>();
            var scriptHandler = Substitute.For<IScriptHandler>();

            var sut = new SystemdUnitService(logger, scriptHandler, new Result(), s_configurableServices);

            // Act
            var result = await sut.StopSystemdUnit(NotAllowedUnitName);

            // Assert
            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().Contain(expectedMessage);
        }

        [Theory]
        [MemberData(nameof(ServiceControlActions))]
        public async Task ShouldAddError_WhenScriptReturnsError(ServiceControlAction action)
        {
            // Arrange
            var expectedMessage = action switch
            {
                ServiceControlAction.Start => $"Starting systemd unit '{ValidUnitName}' failed.",
                ServiceControlAction.Stop => $"Stopping systemd unit '{ValidUnitName}' failed.",
                ServiceControlAction.Restart => $"Restarting systemd unit '{ValidUnitName}' failed.",
                _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
            };

            var logger = Substitute.For<ILogger>();
            var scriptHandler = Substitute.For<IScriptHandler>();

            scriptHandler
                .ExecuteSetSystemdUnitStateScript(Arg.Any<ScriptOptions>())
                .Returns(OperationStatus.Error);

            var sut = new SystemdUnitService(logger, scriptHandler, new Result(), s_configurableServices);

            // Act
            var result = action switch
            {
                ServiceControlAction.Start => await sut.StartSystemdUnit(ValidUnitName),
                ServiceControlAction.Stop => await sut.StopSystemdUnit(ValidUnitName),
                ServiceControlAction.Restart => await sut.RestartSystemdUnit(ValidUnitName),
                _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
            };

            // Assert
            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().Contain(expectedMessage);
        }

        [Theory]
        [MemberData(nameof(ServiceControlActions))]
        public async Task ShouldReturnSuccess_WhenScriptReturnsSuccess(ServiceControlAction action)
        {
            // Arrange
            var logger = Substitute.For<ILogger>();
            var scriptHandler = Substitute.For<IScriptHandler>();

            scriptHandler
                .ExecuteSetSystemdUnitStateScript(Arg.Any<ScriptOptions>())
                .Returns(OperationStatus.Success);

            var sut = new SystemdUnitService(logger, scriptHandler, new Result(), s_configurableServices);

            // Act
            var result = action switch
            {
                ServiceControlAction.Start => await sut.StartSystemdUnit(ValidUnitName),
                ServiceControlAction.Stop => await sut.StopSystemdUnit(ValidUnitName),
                ServiceControlAction.Restart => await sut.RestartSystemdUnit(ValidUnitName),
                _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
            };

            // Assert
            result.Status.Should().Be(OperationStatus.Success);
            result.Message.Should().BeEmpty();
        }
    }
}
