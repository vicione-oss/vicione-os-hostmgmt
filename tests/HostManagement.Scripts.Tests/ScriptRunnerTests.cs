using System.Diagnostics;
using System.IO.Abstractions;
using System.Text;
using AwesomeAssertions;
using HostManagement.Scripts.Enums;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace HostManagement.Scripts.Tests;

[Collection("Non Parallel Tests")]
public class ScriptRunnerTests
{
    private const string ScriptsDirectoryName = "Scripts";
    private const string ScriptsDirectory = "HostManagement.Scripts.Resources.Scripts";

    private const string EdgeSViciOne = "edges-vicione";

    private static readonly string s_testResourceDeployDirectory = Path.Combine("/tmp", "HostManagement_Resources");

    public class GetInstance : ScriptRunnerTests
    {
        [Fact]
        public void ShouldThrow_WhenInstanceIsNull()
        {
            // Arrange
            ScriptRunner.ResetInstance();

            // Act
            var result = ScriptRunner.GetInstance;

            // Assert
            result.Should().Throw<InvalidOperationException>()
                .WithMessage("ScriptRunner is not initialized, use Initialize first.");
        }
    }

    public class Initialize : ScriptRunnerTests
    {
        [Fact]
        public void ShouldInitializeScriptRunner_WhenCalledWithValidParameters()
        {
            // Arrange
            ScriptRunner.ResetInstance();

            // Act
            ScriptRunner.Initialize(s_testResourceDeployDirectory);

            // Assert
            ScriptRunner.GetInstance().Should().NotBeNull();
        }
    }

    public class DefaultTimeoutInMillisecondsProperty : ScriptRunnerTests
    {
        [Fact]
        public void ShouldReturnDefaultTimeoutInMilliseconds_WhenCalled()
        {
            // Arrange
            ScriptRunner.ResetInstance();

            // Act + Assert
            ScriptRunner.DefaultTimeoutInMilliseconds.Should().Be(60_000);
        }
    }

    public class Run : ScriptRunnerTests
    {
        private static readonly string s_testEmbeddedResourceName = ScriptIdentifier.UnitTesting.ToString();
        private static readonly string s_testEmbeddedResourceFile = string.Concat(s_testEmbeddedResourceName, ".sh");
        private static readonly string s_testEmbeddedResourceLocation = string.Concat(ScriptsDirectory, ".", s_testEmbeddedResourceFile);
        private static readonly string s_testEmbeddedResourceContent = "echo HelloWorld!";
        private static readonly byte[] s_testEmbeddedResourceContentBytes = Encoding.UTF8.GetBytes(s_testEmbeddedResourceContent);
        private static readonly string s_testResourceDeployDirectoryStructure = Path.Combine(s_testResourceDeployDirectory, ScriptsDirectoryName);

        [Fact]
        public async Task ShouldUseDefaultTimeout_WhenTimeoutLessThanDefault()
        {
            // Arrange
            var (logger, fileSystem, resourceHelper, processWrapper) = CreateSubstitutes();
            var timeout = 30_000;

            var expectedTimeout = ScriptRunner.DefaultTimeoutInMilliseconds;

            ResetSingletonsAndInitializeSubstitutes(fileSystem, resourceHelper);

            processWrapper
                .StartScriptAsync(Arg.Any<ProcessStartInfo>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
                .Returns(new ScriptResponse(0, "HelloWorld!", ""));

            ScriptRunner.Initialize(s_testResourceDeployDirectory);

            ResourceManager.Initialize(
                logger,
                fileSystem,
                resourceHelper,
                processWrapper,
                s_testResourceDeployDirectory,
                EdgeSViciOne);

            // Act
            var response = await ScriptRunner
                .GetInstance()
                .Run(timeout, ScriptIdentifier.UnitTesting, processWrapper, CancellationToken.None);

            // Assert
            response.Should().NotBeNull();
            response.ExitCode.Should().Be(0);
            response.StandardOutput.Should().Be("HelloWorld!");
            response.StandardError.Should().BeEmpty();

            await processWrapper.Received(1)
                .StartScriptAsync(Arg.Any<ProcessStartInfo>(), expectedTimeout, Arg.Any<CancellationToken>());
        }

        [Theory]
        [InlineData(60_000)]
        [InlineData(60_001)]
        [InlineData(3_600_000)]
        public async Task ShouldUseCustomTimeout_WhenTimeoutGreaterOrEqualThanDefault(int timeout)
        {
            // Arrange
            var (logger, fileSystem, resourceHelper, processWrapper) = CreateSubstitutes();

            ResetSingletonsAndInitializeSubstitutes(fileSystem, resourceHelper);

            processWrapper
                .StartScriptAsync(Arg.Any<ProcessStartInfo>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
                .Returns(new ScriptResponse(0, "HelloWorld!", ""));

            ScriptRunner.Initialize(s_testResourceDeployDirectory);

            ResourceManager.Initialize(
                logger,
                fileSystem,
                resourceHelper,
                processWrapper,
                s_testResourceDeployDirectory,
                EdgeSViciOne);

            // Act
            var response = await ScriptRunner
                .GetInstance()
                .Run(timeout, ScriptIdentifier.UnitTesting, processWrapper, CancellationToken.None);

            // Assert
            response.Should().NotBeNull();
            response.ExitCode.Should().Be(0);
            response.StandardOutput.Should().Be("HelloWorld!");
            response.StandardError.Should().BeEmpty();

            await processWrapper.Received(1)
                .StartScriptAsync(Arg.Any<ProcessStartInfo>(), timeout, Arg.Any<CancellationToken>());
        }

        [Theory]
        [InlineData(ScriptIdentifier.SetExecutePermission, "set_execute_permission")]
        [InlineData(ScriptIdentifier.CheckUserExists, "check_user_exists")]
        [InlineData(ScriptIdentifier.CheckGroupExists, "check_group_exists")]
        [InlineData(ScriptIdentifier.SetOwnership, "set_ownership")]
        [InlineData(ScriptIdentifier.CommonSystemArchitectureGet, "get_system_architecture")]
        [InlineData(ScriptIdentifier.ResetSystem, "reset_system")]
        [InlineData(ScriptIdentifier.ShutdownSystem, "shutdown_system")]
        [InlineData(ScriptIdentifier.UpdateSystem, "update_system")]
        [InlineData(ScriptIdentifier.SystemdUnitActiveStateGet, "get_systemd_unit_active_state")]
        [InlineData(ScriptIdentifier.SystemdUnitStateGet, "get_systemd_unit_state")]
        [InlineData(ScriptIdentifier.SystemdUnitStateSet, "set_systemd_unit_state")]
        [InlineData(ScriptIdentifier.ApplyNetplanConfiguration, "apply_netplan_configuration")]
        [InlineData(ScriptIdentifier.NetworkInterfacesGet, "get_network_interfaces")]
        [InlineData(ScriptIdentifier.OriginalPhysicalAddressGet, "get_original_physical_address")]
        [InlineData(ScriptIdentifier.NetworkInterfacesCommonInformationGet, "get_network_interfaces_common_information")]
        [InlineData(ScriptIdentifier.NetworkInterfaceIPv4SettingsGet, "get_network_interface_ipv4_settings")]
        [InlineData(ScriptIdentifier.NetworkInterfaceIPv4SettingsSet, "set_network_interface_ipv4_settings")]
        [InlineData(ScriptIdentifier.NetworkInterfaceIPv4DHCPLeaseGet, "get_network_interface_ipv4_dhcp_lease")]
        [InlineData(ScriptIdentifier.RenewDHCPLease, "renew_dhcp_lease")]
        [InlineData(ScriptIdentifier.NetworkStatusInformationGet, "get_network_status_information")]
        [InlineData(ScriptIdentifier.HostnameSet, "set_hostname")]
        [InlineData(ScriptIdentifier.NetworkStaticHostsSet, "set_static_hosts")]
        [InlineData(ScriptIdentifier.NetworkMulticastDNSSettingsSet, "set_network_multicast_dns_settings")]
        [InlineData(ScriptIdentifier.NetworkDNSSettingsGet, "get_network_dns_settings")]
        [InlineData(ScriptIdentifier.NetworkDNSSettingsSet, "set_network_dns_settings")]
        [InlineData(ScriptIdentifier.NetworkProxySettingsGet, "get_network_proxy_settings")]
        [InlineData(ScriptIdentifier.NetworkProxySettingsSet, "set_network_proxy_settings")]
        [InlineData(ScriptIdentifier.NetworkNTPFallbackInformationGet, "get_network_ntp_fallback_information")]
        [InlineData(ScriptIdentifier.NetworkNTPSettingsGet, "get_network_ntp_settings")]
        [InlineData(ScriptIdentifier.NetworkNTPSettingsSet, "set_network_ntp_settings")]
        [InlineData(ScriptIdentifier.ViciOneSuiteInstalledVersionGet, "get_vicione_suite_installed_version")]
        [InlineData(ScriptIdentifier.ViciOneSuiteVersionsGet, "get_vicione_suite_versions")]
        [InlineData(ScriptIdentifier.ViciOneSuiteDebFileGet, "get_vicione_suite_deb_file")]
        [InlineData(ScriptIdentifier.ViciOneSuiteInstallDebFile, "install_deb_file")]
        [InlineData(ScriptIdentifier.InstallDebianPackage, "install_deb_package")]
        public async Task ShouldGetScriptContent_WhenRunCalled(
            ScriptIdentifier scriptIdentifier,
            string resourceName)
        {
            // Arrange
            var (logger, fileSystem, resourceHelper, processWrapper) = CreateSubstitutes();

            var testEmbeddedResourceFile = string.Concat(resourceName, ".sh");
            var testEmbeddedResourceLocation = string.Concat(ScriptsDirectory, ".", testEmbeddedResourceFile);
            var testEmbeddedResourceContent = resourceName;
            var testEmbeddedResourceContentBytes = Encoding.UTF8.GetBytes(testEmbeddedResourceContent);
            var testResourceDeployDirectoryStructure = Path.Combine(s_testResourceDeployDirectory, ScriptsDirectoryName);

            ResetSingletonsAndInitializeSubstitutes(
                fileSystem,
                resourceHelper,
                testEmbeddedResourceLocation,
                testResourceDeployDirectoryStructure,
                testEmbeddedResourceFile,
                testEmbeddedResourceContentBytes);

            processWrapper
                .StartScriptAsync(Arg.Any<ProcessStartInfo>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
                .Returns(new ScriptResponse(0, scriptIdentifier.ToString(), ""));

            ScriptRunner.Initialize(s_testResourceDeployDirectory);

            ResourceManager.Initialize(
                logger,
                fileSystem,
                resourceHelper,
                processWrapper,
                s_testResourceDeployDirectory,
                EdgeSViciOne);

            // Act
            var response = await ScriptRunner
                .GetInstance()
                .Run(60_000, scriptIdentifier, processWrapper, CancellationToken.None);

            // Assert
            response.Should().NotBeNull();
            response.ExitCode.Should().Be(0);
            response.StandardOutput.Should().Be(scriptIdentifier.ToString());
            response.StandardError.Should().BeEmpty();
        }

        [Fact]
        public async Task ShouldThrow_WhenGetScriptContentCalledWithUnknownIdentifier()
        {
            // Arrange
            var (logger, fileSystem, resourceHelper, processWrapper) = CreateSubstitutes();

            var invalidScriptIdentifier = (ScriptIdentifier)(-1);
            var expectedExceptionMessage = $"Unknown script identifier '{invalidScriptIdentifier}'.";

            ResetSingletonsAndInitializeSubstitutes(fileSystem, resourceHelper);

            ScriptRunner.Initialize(s_testResourceDeployDirectory);

            ResourceManager.Initialize(
                logger,
                fileSystem,
                resourceHelper,
                processWrapper,
                s_testResourceDeployDirectory,
                EdgeSViciOne);

            // Act
            var result = () => ScriptRunner
                .GetInstance()
                .Run(60_000, invalidScriptIdentifier, processWrapper, CancellationToken.None);

            // Assert
            await result.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage(expectedExceptionMessage);
        }

        [Fact]
        public async Task ShouldThrow_WhenGetScriptContentReturnsEmptyString()
        {
            // Arrange
            var (logger, fileSystem, resourceHelper, processWrapper) = CreateSubstitutes();

            var expectedExceptionMessage =
                $"Script content for script with identifier '{ScriptIdentifier.UnitTesting}' was empty.";

            ResetSingletonsAndInitializeSubstitutes(fileSystem, resourceHelper);

            fileSystem.File
                .ReadAllBytes(Path.Combine(s_testResourceDeployDirectoryStructure, s_testEmbeddedResourceFile))
                .Returns([]);

            ScriptRunner.Initialize(s_testResourceDeployDirectory);

            ResourceManager.Initialize(
                logger,
                fileSystem,
                resourceHelper,
                processWrapper,
                s_testResourceDeployDirectory,
                EdgeSViciOne);

            // Act
            var result = () => ScriptRunner
                .GetInstance()
                .Run(60_000, ScriptIdentifier.UnitTesting, processWrapper, CancellationToken.None);

            // Assert
            await result.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage(expectedExceptionMessage);
        }

        [Fact]
        public async Task ShouldReturnScriptResponse_WhenRunCalled()
        {
            // Arrange
            var (logger, fileSystem, resourceHelper, processWrapper) = CreateSubstitutes();

            ResetSingletonsAndInitializeSubstitutes(fileSystem, resourceHelper);

            processWrapper
                .StartScriptAsync(Arg.Any<ProcessStartInfo>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
                .Returns(new ScriptResponse(0, "HelloWorld!", ""));

            ScriptRunner.Initialize(s_testResourceDeployDirectory);

            ResourceManager.Initialize(
                logger,
                fileSystem,
                resourceHelper,
                processWrapper,
                s_testResourceDeployDirectory,
                EdgeSViciOne);

            // Act
            var response = await ScriptRunner
                .GetInstance()
                .Run(60_000, ScriptIdentifier.UnitTesting, processWrapper, CancellationToken.None);

            // Assert
            response.Should().NotBeNull();
            response.ExitCode.Should().Be(0);
            response.StandardOutput.Should().Be("HelloWorld!");
            response.StandardError.Should().BeEmpty();
        }

        [Fact]
        public async Task ShouldReturnScriptErrorResponse_WhenProcessWrapperStartExceptionOccurred()
        {
            // Arrange
            var (logger, fileSystem, resourceHelper, processWrapper) = CreateSubstitutes();

            var expectedScriptResponse =
                $"Failed to start process for script with identifier '{ScriptIdentifier.UnitTesting}'.";

            ResetSingletonsAndInitializeSubstitutes(fileSystem, resourceHelper);

            processWrapper
                .StartScriptAsync(Arg.Any<ProcessStartInfo>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
                .ThrowsAsync<ProcessWrapperStartException>();

            ScriptRunner.Initialize(s_testResourceDeployDirectory);

            ResourceManager.Initialize(
                logger,
                fileSystem,
                resourceHelper,
                processWrapper,
                s_testResourceDeployDirectory,
                EdgeSViciOne);

            // Act
            var response = await ScriptRunner
                .GetInstance()
                .Run(60_000, ScriptIdentifier.UnitTesting, processWrapper, CancellationToken.None);

            // Assert
            response.Should().NotBeNull();
            response.ExitCode.Should().Be(1);
            response.StandardOutput.Should().BeEmpty();
            response.StandardError.Should()
                .Contain(expectedScriptResponse).And
                .Contain("ProcessWrapperStartException");
        }

        [Fact]
        public async Task ShouldReturnScriptErrorResponse_WhenTimeoutExceptionOccurred()
        {
            // Arrange
            var (logger, fileSystem, resourceHelper, processWrapper) = CreateSubstitutes();
            var timeout = 90_000;

            var expectedScriptResponse =
                $"Script with identifier '{ScriptIdentifier.UnitTesting}' exceeded the timeout of " +
                $"{timeout:N0} milliseconds and was aborted.";

            ResetSingletonsAndInitializeSubstitutes(fileSystem, resourceHelper);

            processWrapper
                .StartScriptAsync(Arg.Any<ProcessStartInfo>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
                .ThrowsAsync<TimeoutException>();

            ScriptRunner.Initialize(s_testResourceDeployDirectory);

            ResourceManager.Initialize(
                logger,
                fileSystem,
                resourceHelper,
                processWrapper,
                s_testResourceDeployDirectory,
                EdgeSViciOne);

            // Act
            var response = await ScriptRunner
                .GetInstance()
                .Run(timeout, ScriptIdentifier.UnitTesting, processWrapper, CancellationToken.None);

            // Assert
            response.Should().NotBeNull();
            response.ExitCode.Should().Be(1);
            response.StandardOutput.Should().BeEmpty();
            response.StandardError.Should().Contain(expectedScriptResponse);
        }

        [Fact]
        public async Task ShouldReturnScriptErrorResponse_WhenOperationCanceledExceptionOccurred()
        {
            // Arrange
            var (logger, fileSystem, resourceHelper, processWrapper) = CreateSubstitutes();

            var expectedScriptResponse = $"Script with identifier '{ScriptIdentifier.UnitTesting}' was cancelled.";

            ResetSingletonsAndInitializeSubstitutes(fileSystem, resourceHelper);

            ScriptRunner.Initialize(s_testResourceDeployDirectory);

            ResourceManager.Initialize(
                logger,
                fileSystem,
                resourceHelper,
                processWrapper,
                s_testResourceDeployDirectory,
                EdgeSViciOne);

            // Act
            using var cts = new CancellationTokenSource();
            await cts.CancelAsync();

            var response = await ScriptRunner
                .GetInstance()
                .Run(60_000, ScriptIdentifier.UnitTesting, new ProcessWrapper(), cts.Token);

            // Assert
            response.Should().NotBeNull();
            response.ExitCode.Should().Be(1);
            response.StandardOutput.Should().BeEmpty();
            response.StandardError.Should().Contain(expectedScriptResponse);
        }

        private static (
            ILogger logger,
            IFileSystem fileSystem,
            IResourceHelper resourceHelper,
            IProcessWrapper processWrapper)
            CreateSubstitutes()
        {
            return (Substitute.For<ILogger>(),
                    Substitute.For<IFileSystem>(),
                    Substitute.For<IResourceHelper>(),
                    Substitute.For<IProcessWrapper>());
        }

        private static void ResetSingletonsAndInitializeSubstitutes(IFileSystem fileSystemSubstitute, IResourceHelper resourceHelperSubstitute)
        {
            ResetSingletonsAndInitializeSubstitutes(
                fileSystemSubstitute,
                resourceHelperSubstitute,
                s_testEmbeddedResourceLocation,
                s_testResourceDeployDirectoryStructure,
                s_testEmbeddedResourceFile,
                s_testEmbeddedResourceContentBytes);
        }

        private static void ResetSingletonsAndInitializeSubstitutes(
            IFileSystem fileSystemSubstitute,
            IResourceHelper resourceHelperSubstitute,
            string testEmbeddedResourceLocation,
            string testResourceDeployDirectoryStructure,
            string testEmbeddedResourceFile,
            byte[] testEmbeddedResourceContentBytes)
        {
            // Reset singleton instances
            ScriptRunner.ResetInstance();
            ResourceManager.ResetInstance();

            // Mock retrieving embedded resource location, so we can initialize ResourceManager
            resourceHelperSubstitute
                .GetManifestResourceNames(Arg.Any<Func<string, bool>?>())
                .Returns([testEmbeddedResourceLocation]);

            // Mock getting the required resource when ScriptRunner was called
            fileSystemSubstitute.File
                .Exists(Path.Combine(testResourceDeployDirectoryStructure, testEmbeddedResourceFile))
                .Returns(true);

            fileSystemSubstitute.File
                .ReadAllBytes(Path.Combine(testResourceDeployDirectoryStructure, testEmbeddedResourceFile))
                .Returns(testEmbeddedResourceContentBytes);
        }
    }
}
