using AwesomeAssertions;
using NSubstitute;
using Xunit;
using HostManagement.Services;
using Microsoft.Extensions.Logging;
using HostManagement.Shared.Contracts.System;
using HostManagement.Shared.Communication.Enums;
using System.Text;

namespace HostManagement.Tests.Services;

[Collection("Non Parallel Tests")]
public class SystemServiceTests
{
    public class InstallSignedDebianPackage
    {
        [Fact]
        public async Task ShouldLogErrorAndReturnErrorResult_WhenExceptionIsThrown()
        {
            // Arrange
            var logger = Substitute.For<ILogger>();
            logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);

            var scriptResult = new Result();
            var scriptHandler = Substitute.For<IScriptHandler>();

            var publicKeys = new List<string> { "valid-public-key" };

            var signedPackage = new SignedDebianPackage
            {
                FilePath = "/path/to/package.deb",
                SignatureFilePath = "/path/to/package.deb.minisig"
            };

            // Minisign will not find the signature file so it will throw an exception
            var expectedException = new FileNotFoundException("could not find signatureFile");
            var expectedMessage =
                $"Installing signed Debian package failed ('{signedPackage.FilePath}'). {expectedException.Message}";

            var sut = new SystemService(logger, scriptHandler, scriptResult);

            // Act
            var result = await sut.InstallSignedDebianPackage(signedPackage, publicKeys, CancellationToken.None);

            // Assert
            logger.Received(1).Log(
                Arg.Is(LogLevel.Error),
                Arg.Any<EventId>(),
                Arg.Any<object>(),
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
            var logger = Substitute.For<ILogger>();
            logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);

            var result = new Result();
            var installScriptHandler = Substitute.For<IScriptHandler>();

            var unitTestDir = Path.Combine(Path.GetTempPath(), "_hm-unit-test");
            var packagePath = Path.Combine(unitTestDir, "package.deb");
            var publicKeyPath = Path.Combine(unitTestDir, "minisign.pub");

            // Create dummy package
            Directory.CreateDirectory(unitTestDir);
            await File.WriteAllTextAsync(packagePath, "dummy package");

            // Generate a key pair for testing...
            var minisignKeyPair = Minisign.Core.GenerateKeyPair(
                password: "hm-unit-test",
                writeOutputFiles: true,
                outputFolder: unitTestDir);

            // ...and sign the package; Sign creates the file and returns the signature path
            var signaturePath = Minisign.Core.Sign(packagePath, minisignKeyPair.MinisignPrivateKey);

            // Read the public key and extract the  base64 string
            var publicKeyLines = await File.ReadAllLinesAsync(publicKeyPath);
            var publicKeyBase64 = publicKeyLines.FirstOrDefault(line => !line.StartsWith("untrusted comment:"));

            var signedDebianPackage = new SignedDebianPackage() { FilePath = packagePath, SignatureFilePath = signaturePath };
            var expectedLogMessage = $"Installing signed Debian package was canceled ('{packagePath}').";
            var sut = new SystemService(logger, installScriptHandler, result);

            // Act
            using var cts = new CancellationTokenSource();
            await cts.CancelAsync();
            var systemControlResult = await sut.InstallSignedDebianPackage(signedDebianPackage, [publicKeyBase64!], cts.Token);

            // Assert
            logger.Received(1).Log(
                Arg.Is(LogLevel.Warning),
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString()!.Contains(expectedLogMessage)),
                null,
                Arg.Any<Func<object, Exception?, string>>());

            systemControlResult.Status.Should().Be(OperationStatus.Warning);
            systemControlResult.Message.Should().Contain(expectedLogMessage);
        }

        [Fact]
        public async Task ShouldReturnError_WhenPackageSignatureInvalid()
        {
            // Arrange
            var logger = Substitute.For<ILogger>();
            logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);

            var result = new Result();
            var installScriptHandler = Substitute.For<IScriptHandler>();
            var unitTestDir = Path.Combine(Path.GetTempPath(), "_hm-unit-test");

            var invalidPublicKeyBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes("invalid-public-key"));

            var packagePath = Path.Combine(unitTestDir, "package.deb");
            var signaturePath = Path.Combine(unitTestDir, "package.deb.minisig");
            var invalidSignature =
                """
                untrusted comment: signature from minisign secret key
                RUSdbzO140cELjvXFidzF+eQAAZBjdqVGnUCcfR0q/uK4Cq2ApHdQuX4W8n0VA/ep4+vRnwWD9wuH3rlSqvmHjqq9saALhfHTAs=
                trusted comment: timestamp:1763291678    file:package.deb    hashed
                EmgYQcdc0B/35ZL202oDGlTmczYv40NlWOatNxjCu4FboErErG1L8Fh6OMsxjw5NPb5+LYf9eH3Nng4Lg8+ECw==
                """;
            var expectedMessage = $"Installing signed Debian package failed ('{packagePath}'). Package signature is invalid.";

            // Create dummy files
            Directory.CreateDirectory(unitTestDir);
            await File.WriteAllTextAsync(packagePath, "dummy package");
            await File.WriteAllTextAsync(signaturePath, invalidSignature);

            var sut = new SystemService(logger, installScriptHandler, result);

            var signedPackage = new SignedDebianPackage() { FilePath = packagePath, SignatureFilePath = signaturePath };

            // Act
            var systemControlResult = await sut.InstallSignedDebianPackage(signedPackage, [invalidPublicKeyBase64], CancellationToken.None);

            // Assert
            systemControlResult.Status.Should().Be(OperationStatus.Error);
            systemControlResult.Message.Should().Be(expectedMessage);

            // Cleanup
            Directory.Delete(unitTestDir, recursive: true);
        }

        [Fact]
        public async Task ShouldReturnSuccess_WhenPackageWasInstalled()
        {
            // Arrange
            var logger = Substitute.For<ILogger>();
            var result = new Result();
            var installScriptHandler = Substitute.For<IScriptHandler>();

            var unitTestDir = Path.Combine(Path.GetTempPath(), "_hm-unit-test");
            var packagePath = Path.Combine(unitTestDir, "package.deb");
            var publicKeyPath = Path.Combine(unitTestDir, "minisign.pub");

            // Create dummy package
            Directory.CreateDirectory(unitTestDir);
            await File.WriteAllTextAsync(packagePath, "dummy package");

            // Generate a key pair for testing...
            var minisignKeyPair = Minisign.Core.GenerateKeyPair(
                password: "hm-unit-test",
                writeOutputFiles: true,
                outputFolder: unitTestDir);

            // ...and sign the package; Sign creates the file and returns the signature path
            var signaturePath = Minisign.Core.Sign(packagePath, minisignKeyPair.MinisignPrivateKey);

            // Read the public key and extract the  base64 string
            var publicKeyLines = await File.ReadAllLinesAsync(publicKeyPath);
            var publicKeyBase64 = publicKeyLines.FirstOrDefault(line => !line.StartsWith("untrusted comment:"));

            installScriptHandler.ExecuteInstallDebianPackageScript(packagePath)
                .Returns(Task.FromResult(OperationStatus.Success));

            var sut = new SystemService(logger, installScriptHandler, result);

            var signedPackage = new SignedDebianPackage() { FilePath = packagePath, SignatureFilePath = signaturePath };

            // Act
            var systemControlResult = await sut.InstallSignedDebianPackage(signedPackage, [publicKeyBase64!], CancellationToken.None);

            // Assert
            systemControlResult.Status.Should().Be(OperationStatus.Success);
            systemControlResult.Message.Should().BeEmpty();

            // Cleanup
            Directory.Delete(unitTestDir, recursive: true);
        }
    }
}
