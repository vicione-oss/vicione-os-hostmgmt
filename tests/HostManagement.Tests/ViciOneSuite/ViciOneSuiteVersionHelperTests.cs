using FluentAssertions;
using NSubstitute;
using Xunit;
using HostManagement.Shared.Communication.Enums;
using HostManagement.ViciOneSuite;
using HostManagement.Common.Abstractions;
using HostManagement.Common.Wrappers;

namespace HostManagement.Tests.ViciOneSuite;

public class ViciOneSuiteVersionHelperTests
{
    public static readonly TheoryData<string> VersionsCsvDataWithInvalidColumnCount =
    [
        """
        HostManagement Version,Suite Debian Package Version
        0.5.0vicione-suite_0.13.0_arm64.deb
        0.5.0,vicione-suite_0.13.0_amd64.deb
        """,
        """
        HostManagement Version,Suite Debian Package Version
        0.5.0,vicione-suite_0.13.0_arm64.deb,
        0.5.0,vicione-suite_0.13.0_amd64.deb
        """
    ];

    public class GetVersionMapping : ViciOneSuiteVersionHelperTests
    {
        [Fact]
        public async Task ShouldReturnError_WhenVersionMappingFileEmpty()
        {
            // Arrange
            var expectedMessage = "Version mapping file is empty.";
            var mappingFileWithoutContent = "";

            var result = new Result();
            var sut = new ViciOneSuiteVersionHelper(
                Substitute.For<ICommonScriptHandler>(),
                Substitute.For<IStringWrapper>(),
                result);

            // Act
            var (status, versionMapping) = await sut.GetVersionMapping(mappingFileWithoutContent);

            // Assert
            status.Should().Be(OperationStatus.Error);
            versionMapping.Should().BeEmpty();
            result.Messages.Should().Contain(expectedMessage);
            result.OperationStatus.Should().Be(OperationStatus.Error);
        }

        [Fact]
        public async Task ShouldReturnError_WhenVersionMappingFileHeaderMissing()
        {
            // Arrange
            var expectedMessage = "Version mapping file has no valid header.";
            var mappingFileWithoutHeader =
                """
                0.5.0,vicione-suite_0.13.0_arm64.deb
                0.5.0,vicione-suite_0.13.0_amd64.deb
                """;

            var result = new Result();
            var sut = new ViciOneSuiteVersionHelper(
                Substitute.For<ICommonScriptHandler>(),
                Substitute.For<IStringWrapper>(),
                result);

            // Act
            var (status, versionMapping) = await sut.GetVersionMapping(mappingFileWithoutHeader);

            // Assert
            status.Should().Be(OperationStatus.Error);
            versionMapping.Should().BeEmpty();
            result.Messages.Should().Contain(expectedMessage);
            result.OperationStatus.Should().Be(OperationStatus.Error);
        }

        [Fact]
        public async Task ShouldReturnError_WhenVersionMappingFileHeaderInvalid()
        {
            // Arrange
            var expectedMessage = "Version mapping file has no valid header.";
            var mappingFileWithInvalidHeader =
                """
                HostManagement,Suite
                0.5.0,vicione-suite_0.13.0_arm64.deb
                0.5.0,vicione-suite_0.13.0_amd64.deb
                """;

            var result = new Result();
            var sut = new ViciOneSuiteVersionHelper(
                Substitute.For<ICommonScriptHandler>(),
                Substitute.For<IStringWrapper>(),
                result);

            // Act
            var (status, versionMapping) = await sut.GetVersionMapping(mappingFileWithInvalidHeader);

            // Assert
            status.Should().Be(OperationStatus.Error);
            versionMapping.Should().BeEmpty();
            result.Messages.Should().Contain(expectedMessage);
            result.OperationStatus.Should().Be(OperationStatus.Error);
        }

        [Theory]
        [MemberData(nameof(VersionsCsvDataWithInvalidColumnCount))]
        public async Task ShouldReturnError_WhenVersionMappingLineCouldNotBeParsed(
            string mappingFileContent)
        {
            // Arrange
            var expectedLine = 2;
            var expectedMessage = $"Could not parse version mapping line {expectedLine}.";
            var result = new Result();
            var sut = new ViciOneSuiteVersionHelper(
                Substitute.For<ICommonScriptHandler>(),
                Substitute.For<IStringWrapper>(),
                result);

            // Act
            var (status, versionMapping) = await sut.GetVersionMapping(mappingFileContent);

            // Assert
            status.Should().Be(OperationStatus.Error);
            versionMapping.Should().BeEmpty();
            result.Messages.Should().Contain(expectedMessage);
            result.OperationStatus.Should().Be(OperationStatus.Error);
        }

        [Fact]
        public async Task ShouldReturnError_WhenVersionMappingFileHasNoSuiteVersionPrefix()
        {
            // Arrange
            var expectedLine = 3;
            var expectedMessage = $"Could not parse version mapping line {expectedLine}.";

            var mappingFileWithoutSuiteVersionPrefix =
                """
                HostManagement Version,Suite Debian Package Version
                0.5.0,vicione-suite_0.13.0_arm64.deb
                0.5.0,0.13.0_amd64.deb
                """;

            var result = new Result();
            var sut = new ViciOneSuiteVersionHelper(
                Substitute.For<ICommonScriptHandler>(),
                Substitute.For<IStringWrapper>(),
                result);

            // Act
            var (status, versionMapping) =
                await sut.GetVersionMapping(mappingFileWithoutSuiteVersionPrefix);

            // Assert
            status.Should().Be(OperationStatus.Error);
            versionMapping.Should().BeEmpty();
            result.Messages.Should().Contain(expectedMessage);
            result.OperationStatus.Should().Be(OperationStatus.Error);
        }

        [Fact]
        public async Task ShouldReturnWarning_WhenNoMappingsFound()
        {
            // Arrange
            var expectedMessage = "No version mappings found.";
            var mappingFileWithoutData =
                """
                HostManagement Version,Suite Debian Package Version
                """;

            var result = new Result();
            var sut = new ViciOneSuiteVersionHelper(
                Substitute.For<ICommonScriptHandler>(),
                Substitute.For<IStringWrapper>(),
                result);

            // Act
            var (status, versionMapping) =
                await sut.GetVersionMapping(mappingFileWithoutData);

            // Assert
            status.Should().Be(OperationStatus.Warning);
            versionMapping.Should().BeEmpty();
            result.Messages.Should().Contain(expectedMessage);
            result.OperationStatus.Should().Be(OperationStatus.Warning);
        }

        [Fact]
        public async Task ShouldIgnoreSuiteVersion_WhenAlreadyAddedToMappings()
        {
            // Arrange
            var expectedVersionMappings = new Dictionary<string, IList<string>>
            {
                { "0.5.0", new List<string>{ "0.13.0_arm64.deb", "0.13.0_amd64.deb" } }
            };

            var mappingFileWithDuplicateEntries =
                """
                HostManagement Version,Suite Debian Package Version
                0.5.0,vicione-suite_0.13.0_arm64.deb
                0.5.0,vicione-suite_0.13.0_amd64.deb
                0.5.0,vicione-suite_0.13.0_amd64.deb
                """;

            var result = new Result();
            var sut = new ViciOneSuiteVersionHelper(
                Substitute.For<ICommonScriptHandler>(),
                Substitute.For<IStringWrapper>(),
                result);

            // Act
            var (status, versionMapping) =
                await sut.GetVersionMapping(mappingFileWithDuplicateEntries);

            // Assert
            status.Should().Be(OperationStatus.Success);
            versionMapping.Should().BeEquivalentTo(expectedVersionMappings);
            result.Messages.Should().BeEmpty();
        }

        [Fact]
        public async Task ShouldReturnSuccess_WhenMappingsAvailable()
        {
            // Arrange
            var result = new Result();
            var sut = new ViciOneSuiteVersionHelper(
                Substitute.For<ICommonScriptHandler>(),
                Substitute.For<IStringWrapper>(),
                result);

            // Act
            var (status, versionMapping) =
                await sut.GetVersionMapping(VersionsSampleData.VersionsCsvContent);

            // Assert
            status.Should().Be(OperationStatus.Success);
            versionMapping.Should().BeEquivalentTo(VersionsSampleData.VersionMapping);
            result.Messages.Should().BeEmpty();
        }
    }

    public class GetCompatibleSuiteVersions
    {
        [Fact]
        public async Task ShouldReturnWarning_WhenNoCompatibleVersionsFound()
        {
            // Arrange
            var hostManagementVersion = "0.0.0";
            var installedSuiteVersion = "0.16.0";
            var expectedMessage = $"No compatible ViciOne Suite versions for Host Management " +
                $"'{hostManagementVersion}' found.";

            var result = new Result();
            var sut = new ViciOneSuiteVersionHelper(
                Substitute.For<ICommonScriptHandler>(),
                Substitute.For<IStringWrapper>(),
                result);

            // Act
            var (status, versions) = await sut.GetCompatibleSuiteVersions(
                hostManagementVersion,
                installedSuiteVersion,
                VersionsSampleData.VersionMapping);

            // Assert
            status.Should().Be(OperationStatus.Warning);
            versions.Should().BeEmpty();
            result.Messages.Should().Contain(expectedMessage);
            result.OperationStatus.Should().Be(OperationStatus.Warning);
        }

        [Fact]
        public async Task ShouldReturnError_WhenExecuteGetArchitectureScriptReturnsError()
        {
            // Arrange
            var installedSuiteVersion = "0.16.0";

            var commonScriptHandlerSubstitute = Substitute.For<ICommonScriptHandler>();
            commonScriptHandlerSubstitute
                .ExecuteGetArchitectureScript()
                .Returns((OperationStatus.Error, string.Empty));

            var result = new Result();
            var sut = new ViciOneSuiteVersionHelper(
                commonScriptHandlerSubstitute,
                Substitute.For<IStringWrapper>(),
                result);

            // Act
            var (status, versions) = await sut.GetCompatibleSuiteVersions(
                VersionsSampleData.HostManagementVersion,
                installedSuiteVersion,
                VersionsSampleData.VersionMapping);

            // Assert
            status.Should().Be(OperationStatus.Error);
            versions.Should().BeEmpty();
        }

        [Fact]
        public async Task ShouldReturnWarning_WhenNoCompatibleVersionsForArchitectureFound()
        {
            // Arrange
            var installedSuiteVersion = "0.16.0";
            var supportedArchitecture = "arm128";

            var expectedMessage = $"No compatible ViciOne Suite versions for architecture " +
                $"'{supportedArchitecture}' found.";

            var commonScriptHandlerSubstitute = Substitute.For<ICommonScriptHandler>();
            commonScriptHandlerSubstitute
                .ExecuteGetArchitectureScript()
                .Returns((OperationStatus.Success, supportedArchitecture));

            var result = new Result();
            var sut = new ViciOneSuiteVersionHelper(
                commonScriptHandlerSubstitute,
                Substitute.For<IStringWrapper>(),
                result);

            // Act
            var (status, versions) = await sut.GetCompatibleSuiteVersions(
                VersionsSampleData.HostManagementVersion,
                installedSuiteVersion,
                VersionsSampleData.VersionMapping);

            // Assert
            status.Should().Be(OperationStatus.Warning);
            versions.Should().BeEmpty();
            result.Messages.Should().Contain(expectedMessage);
            result.OperationStatus.Should().Be(OperationStatus.Warning);
        }

        [Fact]
        public async Task ShouldReturnError_WhenVersionCouldNotBeParsed()
        {
            // Arrange
            var installedSuiteVersion = "0.17.0";
            var supportedArchitecture = "arm64";
            var invalidSuiteVersion = "arm64.deb";
            var expectedMessage = $"Could not parse ViciOne Suite version '{invalidSuiteVersion}'.";

            var versionMappingWithInvalidSuiteVersion = new Dictionary<string, IList<string>>()
            {
                {
                    VersionsSampleData.HostManagementVersion,
                    new List<string> { invalidSuiteVersion }
                }
            };

            var commonScriptHandlerSubstitute = Substitute.For<ICommonScriptHandler>();
            commonScriptHandlerSubstitute
                .ExecuteGetArchitectureScript()
                .Returns((OperationStatus.Success, supportedArchitecture));

            var result = new Result();
            var sut = new ViciOneSuiteVersionHelper(
                commonScriptHandlerSubstitute,
                Substitute.For<IStringWrapper>(),
                result);

            // Act
            var (status, versions) = await sut.GetCompatibleSuiteVersions(
                VersionsSampleData.HostManagementVersion,
                installedSuiteVersion,
                versionMappingWithInvalidSuiteVersion);

            // Assert
            status.Should().Be(OperationStatus.Error);
            versions.Should().BeEmpty();
            result.Messages.Should().Contain(expectedMessage);
            result.OperationStatus.Should().Be(OperationStatus.Error);
        }

        [Theory]
        [InlineData("0.17.0~1000123")]
        [InlineData("0.17.0")]
        public async Task ShouldReturnWarning_WhenNoCompatibleVersionsForInstalledSuiteFound(
            string installedSuiteVersion)
        {
            // Arrange
            var supportedArchitecture = "arm64";

            var expectedMessage = $"No compatible ViciOne Suite versions for installed version " +
                $"'{installedSuiteVersion}' found.";

            var commonScriptHandlerSubstitute = Substitute.For<ICommonScriptHandler>();
            commonScriptHandlerSubstitute
                .ExecuteGetArchitectureScript()
                .Returns((OperationStatus.Success, supportedArchitecture));

            var result = new Result();
            var sut = new ViciOneSuiteVersionHelper(
                commonScriptHandlerSubstitute,
                Substitute.For<IStringWrapper>(),
                result);

            // Act
            var (status, versions) = await sut.GetCompatibleSuiteVersions(
                VersionsSampleData.HostManagementVersion,
                installedSuiteVersion,
                VersionsSampleData.VersionMapping);

            // Assert
            status.Should().Be(OperationStatus.Warning);
            versions.Should().BeEmpty();
            result.Messages.Should().Contain(expectedMessage);
            result.OperationStatus.Should().Be(OperationStatus.Warning);
        }

        [Theory]
        [InlineData("0.15.0~1000088")]
        [InlineData("0.16.0~1000077")]
        public async Task ShouldReturnSuccess_WhenCompatibleVersionsFound(
            string installedSuiteVersion)
        {
            // Arrange
            var supportedArchitecture = "arm64";

            var commonScriptHandlerSubstitute = Substitute.For<ICommonScriptHandler>();
            commonScriptHandlerSubstitute
                .ExecuteGetArchitectureScript()
                .Returns((OperationStatus.Success, supportedArchitecture));

            var result = new Result();
            var sut = new ViciOneSuiteVersionHelper(
                commonScriptHandlerSubstitute,
                Substitute.For<IStringWrapper>(),
                result);

            // Act
            var (status, versions) = await sut.GetCompatibleSuiteVersions(
                VersionsSampleData.HostManagementVersion,
                installedSuiteVersion,
                VersionsSampleData.VersionMapping);

            // Assert
            status.Should().Be(OperationStatus.Success);
            versions.Should().BeEquivalentTo(VersionsSampleData.CompatibleArm64Versions);
            result.Messages.Should().BeEmpty();
        }
    }

    public class GetFilteredSuiteVersions
    {
        [Fact]
        public async Task ShouldReturnWarning_WhenNoVersionsFound()
        {
            // Arrange
            var expectedMessage = "No compatible ViciOne Suite versions found.";

            var result = new Result();
            var sut = new ViciOneSuiteVersionHelper(
                Substitute.For<ICommonScriptHandler>(),
                Substitute.For<IStringWrapper>(),
                result);

            // Act
            var (status, versions) = await sut.GetFilteredSuiteVersions(
                VersionsSampleData.CompatibleArm64CIVersions,
                sut.NonPreReleaseVersionsFilter);

            // Assert
            status.Should().Be(OperationStatus.Warning);
            versions.Should().BeEmpty();
            result.Messages.Should().Contain(expectedMessage);
            result.OperationStatus.Should().Be(OperationStatus.Warning);
        }

        [Fact]
        public async Task ShouldReturnSuccess_WhenVersionsFound()
        {
            // Arrange
            var result = new Result();
            var sut = new ViciOneSuiteVersionHelper(
                Substitute.For<ICommonScriptHandler>(),
                Substitute.For<IStringWrapper>(),
                result);

            // Act
            var (status, versions) = await sut.GetFilteredSuiteVersions(
                VersionsSampleData.CompatibleArm64Versions,
                sut.NonPreReleaseVersionsFilter);

            // Assert
            status.Should().Be(OperationStatus.Success);
            versions.Should().BeEquivalentTo(VersionsSampleData.CompatibleArm64ReleaseVersions);
            result.Messages.Should().BeEmpty();
        }
    }

    public class IsVersionContained
    {
        [Fact]
        public async Task ShouldReturnError_WhenVersionIsNotContainedInVersionList()
        {
            // Arrange
            var suiteVersion = "0.17.0";
            var expectedMessage =
                $"Installing ViciOne Suite version '{suiteVersion}' not supported for HostManagement '{VersionsSampleData.HostManagementVersion}'.";

            var result = new Result();
            var sut = new ViciOneSuiteVersionHelper(
                Substitute.For<ICommonScriptHandler>(),
                Substitute.For<IStringWrapper>(),
                result);

            // Act
            var status = await sut.IsVersionContainedInVersionList(
                VersionsSampleData.HostManagementVersion,
                VersionsSampleData.CompatibleArm64Versions,
                suiteVersion);

            // Assert
            status.Should().Be(OperationStatus.Error);
            result.Messages.Should().Contain(expectedMessage);
            result.OperationStatus.Should().Be(OperationStatus.Error);
        }

        [Fact]
        public async Task ShouldReturnSuccess_WhenVersionIsContainedInVersionList()
        {
            // Arrange
            var suiteVersion = "0.16.0";
            
            var result = new Result();
            var sut = new ViciOneSuiteVersionHelper(
                Substitute.For<ICommonScriptHandler>(),
                Substitute.For<IStringWrapper>(),
                result);

            // Act
            var status = await sut.IsVersionContainedInVersionList(
                VersionsSampleData.HostManagementVersion,
                VersionsSampleData.CompatibleArm64Versions,
                suiteVersion);

            // Assert
            status.Should().Be(OperationStatus.Success);
            result.Messages.Should().BeEmpty();
        }
    }

    public class GetSuiteVersionDebianFileName
    {
        [Fact]
        public async Task ShouldReturnError_WhenExecuteGetArchitectureScriptReturnsError()
        {
            // Arrange
            var suiteVersion = "0.16.0";

            var commonScriptHandlerSubstitute = Substitute.For<ICommonScriptHandler>();
            commonScriptHandlerSubstitute
                .ExecuteGetArchitectureScript()
                .Returns((OperationStatus.Error, string.Empty));

            var result = new Result();
            var sut = new ViciOneSuiteVersionHelper(
                commonScriptHandlerSubstitute,
                Substitute.For<IStringWrapper>(),
                result);

            // Act
            var (status, filename) = await sut.GetSuiteVersionDebianFileName(suiteVersion);

            // Assert
            status.Should().Be(OperationStatus.Error);
            filename.Should().BeEmpty();
        }

        [Fact]
        public async Task ShouldReturnError_WhenStringConcatThrows()
        {
            // Arrange
            var suiteVersion = "0.16.0";
            var architecture = "arm64";
            var expectedMessage =
                $"Could not create ViciOne Suite Debian file name for version '{suiteVersion}' and architecture '{architecture}'.";

            var commonScriptHandlerSubstitute = Substitute.For<ICommonScriptHandler>();
            commonScriptHandlerSubstitute
                .ExecuteGetArchitectureScript()
                .Returns((OperationStatus.Success, architecture));

            var stringWrapperSubstitute = Substitute.For<IStringWrapper>();
            stringWrapperSubstitute
                .When(stringWrapper => stringWrapper.ConcatenateStrings(Arg.Any<string[]>()))
                .Throw<ArgumentNullException>();

            var result = new Result();
            var sut = new ViciOneSuiteVersionHelper(
                commonScriptHandlerSubstitute,
                stringWrapperSubstitute,
                result);

            // Act
            var (status, filename) = await sut.GetSuiteVersionDebianFileName(suiteVersion);

            // Assert
            status.Should().Be(OperationStatus.Error);
            filename.Should().BeEmpty();
            result.Messages.Should().Contain(expectedMessage);
            result.OperationStatus.Should().Be(OperationStatus.Error);
        }

        [Fact]
        public async Task ShouldReturnSuccess_WhenFileNameCouldBeCreated()
        {
            // Arrange
            var suiteVersion = "0.16.0";
            var architecture = "arm64";
            var expectedFileName = $"vicione-suite_{suiteVersion}_{architecture}.deb";

            var commonScriptHandlerSubstitute = Substitute.For<ICommonScriptHandler>();
            commonScriptHandlerSubstitute
                .ExecuteGetArchitectureScript()
                .Returns((OperationStatus.Success, architecture));

            var result = new Result();
            var sut = new ViciOneSuiteVersionHelper(
                commonScriptHandlerSubstitute,
                new StringWrapper(),
                result);

            // Act
            var (status, filename) = await sut.GetSuiteVersionDebianFileName(suiteVersion);

            // Assert
            status.Should().Be(OperationStatus.Success);
            filename.Should().Be(expectedFileName);
        }
    }
}
