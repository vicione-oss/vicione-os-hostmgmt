using Microsoft.Extensions.Logging;
using FluentAssertions;
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

    public static TheoryData<string?> EmptyUnitNames =>
    [
        null,
        "",
        "  "
    ];

    public class ControlSystemdUnit : SystemdUnitServiceTests
    {
        [Theory]
        [MemberData(nameof(ServiceControlActions))]
        public async Task ShouldThrow_WhenExceptionOccurs(ServiceControlAction action)
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

            var loggerSubstitute = Substitute.For<ILogger>();
            loggerSubstitute
                .IsEnabled(Arg.Any<LogLevel>())
                .Returns(true);

            var scriptHandlerSubstitute = Substitute.For<IScriptHandler>();
            scriptHandlerSubstitute
                .ExecuteSetSystemdUnitStateScript(Arg.Any<ScriptOptions>())
                .ThrowsAsync(expectedException);

            var sut = new SystemdUnitService(
                loggerSubstitute,
                scriptHandlerSubstitute,
                new Result(),
                s_configurableServices);

            // Act
            var result = action switch
            {
                ServiceControlAction.Start => await sut.StartSystemdUnit(ValidUnitName),
                ServiceControlAction.Stop => await sut.StopSystemdUnit(ValidUnitName),
                ServiceControlAction.Restart => await sut.RestartSystemdUnit(ValidUnitName),
                _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
            };

            // Assert
            loggerSubstitute.Received(1).Log(
                Arg.Is(LogLevel.Error),
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString()!.Contains(expectedMessage)),
                Arg.Is<Exception>(expectedException),
                Arg.Any<Func<object, Exception?, string>>());

            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().Contain(expectedMessage);
        }

        [Theory]
        [MemberData(nameof(EmptyUnitNames))]
        public async Task ShouldAddError_WhenSystemdUnitNameEmpty(string? emptyUnitName)
        {
            // Arrange
            var loggerSubstitute = Substitute.For<ILogger>();
            var scriptHandlerSubstitute = Substitute.For<IScriptHandler>();

            var expectedMessage = $"Systemd unit '{emptyUnitName}' can not be empty.";

            var sut = new SystemdUnitService(
                loggerSubstitute,
                scriptHandlerSubstitute,
                new Result(),
                s_configurableServices);

            // Act
            var result = await sut.StartSystemdUnit(emptyUnitName!);

            // Assert
            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().Contain(expectedMessage);
        }

        [Fact]
        public async Task ShouldAddError_WhenSystemdUnitIsNotAllowed()
        {
            // Arrange
            var loggerSubstitute = Substitute.For<ILogger>();
            var scriptHandlerSubstitute = Substitute.For<IScriptHandler>();

            var expectedMessage = $"Systemd unit '{NotAllowedUnitName}' is not allowed.";

            var sut = new SystemdUnitService(
                loggerSubstitute,
                scriptHandlerSubstitute,
                new Result(),
                s_configurableServices);

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
            var loggerSubstitute = Substitute.For<ILogger>();
            var scriptHandlerSubstitute = Substitute.For<IScriptHandler>();

            scriptHandlerSubstitute
                .ExecuteSetSystemdUnitStateScript(Arg.Any<ScriptOptions>())
                .Returns(OperationStatus.Error);

            var sut = new SystemdUnitService(
                loggerSubstitute,
                scriptHandlerSubstitute,
                new Result(),
                s_configurableServices);

            var expectedMessage = action switch
            {
                ServiceControlAction.Start => $"Starting systemd unit '{ValidUnitName}' failed.",
                ServiceControlAction.Stop => $"Stopping systemd unit '{ValidUnitName}' failed.",
                ServiceControlAction.Restart => $"Restarting systemd unit '{ValidUnitName}' failed.",
                _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
            };

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
            var loggerSubstitute = Substitute.For<ILogger>();
            var scriptHandlerSubstitute = Substitute.For<IScriptHandler>();

            scriptHandlerSubstitute
                .ExecuteSetSystemdUnitStateScript(Arg.Any<ScriptOptions>())
                .Returns(OperationStatus.Success);

            var sut = new SystemdUnitService(
                loggerSubstitute,
                scriptHandlerSubstitute,
                new Result(),
                s_configurableServices);

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
