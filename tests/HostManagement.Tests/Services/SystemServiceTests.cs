using System.Text;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using HostManagement.Scripts;
using HostManagement.Scripts.Enums;
using HostManagement.Services;
using HostManagement.Shared.Contracts.System;
using HostManagement.Shared.Communication.Enums;

namespace HostManagement.Tests.Services;

public class SystemServiceTests
{
    private const string InstallFailedMessage = "Installing signed Debian package failed";

    [Collection("Non Parallel Tests")]
    public class InstallSignedDebianPackage
    {
        private static readonly string s_unitTestDir = Path.Combine(Path.GetTempPath(), "_hm-unit-test");

        [Fact]
        public async Task ShouldLogErrorAndReturnErrorResult_WhenExceptionIsThrown()
        {
            // Arrange
            var logger = GetLoggerSubstitute();
            var scriptHandler = Substitute.For<IScriptHandler>();

            var publicKeys = new List<string> { "valid-public-key" };
            var signedPackage = new SignedDebianPackage
            {
                FilePath = "/path/to/package.deb",
                SignatureFilePath = "/path/to/package.deb.minisig"
            };

            // Minisign will not find the signature file so it will throw an exception
            var expectedException = new FileNotFoundException("could not find signatureFile");
            var expectedMessage = $"{InstallFailedMessage} ('{signedPackage.FilePath}'). {expectedException.Message}";

            var sut = new SystemService(logger, scriptHandler, new Result());

            // Act
            var result = await sut.InstallSignedDebianPackage(signedPackage, publicKeys, CancellationToken.None);

            // Assert
            logger.Received(1).Log(
                Arg.Is(LogLevel.Error),
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
            try
            {
                // Arrange
                var (package, publicKeyBase64) = await CreateSignedPackageTestFilesAsync();
                var logger = GetLoggerSubstitute();
                var scriptHandler = Substitute.For<IScriptHandler>();
                var expectedLogMessage = $"Installing signed Debian package was canceled ('{package.FilePath}').";

                var sut = new SystemService(logger, scriptHandler, new Result());

                // Act
                using var cts = new CancellationTokenSource();
                await cts.CancelAsync();
                var result = await sut.InstallSignedDebianPackage(package, [publicKeyBase64], cts.Token);

                // Assert
                logger.Received(1).Log(
                    Arg.Is(LogLevel.Warning),
                    Arg.Any<EventId>(),
                    Arg.Is<object>(o => o.ToString()!.Contains(expectedLogMessage)),
                    null,
                    Arg.Any<Func<object, Exception?, string>>());

                result.Status.Should().Be(OperationStatus.Warning);
                result.Message.Should().Contain(expectedLogMessage);
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(s_unitTestDir))
                    Directory.Delete(s_unitTestDir, recursive: true);
            }
        }

        [Fact]
        public async Task ShouldReturnError_WhenPackageSignatureInvalid()
        {
            try
            {
                // Arrange
                var logger = GetLoggerSubstitute();
                var scriptHandler = Substitute.For<IScriptHandler>();

                var packagePath = Path.Combine(s_unitTestDir, "package.deb");
                var signaturePath = Path.Combine(s_unitTestDir, "package.deb.minisig");
                var expectedMessage = $"{InstallFailedMessage} ('{packagePath}'). Package signature is invalid.";

                var invalidPublicKeyBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes("invalid-public-key"));
                var invalidSignature =
                    """
                    untrusted comment: signature from minisign secret key
                    RUSdbzO140cELjvXFidzF+eQAAZBjdqVGnUCcfR0q/uK4Cq2ApHdQuX4W8n0VA/ep4+vRnwWD9wuH3rlSqvmHjqq9saALhfHTAs=
                    trusted comment: timestamp:1763291678    file:package.deb    hashed
                    EmgYQcdc0B/35ZL202oDGlTmczYv40NlWOatNxjCu4FboErErG1L8Fh6OMsxjw5NPb5+LYf9eH3Nng4Lg8+ECw==
                    """;

                // Create dummy files
                Directory.CreateDirectory(s_unitTestDir);
                await File.WriteAllTextAsync(packagePath, "dummy package");
                await File.WriteAllTextAsync(signaturePath, invalidSignature);

                var sut = new SystemService(logger, scriptHandler, new Result());

                // Act
                var result = await sut.InstallSignedDebianPackage(
                    new() { FilePath = packagePath, SignatureFilePath = signaturePath },
                    [invalidPublicKeyBase64],
                    CancellationToken.None);

                // Assert
                result.Status.Should().Be(OperationStatus.Error);
                result.Message.Should().Be(expectedMessage);
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(s_unitTestDir))
                    Directory.Delete(s_unitTestDir, recursive: true);
            }
        }

        [Theory]
        [InlineData(OperationStatus.Warning, "A script warning.")]
        [InlineData(OperationStatus.Error, "A script error.")]
        public async Task ShouldReturnError_WhenPackageInstallIsNotSuccess(
            OperationStatus operationStatus,
            string expectedScriptMessage)
        {
            try
            {
                // Arrange
                var (package, publicKeyBase64) = await CreateSignedPackageTestFilesAsync();
                var logger = GetLoggerSubstitute();
                var expectedMessage = $"{InstallFailedMessage} ('{package.FilePath}').";

                var result = new Result();
                var scriptOptions = new ScriptOptions();
                scriptOptions.Add(ScriptOptionNames.InstallDebianPackage.DebianPackage, package.FilePath);

                var scriptHandler = Substitute.For<IScriptHandler>();
                scriptHandler
                    .ExecuteInstallDebianPackageScript(Arg.Any<ScriptOptions>())
                    .Returns(Task.FromResult(operationStatus));

                if (operationStatus is OperationStatus.Warning)
                    result.AddScriptWarning(ScriptIdentifier.InstallDebianPackage, expectedScriptMessage);
                else
                    result.AddScriptError(ScriptIdentifier.InstallDebianPackage, expectedScriptMessage);

                var sut = new SystemService(logger, scriptHandler, result);

                // Act
                var systemControlResult = await sut.InstallSignedDebianPackage(
                    package,
                    [publicKeyBase64],
                    CancellationToken.None);

                // Assert
                systemControlResult.Status.Should().Be(OperationStatus.Error);
                systemControlResult.Message.Should().StartWith(expectedMessage).And.Contain(expectedScriptMessage);
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(s_unitTestDir))
                    Directory.Delete(s_unitTestDir, recursive: true);
            }
        }

        [Fact]
        public async Task ShouldReturnSuccess_WhenPackageWasInstalled()
        {
            try
            {
                // Arrange
                var (package, publicKeyBase64) = await CreateSignedPackageTestFilesAsync();
                var logger = GetLoggerSubstitute();

                var scriptOptions = new ScriptOptions();
                scriptOptions.Add(ScriptOptionNames.InstallDebianPackage.DebianPackage, package.FilePath);

                var scriptHandler = Substitute.For<IScriptHandler>();
                scriptHandler
                    .ExecuteInstallDebianPackageScript(scriptOptions)
                    .Returns(Task.FromResult(OperationStatus.Success));

                var sut = new SystemService(logger, scriptHandler, new Result());

                // Act
                var result = await sut.InstallSignedDebianPackage(package, [publicKeyBase64], CancellationToken.None);

                // Assert
                result.Status.Should().Be(OperationStatus.Success);
                result.Message.Should().BeEmpty();
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(s_unitTestDir))
                    Directory.Delete(s_unitTestDir, recursive: true);
            }
        }

        private static async Task<(SignedDebianPackage package, string publicKeyBase64)> CreateSignedPackageTestFilesAsync()
        {
            var packagePath = Path.Combine(s_unitTestDir, "package.deb");
            var publicKeyPath = Path.Combine(s_unitTestDir, "minisign.pub");

            // Create dummy package
            Directory.CreateDirectory(s_unitTestDir);
            await File.WriteAllTextAsync(packagePath, "dummy package");

            // Generate a key pair for testing...
            var minisignKeyPair = Minisign.Core.GenerateKeyPair(
                password: "hm-unit-test",
                writeOutputFiles: true,
                outputFolder: s_unitTestDir);

            // ...and sign the package; Sign creates the file and returns the signature path
            var signaturePath = Minisign.Core.Sign(packagePath, minisignKeyPair.MinisignPrivateKey);

            // Read the public key and extract the  base64 string
            var publicKeyLines = await File.ReadAllLinesAsync(publicKeyPath);
            var publicKeyBase64 = publicKeyLines.FirstOrDefault(line => !line.StartsWith("untrusted comment:"));

            publicKeyBase64.Should().NotBeNullOrWhiteSpace();

            // Return SignedDebianPackage and publicKeyBase64
            var signedPackage = new SignedDebianPackage()
            {
                FilePath = packagePath,
                SignatureFilePath = signaturePath
            };

            return (signedPackage, publicKeyBase64);
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

            var scriptOptions = new ScriptOptions();
            scriptOptions.Add(ScriptOptionNames.UpdateSystem.FilePath, PathToUpdateFile);

            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler.ExecuteUpdateSystemScript(Arg.Any<ScriptOptions>()).ThrowsAsync(expectedException);

            var sut = new SystemService(logger, scriptHandler, new Result());

            // Act
            var result = await sut.UpdateSystem(PathToUpdateFile);

            // Assert
            logger.Received(1).Log(
                Arg.Is(LogLevel.Error),
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
            var expectedScriptMessage = "A script error.";

            var result = new Result();
            result.AddScriptError(ScriptIdentifier.UpdateSystem, expectedScriptMessage);

            var scriptOptions = new ScriptOptions();
            scriptOptions.Add(ScriptOptionNames.UpdateSystem.FilePath, PathToUpdateFile);

            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler
                .ExecuteUpdateSystemScript(Arg.Any<ScriptOptions>())
                .Returns(Task.FromResult(OperationStatus.Error));

            var sut = new SystemService(logger, scriptHandler, result);

            // Act
            var systemControlResult = await sut.UpdateSystem(PathToUpdateFile);

            // Assert
            logger.Received(1).Log(
                Arg.Is(LogLevel.Error),
                Arg.Any<EventId>(),
                Arg.Is<object>(o =>
                    o.ToString()!.StartsWith(SystemCouldNotBeUpdatedMessage) &&
                    o.ToString()!.Contains(expectedScriptMessage)),
                null,
                Arg.Any<Func<object, Exception?, string>>());

            systemControlResult.Status.Should().Be(OperationStatus.Error);
            systemControlResult.Message.Should()
                .StartWith(SystemCouldNotBeUpdatedMessage).And
                .Contain(expectedScriptMessage);
        }

        [Fact]
        public async Task ShouldReturnSuccess_WhenUpdateSystemSucceeds()
        {
            // Arrange
            var logger = GetLoggerSubstitute();

            var scriptOptions = new ScriptOptions();
            scriptOptions.Add(ScriptOptionNames.UpdateSystem.FilePath, PathToUpdateFile);

            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler.ExecuteUpdateSystemScript(scriptOptions).Returns(Task.FromResult(OperationStatus.Success));

            var sut = new SystemService(logger, scriptHandler, new Result());

            // Act
            var systemControlResult = await sut.UpdateSystem(PathToUpdateFile);

            // Assert
            systemControlResult.Status.Should().Be(OperationStatus.Success);
            systemControlResult.Message.Should().BeEmpty();
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

            var sut = new SystemService(logger, scriptHandler, new Result());

            // Act
            var result = await sut.ResetSystem();

            // Assert
            logger.Received(1).Log(
                Arg.Is(LogLevel.Error),
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
            var expectedScriptMessage = "A script error.";

            var result = new Result();
            result.AddScriptError(ScriptIdentifier.ResetSystem, expectedScriptMessage);

            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler.ExecuteResetSystemScript().Returns(Task.FromResult(OperationStatus.Error));

            var sut = new SystemService(logger, scriptHandler, result);

            // Act
            var systemControlResult = await sut.ResetSystem();

            // Assert
            logger.Received(1).Log(
                Arg.Is(LogLevel.Error),
                Arg.Any<EventId>(),
                Arg.Is<object>(o =>
                    o.ToString()!.StartsWith(SystemCouldNotBeResetMessage) &&
                    o.ToString()!.Contains(expectedScriptMessage)),
                null,
                Arg.Any<Func<object, Exception?, string>>());

            systemControlResult.Status.Should().Be(OperationStatus.Error);
            systemControlResult.Message.Should()
                .StartWith(SystemCouldNotBeResetMessage).And
                .Contain(expectedScriptMessage);
        }

        [Fact]
        public async Task ShouldReturnSuccess_WhenResetSystemSucceeds()
        {
            // Arrange
            var logger = GetLoggerSubstitute();
            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler.ExecuteResetSystemScript().Returns(Task.FromResult(OperationStatus.Success));

            var sut = new SystemService(logger, scriptHandler, new Result());

            // Act
            var systemControlResult = await sut.ResetSystem();

            // Assert
            systemControlResult.Status.Should().Be(OperationStatus.Success);
            systemControlResult.Message.Should().BeEmpty();
        }
    }

    public class RestartSystem
    {
        private const string SystemCouldNotBeRestartedMessage = "System could not be restarted.";

        [Fact]
        public async Task ShouldLogErrorAndReturnErrorResult_WhenExceptionIsThrown()
        {
            // Arrange
            var logger = GetLoggerSubstitute();
            var expectedException = new InvalidOperationException("invalid operation exception");

            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler.ExecuteRestartSystemScript().ThrowsAsync(expectedException);

            var sut = new SystemService(logger, scriptHandler, new Result());

            // Act
            var result = await sut.RestartSystem();

            // Assert
            logger.Received(1).Log(
                Arg.Is(LogLevel.Error),
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
            var expectedScriptMessage = "A script error.";

            var result = new Result();
            result.AddScriptError(ScriptIdentifier.RestartSystem, expectedScriptMessage);

            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler.ExecuteRestartSystemScript().Returns(Task.FromResult(OperationStatus.Error));

            var sut = new SystemService(logger, scriptHandler, result);

            // Act
            var systemControlResult = await sut.RestartSystem();

            // Assert
            logger.Received(1).Log(
                Arg.Is(LogLevel.Error),
                Arg.Any<EventId>(),
                Arg.Is<object>(o =>
                    o.ToString()!.StartsWith(SystemCouldNotBeRestartedMessage) &&
                    o.ToString()!.Contains(expectedScriptMessage)),
                null,
                Arg.Any<Func<object, Exception?, string>>());

            systemControlResult.Status.Should().Be(OperationStatus.Error);
            systemControlResult.Message.Should()
                .StartWith(SystemCouldNotBeRestartedMessage).And
                .Contain(expectedScriptMessage);
        }

        [Fact]
        public async Task ShouldReturnSuccess_WhenRestartSystemSucceeds()
        {
            // Arrange
            var logger = GetLoggerSubstitute();
            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler.ExecuteRestartSystemScript().Returns(Task.FromResult(OperationStatus.Success));

            var sut = new SystemService(logger, scriptHandler, new Result());

            // Act
            var systemControlResult = await sut.RestartSystem();

            // Assert
            systemControlResult.Status.Should().Be(OperationStatus.Success);
            systemControlResult.Message.Should().BeEmpty();
        }
    }

    public class ShutdownSystem
    {
        private const string SystemCouldNotBeShutDownMessage = "System could not be shut down.";

        [Fact]
        public async Task ShouldLogErrorAndReturnErrorResult_WhenExceptionIsThrown()
        {
            // Arrange
            var logger = GetLoggerSubstitute();
            var expectedException = new InvalidOperationException("invalid operation exception");

            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler.ExecuteShutdownSystemScript().ThrowsAsync(expectedException);

            var sut = new SystemService(logger, scriptHandler, new Result());

            // Act
            var result = await sut.ShutdownSystem();

            // Assert
            logger.Received(1).Log(
                Arg.Is(LogLevel.Error),
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
            var expectedScriptMessage = "A script error.";

            var result = new Result();
            result.AddScriptError(ScriptIdentifier.ShutdownSystem, expectedScriptMessage);

            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler.ExecuteShutdownSystemScript().Returns(Task.FromResult(OperationStatus.Error));

            var sut = new SystemService(logger, scriptHandler, result);

            // Act
            var systemControlResult = await sut.ShutdownSystem();

            // Assert
            logger.Received(1).Log(
                Arg.Is(LogLevel.Error),
                Arg.Any<EventId>(),
                Arg.Is<object>(o =>
                    o.ToString()!.StartsWith(SystemCouldNotBeShutDownMessage) &&
                    o.ToString()!.Contains(expectedScriptMessage)),
                null,
                Arg.Any<Func<object, Exception?, string>>());

            systemControlResult.Status.Should().Be(OperationStatus.Error);
            systemControlResult.Message.Should()
                .StartWith(SystemCouldNotBeShutDownMessage).And
                .Contain(expectedScriptMessage);
        }

        [Fact]
        public async Task ShouldReturnSuccess_WhenShutdownSystemSucceeds()
        {
            // Arrange
            var logger = GetLoggerSubstitute();
            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler.ExecuteShutdownSystemScript().Returns(Task.FromResult(OperationStatus.Success));

            var sut = new SystemService(logger, scriptHandler, new Result());

            // Act
            var systemControlResult = await sut.ShutdownSystem();

            // Assert
            systemControlResult.Status.Should().Be(OperationStatus.Success);
            systemControlResult.Message.Should().BeEmpty();
        }
    }

    private static ILogger GetLoggerSubstitute()
    {
        var logger = Substitute.For<ILogger>();
        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        return logger;
    }
}
