using AwesomeAssertions;
using HostManagement.Services;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Contracts.System;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace HostManagement.Tests.Services;

public class SystemServiceTests
{
    public class InstallSignedDebianPackage
    {
        private const string InstallFailedMessage = "Installing signed Debian package failed";
        private const string PackageFilePath = "/path/to/package.deb";
        private const string PackageSignatureFilePath = "/path/to/package.deb.minisig";

        [Fact]
        public async Task ShouldLogErrorAndReturnErrorResult_WhenExceptionIsThrown()
        {
            // Arrange
            var logger = GetLoggerSubstitute();
            var scriptHandler = Substitute.For<IScriptHandler>();
            var signatureValidator = Substitute.For<ISignedDebianPackageSignatureValidator>();

            var signedPackage = new SignedDebianPackage
            {
                FilePath = PackageFilePath,
                SignatureFilePath = PackageSignatureFilePath
            };

            var publicKeys = new List<string> { "valid-public-key" };

            var expectedException = new FileNotFoundException("could not find signatureFile");
            signatureValidator
                .IsSignatureValid(Arg.Any<SignedDebianPackage>(), Arg.Any<IReadOnlyCollection<string>>())
                .Returns(_ => throw expectedException);

            var expectedMessage = $"{InstallFailedMessage} ('{signedPackage.FilePath}'). {expectedException.Message}";
            var sut = new SystemService(logger, scriptHandler, signatureValidator);

            // Act
            var result = await sut.InstallSignedDebianPackage(signedPackage, publicKeys, CancellationToken.None);

            // Assert
            logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString()!.StartsWith(InstallFailedMessage)),
                Arg.Is<Exception>(ex =>
                    ex is FileNotFoundException &&
                    ex.Message == expectedException.Message),
                Arg.Any<Func<object, Exception?, string>>());

            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().Contain(expectedMessage);
        }

        [Fact]
        public async Task ShouldLogWarningAndReturnWarningResult_WhenOperationCanceled()
        {
            // Arrange
            var logger = GetLoggerSubstitute();
            var scriptHandler = Substitute.For<IScriptHandler>();
            var signatureValidator = GetSignatureValidatorSubstitute(isSignatureValid: true);

            var signedPackage = new SignedDebianPackage
            {
                FilePath = PackageFilePath,
                SignatureFilePath = PackageSignatureFilePath
            };

            var expectedLogMessage = $"Installing signed Debian package was canceled ('{signedPackage.FilePath}').";
            var sut = new SystemService(logger, scriptHandler, signatureValidator);

            // Act
            using var cts = new CancellationTokenSource();
            await cts.CancelAsync();
            var result = await sut.InstallSignedDebianPackage(signedPackage, ["valid-public-key"], cts.Token);

            // Assert
            logger.Received(1).Log(
                LogLevel.Warning,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString()!.Contains(expectedLogMessage)),
                null,
                Arg.Any<Func<object, Exception?, string>>());

            result.Status.Should().Be(OperationStatus.Warning);
            result.Message.Should().Contain(expectedLogMessage);
        }

        [Fact]
        public async Task ShouldReturnError_WhenPackageSignatureInvalid()
        {
            // Arrange
            var logger = GetLoggerSubstitute();
            var scriptHandler = Substitute.For<IScriptHandler>();
            var signatureValidator = GetSignatureValidatorSubstitute(isSignatureValid: false);

            var signedPackage = new SignedDebianPackage
            {
                FilePath = PackageFilePath,
                SignatureFilePath = PackageSignatureFilePath
            };

            var expectedMessage = $"{InstallFailedMessage} ('{signedPackage.FilePath}'). Package signature is invalid.";
            var sut = new SystemService(logger, scriptHandler, signatureValidator);

            // Act
            var result = await sut.InstallSignedDebianPackage(signedPackage, ["invalid-public-key"], CancellationToken.None);

            // Assert
            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().Be(expectedMessage);
        }

        [Theory]
        [InlineData(OperationStatus.Warning)]
        [InlineData(OperationStatus.Error)]
        public async Task ShouldReturnError_WhenPackageInstallIsNotSuccess(OperationStatus operationStatus)
        {
            // Arrange
            var logger = GetLoggerSubstitute();
            var signatureValidator = GetSignatureValidatorSubstitute(isSignatureValid: true);

            var signedPackage = new SignedDebianPackage
            {
                FilePath = PackageFilePath,
                SignatureFilePath = PackageSignatureFilePath
            };

            var expectedMessage = $"{InstallFailedMessage} ('{signedPackage.FilePath}').";

            var expectedResult = operationStatus is OperationStatus.Warning
                ? Result.TestWarningResult
                : Result.TestErrorResult;

            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler
                .ExecuteInstallDebianPackageScript(Arg.Any<ScriptOptions>())
                .Returns(Task.FromResult(expectedResult));

            var sut = new SystemService(logger, scriptHandler, signatureValidator);

            // Act
            var systemControlResult = await sut.InstallSignedDebianPackage(
                signedPackage,
                ["valid-public-key"],
                CancellationToken.None);

            // Assert
            systemControlResult.Status.Should().Be(OperationStatus.Error);
            systemControlResult.Message.Should().StartWith(expectedMessage).And.Contain(expectedResult.FormattedMessages);
        }

        [Fact]
        public async Task ShouldReturnSuccess_WhenPackageWasInstalled()
        {
            // Arrange
            var logger = GetLoggerSubstitute();
            var signatureValidator = GetSignatureValidatorSubstitute(isSignatureValid: true);

            var signedPackage = new SignedDebianPackage
            {
                FilePath = PackageFilePath,
                SignatureFilePath = PackageSignatureFilePath
            };

            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler
                .ExecuteInstallDebianPackageScript(Arg.Any<ScriptOptions>())
                .Returns(Task.FromResult(Result.SuccessResult));

            var sut = new SystemService(logger, scriptHandler, signatureValidator);

            // Act
            var result = await sut.InstallSignedDebianPackage(signedPackage, ["valid-public-key"], CancellationToken.None);

            // Assert
            result.Status.Should().Be(OperationStatus.Success);
            result.Message.Should().BeEmpty();
        }
    }

    public class UpdateSystem
    {
        private const string SystemCouldNotBeUpdatedMessage = "System could not be updated.";
        private const string PathToUpdateFile = "path/to/file";

        [Fact]
        public async Task ShouldLogErrorAndReturnErrorResult_WhenExceptionIsThrown()
        {
            // Arrange
            var logger = GetLoggerSubstitute();
            var expectedException = new InvalidOperationException("invalid operation exception");

            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler.ExecuteUpdateSystemScript(Arg.Any<ScriptOptions>()).ThrowsAsync(expectedException);

            var sut = new SystemService(logger, scriptHandler, GetSignatureValidatorSubstitute());

            // Act
            var result = await sut.UpdateSystem(PathToUpdateFile);

            // Assert
            logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString()!.Contains(SystemCouldNotBeUpdatedMessage)),
                Arg.Is<Exception>(ex =>
                    ex is InvalidOperationException &&
                    ex.Message == expectedException.Message),
                Arg.Any<Func<object, Exception?, string>>());

            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().Contain(SystemCouldNotBeUpdatedMessage);
        }

        [Fact]
        public async Task ShouldLogErrorAndReturnErrorResult_WhenUpdateSystemReturnsError()
        {
            // Arrange
            var logger = GetLoggerSubstitute();

            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler
                .ExecuteUpdateSystemScript(Arg.Any<ScriptOptions>())
                .Returns(Task.FromResult(Result.TestErrorResult));

            var sut = new SystemService(logger, scriptHandler, GetSignatureValidatorSubstitute());

            // Act
            var systemControlResult = await sut.UpdateSystem(PathToUpdateFile);

            // Assert
            var expectedLogError = Result.TestErrorMessage;
            logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(o =>
                    o.ToString()!.StartsWith(SystemCouldNotBeUpdatedMessage) &&
                    o.ToString()!.Contains(expectedLogError)),
                null,
                Arg.Any<Func<object, Exception?, string>>());

            systemControlResult.Status.Should().Be(OperationStatus.Error);
            systemControlResult.Message.Should()
                .StartWith(SystemCouldNotBeUpdatedMessage).And
                .Contain(Result.TestErrorMessage);
        }

        [Fact]
        public async Task ShouldReturnSuccess_WhenUpdateSystemSucceeds()
        {
            // Arrange
            var logger = GetLoggerSubstitute();

            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler
                .ExecuteUpdateSystemScript(Arg.Any<ScriptOptions>())
                .Returns(Task.FromResult(Result.SuccessResult));
            scriptHandler
                .ExecuteShutdownSystemScript(Arg.Any<ScriptOptions>())
                .Returns(Task.FromResult(Result.SuccessResult));

            var sut = new SystemService(logger, scriptHandler, GetSignatureValidatorSubstitute());

            // Act
            var systemControlResult = await sut.UpdateSystem(PathToUpdateFile);

            // Assert
            systemControlResult.Status.Should().Be(OperationStatus.Success);
            systemControlResult.Message.Should().Contain("The system will restart now.");
        }
    }

    public class ResetSystem
    {
        private const string SystemCouldNotBeResetMessage = "System could not be reset.";

        [Fact]
        public async Task ShouldLogErrorAndReturnErrorResult_WhenExceptionIsThrown()
        {
            // Arrange
            var logger = GetLoggerSubstitute();
            var expectedException = new InvalidOperationException("invalid operation exception");

            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler.ExecuteResetSystemScript().ThrowsAsync(expectedException);

            var sut = new SystemService(logger, scriptHandler, GetSignatureValidatorSubstitute());

            // Act
            var result = await sut.ResetSystem();

            // Assert
            logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString()!.Contains(SystemCouldNotBeResetMessage)),
                Arg.Is<Exception>(ex =>
                    ex is InvalidOperationException &&
                    ex.Message == expectedException.Message),
                Arg.Any<Func<object, Exception?, string>>());

            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().Contain(SystemCouldNotBeResetMessage);
        }

        [Fact]
        public async Task ShouldLogErrorAndReturnErrorResult_WhenResetSystemReturnsError()
        {
            // Arrange
            var logger = GetLoggerSubstitute();
            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler.ExecuteResetSystemScript().Returns(Task.FromResult(Result.TestErrorResult));

            var sut = new SystemService(logger, scriptHandler, GetSignatureValidatorSubstitute());

            // Act
            var systemControlResult = await sut.ResetSystem();

            // Assert
            var expectedLogError = Result.TestErrorMessage;
            logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(o =>
                    o.ToString()!.StartsWith(SystemCouldNotBeResetMessage) &&
                    o.ToString()!.Contains(expectedLogError)),
                null,
                Arg.Any<Func<object, Exception?, string>>());

            systemControlResult.Status.Should().Be(OperationStatus.Error);
            systemControlResult.Message.Should()
                .StartWith(SystemCouldNotBeResetMessage).And
                .Contain(Result.TestErrorMessage);
        }

        [Fact]
        public async Task ShouldLogErrorAndReturnErrorResult_WhenClearProxySettingsReturnsError()
        {
            // Arrange
            var logger = GetLoggerSubstitute();

            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler
                .ExecuteResetSystemScript()
                .Returns(Task.FromResult(Result.SuccessResult));
            scriptHandler
                .ExecuteSetNetworkProxySettingsScript(Arg.Any<ScriptOptions>())
                .Returns(Task.FromResult(Result.TestErrorResult));

            var sut = new SystemService(logger, scriptHandler, GetSignatureValidatorSubstitute());

            // Act
            var systemControlResult = await sut.ResetSystem();

            // Assert
            var expectedLogError = Result.TestErrorMessage;
            logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(o =>
                    o.ToString()!.StartsWith(SystemCouldNotBeResetMessage) &&
                    o.ToString()!.Contains(expectedLogError)),
                null,
                Arg.Any<Func<object, Exception?, string>>());

            systemControlResult.Status.Should().Be(OperationStatus.Error);
            systemControlResult.Message.Should()
                .StartWith(SystemCouldNotBeResetMessage).And
                .Contain(Result.TestErrorMessage);
        }

        [Fact]
        public async Task ShouldReturnSuccess_WhenResetSystemSucceeds()
        {
            // Arrange
            var logger = GetLoggerSubstitute();
            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler
                .ExecuteResetSystemScript()
                .Returns(Task.FromResult(Result.SuccessResult));
            scriptHandler
                .ExecuteSetNetworkProxySettingsScript(Arg.Any<ScriptOptions>())
                .Returns(Task.FromResult(Result.SuccessResult));
            scriptHandler
                .ExecuteShutdownSystemScript(Arg.Any<ScriptOptions>())
                .Returns(Task.FromResult(Result.SuccessResult));

            var sut = new SystemService(logger, scriptHandler, GetSignatureValidatorSubstitute());

            // Act
            var systemControlResult = await sut.ResetSystem();

            // Assert
            systemControlResult.Status.Should().Be(OperationStatus.Success);
            systemControlResult.Message.Should().Contain("The system will restart now.");

            _ = scriptHandler.Received(1).ExecuteSetNetworkProxySettingsScript(Arg.Any<ScriptOptions>());
        }
    }

    public class RestartSystem
    {
        private const string RestartFlag = "--restart";
        private const string SystemCouldNotBeRestartedMessage = "System could not be restarted.";

        [Fact]
        public async Task ShouldLogErrorAndReturnErrorResult_WhenExceptionIsThrown()
        {
            // Arrange
            var logger = GetLoggerSubstitute();
            var expectedException = new InvalidOperationException("invalid operation exception");

            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler
                .ExecuteShutdownSystemScript(Arg.Is<ScriptOptions>(so => so.Options.Contains(RestartFlag)))
                .ThrowsAsync(expectedException);

            var sut = new SystemService(logger, scriptHandler, GetSignatureValidatorSubstitute());

            // Act
            var result = await sut.RestartSystem();

            // Assert
            logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString()!.Contains(SystemCouldNotBeRestartedMessage)),
                Arg.Is<Exception>(ex =>
                    ex is InvalidOperationException &&
                    ex.Message == expectedException.Message),
                Arg.Any<Func<object, Exception?, string>>());

            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().Contain(SystemCouldNotBeRestartedMessage);
        }

        [Fact]
        public async Task ShouldLogErrorAndReturnErrorResult_WhenRestartSystemReturnsError()
        {
            // Arrange
            var logger = GetLoggerSubstitute();
            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler
                .ExecuteShutdownSystemScript(Arg.Is<ScriptOptions>(so => so.Options.Contains(RestartFlag)))
                .Returns(Task.FromResult(Result.TestErrorResult));

            var sut = new SystemService(logger, scriptHandler, GetSignatureValidatorSubstitute());

            // Act
            var systemControlResult = await sut.RestartSystem();

            // Assert
            var expectedLogError = Result.TestErrorMessage;
            logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(o =>
                    o.ToString()!.StartsWith(SystemCouldNotBeRestartedMessage) &&
                    o.ToString()!.Contains(expectedLogError)),
                null,
                Arg.Any<Func<object, Exception?, string>>());

            systemControlResult.Status.Should().Be(OperationStatus.Error);
            systemControlResult.Message.Should()
                .StartWith(SystemCouldNotBeRestartedMessage).And
                .Contain(Result.TestErrorMessage);
        }

        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("-5")]
        [InlineData("0")]
        [InlineData("10")]
        public async Task ShouldReturnSuccess_WhenRestartSystemSucceeds(string delayInSecondsMessageContent)
        {
            // Arrange
            var delayFlag = "--delay";
            var logger = GetLoggerSubstitute();
            var expectedMessage = delayInSecondsMessageContent switch
            {
                "" or "  " or "-5" or "0" => "The system will restart now.",
                _ => $"The system will restart in {delayInSecondsMessageContent} seconds."
            };

            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler
                .ExecuteShutdownSystemScript(Arg.Is<ScriptOptions>(so =>
                    so.Options.Contains(RestartFlag) &&
                    so.Options.Contains(delayFlag)))
                .Returns(Task.FromResult(Result.SuccessResult));

            var sut = new SystemService(logger, scriptHandler, GetSignatureValidatorSubstitute());

            // Act
            var systemControlResult = await sut.RestartSystem(delayInSecondsMessageContent);

            // Assert
            systemControlResult.Status.Should().Be(OperationStatus.Success);
            systemControlResult.Message.Should().Be(expectedMessage);
        }
    }

    public class ShutdownSystem
    {
        private const string RestartFlag = "--restart";
        private const string SystemCouldNotBeShutDownMessage = "System could not be shut down.";

        [Fact]
        public async Task ShouldLogErrorAndReturnErrorResult_WhenExceptionIsThrown()
        {
            // Arrange
            var logger = GetLoggerSubstitute();
            var expectedException = new InvalidOperationException("invalid operation exception");

            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler
                .ExecuteShutdownSystemScript(Arg.Is<ScriptOptions>(so => !so.Options.Contains(RestartFlag)))
                .ThrowsAsync(expectedException);

            var sut = new SystemService(logger, scriptHandler, GetSignatureValidatorSubstitute());

            // Act
            var result = await sut.ShutdownSystem();

            // Assert
            logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString()!.Contains(SystemCouldNotBeShutDownMessage)),
                Arg.Is<Exception>(ex =>
                    ex is InvalidOperationException &&
                    ex.Message == expectedException.Message),
                Arg.Any<Func<object, Exception?, string>>());

            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().Contain(SystemCouldNotBeShutDownMessage);
        }

        [Fact]
        public async Task ShouldLogErrorAndReturnErrorResult_WhenShutdownSystemReturnsError()
        {
            // Arrange
            var logger = GetLoggerSubstitute();
            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler
                .ExecuteShutdownSystemScript(Arg.Is<ScriptOptions>(so => !so.Options.Contains(RestartFlag)))
                .Returns(Task.FromResult(Result.TestErrorResult));

            var sut = new SystemService(logger, scriptHandler, GetSignatureValidatorSubstitute());

            // Act
            var systemControlResult = await sut.ShutdownSystem();

            // Assert
            var expectedLogError = Result.TestErrorMessage;
            logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(o =>
                    o.ToString()!.StartsWith(SystemCouldNotBeShutDownMessage) &&
                    o.ToString()!.Contains(expectedLogError)),
                null,
                Arg.Any<Func<object, Exception?, string>>());

            systemControlResult.Status.Should().Be(OperationStatus.Error);
            systemControlResult.Message.Should()
                .StartWith(SystemCouldNotBeShutDownMessage).And
                .Contain(Result.TestErrorMessage);
        }

        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("-5")]
        [InlineData("0")]
        [InlineData("10")]
        public async Task ShouldReturnSuccess_WhenShutdownSystemSucceeds(string delayInSecondsMessageContent)
        {
            // Arrange
            var delayFlag = "--delay";
            var logger = GetLoggerSubstitute();
            var expectedMessage = delayInSecondsMessageContent switch
            {
                "" or "  " or "-5" or "0" => "The system will shutdown now.",
                _ => $"The system will shutdown in {delayInSecondsMessageContent} seconds."
            };

            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler
                .ExecuteShutdownSystemScript(Arg.Is<ScriptOptions>(so =>
                    !so.Options.Contains(RestartFlag) &&
                    so.Options.Contains(delayFlag)))
                .Returns(Task.FromResult(Result.SuccessResult));

            var sut = new SystemService(logger, scriptHandler, GetSignatureValidatorSubstitute());

            // Act
            var systemControlResult = await sut.ShutdownSystem(delayInSecondsMessageContent);

            // Assert
            systemControlResult.Status.Should().Be(OperationStatus.Success);
            systemControlResult.Message.Should().Be(expectedMessage);
        }
    }

    private static ILogger GetLoggerSubstitute()
    {
        var logger = Substitute.For<ILogger>();
        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        return logger;
    }

    private static ISignedDebianPackageSignatureValidator GetSignatureValidatorSubstitute(bool isSignatureValid = true)
    {
        var signatureValidator = Substitute.For<ISignedDebianPackageSignatureValidator>();
        signatureValidator
            .IsSignatureValid(Arg.Any<SignedDebianPackage>(), Arg.Any<IReadOnlyCollection<string>>())
            .Returns(isSignatureValid);

        return signatureValidator;
    }
}
