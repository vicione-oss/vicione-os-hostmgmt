using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using HostManagement.Configuration;
using HostManagement.Services;
using HostManagement.Shared.Capabilities;
using HostManagement.Shared.Communication.Enums;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace HostManagement.Tests.Services;

public class SupportedCapabilitiesServiceTests
{
    private const string ValidCapabilitiesIni =
        """
        [Settings]
        [Topics]
        """;

    private const string GetCanceledMessage = "Getting supported capabilities was canceled.";
    private const string GetFailedMessage = "Getting supported capabilities failed.";

    public class Dispose
    {
        [Fact]
        public void ShouldNotThrow_WhenCalledMultipleTimes()
        {
            // Arrange
            using var sut = CreateService();

            // Act
            var action = () =>
            {
                sut.Dispose();
                sut.Dispose();
            };

            // Assert
            action.Should().NotThrow();
        }
    }

    public class GetSupportedCapabilities
    {
        [Fact]
        public async Task ShouldLogAndReturnError_WhenServiceDisposed()
        {
            // Arrange
            var logger = CreateLogger();
            var sut = CreateService(logger: logger);
            sut.Dispose();

            // Act
            var result = await sut.GetSupportedCapabilities(CancellationToken.None);

            // Assert
            logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(state => state.ToString()!.Contains(GetFailedMessage, StringComparison.Ordinal)),
                Arg.Is<Exception>(exception => exception is ObjectDisposedException),
                Arg.Any<Func<object, Exception?, string>>());

            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().Contain(GetFailedMessage);
        }

        [Fact]
        public async Task ShouldLogAndReturnError_WhenCapabilitiesNotInitialized()
        {
            // Arrange
            var logger = CreateLogger();
            using var sut = CreateService(logger: logger);

            var expectedMessage = "Supported capabilities were not loaded.";

            // Act
            var result = await sut.GetSupportedCapabilities(CancellationToken.None);

            // Assert
            logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(state =>
                    state.ToString()!.Contains(GetFailedMessage, StringComparison.Ordinal) &&
                    state.ToString()!.Contains(expectedMessage, StringComparison.Ordinal)),
                Arg.Is<Exception>(exception => exception is InvalidOperationException),
                Arg.Any<Func<object, Exception?, string>>());

            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().ContainAll(GetFailedMessage, expectedMessage);
        }

        [Fact]
        public async Task ShouldLogAndReturnWarning_WhenCanceled()
        {
            // Arrange
            var logger = CreateLogger();
            using var sut = CreateService(logger: logger);
            using var cancellationTokenSource = new CancellationTokenSource();
            await cancellationTokenSource.CancelAsync();

            // Act
            var result = await sut.GetSupportedCapabilities(cancellationTokenSource.Token);

            // Assert
            logger.Received(1).Log(
                LogLevel.Warning,
                Arg.Any<EventId>(),
                Arg.Is<object>(state => state.ToString()!.Contains(GetCanceledMessage, StringComparison.Ordinal)),
                null,
                Arg.Any<Func<object, Exception?, string>>());

            result.Status.Should().Be(OperationStatus.Warning);
            result.Message.Should().Be(GetCanceledMessage);
        }

        [Fact]
        public async Task ShouldReturnCachedCapabilities_WhenInitialized()
        {
            // Arrange
            using var sut = CreateService(CreateFileSystem());
            var expectedCapabilities = await sut.InitializeSupportedCapabilities(CancellationToken.None);

            // Act
            var result = await sut.GetSupportedCapabilities(CancellationToken.None);

            // Assert
            result.Status.Should().Be(OperationStatus.Success);
            result.Message.Should().BeEmpty();
            result.SupportedCapabilities.Should().BeSameAs(expectedCapabilities);
        }
    }

    public class InitializeSupportedCapabilities
    {
        [Fact]
        public async Task ShouldLoadCapabilities_WhenFileContainsValidIni()
        {
            // Arrange
            using var sut = CreateService(CreateFileSystem());

            // Act
            var result = await sut.InitializeSupportedCapabilities(CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Settings.Should().NotBeNull();
            result.Topics.Should().NotBeNull();
        }

        [Fact]
        public async Task ShouldReturnCachedCapabilities_WhenCalledMultipleTimes()
        {
            // Arrange
            var fileSystem = CreateFileSystem();
            using var sut = CreateService(fileSystem);

            var expectedCapabilities =
                await sut.InitializeSupportedCapabilities(CancellationToken.None);

            fileSystem.File.WriteAllText(AppPaths.SupportedCapabilitiesFilePath, "[Unknown]");

            // Act
            var result = await sut.InitializeSupportedCapabilities(CancellationToken.None);

            // Assert
            result.Should().BeSameAs(expectedCapabilities);
        }

        [Fact]
        public async Task ShouldNotThrow_WhenCapabilitiesFileNotExists()
        {
            // Arrange
            var logger = CreateLogger();
            using var sut = CreateService(new MockFileSystem(), logger);
            var expectedCapabilities = await sut.InitializeSupportedCapabilities(CancellationToken.None);
            var expectedLogMessage = $"Found no supported capabilities file '{AppPaths.SupportedCapabilitiesFilePath}'.";

            // Act
            var result = await sut.GetSupportedCapabilities(CancellationToken.None);

            // Assert
            result.Status.Should().Be(OperationStatus.Success);
            result.Message.Should().BeEmpty();
            result.SupportedCapabilities.Should().BeSameAs(expectedCapabilities);
            expectedCapabilities.Should().BeEquivalentTo(new SupportedCapabilities());

            logger.Received(1).Log(
                LogLevel.Warning,
                Arg.Any<EventId>(),
                Arg.Is<object>(state => state.ToString()!.Contains(expectedLogMessage, StringComparison.Ordinal)),
                null,
                Arg.Any<Func<object, Exception?, string>>());
        }

        [Fact]
        public async Task ShouldThrow_WhenIniEntryOutsideSection()
        {
            // Arrange
            var fileSystem = CreateFileSystem("GetServiceState=Enabled");
            using var sut = CreateService(fileSystem);

            // Act
            var action = () => sut.InitializeSupportedCapabilities(CancellationToken.None);

            // Assert
            await action.Should()
                .ThrowAsync<FormatException>()
                .WithMessage("INI entry at line 1 is not inside a section.");
        }

        [Fact]
        public async Task ShouldThrow_WhenIniSectionInvalid()
        {
            // Arrange
            var fileSystem = CreateFileSystem("[Topics");
            using var sut = CreateService(fileSystem);

            // Act
            var action = () => sut.InitializeSupportedCapabilities(CancellationToken.None);

            // Assert
            await action.Should()
                .ThrowAsync<FormatException>()
                .WithMessage("Invalid INI section at line 1.");
        }

        [Fact]
        public async Task ShouldThrow_WhenCanceled()
        {
            // Arrange
            using var sut = CreateService(CreateFileSystem());
            using var cancellationTokenSource = new CancellationTokenSource();
            await cancellationTokenSource.CancelAsync();

            // Act
            var action = () => sut.InitializeSupportedCapabilities(cancellationTokenSource.Token);

            // Assert
            await action.Should().ThrowAsync<OperationCanceledException>();
        }

        [Fact]
        public async Task ShouldThrow_WhenServiceDisposed()
        {
            // Arrange
            var sut = CreateService(CreateFileSystem());
            sut.Dispose();

            // Act
            var action = () => sut.InitializeSupportedCapabilities(CancellationToken.None);

            // Assert
            await action.Should().ThrowAsync<ObjectDisposedException>();
        }
    }

    private static SupportedCapabilitiesService CreateService(
        MockFileSystem? fileSystem = null,
        ILogger? logger = null)
            => new(fileSystem ?? new MockFileSystem(), logger ?? Substitute.For<ILogger>());

    private static MockFileSystem CreateFileSystem(string capabilitiesIni = ValidCapabilitiesIni)
        => new(
            new Dictionary<string, MockFileData>
            {
                [AppPaths.SupportedCapabilitiesFilePath] = new(capabilitiesIni)
            });

    private static ILogger CreateLogger()
    {
        var logger = Substitute.For<ILogger>();
        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        return logger;
    }
}
