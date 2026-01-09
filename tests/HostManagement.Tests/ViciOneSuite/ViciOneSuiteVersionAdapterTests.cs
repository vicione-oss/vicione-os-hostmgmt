using Microsoft.Extensions.Logging;
using FluentAssertions;
using NSubstitute;
using System.IO.Abstractions;
using Xunit;
using HostManagement.ViciOneSuite;
using HostManagement.Common.Wrappers;
using HostManagement.Configuration;
using HostManagement.Shared.Communication.Enums;

namespace HostManagement.Tests.ViciOneSuite;

public class ViciOneSuiteVersionAdapterTests
{
    private static readonly FileSystemWrapper s_fileSystemWrapper = new(
        new FileSystem(),
        new ConsoleLogger(EnvironmentVariables.LogLevel));

    public class GetNonPreReleaseSuiteVersions
    {
        [Fact]
        public async Task ShouldReturnError_WhenGetVersionsReturnsError()
        {
            // Arrange
            var suiteInstallScriptSubstitute = Substitute.For<IViciOneSuiteInstallScriptHandler>();
            suiteInstallScriptSubstitute
                .ExecuteGetViciOneSuiteVersionsScript()
                .Returns((OperationStatus.Error, string.Empty));

            var sut = new ViciOneSuiteVersionAdapter(
                suiteInstallScriptSubstitute,
                Substitute.For<IViciOneSuiteVersionHelper>(),
                s_fileSystemWrapper);

            // Act
            using var cts = new CancellationTokenSource();
            var (status, versions) = await sut.GetNonPreReleaseSuiteVersions(cts.Token);

            // Assert
            status.Should().Be(OperationStatus.Error);
            versions.Should().BeEmpty();
        }

        [Fact]
        public async Task ShouldReturnError_WhenGetFilteredSuiteVersionsReturnsError()
        {
            // Arrange
            var suiteInstallScriptSubstitute = Substitute.For<IViciOneSuiteInstallScriptHandler>();
            suiteInstallScriptSubstitute
                .ExecuteGetViciOneSuiteVersionsScript()
                .Returns((OperationStatus.Success, string.Empty));

            var suiteVersionHelperSubstitute = Substitute.For<IViciOneSuiteVersionHelper>();
            suiteVersionHelperSubstitute
                .GetFilteredSuiteVersions(Arg.Any<IEnumerable<string>>(), Arg.Any<Func<string, bool>>())
                .Returns((OperationStatus.Error, Enumerable.Empty<string>()));

            var sut = new ViciOneSuiteVersionAdapter(
                suiteInstallScriptSubstitute,
                suiteVersionHelperSubstitute,
                s_fileSystemWrapper);

            // Act
            using var cts = new CancellationTokenSource();
            var (status, versions) = await sut.GetNonPreReleaseSuiteVersions(cts.Token);

            // Assert
            status.Should().Be(OperationStatus.Error);
            versions.Should().BeEmpty();
        }

        [Fact]
        public async Task ShouldReturnWarning_WhenGetVersionsReturnsWarning()
        {
            // Arrange
            var suiteInstallScriptSubstitute = Substitute.For<IViciOneSuiteInstallScriptHandler>();
            suiteInstallScriptSubstitute
                .ExecuteGetViciOneSuiteVersionsScript()
                .Returns((OperationStatus.Success, VersionsSampleData.VersionsCsvContent));

            suiteInstallScriptSubstitute
                .ExecuteGetInstalledSuiteVersionScript()
                .Returns((OperationStatus.Success, string.Empty));

            var suiteVersionHelperSubstitute = Substitute.For<IViciOneSuiteVersionHelper>();
            suiteVersionHelperSubstitute
                .GetVersionMapping(Arg.Any<string>())
                .Returns((OperationStatus.Success, VersionsSampleData.VersionMapping));

            suiteVersionHelperSubstitute
                .GetCompatibleSuiteVersions(
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<Dictionary<string, IList<string>>>())
                .Returns((OperationStatus.Warning, Enumerable.Empty<string>()));

            var sut = new ViciOneSuiteVersionAdapter(
                suiteInstallScriptSubstitute,
                suiteVersionHelperSubstitute,
                s_fileSystemWrapper);

            // Act
            using var cts = new CancellationTokenSource();
            var (status, versions) = await sut.GetNonPreReleaseSuiteVersions(cts.Token);

            // Assert
            status.Should().Be(OperationStatus.Warning);
            versions.Should().BeEmpty();
        }

        [Fact]
        public async Task ShouldReturnSuccess_WhenAllOperationsReturnSuccess()
        {
            // Arrange
            var suiteInstallScriptSubstitute = Substitute.For<IViciOneSuiteInstallScriptHandler>();
            suiteInstallScriptSubstitute
                .ExecuteGetViciOneSuiteVersionsScript()
                .Returns((OperationStatus.Success, VersionsSampleData.VersionsCsvContent));

            var suiteVersionHelperSubstitute = Substitute.For<IViciOneSuiteVersionHelper>();
            suiteVersionHelperSubstitute
                .GetVersionMapping(Arg.Any<string>())
                .Returns((OperationStatus.Success, VersionsSampleData.VersionMapping));

            suiteVersionHelperSubstitute
                .GetCompatibleSuiteVersions(
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<Dictionary<string, IList<string>>>())
                .Returns((OperationStatus.Success, VersionsSampleData.CompatibleVersions));

            suiteVersionHelperSubstitute
                .GetFilteredSuiteVersions(Arg.Any<IEnumerable<string>>(), Arg.Any<Func<string, bool>>())
                .Returns((OperationStatus.Success, VersionsSampleData.CompatibleArm64ReleaseVersions));

            var sut = new ViciOneSuiteVersionAdapter(
                suiteInstallScriptSubstitute,
                suiteVersionHelperSubstitute,
                s_fileSystemWrapper);

            // Act
            using var cts = new CancellationTokenSource();
            var (status, versions) = await sut.GetNonPreReleaseSuiteVersions(cts.Token);

            // Assert
            status.Should().Be(OperationStatus.Success);
            versions.Should()
                .NotBeEmpty().And
                .BeEquivalentTo(VersionsSampleData.CompatibleArm64ReleaseVersions);
        }
    }

    public class GetSuiteVersions
    {
        [Fact]
        public async Task ShouldReturnError_WhenExecuteVersionsScriptReturnsError()
        {
            // Arrange
            var suiteInstallScriptSubstitute = Substitute.For<IViciOneSuiteInstallScriptHandler>();
            suiteInstallScriptSubstitute
                .ExecuteGetViciOneSuiteVersionsScript()
                .Returns((OperationStatus.Error, string.Empty));

            var sut = new ViciOneSuiteVersionAdapter(
                suiteInstallScriptSubstitute,
                Substitute.For<IViciOneSuiteVersionHelper>(),
                s_fileSystemWrapper);

            // Act
            using var cts = new CancellationTokenSource();
            var (status, versions) = await sut.GetSuiteVersions(cts.Token);

            // Assert
            status.Should().Be(OperationStatus.Error);
            versions.Should().BeEmpty();
        }

        [Fact]
        public async Task ShouldThrowOperationCanceledException_WhenCancellationRequested()
        {
            // Arrange
            var suiteInstallScriptMock = Substitute.For<IViciOneSuiteInstallScriptHandler>();
            suiteInstallScriptMock
                .ExecuteGetViciOneSuiteVersionsScript()
                .Returns((OperationStatus.Success, VersionsSampleData.VersionsCsvContent));

            var sut = new ViciOneSuiteVersionAdapter(
                suiteInstallScriptMock,
                Substitute.For<IViciOneSuiteVersionHelper>(),
                s_fileSystemWrapper);

            // Act
            using var cts = new CancellationTokenSource();
            await cts.CancelAsync();
            var result = async () => await sut.GetSuiteVersions(cts.Token);

            // Assert
            await result.Should().ThrowAsync<OperationCanceledException>();
        }

        [Fact]
        public async Task ShouldReturnError_WhenGetVersionMappingReturnsError()
        {
            // Arrange
            var suiteInstallScriptSubstitute = Substitute.For<IViciOneSuiteInstallScriptHandler>();
            suiteInstallScriptSubstitute
                .ExecuteGetViciOneSuiteVersionsScript()
                .Returns((OperationStatus.Success, VersionsSampleData.VersionsCsvContent));

            var suiteVersionHelperSubstitute = Substitute.For<IViciOneSuiteVersionHelper>();
            suiteVersionHelperSubstitute
                .GetVersionMapping(Arg.Any<string>())
                .Returns((OperationStatus.Error, new Dictionary<string, IList<string>>()));

            var sut = new ViciOneSuiteVersionAdapter(
                suiteInstallScriptSubstitute,
                suiteVersionHelperSubstitute,
                s_fileSystemWrapper);

            // Act
            using var cts = new CancellationTokenSource();
            var (status, versions) = await sut.GetSuiteVersions(cts.Token);

            // Assert
            status.Should().Be(OperationStatus.Error);
            versions.Should().BeEmpty();
        }

        [Fact]
        public async Task ShouldReturnWarning_WhenGetCompatibleSuiteVersionsReturnsWarning()
        {
            // Arrange
            var suiteInstallScriptSubstitute = Substitute.For<IViciOneSuiteInstallScriptHandler>();
            suiteInstallScriptSubstitute
                .ExecuteGetViciOneSuiteVersionsScript()
                .Returns((OperationStatus.Success, VersionsSampleData.VersionsCsvContent));

            var suiteVersionHelperSubstitute = Substitute.For<IViciOneSuiteVersionHelper>();
            suiteVersionHelperSubstitute
                .GetVersionMapping(Arg.Any<string>())
                .Returns((OperationStatus.Success, VersionsSampleData.VersionMapping));

            suiteVersionHelperSubstitute
                .GetCompatibleSuiteVersions(
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<Dictionary<string, IList<string>>>())
                .Returns((OperationStatus.Warning, Enumerable.Empty<string>()));

            var sut = new ViciOneSuiteVersionAdapter(
                suiteInstallScriptSubstitute,
                suiteVersionHelperSubstitute,
                s_fileSystemWrapper);

            // Act
            using var cts = new CancellationTokenSource();
            var (status, versions) = await sut.GetSuiteVersions(cts.Token);

            // Assert
            status.Should().Be(OperationStatus.Warning);
            versions.Should().BeEmpty();
        }

        [Fact]
        public async Task ShouldReturnError_WhenGetCompatibleSuiteVersionsReturnsError()
        {
            // Arrange
            var suiteInstallScriptSubstitute = Substitute.For<IViciOneSuiteInstallScriptHandler>();
            suiteInstallScriptSubstitute
                .ExecuteGetViciOneSuiteVersionsScript()
                .Returns((OperationStatus.Success, VersionsSampleData.VersionsCsvContent));

            var suiteVersionHelperSubstitute = Substitute.For<IViciOneSuiteVersionHelper>();
            suiteVersionHelperSubstitute
                .GetVersionMapping(Arg.Any<string>())
                .Returns((OperationStatus.Success, VersionsSampleData.VersionMapping));

            suiteVersionHelperSubstitute
                .GetCompatibleSuiteVersions(
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<Dictionary<string, IList<string>>>())
                .Returns((OperationStatus.Error, Enumerable.Empty<string>()));

            var sut = new ViciOneSuiteVersionAdapter(
                suiteInstallScriptSubstitute,
                suiteVersionHelperSubstitute,
                s_fileSystemWrapper);

            // Act
            using var cts = new CancellationTokenSource();
            var (status, versions) = await sut.GetSuiteVersions(cts.Token);

            // Assert
            status.Should().Be(OperationStatus.Error);
            versions.Should().BeEmpty();
        }

        [Fact]
        public async Task ShouldReturnSuccess_WhenAllOperationsReturnSuccess()
        {
            // Arrange
            var suiteInstallScriptSubstitute = Substitute.For<IViciOneSuiteInstallScriptHandler>();
            suiteInstallScriptSubstitute
                .ExecuteGetViciOneSuiteVersionsScript()
                .Returns((OperationStatus.Success, VersionsSampleData.VersionsCsvContent));

            var suiteVersionHelperSubstitute = Substitute.For<IViciOneSuiteVersionHelper>();
            suiteVersionHelperSubstitute
                .GetVersionMapping(Arg.Any<string>())
                .Returns((OperationStatus.Success, VersionsSampleData.VersionMapping));

            suiteVersionHelperSubstitute
                .GetCompatibleSuiteVersions(
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<Dictionary<string, IList<string>>>())
                .Returns((OperationStatus.Success, VersionsSampleData.CompatibleVersions));

            var sut = new ViciOneSuiteVersionAdapter(
                suiteInstallScriptSubstitute,
                suiteVersionHelperSubstitute,
                s_fileSystemWrapper);

            // Act
            using var cts = new CancellationTokenSource();
            var (status, versions) = await sut.GetSuiteVersions(cts.Token);

            // Assert
            status.Should().Be(OperationStatus.Success);
            versions.Should()
                .NotBeEmpty().And
                .BeEquivalentTo(VersionsSampleData.CompatibleVersions);
        }
    }

    public class InstallVersion
    {
        [Fact]
        public async Task ShouldReturnError_WhenGetSuiteVersionDebianFileNameReturnsError()
        {
            // Arrange
            var versionToInstall = "0.13.0";

            var suiteVersionHelperSubstitute = Substitute.For<IViciOneSuiteVersionHelper>();
            suiteVersionHelperSubstitute
                .GetSuiteVersionDebianFileName(versionToInstall)
                .Returns((OperationStatus.Error, string.Empty));

            var sut = new ViciOneSuiteVersionAdapter(
                Substitute.For<IViciOneSuiteInstallScriptHandler>(),
                suiteVersionHelperSubstitute,
                s_fileSystemWrapper);

            // Act
            using var cts = new CancellationTokenSource();
            var status = await sut.InstallVersion(versionToInstall, cts.Token);

            // Assert
            status.Should().Be(OperationStatus.Error);
        }

        [Fact]
        public async Task ShouldReturnError_WhenExecuteGetViciOneSuiteDebFileScriptReturnsError()
        {
            // Arrange
            var versionToInstall = "0.13.0";
            var suiteDebianFileName = $"vicione-suite_{versionToInstall}_amd64.deb";

            var suiteVersionHelperSubstitute = Substitute.For<IViciOneSuiteVersionHelper>();
            suiteVersionHelperSubstitute
                .GetSuiteVersionDebianFileName(Arg.Any<string>())
                .Returns((OperationStatus.Success, suiteDebianFileName));

            var suiteInstallScriptSubstitute = Substitute.For<IViciOneSuiteInstallScriptHandler>();
            suiteInstallScriptSubstitute
                .ExecuteGetViciOneSuiteDebFileScript(suiteDebianFileName)
                .Returns(OperationStatus.Error);

            var sut = new ViciOneSuiteVersionAdapter(
                suiteInstallScriptSubstitute,
                suiteVersionHelperSubstitute,
                s_fileSystemWrapper);

            // Act
            using var cts = new CancellationTokenSource();
            var status = await sut.InstallVersion(versionToInstall, cts.Token);

            // Assert
            status.Should().Be(OperationStatus.Error);
        }

        [Fact]
        public async Task ShouldThrowOperationCanceledException_WhenCancellationRequested()
        {
            // Arrange
            var versionToInstall = "0.13.0";
            var suiteDebianFileName = $"vicione-suite_{versionToInstall}_amd64.deb";

            var suiteVersionHelperSubstitute = Substitute.For<IViciOneSuiteVersionHelper>();
            suiteVersionHelperSubstitute
                .GetSuiteVersionDebianFileName(Arg.Any<string>())
                .Returns((OperationStatus.Success, suiteDebianFileName));

            var suiteInstallScriptSubstitute = Substitute.For<IViciOneSuiteInstallScriptHandler>();
            suiteInstallScriptSubstitute
                .ExecuteGetViciOneSuiteDebFileScript(suiteDebianFileName)
                .Returns(OperationStatus.Success);

            var sut = new ViciOneSuiteVersionAdapter(
                suiteInstallScriptSubstitute,
                suiteVersionHelperSubstitute,
                s_fileSystemWrapper);

            // Act
            using var cts = new CancellationTokenSource();
            await cts.CancelAsync();
            var result = async () => await sut.InstallVersion(versionToInstall, cts.Token);

            // Assert
            await result.Should().ThrowAsync<OperationCanceledException>();
        }

        [Fact]
        public async Task ShouldNotDeleteDebianFile_WhenCancellationRequestedAndNoFileExists()
        {
            // Arrange
            var versionToInstall = "0.13.0";
            var suiteDebianFileName = $"vicione-suite_{versionToInstall}_amd64.deb";
            var suiteDebianFilePath = Path.Combine(
                EnvironmentVariables.SuiteDownloadDirectory,
                suiteDebianFileName);

            var suiteVersionHelperSubstitute = Substitute.For<IViciOneSuiteVersionHelper>();
            suiteVersionHelperSubstitute
                .GetSuiteVersionDebianFileName(Arg.Any<string>())
                .Returns((OperationStatus.Success, suiteDebianFileName));

            var suiteInstallScriptSubstitute = Substitute.For<IViciOneSuiteInstallScriptHandler>();
            suiteInstallScriptSubstitute
                .ExecuteGetViciOneSuiteDebFileScript(suiteDebianFileName)
                .Returns(OperationStatus.Success);

            var fileSystemSubstitute = Substitute.For<IFileSystem>();
            fileSystemSubstitute
                .File.Exists(suiteDebianFilePath)
                .Returns(false);

            var fileSystemWrapper = new FileSystemWrapper(
                fileSystemSubstitute,
                Substitute.For<ILogger>());

            var sut = new ViciOneSuiteVersionAdapter(
                suiteInstallScriptSubstitute,
                suiteVersionHelperSubstitute,
                fileSystemWrapper);

            // Act
            using var cts = new CancellationTokenSource();
            await cts.CancelAsync();
            var result = async () => await sut.InstallVersion(versionToInstall, cts.Token);

            // Assert
            await result.Should().ThrowAsync<OperationCanceledException>();
            fileSystemSubstitute.File.Received(0).Delete(suiteDebianFilePath);
        }

        [Fact]
        public async Task ShouldDeleteDebianFile_WhenCancellationRequestedAndFileExists()
        {
            // Arrange
            var versionToInstall = "0.13.0";
            var suiteDebianFileName = $"vicione-suite_{versionToInstall}_amd64.deb";
            var suiteDebianFilePath = Path.Combine(
                EnvironmentVariables.SuiteDownloadDirectory,
                suiteDebianFileName);

            var suiteVersionHelperSubstitute = Substitute.For<IViciOneSuiteVersionHelper>();
            suiteVersionHelperSubstitute
                .GetSuiteVersionDebianFileName(Arg.Any<string>())
                .Returns((OperationStatus.Success, suiteDebianFileName));

            var suiteInstallScriptSubstitute = Substitute.For<IViciOneSuiteInstallScriptHandler>();
            suiteInstallScriptSubstitute
                .ExecuteGetViciOneSuiteDebFileScript(suiteDebianFileName)
                .Returns(OperationStatus.Success);

            var fileSystemSubstitute = Substitute.For<IFileSystem>();
            fileSystemSubstitute
                .File.Exists(suiteDebianFilePath)
                .Returns(true);

            var fileSystemWrapper = new FileSystemWrapper(
                fileSystemSubstitute,
                Substitute.For<ILogger>());

            var sut = new ViciOneSuiteVersionAdapter(
                suiteInstallScriptSubstitute,
                suiteVersionHelperSubstitute,
                fileSystemWrapper);

            // Act
            using var cts = new CancellationTokenSource();
            await cts.CancelAsync();
            var result = async () => await sut.InstallVersion(versionToInstall, cts.Token);

            // Assert
            await result.Should().ThrowAsync<OperationCanceledException>();
            fileSystemSubstitute.File.Received(1).Delete(suiteDebianFilePath);
        }

        [Theory]
        [InlineData(OperationStatus.Error)]
        [InlineData(OperationStatus.Warning)]
        [InlineData(OperationStatus.Success)]
        public async Task ShouldReturnStatus_WhenExecuteViciOneSuiteInstallScriptReturns(
            OperationStatus expectedOperationStatus)
        {
            // Arrange
            var versionToInstall = "0.13.0";
            var suiteDebianFileName = $"vicione-suite_{versionToInstall}_amd64.deb";

            var suiteVersionHelperSubstitute = Substitute.For<IViciOneSuiteVersionHelper>();
            suiteVersionHelperSubstitute
                .GetSuiteVersionDebianFileName(Arg.Any<string>())
                .Returns((OperationStatus.Success, suiteDebianFileName));

            var suiteInstallScriptSubstitute = Substitute.For<IViciOneSuiteInstallScriptHandler>();
            suiteInstallScriptSubstitute
                .ExecuteGetViciOneSuiteDebFileScript(suiteDebianFileName)
                .Returns(OperationStatus.Success);

            suiteInstallScriptSubstitute
                .ExecuteViciOneSuiteInstallScript(suiteDebianFileName)
                .Returns(expectedOperationStatus);

            var sut = new ViciOneSuiteVersionAdapter(
                suiteInstallScriptSubstitute,
                suiteVersionHelperSubstitute,
                s_fileSystemWrapper);

            // Act
            using var cts = new CancellationTokenSource();
            var status = await sut.InstallVersion(versionToInstall, cts.Token);

            // Assert
            status.Should().Be(expectedOperationStatus);
        }
    }
}
