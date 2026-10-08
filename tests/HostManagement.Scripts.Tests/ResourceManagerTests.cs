using System.IO.Abstractions;
using System.Text;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace HostManagement.Scripts.Tests;

[Collection("Non Parallel Tests")]
public class ResourceManagerTests
{
    private const string ScriptsDirectoryLocation = "HostManagement.Scripts.Resources.Scripts";
    private const string ScriptsDirectoryName = "Scripts";
    private const string TargetsDirectory = "targets";

    // Environments
    private const string Unknown = "unknown";
    private const string Debian = "debian";
    private const string EdgeSViciOne = "edges-vicione";

    private const string NormalizedDebian = Debian;
    private const string NormalizedEdgeSViciOne = "edges_vicione";

    private readonly string _testResourceDeployDirectory = Path.Combine("var", "lib", "hostmanagement");

    public class IsDeploymentInProgressProperty : ResourceManagerTests
    {
        [Fact]
        public void ShouldBeFalse_AfterDeploymentFailure()
        {
            // Arrange
            ResourceManager.ResetInstance();

            var (logger, fileSystem, directoryInfo, resourceHelper, processWrapper) = CreateSubstitutes();

            var scriptFile = "flag.sh";
            var scriptContentBytes = Encoding.UTF8.GetBytes("flag content");
            var resourceDeployDirectoryStructure = GetDeployDirectoryStructure(_testResourceDeployDirectory);
            var scriptTargetFilePath = Path.Combine(resourceDeployDirectoryStructure, scriptFile);
            var resourceLocation = BuildEnvironmentResource(scriptFile, NormalizedEdgeSViciOne);

            SetupFilteredManifestResourceNames(resourceHelper, resourceLocation);
            SetupDirectoryCreation(fileSystem, directoryInfo, resourceDeployDirectoryStructure);
            SetupSuccessfulSingleScriptDeployment(
                resourceHelper,
                fileSystem,
                resourceLocation,
                scriptContentBytes,
                scriptTargetFilePath);

            // chmod returns error
            SetupProcessWrapperFailure(processWrapper);

            ResourceManager.Initialize(
                logger,
                fileSystem,
                resourceHelper,
                processWrapper,
                _testResourceDeployDirectory,
                EdgeSViciOne);

            // Act
            var result = ResourceManager.GetInstance().DeployScripts();

            // Assert
            result.Should().BeFalse();
            ResourceManager.GetInstance().IsDeploymentInProgress.Should().BeFalse();
        }
    }

    public class GetInstance : ResourceManagerTests
    {
        [Fact]
        public void ShouldThrow_WhenInstanceIsNull()
        {
            // Arrange
            ResourceManager.ResetInstance();

            // Act
            var result = ResourceManager.GetInstance;

            // Assert
            result.Should().Throw<InvalidOperationException>()
                .WithMessage("ResourceManager is not initialized, use Initialize first.");
        }
    }

    public class Initialize : ResourceManagerTests
    {
        [Fact]
        public void ShouldInitializeResourceManager_WhenCalledWithValidParameters()
        {
            // Arrange
            ResourceManager.ResetInstance();

            var (logger, fileSystem, _, resourceHelper, processWrapper) = CreateSubstitutes();

            // Act
            ResourceManager.Initialize(
                logger,
                fileSystem,
                resourceHelper,
                processWrapper,
                _testResourceDeployDirectory,
                EdgeSViciOne);

            // Assert
            ResourceManager.GetInstance().Should().NotBeNull();
        }

        [Fact]
        public void ShouldNormalizeTargetEnvironmentString_WhenInitializedWithHyphenatedEnvironment()
        {
            // Arrange
            ResourceManager.ResetInstance();

            var (logger, fileSystem, directoryInfo, resourceHelper, processWrapper) = CreateSubstitutes();

            var file = "file.sh";
            var fileContent = "file content";
            var fileContentBytes = Encoding.UTF8.GetBytes(fileContent);
            var resourceDeployDirectoryStructure = Path.Combine(_testResourceDeployDirectory, ScriptsDirectoryName);
            var targetFilePath = Path.Combine(resourceDeployDirectoryStructure, file);
            var resourceLocation = BuildEnvironmentResource(file, NormalizedEdgeSViciOne);

            SetupFilteredManifestResourceNames(resourceHelper, resourceLocation);
            SetupDirectoryCreation(fileSystem, directoryInfo, resourceDeployDirectoryStructure);
            SetupSuccessfulSingleScriptDeployment(
                resourceHelper,
                fileSystem,
                resourceLocation,
                fileContentBytes,
                targetFilePath);

            SetupProcessWrapperSuccess(processWrapper);

            // Act
            ResourceManager.Initialize(
                logger,
                fileSystem,
                resourceHelper,
                processWrapper,
                _testResourceDeployDirectory,
                EdgeSViciOne);

            var deployResult = ResourceManager.GetInstance().DeployScripts();

            // Assert
            deployResult.Should().BeTrue();
            fileSystem.Directory.Received(1).CreateDirectory(resourceDeployDirectoryStructure);
            fileSystem.File.Received(1).CreateText(targetFilePath);

            // Make sure no 'targets' directory or normalized environment has been created
            fileSystem.Directory
                .DidNotReceive()
                .CreateDirectory(Arg.Is<string>(path =>
                    path.Contains(TargetsDirectory, StringComparison.Ordinal) ||
                    path.Contains(NormalizedEdgeSViciOne, StringComparison.Ordinal)));
        }

        [Fact]
        public void ShouldHaveNoResources_WhenInitializedWithUnknownEnvironment()
        {
            // Arrange
            ResourceManager.ResetInstance();

            var (logger, fileSystem, _, _, processWrapper) = CreateSubstitutes();

            // Act
            ResourceManager.Initialize(
                logger,
                fileSystem,
                new ResourceHelper(),
                processWrapper,
                _testResourceDeployDirectory,
                Unknown);

            // Assert
            var instance = ResourceManager.GetInstance();
            instance.Should().NotBeNull();
            instance.ResourceDictionary.Should().BeEmpty();
        }

        [Theory]
        [MemberData(nameof(EmbeddedResourceTestData))]
        public void ShouldHaveCorrectResourceKeysAndValues_WhenInitialized(EmbeddedResourceFile embeddedResource)
        {
            // Arrange
            ResourceManager.ResetInstance();

            var (logger, fileSystem, _, resourceHelper, processWrapper) = CreateSubstitutes();

            var testEmbeddedResourceContent = "test content";
            var testEmbeddedResourceContentBytes = Encoding.UTF8.GetBytes(testEmbeddedResourceContent);
            var testResourceDeployDirectoryStructure = Path.Combine(_testResourceDeployDirectory, embeddedResource.DirectoryStructure);
            var targetFilePath = Path.Combine(testResourceDeployDirectoryStructure, embeddedResource.FileNameAndExtension);

            resourceHelper.GetManifestResourceNames(Arg.Any<Func<string, bool>?>()).Returns([embeddedResource.Location]);

            // Act
            ResourceManager.Initialize(
                logger,
                fileSystem,
                resourceHelper,
                processWrapper,
                _testResourceDeployDirectory,
                EdgeSViciOne);

            // Assert
            var instance = ResourceManager.GetInstance();
            instance.Should().NotBeNull();
            instance.ResourceDictionary.Should().ContainKey(embeddedResource.FileName);
            instance.ResourceDictionary.TryGetValue(embeddedResource.FileName, out var resourceInfo).Should().BeTrue();
            resourceInfo.Should().NotBeNull();
            resourceInfo.DeployPath.Should().BeEquivalentTo(targetFilePath);
        }

        [Theory]
        [MemberData(nameof(DebianScriptNamesData))]
        [MemberData(nameof(EdgeSViciOneScriptNamesData))]
        public void ShouldHaveOnlyEnvironmentSpecificResources_WhenInitialized(
            (string, List<string>) targetEnvironmentScriptNamesData)
        {
            // Arrange
            ResourceManager.ResetInstance();

            var (logger, fileSystem, _, _, processWrapper) = CreateSubstitutes();
            var (targetEnvironmentString, scriptNames) = targetEnvironmentScriptNamesData;

            // Act
            ResourceManager.Initialize(
                logger,
                fileSystem,
                new ResourceHelper(),
                processWrapper,
                _testResourceDeployDirectory,
                targetEnvironmentString);

            // Assert
            var instance = ResourceManager.GetInstance();
            instance.Should().NotBeNull();
            instance.ResourceDictionary.Should().HaveSameCount(scriptNames).And.ContainKeys(scriptNames);
        }

        public static TheoryData<EmbeddedResourceFile> EmbeddedResourceTestData
            => [.. EnumerateEmbeddedResourceTestData()];

        public static IEnumerable<EmbeddedResourceFile> EnumerateEmbeddedResourceTestData()
        {
            const string Dir = "Dir";
            const string SubDir = "SubDir";
            const string File = "file";
            const string FileWithExtension = "file.sh";

            // Files under root
            yield return new()
            {
                Location = $"{ScriptsDirectoryLocation}.{File}",
                DirectoryStructure = ScriptsDirectoryName,
                FileName = File,
                FileNameAndExtension = File
            };

            yield return new()
            {
                Location = $"{ScriptsDirectoryLocation}.{FileWithExtension}",
                DirectoryStructure = ScriptsDirectoryName,
                FileName = FileWithExtension.Split('.').First(),
                FileNameAndExtension = FileWithExtension
            };

            // Files under root/dir
            yield return new()
            {
                Location = $"{ScriptsDirectoryLocation}.{Dir}.{File}",
                DirectoryStructure = Path.Combine(ScriptsDirectoryName, Dir),
                FileName = File,
                FileNameAndExtension = File
            };

            yield return new()
            {
                Location = $"{ScriptsDirectoryLocation}.{Dir}.{FileWithExtension}",
                DirectoryStructure = Path.Combine(ScriptsDirectoryName, Dir),
                FileName = FileWithExtension.Split('.').First(),
                FileNameAndExtension = FileWithExtension
            };

            // Files under root/dir/subdir
            yield return new()
            {
                Location = $"{ScriptsDirectoryLocation}.{Dir}.{SubDir}.{File}",
                DirectoryStructure = Path.Combine(ScriptsDirectoryName, Dir, SubDir),
                FileName = File,
                FileNameAndExtension = File
            };

            yield return new()
            {
                Location = $"{ScriptsDirectoryLocation}.{Dir}.{SubDir}.{FileWithExtension}",
                DirectoryStructure = Path.Combine(ScriptsDirectoryName, Dir, SubDir),
                FileName = FileWithExtension.Split('.').First(),
                FileNameAndExtension = FileWithExtension
            };
        }

        public static TheoryData<(string, List<string>)> DebianScriptNamesData =>
        [
            (
                Debian,
                new List<string>
                {
                    // Common
                    "get_system_architecture",
                    "set_execute_permission",
                    "set_pipe_ownership",

                    // System
                    "install_deb_package",

                    // SystemdUnit
                    "get_systemd_unit_active_state",
                    "get_systemd_unit_state",
                    "set_systemd_unit_state"
                }
            )
        ];

        public static TheoryData<(string, List<string>)> EdgeSViciOneScriptNamesData =>
        [
            (
                EdgeSViciOne,
                new List<string>
                {
                    // Common
                    "get_system_architecture",
                    "set_execute_permission",
                    "set_pipe_ownership",

                    // Network
                    "apply_netplan_configuration",
                    "get_network_interface_ipv4_dhcp_lease",
                    "get_network_interface_settings",
                    "get_network_interfaces_common_information",
                    "get_network_status_information",
                    "get_original_physical_address",
                    "renew_dhcp_lease",
                    "set_network_interface_settings",

                    // Network > DNS
                    "get_network_dns_settings",
                    "set_hostname",
                    "set_network_dns_settings",
                    "set_network_multicast_dns_settings",
                    "set_static_hosts",

                    // Network > Helper
                    "convert_ipv4_cidr_to_netmask",
                    "get_network_interfaces",
                    "get_network_vlan_name",
                    "is_dhcp_enabled",
                    "is_network_interface_available",

                    // Network > NTP
                    "get_network_ntp_fallback_information",
                    "get_network_ntp_settings",
                    "set_network_ntp_settings",

                    // Network > Proxy
                    "get_network_proxy_settings",
                    "set_network_proxy_settings",

                    // Syslog
                    "get_syslog_settings",
                    "set_syslog_settings",

                    // System
                    "install_deb_package",
                    "reset_system",
                    "shutdown_system",
                    "update_system",

                    // SystemdUnit
                    "get_systemd_unit_active_state",
                    "get_systemd_unit_state",
                    "set_systemd_unit_state"
                }
            )
        ];

        public sealed record EmbeddedResourceFile
        {
            /// <summary>
            /// The embedded resource file location, e.g.: 'HostManagement.Scripts.Resources.Scripts.Dir.SubDir.file.sh').
            /// </summary>
            public required string Location { get; init; }

            /// <summary>
            /// The relative directory structure of the embedded resource file (e.g.: '/Scripts/Dir/SubDir/')
            /// </summary>
            public required string DirectoryStructure { get; init; }

            /// <summary>
            /// The embedded resource file name (e.g.: 'file').
            /// </summary>
            public required string FileName { get; init; }

            /// <summary>
            /// The embedded resource file name and extension (e.g.: 'file.sh' or 'file' if no extension).
            /// </summary>
            public required string FileNameAndExtension { get; init; }
        }
    }

    public class GetResourcePath : ResourceManagerTests
    {
        [Fact]
        public void ShouldThrow_WhenResourceNameIsUnknown()
        {
            // Arrange
            ResourceManager.ResetInstance();

            var (logger, fileSystem, _, resourceHelper, processWrapper) = CreateSubstitutes();

            ResourceManager.Initialize(
                logger,
                fileSystem,
                resourceHelper,
                processWrapper,
                _testResourceDeployDirectory,
                EdgeSViciOne);

            var unknownResourceName = "unknownResource";
            var expectedMessage = $"Resource with name '{unknownResourceName}' is unknown.";

            // Act
            var result = () => ResourceManager.GetInstance().GetResourcePath(unknownResourceName);

            // Assert
            result.Should()
                .Throw<InvalidOperationException>()
                .WithMessage(expectedMessage);
        }

        [Fact]
        public void ShouldThrow_WhenResourceFileIsMissing()
        {
            // Arrange
            ResourceManager.ResetInstance();

            var (logger, fileSystem, _, resourceHelper, processWrapper) = CreateSubstitutes();

            var scriptName = "script";
            var scriptFile = "script.sh";
            var resourceLocation = BuildEnvironmentResource(scriptFile, NormalizedEdgeSViciOne);

            SetupFilteredManifestResourceNames(resourceHelper, resourceLocation);

            // No file shall exist, so exception is thrown later
            fileSystem.File.Exists(Arg.Any<string>()).Returns(false);

            ResourceManager.Initialize(
                logger,
                fileSystem,
                resourceHelper,
                processWrapper,
                _testResourceDeployDirectory,
                EdgeSViciOne);

            // Act
            var func = () => ResourceManager.GetInstance().GetResourcePath(scriptName);

            // Assert
            func.Should()
                .Throw<InvalidOperationException>()
                .WithMessage($"Resource with name '{scriptName}' could not be found.");
        }
    }

    public class DeployScripts : ResourceManagerTests
    {
        private const string EdgeScript = "edge_script.sh";
        private const string DebianScript = "debian_script.sh";
        private const string GlobalScript = "global.sh";

        [Fact]
        public void ShouldDeployOnlyMatchingEnvironmentScripts_WhenMultipleEnvironmentsPresent()
        {
            // Arrange
            ResourceManager.ResetInstance();

            var (logger, fileSystem, directoryInfo, resourceHelper, processWrapper) = CreateSubstitutes();

            var edgeScriptContentBytes = Encoding.UTF8.GetBytes("edge content");
            var resourceDeployDirectoryStructure = GetDeployDirectoryStructure(_testResourceDeployDirectory);
            var edgeScriptTargetFilePath = Path.Combine(resourceDeployDirectoryStructure, EdgeScript);

            var edgeResource = BuildEnvironmentResource(EdgeScript, NormalizedEdgeSViciOne);
            var debianResource = BuildEnvironmentResource(DebianScript, NormalizedDebian);
            var globalResource = $"{ScriptsDirectoryLocation}.{GlobalScript}"; // no environment > gets filtered out
            var allResources = new string[] { edgeResource, debianResource, globalResource };

            SetupFilteredManifestResourceNames(resourceHelper, allResources);
            SetupDirectoryCreation(fileSystem, directoryInfo, resourceDeployDirectoryStructure);
            SetupSuccessfulSingleScriptDeployment(
                resourceHelper,
                fileSystem,
                edgeResource,
                edgeScriptContentBytes,
                edgeScriptTargetFilePath);

            // For other resources no stream shall be requested
            resourceHelper.GetResourceStream(debianResource).Returns((Stream?)null);
            resourceHelper.GetResourceStream(globalResource).Returns((Stream?)null);

            SetupProcessWrapperSuccess(processWrapper);

            // Act
            ResourceManager.Initialize(
                logger,
                fileSystem,
                resourceHelper,
                processWrapper,
                _testResourceDeployDirectory,
                EdgeSViciOne);

            var deployResult = ResourceManager.GetInstance().DeployScripts();

            // Assert
            deployResult.Should().BeTrue();

            fileSystem.Directory.Received(1).CreateDirectory(resourceDeployDirectoryStructure);

            // edge-specific script created
            fileSystem.File.Received(1).CreateText(edgeScriptTargetFilePath);

            // Other environment scripts were not created and global scripts have been filtered out
            fileSystem.File
                .DidNotReceive()
                .CreateText(Arg.Is<string>(path => path.EndsWith(DebianScript) || path.EndsWith(GlobalScript)));
        }

        [Fact]
        public void ShouldNotDeployScripts_WhenTargetEnvironmentIsUnknown()
        {
            // Arrange
            ResourceManager.ResetInstance();

            var (logger, fileSystem, _, resourceHelper, processWrapper) = CreateSubstitutes();

            var resourceDeployDirectoryStructure = GetDeployDirectoryStructure(_testResourceDeployDirectory);

            var edgeResource = BuildEnvironmentResource(EdgeScript, NormalizedEdgeSViciOne);
            var debianResource = BuildEnvironmentResource(DebianScript, NormalizedDebian);
            var globalResource = $"{ScriptsDirectoryLocation}.{GlobalScript}";
            var allResources = new[] { edgeResource, debianResource, globalResource };

            fileSystem.Directory.Exists(resourceDeployDirectoryStructure).Returns(false);

            SetupFilteredManifestResourceNames(resourceHelper, allResources);

            // No resource streams shall be requested
            resourceHelper.GetResourceStream(Arg.Any<string>()).Returns((Stream?)null);

            // Act
            ResourceManager.Initialize(
                logger,
                fileSystem,
                resourceHelper,
                processWrapper,
                _testResourceDeployDirectory,
                Unknown);

            var deployResult = ResourceManager.GetInstance().DeployScripts();

            // Assert
            deployResult.Should().BeTrue(); // Deployment should be ok, but no scripts actually deployed

            resourceHelper.Received(1).GetManifestResourceNames(Arg.Any<Func<string, bool>?>());

            fileSystem.Directory.DidNotReceive().CreateDirectory(Arg.Any<string>());
            fileSystem.File.DidNotReceive().CreateText(Arg.Any<string>());

            processWrapper.DidNotReceive().Start(Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public void ShouldCleanUpDirectory_WhenRedeployIsRequested()
        {
            // Arrange
            ResourceManager.ResetInstance();

            var (logger, fileSystem, directoryInfo, resourceHelper, processWrapper) = CreateSubstitutes();

            var scriptFile = "redeploy.sh";
            var scriptContentBytes = Encoding.UTF8.GetBytes("content");
            var resourceDeployDirectoryStructure = GetDeployDirectoryStructure(_testResourceDeployDirectory);
            var targetScriptFilePath = Path.Combine(resourceDeployDirectoryStructure, scriptFile);
            var resourceLocation = BuildEnvironmentResource(scriptFile, NormalizedEdgeSViciOne);

            SetupFilteredManifestResourceNames(resourceHelper, resourceLocation);

            // Resources shall exist -> Cleanup should be called...
            fileSystem.Directory.Exists(_testResourceDeployDirectory).Returns(true);

            // ...after that no resources are deployed anymore, so create directory
            SetupDirectoryCreation(fileSystem, directoryInfo, resourceDeployDirectoryStructure);
            SetupSuccessfulSingleScriptDeployment(
                resourceHelper,
                fileSystem,
                resourceLocation,
                scriptContentBytes,
                targetScriptFilePath);

            SetupProcessWrapperSuccess(processWrapper);

            ResourceManager.Initialize(
                logger,
                fileSystem,
                resourceHelper,
                processWrapper,
                _testResourceDeployDirectory,
                EdgeSViciOne);

            // Act
            var deployResult = ResourceManager.GetInstance().DeployScripts(isRedeployment: true);

            // Assert
            deployResult.Should().BeTrue();

            fileSystem.Directory.Received(1).Delete(_testResourceDeployDirectory, true);
            fileSystem.Directory.Received(1).CreateDirectory(resourceDeployDirectoryStructure);
        }

        [Fact]
        public void ShouldReturnFalse_WhenSettingExecutePermissionFails()
        {
            // Arrange
            ResourceManager.ResetInstance();

            var (logger, fileSystem, directoryInfo, resourceHelper, processWrapper) = CreateSubstitutes();

            var scriptFile = "perm.sh";
            var scriptContentBytes = Encoding.UTF8.GetBytes("perm content");
            var resourceDeployDirectoryStructure = GetDeployDirectoryStructure(_testResourceDeployDirectory);
            var targetScriptFilePath = Path.Combine(resourceDeployDirectoryStructure, scriptFile);
            var resourceLocation = BuildEnvironmentResource(scriptFile, NormalizedEdgeSViciOne);

            SetupFilteredManifestResourceNames(resourceHelper, resourceLocation);
            SetupDirectoryCreation(fileSystem, directoryInfo, resourceDeployDirectoryStructure);
            SetupSuccessfulSingleScriptDeployment(
                resourceHelper,
                fileSystem,
                resourceLocation,
                scriptContentBytes,
                targetScriptFilePath);

            // chmod returns error
            SetupProcessWrapperFailure(processWrapper);

            ResourceManager.Initialize(
                logger,
                fileSystem,
                resourceHelper,
                processWrapper,
                _testResourceDeployDirectory,
                EdgeSViciOne);

            // Act
            var result = ResourceManager.GetInstance().DeployScripts();

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldSkipDeployment_WhenScriptAlreadyExists()
        {
            // Arrange
            ResourceManager.ResetInstance();

            var (logger, fileSystem, _, resourceHelper, processWrapper) = CreateSubstitutes();

            var scriptFile = "exists.sh";
            var resourceDeployDirectoryStructure = GetDeployDirectoryStructure(_testResourceDeployDirectory);
            var deployPath = Path.Combine(resourceDeployDirectoryStructure, scriptFile);
            var resourceLocation = BuildEnvironmentResource(scriptFile, NormalizedEdgeSViciOne);

            SetupFilteredManifestResourceNames(resourceHelper, resourceLocation);

            // Resources shall be deployed already...
            fileSystem.Directory.Exists(Arg.Any<string>()).Returns(true);
            fileSystem.File.Exists(deployPath).Returns(true);

            ResourceManager.Initialize(
                logger,
                fileSystem,
                resourceHelper,
                processWrapper,
                _testResourceDeployDirectory,
                EdgeSViciOne);

            // Act
            var result = ResourceManager.GetInstance().DeployScripts();

            // Assert
            result.Should().BeTrue();
            fileSystem.File.DidNotReceive().CreateText(Arg.Any<string>());
        }

        [Fact]
        public void ShouldThrow_WhenDuplicateResourceNamesDetected()
        {
            // Arrange
            ResourceManager.ResetInstance();

            var (logger, fileSystem, _, resourceHelper, processWrapper) = CreateSubstitutes();

            var resourceLocation = BuildEnvironmentResource("dup.sh", NormalizedEdgeSViciOne);

            // Resource names returns duplicates
            resourceHelper
                .GetManifestResourceNames(Arg.Any<Func<string, bool>?>())
                .Returns([resourceLocation, resourceLocation]);

            // Act
            var func = () => ResourceManager.Initialize(
                logger,
                fileSystem,
                resourceHelper,
                processWrapper,
                _testResourceDeployDirectory,
                EdgeSViciOne);

            // Assert
            func.Should()
                .Throw<InvalidOperationException>()
                .WithMessage($"Could not add resource '{resourceLocation}' with key*");
        }
    }

    private static (
        ILogger logger,
        IFileSystem fileSystem,
        IDirectoryInfo directoryInfo,
        IResourceHelper resourceHelper,
        IProcessWrapper processWrapper)
        CreateSubstitutes()
    {
        return (Substitute.For<ILogger>(),
                Substitute.For<IFileSystem>(),
                Substitute.For<IDirectoryInfo>(),
                Substitute.For<IResourceHelper>(),
                Substitute.For<IProcessWrapper>());
    }

    private static string BuildEnvironmentResource(string scriptFile, string normalizedEnvironment)
        => $"{ScriptsDirectoryLocation}.{TargetsDirectory}.{normalizedEnvironment}.{scriptFile}";

    private static string GetDeployDirectoryStructure(string baseDeployDirectory)
        => Path.Combine(baseDeployDirectory, ScriptsDirectoryName);

    private static void SetupFilteredManifestResourceNames(IResourceHelper helper, params string[] resources)
    {
        // Resource names (filtered via predicate)
        helper
            .GetManifestResourceNames(Arg.Any<Func<string, bool>?>())
            .Returns(callInfo =>
            {
                var predicate = callInfo.Arg<Func<string, bool>?>();

                return predicate is null
                    ? resources
                    : [.. resources.Where(resource => predicate(resource))];
            });
    }

    private static void SetupDirectoryCreation(
        IFileSystem fileSystem,
        IDirectoryInfo directoryInfo,
        string deployDirectoryStructure)
    {
        // No resources shall be deployed yet and no directory structure shall exist...
        fileSystem.Directory.Exists(deployDirectoryStructure).Returns(false);

        // ...so CreateDirectory shall return a new IDirectoryInfo for each created directory
        directoryInfo.Exists.Returns(true);
        directoryInfo.FullName.Returns(deployDirectoryStructure);
        directoryInfo.Name.Returns(ScriptsDirectoryName);

        fileSystem.Directory.CreateDirectory(deployDirectoryStructure).Returns(directoryInfo);
    }

    private static void SetupSuccessfulSingleScriptDeployment(
        IResourceHelper resourceHelper,
        IFileSystem fileSystem,
        string resourceLocation,
        byte[] scriptContentBytes,
        string targetFilePath)
    {
        // GetResourceStream shall return the resource stream of the resource...
        resourceHelper.GetResourceStream(resourceLocation).Returns(new MemoryStream(scriptContentBytes));

        // ...and deploy it
        fileSystem.File.Exists(targetFilePath).Returns(false);

        fileSystem.File
            .CreateText(targetFilePath)
            .Returns(_ => new StreamWriter(new MemoryStream(), Encoding.UTF8, 16, true));
    }

    protected static void SetupProcessWrapperSuccess(IProcessWrapper processWrapper)
    {
        processWrapper
            .Start(Arg.Any<string>(), Arg.Any<string>())
            .Returns(new ScriptResponse(0, "", ""));
    }

    protected static void SetupProcessWrapperFailure(
        IProcessWrapper processWrapper,
        int exitCode = 42,
        string error = "error")
    {
        processWrapper
            .Start(Arg.Any<string>(), Arg.Any<string>())
            .Returns(new ScriptResponse(exitCode, "", error));
    }
}
