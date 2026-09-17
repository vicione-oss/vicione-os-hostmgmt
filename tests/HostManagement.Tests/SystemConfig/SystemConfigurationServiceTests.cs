using System.Net;
using System.Net.NetworkInformation;
using AwesomeAssertions;
using HostManagement.Configuration;
using HostManagement.Shared.Capabilities;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Contracts;
using HostManagement.SystemConfig;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using InternalSystemConfiguration = HostManagement.SystemConfig.Contracts.SystemConfiguration;

namespace HostManagement.Tests.SystemConfig;

[Collection("Non Parallel Tests")]
public class SystemConfigurationServiceTests
{
    private const string Lan1 = "lan1";

    private const string DefaultConfigFile = "DefaultSystemConfiguration.json";

    private const string GetConfigFromScriptsFailed = "Get system configuration from scripts failed.";
    private const string GetConfigFromScriptsWarning = "Get system configuration from scripts completed with warnings.";

    private const string TestHostname = "test-host";
    private const string TestExceptionMessage = "Test exception";

    public SystemConfigurationServiceTests() => SystemConfigurationCache.Instance.ResetForTests();

    public void Dispose() => SystemConfigurationCache.Instance.ResetForTests();

    private static InternalSystemConfiguration CreateInternalConfig()
        => new()
        {
            Version = 2,
            NetworkDNSSettings = new()
            {
                Hostname = TestHostname,
                PrimaryDNSSuffix = new()
                {
                    Enabled = false,
                    Suffix = "company"
                }
            }
        };

    private static InternalSystemConfiguration CreateInternalDefaultConfigWithInterface(
        string interfaceName = Lan1,
        bool enabled = false,
        bool dhcpEnabled = false,
        string physicalAddress = "aa:bb:cc:dd:ee:ff")
        => new()
        {
            Version = 2,
            NetworkDNSSettings = new() { Hostname = TestHostname },
            NetworkInterfaces =
            [
                new()
                {
                    CommonInformation = new()
                    {
                        Name = interfaceName,
                        Enabled = enabled,
                        UserDefinedMACAddress = new()
                        {
                            Enabled = true,
                            Address = PhysicalAddress.Parse(physicalAddress)
                        }
                    },
                    IPv4 = new()
                    {
                        DHCPEnabled = dhcpEnabled,
                        Gateway = IPAddress.Parse("192.168.1.1"),
                        IPv4Details =
                        [
                            new()
                            {
                                IPAddress = IPAddress.Parse("192.168.1.2"),
                                Netmask = IPAddress.Parse("255.255.255.0")
                            }
                        ],
                    },
                    VLAN = new()
                    {
                        Enabled = true,
                        ID = 100
                    }
                }
            ]
        };

    private static SystemConfiguration CreateSharedConfig() => new()
    {
        Version = 2,
        NetworkDNSSettings = new()
        {
            Hostname = TestHostname,
            PrimaryDNSSuffix = new()
            {
                Enabled = false,
                Suffix = "company"
            }
        },
        NetworkInterfaces =
        [
            new()
            {
                CommonInformation = new()
                {
                    Name = Lan1,
                    Enabled = true
                },
                IPv4 = new()
                {
                    DHCPEnabled = true
                }
            }
        ]
    };

    public sealed class GetConfiguration : SystemConfigurationServiceTests
    {
        [Fact]
        public async Task ShouldReturnErrorResult_WhenScriptsFail()
        {
            var context = new SystemConfigurationServiceContext();
            context.ScriptHandler
                .ExecuteGetNetworkInterfacesCommonInformationScript(Arg.Any<ScriptOptions>())
                .ReturnsForAnyArgs((Result.ErrorResult, []));

            var result = await context.Sut.GetConfiguration(CancellationToken.None);

            result.Status.Should().Be(OperationStatus.Error);
            result.Configuration.Should().BeNull();
            result.Message.Should().Contain(GetConfigFromScriptsFailed);
        }

        [Fact]
        public async Task ShouldReturnWarningResult_WhenScriptsReturnWarning()
        {
            // Reset cache, so config is read again via scripts
            SystemConfigurationCache.Instance.ResetForTests();

            var context = new SystemConfigurationServiceContext();
            context.ScriptHandler
                .ExecuteGetNetworkInterfacesCommonInformationScript(Arg.Any<ScriptOptions>())
                .ReturnsForAnyArgs((Result.WarningResult, []));

            var result = await context.Sut.GetConfiguration(CancellationToken.None);

            result.Status.Should().Be(OperationStatus.Warning);
            result.Configuration.Should().NotBeNull();
            result.Message.Should().Contain(GetConfigFromScriptsWarning);
        }

        [Fact]
        public async Task ShouldReturnErrorResult_WhenExceptionThrown()
        {
            var context = new SystemConfigurationServiceContext();
            context.ScriptHandler
                .ExecuteGetNetworkInterfacesCommonInformationScript(Arg.Any<ScriptOptions>())
                .ThrowsAsync(new InvalidOperationException(TestExceptionMessage));

            var result = await context.Sut.GetConfiguration(CancellationToken.None);

            result.Status.Should().Be(OperationStatus.Error);
            result.Configuration.Should().BeNull();
            result.Message.Should().Contain(TestExceptionMessage);
        }

        [Fact]
        public async Task ShouldReturnSuccessResult_WhenScriptsSucceed()
        {
            var context = new SystemConfigurationServiceContext();

            var result = await context.Sut.GetConfiguration(CancellationToken.None);

            result.Status.Should().Be(OperationStatus.Success);
            result.Configuration.Should().NotBeNull();
            result.Message.Should().NotBeEmpty();
        }
    }

    public sealed class GetConfigurationOnDeviceStartup : SystemConfigurationServiceTests
    {
        [Fact]
        public async Task ShouldReturnWarning_WhenNoJsonFilesExists()
        {
            // Arrange
            var context = new SystemConfigurationServiceContext();
            context.Persistence.ExistsUserConfiguration().Returns(false);
            context.Persistence.GetUserConfigurationPath().Returns(AppPaths.SystemConfigurationFilePath);

            context.Persistence.ExistsPreparationConfiguration().Returns(false);
            context.Persistence.GetPreparationConfigurationPath().Returns(AppPaths.DefaultSystemConfigurationFilePath);

            // Act
            var result = await context.Sut.GetConfigurationOnDeviceStartup(CancellationToken.None);

            // Assert
            AssertFoundNoUserConfigLogReceived(context);
            AssertFoundNoPreparationConfigLogReceived(context);
            AssertGetJsonConfigNotPossibleLogReceived(context);

            result.Status.Should().Be(OperationStatus.Warning);
            result.Configuration.Should().BeNull();
        }

        [Fact]
        public async Task ShouldMarkUserConfigurationDefective_WhenLoadingUserConfigurationFails()
        {
            // Arrange
            var context = new SystemConfigurationServiceContext();
            context.Persistence.ExistsUserConfiguration().Returns(true);
            context.Persistence.GetUserConfigurationPath().Returns(AppPaths.SystemConfigurationFilePath);
            context.Persistence
                .LoadUserConfiguration(Arg.Any<CancellationToken>())
                .ThrowsAsyncForAnyArgs(new InvalidSystemConfigurationException(TestExceptionMessage));

            // Act
            var result = await context.Sut.GetConfigurationOnDeviceStartup(CancellationToken.None);

            // Assert
            context.Persistence.Received(1).MarkUserConfigurationDefective();

            AssertFoundExistingUserConfigLogReceived(context);
            AssertGetUserConfigFailedLogReceived(context);
            AssertMarkedUserConfigDefectiveLogReceived(context);

            result.Status.Should().Be(OperationStatus.Warning);
            result.Configuration.Should().BeNull();
        }

        [Fact]
        public async Task ShouldMarkPreparationConfigurationDefective_WhenLoadingPreparationConfigurationFails()
        {
            // Arrange
            var context = new SystemConfigurationServiceContext();
            context.Persistence.ExistsUserConfiguration().Returns(false);
            context.Persistence.GetUserConfigurationPath().Returns(AppPaths.SystemConfigurationFilePath);
            context.Persistence.ExistsDefectiveUserConfiguration().Returns(false);

            context.Persistence.ExistsPreparationConfiguration().Returns(true);
            context.Persistence.GetPreparationConfigurationPath().Returns(AppPaths.DefaultSystemConfigurationFilePath);
            context.Persistence
                .LoadPreparationConfiguration(Arg.Any<CancellationToken>())
                .ThrowsAsyncForAnyArgs(new InvalidSystemConfigurationException(TestExceptionMessage));

            // Act
            var result = await context.Sut.GetConfigurationOnDeviceStartup(CancellationToken.None);

            // Assert
            AssertFoundNoUserConfigLogReceived(context);
            AssertFoundExistingPreparationConfigLogReceived(context);
            AssertGetPreparationConfigFailedLogReceived(context);
            AssertMarkedPreparationConfigDefectiveLogReceived(context);

            context.Persistence.Received(1).MarkPreparationConfigurationDefective();

            result.Status.Should().Be(OperationStatus.Warning);
            result.Configuration.Should().BeNull();
        }

        [Fact]
        public async Task ShouldReturnWarning_WhenPreparationConfigurationAndDefectiveUserConfigurationExists()
        {
            // Arrange
            var context = new SystemConfigurationServiceContext();
            context.Persistence.ExistsUserConfiguration().Returns(false);
            context.Persistence.GetUserConfigurationPath().Returns(AppPaths.SystemConfigurationFilePath);
            context.Persistence.ExistsDefectiveUserConfiguration().Returns(true);
            context.Persistence.GetDefectiveUserConfigurationPath().Returns(AppPaths.DefectiveSystemConfigurationFilePath);

            context.Persistence.ExistsPreparationConfiguration().Returns(true);
            context.Persistence.GetPreparationConfigurationPath().Returns(AppPaths.DefaultSystemConfigurationFilePath);
            context.Persistence.ExistsDefectivePreparationConfiguration().Returns(false);

            // Act
            var result = await context.Sut.GetConfigurationOnDeviceStartup(CancellationToken.None);

            // Assert
            AssertLogReceived(
                context.Logger,
                LogLevel.Warning,
                $"Found only defective system configuration '{AppPaths.DefectiveSystemConfigurationFilePath}'.");

            AssertFoundExistingPreparationConfigLogReceived(context);
            AssertGetJsonConfigNotPossibleLogReceived(context);

            result.Status.Should().Be(OperationStatus.Warning);
            result.Configuration.Should().BeNull();
        }

        [Fact]
        public async Task ShouldReturnWarning_WhenCanceled()
        {
            // Arrange
            var context = new SystemConfigurationServiceContext();
            context.Persistence.ExistsUserConfiguration().Returns(true);
            context.Persistence.GetUserConfigurationPath().Returns(AppPaths.SystemConfigurationFilePath);

            using var cts = new CancellationTokenSource();
            await cts.CancelAsync();

            context.Persistence.LoadUserConfiguration(cts.Token).ThrowsAsync(new OperationCanceledException());

            // Act
            var result = await context.Sut.GetConfigurationOnDeviceStartup(cts.Token);

            // Assert
            AssertFoundExistingUserConfigLogReceived(context);

            AssertLogReceived(
                context.Logger,
                LogLevel.Warning,
                $"Get system configuration '{AppPaths.SystemConfigurationFilePath}' canceled.");

            result.Status.Should().Be(OperationStatus.Warning);
            result.Configuration.Should().BeNull();
        }

        [Fact]
        public async Task ShouldReturnWarning_WhenExceptionOccurs()
        {
            // Arrange
            var context = new SystemConfigurationServiceContext();
            context.Persistence.ExistsUserConfiguration().Returns(true);
            context.Persistence.GetUserConfigurationPath().Returns(AppPaths.SystemConfigurationFilePath);
            context.Persistence.LoadUserConfiguration(Arg.Any<CancellationToken>())
                .ThrowsAsyncForAnyArgs(new InvalidSystemConfigurationException(TestExceptionMessage));

            // Act
            var result = await context.Sut.GetConfigurationOnDeviceStartup(CancellationToken.None);

            // Assert
            AssertFoundExistingUserConfigLogReceived(context);
            AssertGetUserConfigFailedLogReceived(context);

            result.Status.Should().Be(OperationStatus.Warning);
            result.Configuration.Should().BeNull();
        }

        [Fact]
        public async Task ShouldLoadUserConfiguration_WhenUserConfigurationExists()
        {
            // Arrange
            var context = new SystemConfigurationServiceContext();
            context.Persistence.ExistsUserConfiguration().Returns(true);
            context.Persistence.GetUserConfigurationPath().Returns(AppPaths.SystemConfigurationFilePath);
            context.Persistence.LoadUserConfiguration(Arg.Any<CancellationToken>()).Returns(CreateInternalConfig());

            // Act
            var result = await context.Sut.GetConfigurationOnDeviceStartup(CancellationToken.None);

            // Assert
            AssertFoundExistingUserConfigLogReceived(context);
            AssertGetUserConfigSucceededLogReceived(context);

            result.Status.Should().Be(OperationStatus.Success);
            result.Configuration.Should().NotBeNull();
            result.Configuration.NetworkDNSSettings.Hostname.Should().Be(TestHostname);
        }

        [Fact]
        public async Task ShouldLoadPreparationConfiguration_WhenOnlyPreparationConfigurationExists()
        {
            // Arrange
            var context = new SystemConfigurationServiceContext();
            context.Persistence.ExistsUserConfiguration().Returns(false);
            context.Persistence.GetUserConfigurationPath().Returns(AppPaths.SystemConfigurationFilePath);

            context.Persistence.ExistsPreparationConfiguration().Returns(true);
            context.Persistence.GetPreparationConfigurationPath().Returns(AppPaths.DefaultSystemConfigurationFilePath);
            context.Persistence.LoadPreparationConfiguration(Arg.Any<CancellationToken>())
                .Returns(CreateInternalConfig());

            // Act
            var result = await context.Sut.GetConfigurationOnDeviceStartup(CancellationToken.None);

            // Assert
            AssertFoundNoUserConfigLogReceived(context);
            AssertFoundExistingPreparationConfigLogReceived(context);
            AssertGetPreparationConfigSucceededLogReceived(context);

            result.Status.Should().Be(OperationStatus.Success);
            result.Configuration.Should().NotBeNull();
            result.Configuration.NetworkDNSSettings.Hostname.Should().Be(TestHostname);
        }
    }

    public sealed class PrepareConfigurationOnDeviceStartup : SystemConfigurationServiceTests
    {
        [Fact]
        public async Task ShouldReturnWarning_WhenNoJsonFilesExist()
        {
            // Arrange
            var context = new SystemConfigurationServiceContext();

            context.Persistence.ExistsUserConfiguration().Returns(false);
            context.Persistence.GetUserConfigurationPath().Returns(AppPaths.SystemConfigurationFilePath);

            context.Persistence.ExistsPreparationConfiguration().Returns(false);
            context.Persistence.GetPreparationConfigurationPath().Returns(AppPaths.DefaultSystemConfigurationFilePath);

            // Act
            var status = await context.Sut.PrepareConfigurationOnDeviceStartup(CancellationToken.None);

            // Assert
            AssertPrepareConfigAtBootTimeLogReceived(context);
            AssertFoundNoUserConfigLogReceived(context);
            AssertFoundNoPreparationConfigLogReceived(context);

            AssertLogReceived(
                context.Logger,
                LogLevel.Warning,
                "Prepare system configuration from JSON not possible.");

            status.Should().Be(OperationStatus.Warning);
        }

        [Fact]
        public async Task ShouldMarkUserConfigurationDefective_WhenLoadingUserConfigurationFails()
        {
            // Arrange
            var context = new SystemConfigurationServiceContext();
            context.Persistence.ExistsUserConfiguration().Returns(true);
            context.Persistence.GetUserConfigurationPath().Returns(AppPaths.SystemConfigurationFilePath);
            context.Persistence
                .LoadUserConfiguration(Arg.Any<CancellationToken>())
                .ThrowsAsyncForAnyArgs(new InvalidSystemConfigurationException(TestExceptionMessage));

            // Act
            var status = await context.Sut.PrepareConfigurationOnDeviceStartup(CancellationToken.None);

            // Assert
            context.Persistence.Received(1).MarkUserConfigurationDefective();

            AssertFoundExistingUserConfigLogReceived(context);
            AssertGetUserConfigFailedLogReceived(context);
            AssertMarkedUserConfigDefectiveLogReceived(context);
            AssertPrepareUserConfigFailedLogReceived(context);

            status.Should().Be(OperationStatus.Warning);
        }

        [Fact]
        public async Task ShouldMarkPreparationConfigurationDefective_WhenLoadingPreparationConfigurationFails()
        {
            // Arrange
            var context = new SystemConfigurationServiceContext();
            context.Persistence.ExistsUserConfiguration().Returns(false);
            context.Persistence.GetUserConfigurationPath().Returns(AppPaths.SystemConfigurationFilePath);
            context.Persistence.ExistsDefectiveUserConfiguration().Returns(false);

            context.Persistence.ExistsPreparationConfiguration().Returns(true);
            context.Persistence.GetPreparationConfigurationPath().Returns(AppPaths.DefaultSystemConfigurationFilePath);
            context.Persistence
                .LoadPreparationConfiguration(Arg.Any<CancellationToken>())
                .ThrowsAsyncForAnyArgs(new InvalidSystemConfigurationException(TestExceptionMessage));

            // Act
            var result = await context.Sut.GetConfigurationOnDeviceStartup(CancellationToken.None);

            // Assert
            AssertFoundNoUserConfigLogReceived(context);
            AssertFoundExistingPreparationConfigLogReceived(context);
            AssertGetPreparationConfigFailedLogReceived(context);
            AssertMarkedPreparationConfigDefectiveLogReceived(context);

            context.Persistence.Received(1).MarkPreparationConfigurationDefective();

            result.Status.Should().Be(OperationStatus.Warning);
            result.Configuration.Should().BeNull();
        }


        [Fact]
        public async Task ShouldReturnWarning_WhenGetConfigurationFromJsonReturnsWarning()
        {
            // Arrange
            var context = new SystemConfigurationServiceContext();
            context.Persistence.ExistsUserConfiguration().Returns(true);
            context.Persistence.GetUserConfigurationPath().Returns(AppPaths.SystemConfigurationFilePath);
            context.Persistence.LoadUserConfiguration(Arg.Any<CancellationToken>())
                .ThrowsAsync(new InvalidSystemConfigurationException(TestExceptionMessage));

            context.Persistence.ExistsPreparationConfiguration().Returns(true);
            context.Persistence.GetPreparationConfigurationPath().Returns(AppPaths.DefaultSystemConfigurationFilePath);

            // Act
            var status = await context.Sut.PrepareConfigurationOnDeviceStartup(CancellationToken.None);

            // Assert
            AssertPrepareConfigAtBootTimeLogReceived(context);
            AssertFoundExistingUserConfigLogReceived(context);
            AssertGetUserConfigFailedLogReceived(context);
            AssertPrepareUserConfigFailedWithExceptionLogReceived(context);

            status.Should().Be(OperationStatus.Warning);
        }

        [Fact]
        public async Task ShouldReturnWarning_WhenExceptionOccurs()
        {
            // Arrange
            var context = new SystemConfigurationServiceContext();
            context.Persistence.ExistsUserConfiguration().Returns(true);
            context.Persistence.GetUserConfigurationPath().Returns(AppPaths.SystemConfigurationFilePath);

            context.Persistence.ExistsPreparationConfiguration().Returns(true);
            context.Persistence.GetPreparationConfigurationPath().Returns(AppPaths.DefaultSystemConfigurationFilePath);

            var config = CreateInternalConfig();
            context.Persistence.LoadUserConfiguration(Arg.Any<CancellationToken>()).ReturnsForAnyArgs(config);
            context.Persistence.LoadPreparationConfiguration(Arg.Any<CancellationToken>()).ReturnsForAnyArgs(config);

            context.ScriptHandler
                .ExecuteSetNetworkDNSSettingsScript(Arg.Any<ScriptOptions>())
                .ThrowsAsyncForAnyArgs(new InvalidOperationException(TestExceptionMessage));

            // Act
            var status = await context.Sut.PrepareConfigurationOnDeviceStartup(CancellationToken.None);

            // Assert
            AssertPrepareConfigAtBootTimeLogReceived(context);
            AssertFoundExistingUserConfigLogReceived(context);
            AssertGetUserConfigSucceededLogReceived(context);
            AssertPrepareUserConfigFailedWithExceptionLogReceived(context);

            status.Should().Be(OperationStatus.Warning);
        }

        [Fact]
        public async Task ShouldReturnWarning_WhenAnyConfigurationAdapterReturnsError()
        {
            // Arrange
            var context = new SystemConfigurationServiceContext();
            context.Persistence.ExistsUserConfiguration().Returns(true);
            context.Persistence.GetUserConfigurationPath().Returns(AppPaths.SystemConfigurationFilePath);

            context.Persistence.ExistsPreparationConfiguration().Returns(true);
            context.Persistence.GetPreparationConfigurationPath().Returns(AppPaths.DefaultSystemConfigurationFilePath);

            var config = CreateInternalConfig();
            context.Persistence.LoadUserConfiguration(Arg.Any<CancellationToken>()).ReturnsForAnyArgs(config);
            context.Persistence.LoadPreparationConfiguration(Arg.Any<CancellationToken>()).ReturnsForAnyArgs(config);

            context.ScriptHandler
                .ExecuteSetNetworkDNSSettingsScript(Arg.Any<ScriptOptions>())
                .ReturnsForAnyArgs(Result.ErrorResult);

            // Act
            var status = await context.Sut.PrepareConfigurationOnDeviceStartup(CancellationToken.None);

            // Assert
            AssertPrepareConfigAtBootTimeLogReceived(context);
            AssertFoundExistingUserConfigLogReceived(context);
            AssertGetUserConfigSucceededLogReceived(context);
            AssertPrepareUserConfigFailedLogReceived(context);

            status.Should().Be(OperationStatus.Warning);
        }

        [Fact]
        public async Task ShouldPrepareUserConfiguration_WhenUserConfigurationExists()
        {
            // Arrange
            var context = new SystemConfigurationServiceContext();
            context.Persistence.ExistsUserConfiguration().Returns(true);
            context.Persistence.GetUserConfigurationPath().Returns(AppPaths.SystemConfigurationFilePath);
            context.Persistence.LoadUserConfiguration(Arg.Any<CancellationToken>()).Returns(CreateInternalConfig());

            // Act
            var status = await context.Sut.PrepareConfigurationOnDeviceStartup(CancellationToken.None);

            // Assert
            AssertPrepareConfigAtBootTimeLogReceived(context);
            AssertFoundExistingUserConfigLogReceived(context);
            AssertGetUserConfigSucceededLogReceived(context);

            AssertLogReceived(
                context.Logger,
                LogLevel.Information,
                $"Prepare system configuration '{AppPaths.SystemConfigurationFilePath}' succeeded.");

            status.Should().Be(OperationStatus.Success);
        }

        [Fact]
        public async Task ShouldPreparePreparationConfiguration_WhenOnlyPreparationConfigurationExists()
        {
            // Arrange
            var context = new SystemConfigurationServiceContext();
            context.Persistence.ExistsUserConfiguration().Returns(false);
            context.Persistence.GetUserConfigurationPath().Returns(AppPaths.SystemConfigurationFilePath);

            context.Persistence.ExistsPreparationConfiguration().Returns(true);
            context.Persistence.GetPreparationConfigurationPath().Returns(AppPaths.DefaultSystemConfigurationFilePath);
            context.Persistence.LoadPreparationConfiguration(Arg.Any<CancellationToken>())
                .Returns(CreateInternalConfig());

            // Act
            var status = await context.Sut.PrepareConfigurationOnDeviceStartup(CancellationToken.None);

            // Assert
            AssertPrepareConfigAtBootTimeLogReceived(context);
            AssertFoundNoUserConfigLogReceived(context);
            AssertFoundExistingPreparationConfigLogReceived(context);
            AssertGetPreparationConfigSucceededLogReceived(context);

            AssertLogReceived(
                context.Logger,
                LogLevel.Information,
                $"Prepare system configuration '{AppPaths.DefaultSystemConfigurationFilePath}' succeeded.");

            status.Should().Be(OperationStatus.Success);
        }
    }

    public sealed class SetConfiguration : SystemConfigurationServiceTests
    {
        private const string RevertConfigFailed =
            "Revert system configuration failed, after setting system configuration failed.";
        private const string RevertConfigSucceeded =
            "Revert system configuration succeeded, after setting system configuration failed.";
        private const string SetConfigCompletedWithWarnings = "Set system configuration completed with warnings.";
        private const string SetConfigFailed = "Set system configuration failed.";

        [Fact]
        public async Task ShouldReturnErrorResult_WhenGetConfigurationFails()
        {
            // Arrange
            var context = new SystemConfigurationServiceContext();
            context.ScriptHandler
                .ExecuteGetNetworkInterfacesScript()
                .ThrowsAsync(new InvalidOperationException(TestExceptionMessage));

            var proposedConfig = CreateSharedConfig();

            // Act
            var result = await context.Sut.SetConfiguration(
                proposedConfig,
                shouldPersistConfiguration: true,
                CancellationToken.None);

            // Assert
            AssertLogReceived(
                context.Logger,
                LogLevel.Error,
                SetConfigFailed);

            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().ContainAll(SetConfigFailed, TestExceptionMessage);
        }

        [Fact]
        public async Task ShouldReturnErrorResult_WhenGetConfigurationReturnsError()
        {
            // Reset cache, so config is read again via scripts
            SystemConfigurationCache.Instance.ResetForTests();

            // Arrange
            var context = new SystemConfigurationServiceContext();
            context.ScriptHandler
                .ExecuteGetNetworkInterfacesScript()
                .ReturnsForAnyArgs((Result.ErrorResult, ""));

            var proposedConfig = CreateSharedConfig();

            // Act
            var result = await context.Sut.SetConfiguration(
                proposedConfig,
                shouldPersistConfiguration: true,
                CancellationToken.None);

            // Assert
            AssertSetConfigFailedWithMessageLogReceived(context, GetConfigFromScriptsFailed);

            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().Contain(SetConfigFailed + Environment.NewLine + GetConfigFromScriptsFailed);
        }

        [Fact]
        public async Task ShouldReturnWarningResult_WhenGetConfigurationReturnsWarning()
        {
            // Arrange
            var context = new SystemConfigurationServiceContext();
            context.ScriptHandler
                .ExecuteGetNetworkInterfacesCommonInformationScript(Arg.Any<ScriptOptions>())
                .ReturnsForAnyArgs((Result.WarningResult, []));

            var proposedConfig = CreateSharedConfig();
            var expectedMessage = SetConfigCompletedWithWarnings + Environment.NewLine + GetConfigFromScriptsWarning;

            // Act
            var result = await context.Sut.SetConfiguration(
                proposedConfig,
                shouldPersistConfiguration: true,
                CancellationToken.None);

            // Assert
            AssertLogReceived(
                context.Logger,
                LogLevel.Warning,
                expectedMessage);

            result.Status.Should().Be(OperationStatus.Warning);
            result.Message.Should().Contain(expectedMessage);
        }

        [Fact]
        public async Task ShouldReturnErrorResult_WhenValidationFails()
        {
            // Arrange
            var context = new SystemConfigurationServiceContext();
            var proposedConfig = CreateSharedConfig();
            proposedConfig.NetworkInterfaces.Clear();

            var message = "ValidationFailed: No network interfaces specified.";

            // Act
            var result = await context.Sut.SetConfiguration(
                proposedConfig,
                shouldPersistConfiguration: true,
                CancellationToken.None);

            // Assert
            AssertSetConfigFailedWithMessageLogReceived(context, message);

            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().Contain(SetConfigFailed + Environment.NewLine + message);
        }

        [Fact]
        public async Task ShouldReturnSuccessResult_WhenSetConfigurationSucceeds()
        {
            // Arrange
            var context = new SystemConfigurationServiceContext();
            context.Persistence
                .SaveUserConfiguration(default!, Arg.Any<CancellationToken>())
                .ReturnsForAnyArgs(Task.CompletedTask);

            var proposedConfig = CreateSharedConfig();
            var expectedMessage = "Set system configuration succeeded.";

            // Act
            var result = await context.Sut.SetConfiguration(
                proposedConfig,
                shouldPersistConfiguration: true,
                CancellationToken.None);

            // Assert
            AssertLogReceived(
                context.Logger,
                LogLevel.Information,
                expectedMessage);

            result.Status.Should().Be(OperationStatus.Success);
            result.Message.Should().Contain(expectedMessage);
        }

        [Fact]
        public async Task ShouldPersistConfiguration_WhenShouldPersistIsTrue()
        {
            // Arrange
            var context = new SystemConfigurationServiceContext();
            context.Persistence
                .SaveUserConfiguration(default!, Arg.Any<CancellationToken>())
                .ReturnsForAnyArgs(Task.CompletedTask);

            var proposedConfig = CreateSharedConfig();

            // Act
            await context.Sut.SetConfiguration(
                proposedConfig,
                shouldPersistConfiguration: true,
                CancellationToken.None);

            // Assert
            await context.Persistence.Received(1).SaveUserConfiguration(
                Arg.Any<InternalSystemConfiguration>(),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ShouldNotPersistConfiguration_WhenShouldPersistIsFalse()
        {
            // Arrange
            var context = new SystemConfigurationServiceContext();
            var proposedConfig = CreateSharedConfig();

            // Act
            await context.Sut.SetConfiguration(
                proposedConfig,
                shouldPersistConfiguration: false,
                CancellationToken.None);

            // Assert
            await context.Persistence.DidNotReceive().SaveUserConfiguration(
                Arg.Any<InternalSystemConfiguration>(),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ShouldReturnErrorResult_WhenPersistenceFails()
        {
            // Arrange
            var context = new SystemConfigurationServiceContext();
            context.Persistence
                .SaveUserConfiguration(default!, Arg.Any<CancellationToken>())
                .ThrowsAsyncForAnyArgs(new InvalidOperationException(TestExceptionMessage));

            var proposedConfig = CreateSharedConfig();
            var message = "Could not persist system configuration";

            // Act
            var result = await context.Sut.SetConfiguration(
                proposedConfig,
                shouldPersistConfiguration: true,
                CancellationToken.None);

            // Assert
            AssertSetConfigFailedWithMessageLogReceived(context, message);

            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().ContainAll(SetConfigFailed + Environment.NewLine + message, TestExceptionMessage);
        }

        [Fact]
        public async Task ShouldIncludeBothErrorMessages_WhenSetAndRevertConfigurationFail()
        {
            // Arrange
            const string SetErrorMessage = "Setting configuration failed.";
            const string RevertErrorMessage = "Reverting configuration failed.";

            var setResult = new Result();
            setResult.AddError(SetErrorMessage);

            var revertResult = new Result();
            revertResult.AddError(RevertErrorMessage);

            var context = new SystemConfigurationServiceContext();
            context.ScriptHandler
                .ExecuteSetNetworkDNSSettingsScript(Arg.Any<ScriptOptions>())
                .ReturnsForAnyArgs(setResult, revertResult);

            var proposedConfig = CreateSharedConfig();

            // Act
            var result = await context.Sut.SetConfiguration(
                proposedConfig,
                shouldPersistConfiguration: true,
                CancellationToken.None);

            // Assert
            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().ContainAll(
                RevertConfigFailed,
                SetErrorMessage,
                RevertErrorMessage);

            AssertLogReceived(
                context.Logger,
                LogLevel.Error,
                RevertErrorMessage);
        }

        [Fact]
        public async Task ShouldRevertConfiguration_WhenSetConfigurationFails()
        {
            // Arrange
            var context = new SystemConfigurationServiceContext();
            context.ScriptHandler
                .ExecuteSetNetworkDNSSettingsScript(Arg.Any<ScriptOptions>())
                .ReturnsForAnyArgs(
                    x => throw new InvalidOperationException(TestExceptionMessage),
                    x => Result.SuccessResult);

            var proposedConfig = CreateSharedConfig();

            // Act
            var result = await context.Sut.SetConfiguration(
                proposedConfig,
                shouldPersistConfiguration: true,
                CancellationToken.None);

            // Assert
            AssertLogReceived(
                context.Logger,
                LogLevel.Error,
                RevertConfigSucceeded);

            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().ContainAll(RevertConfigSucceeded, TestExceptionMessage);
        }

        [Fact]
        public async Task ShouldRevertConfiguration_WhenSetConfigurationReturnsError()
        {
            // Arrange
            var context = new SystemConfigurationServiceContext();

            // Return error when 'SetConfiguration' is called; return success on 'RevertConfiguration' later
            context.ScriptHandler
                .ExecuteSetNetworkDNSSettingsScript(Arg.Any<ScriptOptions>())
                .ReturnsForAnyArgs(Result.ErrorResult, Result.SuccessResult);

            var proposedConfig = CreateSharedConfig();

            // Act
            var result = await context.Sut.SetConfiguration(
                proposedConfig,
                shouldPersistConfiguration: true,
                CancellationToken.None);

            // Assert
            AssertLogReceived(
                context.Logger,
                LogLevel.Error,
                RevertConfigSucceeded);

            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().Contain(RevertConfigSucceeded);
        }

        [Fact]
        public async Task ShouldReturnWarningResult_WhenSetConfigurationReturnsWarning()
        {
            // Arrange
            var context = new SystemConfigurationServiceContext();
            context.ScriptHandler
                .ExecuteGetNetworkInterfacesCommonInformationScript(Arg.Any<ScriptOptions>())
                .ReturnsForAnyArgs((Result.WarningResult, []));

            var proposedConfig = CreateSharedConfig();
            var expectedMessage = SetConfigCompletedWithWarnings + Environment.NewLine + GetConfigFromScriptsWarning;

            // Act
            var result = await context.Sut.SetConfiguration(
                proposedConfig,
                shouldPersistConfiguration: true,
                CancellationToken.None);

            // Assert
            AssertLogReceived(
                context.Logger,
                LogLevel.Warning,
                expectedMessage);

            result.Status.Should().Be(OperationStatus.Warning);
            result.Message.Should().Contain(expectedMessage);
        }

        [Fact]
        public async Task ShouldPassCancellationToken()
        {
            // Arrange
            var context = new SystemConfigurationServiceContext();
            var proposedConfig = CreateSharedConfig();
            using var cts = new CancellationTokenSource();

            // Act
            await context.Sut.SetConfiguration(proposedConfig, true, cts.Token);

            // Assert
            await context.Persistence.Received(1).SaveUserConfiguration(
                Arg.Any<InternalSystemConfiguration>(),
                cts.Token);
        }
    }

    public sealed class ResetNetworkInterface : SystemConfigurationServiceTests
    {
        [Fact]
        public async Task ShouldReturnErrorResult_WhenPreparationConfigurationNotExists()
        {
            var context = new SystemConfigurationServiceContext();
            context.Persistence.ExistsPreparationConfiguration().Returns(false);
            context.ScriptHandler
                .ExecuteGetNetworkInterfacesCommonInformationScript(Arg.Any<ScriptOptions>())
                .ReturnsForAnyArgs(
                (
                    Result.SuccessResult,
                    [
                        new()
                        {
                            Name = Lan1,
                            Enabled = true,
                            UserDefinedMACAddress = new()
                            {
                                Enabled = true,
                                Address = PhysicalAddress.Parse("11:22:33:44:55:66")
                            }
                        }
                    ]
                ));

            var result = await context.Sut.ResetNetworkInterface(Lan1, CancellationToken.None);

            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().Contain($"Reset network interface '{Lan1}' failed");
            result.Message.Should().Contain(DefaultConfigFile);
        }

        [Fact]
        public async Task ShouldReturnErrorResult_WhenInterfaceNotFoundInPreparationConfiguration()
        {
            var context = new SystemConfigurationServiceContext();
            context.Persistence.ExistsPreparationConfiguration().Returns(true);
            context.Persistence
                .LoadPreparationConfiguration(Arg.Any<CancellationToken>())
                .Returns(CreateInternalDefaultConfigWithInterface(interfaceName: "lan2"));

            context.ScriptHandler
                .ExecuteGetNetworkInterfacesCommonInformationScript(Arg.Any<ScriptOptions>())
                .ReturnsForAnyArgs(
                (
                    Result.SuccessResult,
                    [
                        new()
                        {
                            Name = Lan1,
                            Enabled = true,
                            UserDefinedMACAddress = new()
                            {
                                Enabled = true,
                                Address = PhysicalAddress.Parse("11:22:33:44:55:66")
                            }
                        }
                    ]
                ));

            var result = await context.Sut.ResetNetworkInterface(Lan1, CancellationToken.None);

            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().Contain($"Network interface '{Lan1}' not found in '{DefaultConfigFile}'.");
        }

        [Fact]
        public async Task ShouldPersistUpdatedUserConfiguration_WhenResetSucceeds()
        {
            // Arrange: Default configuration has static IP (DHCP=false)
            var context = new SystemConfigurationServiceContext();
            context.Persistence.ExistsPreparationConfiguration().Returns(true);
            context.Persistence
                .LoadPreparationConfiguration(Arg.Any<CancellationToken>())
                .Returns(CreateInternalDefaultConfigWithInterface(dhcpEnabled: false));
            context.Persistence
                .SaveUserConfiguration(Arg.Any<InternalSystemConfiguration>(), Arg.Any<CancellationToken>())
                .ReturnsForAnyArgs(Task.CompletedTask);

            // Current interface state: DHCP enabled with different IP configuration
            context.ScriptHandler
                .ExecuteGetNetworkInterfacesCommonInformationScript(Arg.Any<ScriptOptions>())
                .ReturnsForAnyArgs(
                (
                    Result.SuccessResult,
                    [
                        new()
                        {
                            Name = Lan1,
                            Enabled = true,
                            UserDefinedMACAddress = new()
                            {
                                Enabled = true,
                                Address = PhysicalAddress.Parse("11:22:33:44:55:66")
                            }
                        }
                    ]
                ));

            context.ScriptHandler
                .ExecuteGetInterfaceSettingsScript(Arg.Any<string>())
                .Returns((
                    Result.SuccessResult,
                    new()
                    {
                        DHCPEnabled = true,
                        Gateway = IPAddress.Parse("10.10.10.1"),
                        IPv4Details =
                        [
                            new()
                            {
                                IPAddress = IPAddress.Parse("10.10.10.2"),
                                Netmask = IPAddress.Parse("255.255.255.0")
                            }
                        ]
                    },
                    new()));

            // Act
            var result = await context.Sut.ResetNetworkInterface(Lan1, CancellationToken.None);

            // Assert: Interface should be reset to static IP from default configuration
            result.Status.Should().Be(OperationStatus.Success);
            result.Message.Should().Contain($"Network interface '{Lan1}' has been reset to default configuration.");

            await context.Persistence.Received(1).SaveUserConfiguration(
                Arg.Is<InternalSystemConfiguration>(static config =>
                    config.NetworkInterfaces![0].CommonInformation!.Enabled == false &&
                    config.NetworkInterfaces![0].CommonInformation!.UserDefinedMACAddress!.Enabled &&
                    config.NetworkInterfaces![0].CommonInformation!.UserDefinedMACAddress!.Address.Equals(
                        PhysicalAddress.Parse("aa:bb:cc:dd:ee:ff")) &&
                    config.NetworkInterfaces![0].IPv4!.DHCPEnabled == false &&
                    config.NetworkInterfaces![0].IPv4!.Gateway!.ToString() == "192.168.1.1" &&
                    config.NetworkInterfaces![0].IPv4!.IPv4Details![0].IPAddress!.ToString() == "192.168.1.2" &&
                    config.NetworkInterfaces![0].IPv4!.IPv4Details![0].Netmask!.ToString() == "255.255.255.0"),
                Arg.Any<CancellationToken>());
        }
    }

    private static void AssertGetJsonConfigNotPossibleLogReceived(SystemConfigurationServiceContext context)
        => AssertLogReceived(
            context.Logger,
            LogLevel.Warning,
            "Get system configuration from JSON not possible.");

    private static void AssertGetPreparationConfigFailedLogReceived(SystemConfigurationServiceContext context)
        => AssertLogReceived<InvalidSystemConfigurationException>(
            context.Logger,
            LogLevel.Warning,
            $"Get system configuration '{AppPaths.DefaultSystemConfigurationFilePath}' failed.");

    private static void AssertGetUserConfigFailedLogReceived(SystemConfigurationServiceContext context)
        => AssertLogReceived<InvalidSystemConfigurationException>(
            context.Logger,
            LogLevel.Warning,
            $"Get system configuration '{AppPaths.SystemConfigurationFilePath}' failed.");

    private static void AssertGetPreparationConfigSucceededLogReceived(SystemConfigurationServiceContext context)
        => AssertLogReceived(
            context.Logger,
            LogLevel.Information,
            $"Get system configuration '{AppPaths.DefaultSystemConfigurationFilePath}' succeeded.");

    private static void AssertGetUserConfigSucceededLogReceived(SystemConfigurationServiceContext context)
        => AssertLogReceived(
            context.Logger,
            LogLevel.Information,
            $"Get system configuration '{AppPaths.SystemConfigurationFilePath}' succeeded.");

    private static void AssertFoundExistingPreparationConfigLogReceived(SystemConfigurationServiceContext context)
        => AssertLogReceived(
            context.Logger,
            LogLevel.Information,
            $"Found existing system configuration '{AppPaths.DefaultSystemConfigurationFilePath}'.");

    private static void AssertFoundExistingUserConfigLogReceived(SystemConfigurationServiceContext context)
        => AssertLogReceived(
            context.Logger,
            LogLevel.Information,
            $"Found existing system configuration '{AppPaths.SystemConfigurationFilePath}'.");

    private static void AssertFoundNoPreparationConfigLogReceived(SystemConfigurationServiceContext context)
        => AssertLogReceived(
            context.Logger,
            LogLevel.Warning,
            $"Found no system configuration '{AppPaths.DefaultSystemConfigurationFilePath}'.");

    private static void AssertFoundNoUserConfigLogReceived(SystemConfigurationServiceContext context)
        => AssertLogReceived(
            context.Logger,
            LogLevel.Warning,
            $"Found no system configuration '{AppPaths.SystemConfigurationFilePath}'.");

    private static void AssertMarkedPreparationConfigDefectiveLogReceived(SystemConfigurationServiceContext context)
        => AssertLogReceived(
            context.Logger,
            LogLevel.Warning,
            $"Marked system configuration '{AppPaths.DefaultSystemConfigurationFilePath}' defective.");

    private static void AssertMarkedUserConfigDefectiveLogReceived(SystemConfigurationServiceContext context)
        => AssertLogReceived(
            context.Logger,
            LogLevel.Warning,
            $"Marked system configuration '{AppPaths.SystemConfigurationFilePath}' defective.");

    private static void AssertPrepareConfigAtBootTimeLogReceived(SystemConfigurationServiceContext context)
        => AssertLogReceived(
            context.Logger,
            LogLevel.Information,
            $"Preparing configuration at boot time started.");

    private static void AssertPrepareUserConfigFailedLogReceived(SystemConfigurationServiceContext context)
        => AssertLogReceived(
            context.Logger,
            LogLevel.Warning,
            $"Prepare system configuration '{AppPaths.SystemConfigurationFilePath}' failed.");

    private static void AssertPrepareUserConfigFailedWithExceptionLogReceived(SystemConfigurationServiceContext context)
        => AssertLogReceived<InvalidOperationException>(
            context.Logger,
            LogLevel.Warning,
            $"Prepare system configuration '{AppPaths.SystemConfigurationFilePath}' failed.");

    private static void AssertSetConfigFailedWithMessageLogReceived(
        SystemConfigurationServiceContext context,
        string message)
            => AssertLogReceived(
                context.Logger,
                LogLevel.Error,
                $"Set system configuration failed.{Environment.NewLine}{message}");

#pragma warning disable CA1873 // Avoid potentially expensive logging
    private static void AssertLogReceived(
        ILogger logger,
        LogLevel logLevel,
        string expectedMessage,
        int expectedCount = 1)
    {
        logger.Received(expectedCount).Log(
            logLevel,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains(expectedMessage)),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    private static void AssertLogReceived<TException>(ILogger logger, LogLevel logLevel, string expectedMessage)
        where TException : Exception
    {
        logger.Received(1).Log(
            logLevel,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains(expectedMessage)),
            Arg.Any<TException>(),
            Arg.Any<Func<object, Exception?, string>>());
    }
#pragma warning restore CA1873

    internal sealed class SystemConfigurationServiceContext
    {
        internal IPersistence Persistence { get; } = Substitute.For<IPersistence>();
        internal IScriptHandler ScriptHandler { get; } = Substitute.For<IScriptHandler>();
        internal ILogger Logger { get; } = Substitute.For<ILogger>();
        internal SupportedSettings SupportedSettings { get; } = new();
        internal SystemConfigurationService Sut { get; }

        internal SystemConfigurationServiceContext()
        {
            const string ServiceName = "test.service";
            const string RawInterfacesString = $"{Lan1}            UP             00:50:56:36:6d:33 <BROADCAST,MULTICAST,UP,LOWER_UP>";

            ScriptHandler.ExecuteIsNetworkInterfaceAvailableScript(Arg.Any<string>()).Returns(Result.SuccessResult);
            ScriptHandler.ExecuteGetOriginalPhysicalAddressScript(Arg.Any<string>()).Returns((Result.SuccessResult, "00:50:56:36:6d:aa"));
            ScriptHandler.ExecuteGetNetworkInterfacesScript().ReturnsForAnyArgs((Result.SuccessResult, RawInterfacesString));
            ScriptHandler.ExecuteGetNetworkInterfacesCommonInformationScript(Arg.Any<ScriptOptions>()).ReturnsForAnyArgs((Result.SuccessResult, []));
            ScriptHandler.ExecuteGetInterfaceSettingsScript(Arg.Any<string>()).ReturnsForAnyArgs((Result.SuccessResult, new(), new()));
            ScriptHandler.ExecuteGetInterfaceIPv4DHCPLeaseScript(Arg.Any<string>()).ReturnsForAnyArgs((Result.SuccessResult, new()));
            ScriptHandler.ExecuteGetNetworkDNSSettingsScript().ReturnsForAnyArgs((Result.SuccessResult, new() { Hostname = TestHostname }));
            ScriptHandler.ExecuteGetNetworkProxySettingsScript().ReturnsForAnyArgs((Result.SuccessResult, new()));
            ScriptHandler.ExecuteGetNetworkNTPSettingsScript().ReturnsForAnyArgs((Result.SuccessResult, new()));
            ScriptHandler.ExecuteGetSystemdUnitStateScript(Arg.Any<ScriptOptions>()).ReturnsForAnyArgs((Result.SuccessResult, new() { Name = ServiceName }));
            ScriptHandler.ExecuteSetInterfaceSettingsScript(Arg.Any<ScriptOptions>()).ReturnsForAnyArgs(Result.SuccessResult);
            ScriptHandler.ExecuteSetNetworkDNSSettingsScript(Arg.Any<ScriptOptions>()).ReturnsForAnyArgs(Result.SuccessResult);
            ScriptHandler.ExecuteSetStaticHostsScript(Arg.Any<ScriptOptions>()).ReturnsForAnyArgs(Result.SuccessResult);
            ScriptHandler.ExecuteSetNetworkMulticastDNSSettingsScript(Arg.Any<ScriptOptions>()).ReturnsForAnyArgs(Result.SuccessResult);
            ScriptHandler.ExecuteSetHostnameScript(Arg.Any<string>()).ReturnsForAnyArgs(Result.SuccessResult);
            ScriptHandler.ExecuteSetNetworkProxySettingsScript(Arg.Any<ScriptOptions>()).ReturnsForAnyArgs(Result.SuccessResult);
            ScriptHandler.ExecuteSetNetworkNTPSettingsScript(Arg.Any<ScriptOptions>()).ReturnsForAnyArgs(Result.SuccessResult);
            ScriptHandler.ExecuteSetSystemdUnitStateScript(Arg.Any<ScriptOptions>()).ReturnsForAnyArgs(Result.SuccessResult);
            ScriptHandler.ExecuteApplyNetplanConfigurationScript().ReturnsForAnyArgs(Result.SuccessResult);

            SupportedSettings.NetworkInterfaces.Capability = CapabilityStatus.Enabled;
            SupportedSettings.NetworkInterfaces.NetworkInterfaceDetail.IPv4.Capability = CapabilityStatus.Enabled;
            SupportedSettings.NetworkInterfaces.NetworkInterfaceDetail.VLAN.Capability = CapabilityStatus.Enabled;

            SupportedSettings.DNS.Capability = CapabilityStatus.Enabled;
            SupportedSettings.DNS.Hostname.Capability = CapabilityStatus.Enabled;
            SupportedSettings.DNS.MulticastDNS.Capability = CapabilityStatus.Enabled;
            SupportedSettings.DNS.StaticHosts.Capability = CapabilityStatus.Enabled;
            SupportedSettings.DNS.NameServers.Capability = CapabilityStatus.Enabled;
            SupportedSettings.DNS.PrimaryDNSSuffix.Capability = CapabilityStatus.Enabled;
            SupportedSettings.DNS.SearchDomains.Capability = CapabilityStatus.Enabled;

            SupportedSettings.Proxy.Capability = CapabilityStatus.Enabled;
            SupportedSettings.NTP.Capability = CapabilityStatus.Enabled;
            SupportedSettings.Services.Capability = CapabilityStatus.Enabled;

            Sut = new(Persistence, ScriptHandler, Logger, SupportedSettings);

            Environment.SetEnvironmentVariable("CONFIGURABLE_SERVICES", ServiceName);
            Environment.SetEnvironmentVariable("SYSTEM_CONFIGURATION_DIRECTORY", "/mnt/persistent/hostmanagement");

            Logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        }
    }
}
