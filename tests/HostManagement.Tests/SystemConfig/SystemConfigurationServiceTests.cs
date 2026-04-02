using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using HostManagement.Configuration;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Contracts;
using HostManagement.SystemConfig;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using InternalSystemConfiguration = HostManagement.SystemConfig.Contracts.SystemConfiguration;

namespace HostManagement.Tests.SystemConfig;

public class SystemConfigurationServiceTests
{
    private const string Lan1 = "lan1";

    private const string DefaultConfigFile = "DefaultSystemConfiguration.json";
    private const string SystemConfigFile = "SystemConfiguration.json";

    private const string GetConfigFromScriptsFailed = "Get system configuration from scripts failed.";
    private const string GetConfigFromScriptsWarning = "Get system configuration from scripts completed with warnings.";

    private const string TestHostname = "test-host";
    private const string TestExceptionMessage = "Test exception";


    private static InternalSystemConfiguration CreateInternalConfig()
        => new()
        {
            Version = 1,
            NetworkDNSSettings = new()
            {
                Hostname = TestHostname,
                DNSSuffixEnabled = false,
                DNSSuffix = "company"
            }
        };

    private static SystemConfiguration CreateSharedConfig()
        => new()
        {
            Version = 1,
            NetworkDNSSettings = new()
            {
                Hostname = TestHostname,
                DNSSuffixEnabled = false,
                DNSSuffix = "company"
            },
            NetworkInterfacesSettings = new()
            {

                NetworkInterfaces =
                [
                    new()
                    {
                        CommonInformation = new()
                        {
                            Name = Lan1,
                            Enabled = true,
                        },
                        IPv4 = new()
                        {
                            DHCPEnabled = true,
                        },
                    },
                ],
            },
        };

    public sealed class GetConfiguration
    {
        [Fact]
        public async Task ShouldReturnSuccess_WhenScriptsSucceed()
        {
            var context = new SystemConfigurationServiceContext();

            var result = await context.Sut.GetConfiguration(CancellationToken.None);

            result.Status.Should().Be(OperationStatus.Success);
            result.Configuration.Should().NotBeNull();
            result.Message.Should().NotBeEmpty();
        }

        [Fact]
        public async Task ShouldReturnError_WhenScriptsFail()
        {
            var context = new SystemConfigurationServiceContext();
            context.ScriptHandler
                .ExecuteGetNetworkInterfacesCommonInformationScript()
                .ReturnsForAnyArgs((OperationStatus.Error, []));

            var result = await context.Sut.GetConfiguration(CancellationToken.None);

            result.Status.Should().Be(OperationStatus.Error);
            result.Configuration.Should().BeNull();
            result.Message.Should().Contain(GetConfigFromScriptsFailed);
        }

        [Fact]
        public async Task ShouldReturnWarning_WhenScriptsReturnWarning()
        {
            var context = new SystemConfigurationServiceContext();
            context.ScriptHandler
                .ExecuteGetNetworkInterfacesCommonInformationScript()
                .ReturnsForAnyArgs((OperationStatus.Warning, []));

            var result = await context.Sut.GetConfiguration(CancellationToken.None);

            result.Status.Should().Be(OperationStatus.Warning);
            result.Configuration.Should().NotBeNull();
            result.Message.Should().Contain(GetConfigFromScriptsWarning);
        }

        [Fact]
        public async Task ShouldReturnError_WhenExceptionThrown()
        {
            var context = new SystemConfigurationServiceContext();
            context.ScriptHandler
                .ExecuteGetNetworkInterfacesCommonInformationScript()
                .ThrowsAsync(new InvalidOperationException(TestExceptionMessage));

            var result = await context.Sut.GetConfiguration(CancellationToken.None);

            result.Status.Should().Be(OperationStatus.Error);
            result.Configuration.Should().BeNull();
            result.Message.Should().Contain(TestExceptionMessage);
        }
    }

    public sealed class GetConfigurationFromJson
    {
        [Fact]
        public async Task ShouldReturnError_WhenNoJsonFileExists()
        {
            var context = new SystemConfigurationServiceContext();
            context.Persistence.GetUserConfigurationPath().Returns(AppPaths.SystemConfigurationFilePath);
            context.Persistence.GetPreparationConfigurationPath().Returns(AppPaths.DefaultSystemConfigurationFilePath);

            var result = await context.Sut.GetConfigurationFromJson(CancellationToken.None);

            result.Status.Should().Be(OperationStatus.Error);
            result.Configuration.Should().BeNull();
            result.Message.Should().Contain(SystemConfigFile);
            result.Message.Should().Contain(DefaultConfigFile);
        }

        [Fact]
        public async Task ShouldLoadSystemConfiguration_WhenSystemConfigExists()
        {
            var context = new SystemConfigurationServiceContext();
            context.Persistence.UserConfigurationExists().Returns(true);
            context.Persistence.GetUserConfigurationPath().Returns(AppPaths.SystemConfigurationFilePath);
            context.Persistence.LoadUserConfiguration(Arg.Any<CancellationToken>()).Returns(CreateInternalConfig());

            var result = await context.Sut.GetConfigurationFromJson(CancellationToken.None);

            result.Status.Should().Be(OperationStatus.Success);
            result.Configuration.Should().NotBeNull();
            result.Configuration!.NetworkDNSSettings!.Hostname.Should().Be(TestHostname);
            result.Message.Should().Contain(SystemConfigFile);
        }

        [Fact]
        public async Task ShouldLoadDefaultConfiguration_WhenOnlyDefaultConfigExists()
        {
            var context = new SystemConfigurationServiceContext();
            context.Persistence.PreparationConfigurationExists().Returns(true);
            context.Persistence.GetPreparationConfigurationPath().Returns(AppPaths.DefaultSystemConfigurationFilePath);
            context.Persistence.LoadPreparationConfiguration(Arg.Any<CancellationToken>()).Returns(CreateInternalConfig());

            var result = await context.Sut.GetConfigurationFromJson(CancellationToken.None);

            result.Status.Should().Be(OperationStatus.Success);
            result.Configuration.Should().NotBeNull();
            result.Configuration.NetworkDNSSettings!.Hostname.Should().Be(TestHostname);
            result.Message.Should().Contain(DefaultConfigFile);
        }

        [Fact]
        public async Task ShouldReturnError_WhenPersistenceThrowsException()
        {
            var context = new SystemConfigurationServiceContext();
            context.Persistence.PreparationConfigurationExists().Returns(true);
            context.Persistence.UserConfigurationExists().Returns(true);
            context.Persistence
                .LoadUserConfiguration(Arg.Any<CancellationToken>())
                .ThrowsAsyncForAnyArgs(new InvalidOperationException(TestExceptionMessage));

            var result = await context.Sut.GetConfigurationFromJson(CancellationToken.None);

            result.Status.Should().Be(OperationStatus.Error);
            result.Configuration.Should().BeNull();
            result.Message.Should().Contain(TestExceptionMessage);
        }

        [Fact]
        public async Task ShouldPassCancellationToken()
        {
            var context = new SystemConfigurationServiceContext();
            context.Persistence.PreparationConfigurationExists().Returns(true);
            context.Persistence.UserConfigurationExists().Returns(true);
            context.Persistence
                .LoadUserConfiguration(Arg.Any<CancellationToken>())
                .ReturnsForAnyArgs(CreateInternalConfig());

            using var cts = new CancellationTokenSource();

            await context.Sut.GetConfigurationFromJson(cts.Token);

            _ = context.Persistence.Received(0).LoadPreparationConfiguration(cts.Token);
            _ = context.Persistence.Received(1).LoadUserConfiguration(cts.Token);
        }
    }

    public sealed class PrepareConfiguration
    {
        private const string PrepareConfigNotPossible = "Prepare system configuration not possible.";

        [Fact]
        public async Task ShouldReturnSuccess_WhenNoConfigFilesExist()
        {
            var context = new SystemConfigurationServiceContext();

            var result = await context.Sut.PrepareConfiguration(CancellationToken.None);

            result.Status.Should().Be(OperationStatus.Success);
            result.Message.Should().Contain(SystemConfigFile);
            result.Message.Should().Contain(DefaultConfigFile);
        }

        [Fact]
        public async Task ShouldPrepareSystemConfig_WhenSystemConfigExists()
        {
            var context = new SystemConfigurationServiceContext();
            context.FileSystem.AddFile(AppPaths.SystemConfigurationFilePath, new MockFileData(string.Empty));
            context.Persistence.UserConfigurationExists().Returns(true);
            context.Persistence.GetUserConfigurationPath().Returns(AppPaths.SystemConfigurationFilePath);
            context.Persistence.LoadUserConfiguration(Arg.Any<CancellationToken>()).Returns(CreateInternalConfig());

            var result = await context.Sut.PrepareConfiguration(CancellationToken.None);

            result.Status.Should().Be(OperationStatus.Success);
            context.ScriptHandler.Received().UseResult(Arg.Any<Result>());
        }

        [Fact]
        public async Task ShouldPrepareDefaultConfig_WhenSystemConfigFailsAndDefaultExists()
        {
            // Arrange
            var context = new SystemConfigurationServiceContext();
            context.FileSystem.AddFile(AppPaths.SystemConfigurationFilePath, new MockFileData(string.Empty));
            context.Persistence.UserConfigurationExists().Returns(true);
            context.Persistence.GetUserConfigurationPath().Returns(AppPaths.SystemConfigurationFilePath);

            context.FileSystem.AddFile(AppPaths.DefaultSystemConfigurationFilePath, new MockFileData(string.Empty));
            context.Persistence.PreparationConfigurationExists().Returns(true);
            context.Persistence.GetPreparationConfigurationPath().Returns(AppPaths.DefaultSystemConfigurationFilePath);

            context.Persistence
                .LoadUserConfiguration(Arg.Any<CancellationToken>())
                .ThrowsAsync(new InvalidOperationException(TestExceptionMessage));

            context.Persistence
                .LoadPreparationConfiguration(Arg.Any<CancellationToken>())
                .Returns(CreateInternalConfig());

            // Act + Assert
            var result = await context.Sut.PrepareConfiguration(CancellationToken.None);

            result.Status.Should().Be(OperationStatus.Success);
            result.Message.Should().Contain(TestExceptionMessage);
        }

        [Fact]
        public async Task ShouldReturnError_WhenBothConfigsFail()
        {
            // Arrange
            var context = new SystemConfigurationServiceContext();
            context.FileSystem.AddFile(AppPaths.SystemConfigurationFilePath, new MockFileData(string.Empty));
            context.Persistence.UserConfigurationExists().Returns(true);
            context.Persistence.GetUserConfigurationPath().Returns(AppPaths.SystemConfigurationFilePath);

            context.FileSystem.AddFile(AppPaths.DefaultSystemConfigurationFilePath, new MockFileData(string.Empty));
            context.Persistence.PreparationConfigurationExists().Returns(true);
            context.Persistence.GetPreparationConfigurationPath().Returns(AppPaths.DefaultSystemConfigurationFilePath);

            context.Persistence
                .LoadUserConfiguration(Arg.Any<CancellationToken>())
                .ThrowsAsyncForAnyArgs(new InvalidOperationException(TestExceptionMessage));

            context.Persistence
                .LoadPreparationConfiguration(Arg.Any<CancellationToken>())
                .ThrowsAsyncForAnyArgs(new InvalidOperationException(TestExceptionMessage));

            // Act + Assert
            var result = await context.Sut.PrepareConfiguration(CancellationToken.None);

            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().Contain(TestExceptionMessage);
        }

        [Fact]
        public async Task ShouldReturnError_WhenAdapterReturnsWarning()
        {
            // Arrange
            var context = new SystemConfigurationServiceContext();
            context.Persistence.UserConfigurationExists().Returns(true);
            context.Persistence.PreparationConfigurationExists().Returns(true);

            var config = CreateInternalConfig();
            context.Persistence.LoadUserConfiguration(Arg.Any<CancellationToken>()).ReturnsForAnyArgs(config);
            context.Persistence.LoadPreparationConfiguration(Arg.Any<CancellationToken>()).ReturnsForAnyArgs(config);

            context.ScriptHandler
                .ExecuteSetNetworkDNSSettingsScript(Arg.Any<ScriptOptions>())
                .ReturnsForAnyArgs(OperationStatus.Warning);

            // Act + Assert
            var result = await context.Sut.PrepareConfiguration(CancellationToken.None);

            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().Contain(PrepareConfigNotPossible);
        }

        [Fact]
        public async Task ShouldReturnError_WhenAdapterReturnsError()
        {
            // Arrange
            var context = new SystemConfigurationServiceContext();
            context.Persistence.UserConfigurationExists().Returns(true);
            context.Persistence.PreparationConfigurationExists().Returns(true);

            var config = CreateInternalConfig();
            context.Persistence.LoadUserConfiguration(Arg.Any<CancellationToken>()).ReturnsForAnyArgs(config);
            context.Persistence.LoadPreparationConfiguration(Arg.Any<CancellationToken>()).ReturnsForAnyArgs(config);

            context.ScriptHandler
                .ExecuteSetNetworkDNSSettingsScript(Arg.Any<ScriptOptions>())
                .ReturnsForAnyArgs(OperationStatus.Error);

            // Act + Assert
            var result = await context.Sut.PrepareConfiguration(CancellationToken.None);

            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().Contain(PrepareConfigNotPossible);
        }

        [Fact]
        public async Task ShouldReturnError_WhenExceptionThrownDuringPrepare()
        {
            // Arrange
            var context = new SystemConfigurationServiceContext();
            context.FileSystem.AddFile(AppPaths.SystemConfigurationFilePath, new MockFileData(string.Empty));
            context.Persistence.UserConfigurationExists().Returns(true);
            context.Persistence.GetUserConfigurationPath().Returns(AppPaths.SystemConfigurationFilePath);

            context.FileSystem.AddFile(AppPaths.DefaultSystemConfigurationFilePath, new MockFileData(string.Empty));
            context.Persistence.PreparationConfigurationExists().Returns(true);
            context.Persistence.GetPreparationConfigurationPath().Returns(AppPaths.DefaultSystemConfigurationFilePath);

            var config = CreateInternalConfig();
            context.Persistence.LoadUserConfiguration(Arg.Any<CancellationToken>()).ReturnsForAnyArgs(config);
            context.Persistence.LoadPreparationConfiguration(Arg.Any<CancellationToken>()).ReturnsForAnyArgs(config);

            context.ScriptHandler
                .ExecuteSetNetworkDNSSettingsScript(Arg.Any<ScriptOptions>())
                .ThrowsAsyncForAnyArgs(new InvalidOperationException(TestExceptionMessage));

            // Act + Assert
            var result = await context.Sut.PrepareConfiguration(CancellationToken.None);

            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().Contain(TestExceptionMessage);
        }
    }

    public sealed class SetConfiguration
    {
        private const string RevertConfig = "Revert system configuration";

        [Fact]
        public async Task ShouldReturnError_WhenGetConfigurationFails()
        {
            var context = new SystemConfigurationServiceContext();
            context.ScriptHandler
                .ExecuteGetNetworkInterfacesCommonInformationScript()
                .ThrowsAsync(new InvalidOperationException(TestExceptionMessage));

            var proposedConfig = CreateSharedConfig();

            var result = await context.Sut
                .SetConfiguration(proposedConfig, shouldPersistConfiguration: true, CancellationToken.None);

            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().Contain(TestExceptionMessage);
        }

        [Fact]
        public async Task ShouldReturnError_WhenGetConfigurationReturnsError()
        {
            var context = new SystemConfigurationServiceContext();
            context.ScriptHandler
                .ExecuteGetNetworkInterfacesCommonInformationScript()
                .ReturnsForAnyArgs((OperationStatus.Error, []));

            var proposedConfig = CreateSharedConfig();

            var result = await context.Sut
                .SetConfiguration(proposedConfig, shouldPersistConfiguration: true, CancellationToken.None);

            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().Contain(GetConfigFromScriptsFailed);
        }

        [Fact]
        public async Task ShouldReturnWarning_WhenGetConfigurationReturnsWarning()
        {
            var context = new SystemConfigurationServiceContext();
            context.ScriptHandler
                .ExecuteGetNetworkInterfacesCommonInformationScript()
                .ReturnsForAnyArgs((OperationStatus.Warning, []));

            var proposedConfig = CreateSharedConfig();

            var result = await context.Sut
                .SetConfiguration(proposedConfig, shouldPersistConfiguration: true, CancellationToken.None);

            result.Status.Should().Be(OperationStatus.Warning);
            result.Message.Should().Contain(GetConfigFromScriptsWarning);
        }

        [Fact]
        public async Task ShouldReturnError_WhenValidationFails()
        {
            var context = new SystemConfigurationServiceContext();
            var proposedConfig = CreateSharedConfig();
            proposedConfig.NetworkInterfacesSettings.NetworkInterfaces.Clear();

            var result = await context.Sut
                .SetConfiguration(proposedConfig, shouldPersistConfiguration: true, CancellationToken.None);

            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().Contain("No network interfaces specified.");
        }

        [Fact]
        public async Task ShouldReturnSuccess_WhenSetSucceeds()
        {
            var context = new SystemConfigurationServiceContext();
            context.Persistence
                .SaveUserConfiguration(default!, Arg.Any<CancellationToken>())
                .ReturnsForAnyArgs(Task.CompletedTask);

            var proposedConfig = CreateSharedConfig();

            var result = await context.Sut
                .SetConfiguration(proposedConfig, shouldPersistConfiguration: true, CancellationToken.None);

            result.Status.Should().Be(OperationStatus.Success);
            result.Message.Should().NotBeEmpty();
        }

        [Fact]
        public async Task ShouldPersistConfiguration_WhenShouldPersistIsTrue()
        {
            var context = new SystemConfigurationServiceContext();
            context.Persistence
                .SaveUserConfiguration(default!, Arg.Any<CancellationToken>())
                .ReturnsForAnyArgs(Task.CompletedTask);

            var proposedConfig = CreateSharedConfig();

            await context.Sut
                .SetConfiguration(proposedConfig, shouldPersistConfiguration: true, CancellationToken.None);

            await context.Persistence.Received(1).SaveUserConfiguration(
                Arg.Any<InternalSystemConfiguration>(),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ShouldNotPersistConfiguration_WhenShouldPersistIsFalse()
        {
            var context = new SystemConfigurationServiceContext();
            var proposedConfig = CreateSharedConfig();

            await context.Sut
                .SetConfiguration(proposedConfig, shouldPersistConfiguration: false, CancellationToken.None);

            await context.Persistence.DidNotReceive().SaveUserConfiguration(
                Arg.Any<InternalSystemConfiguration>(),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ShouldReturnError_WhenPersistenceFails()
        {
            var context = new SystemConfigurationServiceContext();
            context.Persistence
                .SaveUserConfiguration(default!, Arg.Any<CancellationToken>())
                .ThrowsAsyncForAnyArgs(new InvalidOperationException(TestExceptionMessage));

            var proposedConfig = CreateSharedConfig();

            var result = await context.Sut
                .SetConfiguration(proposedConfig, shouldPersistConfiguration: true, CancellationToken.None);

            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().Contain(TestExceptionMessage);
        }

        [Fact]
        public async Task ShouldRevertConfiguration_WhenSetConfigurationFails()
        {
            var context = new SystemConfigurationServiceContext();
            context.ScriptHandler
                .ExecuteSetNetworkDNSSettingsScript(Arg.Any<ScriptOptions>())
                .ReturnsForAnyArgs(
                    x => throw new InvalidOperationException(TestExceptionMessage),
                    x => OperationStatus.Success);

            var proposedConfig = CreateSharedConfig();

            var result = await context.Sut
                .SetConfiguration(proposedConfig, shouldPersistConfiguration: true, CancellationToken.None);

            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().Contain(TestExceptionMessage).And.Contain(RevertConfig);
        }

        [Fact]
        public async Task ShouldRevertConfiguration_WhenSetConfigurationReturnsError()
        {
            var context = new SystemConfigurationServiceContext();
            context.ScriptHandler
                .ExecuteSetNetworkDNSSettingsScript(Arg.Any<ScriptOptions>())
                .ReturnsForAnyArgs(OperationStatus.Error);

            var proposedConfig = CreateSharedConfig();

            var result = await context.Sut
                .SetConfiguration(proposedConfig, shouldPersistConfiguration: true, CancellationToken.None);

            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().Contain(RevertConfig).And.Contain("setting system configuration failed");
        }

        [Fact]
        public async Task ShouldReturnWarning_WhenSetConfigurationReturnsWarning()
        {
            var context = new SystemConfigurationServiceContext();
            context.ScriptHandler
                .ExecuteGetNetworkInterfacesCommonInformationScript()
                .ReturnsForAnyArgs((OperationStatus.Warning, []));

            var proposedConfig = CreateSharedConfig();

            var result = await context.Sut
                .SetConfiguration(proposedConfig, shouldPersistConfiguration: true, CancellationToken.None);

            result.Status.Should().Be(OperationStatus.Warning);
            result.Message.Should().Contain("completed with warnings");
        }

        [Fact]
        public async Task ShouldPassCancellationToken()
        {
            var context = new SystemConfigurationServiceContext();
            var proposedConfig = CreateSharedConfig();
            using var cts = new CancellationTokenSource();

            await context.Sut.SetConfiguration(proposedConfig, true, cts.Token);

            await context.Persistence.Received(1).SaveUserConfiguration(
                Arg.Any<InternalSystemConfiguration>(),
                cts.Token);
        }
    }
}


internal sealed class SystemConfigurationServiceContext
{
    internal MockFileSystem FileSystem { get; } = new();
    internal IPersistence Persistence { get; } = Substitute.For<IPersistence>();
    internal IScriptHandler ScriptHandler { get; } = Substitute.For<IScriptHandler>();
    internal SystemConfigurationService Sut { get; }

    internal SystemConfigurationServiceContext()
    {
        const string ServiceName = "test.service";

        ScriptHandler.ExecuteGetNetworkInterfacesCommonInformationScript().ReturnsForAnyArgs((OperationStatus.Success, []));
        ScriptHandler.ExecuteGetInterfaceIPv4SettingsScript(Arg.Any<string>()).ReturnsForAnyArgs((OperationStatus.Success, new()));
        ScriptHandler.ExecuteGetInterfaceIPv4DHCPLeaseScript(Arg.Any<string>()).ReturnsForAnyArgs((OperationStatus.Success, new()));
        ScriptHandler.ExecuteGetNetworkDNSSettingsScript().ReturnsForAnyArgs((OperationStatus.Success, new()));
        ScriptHandler.ExecuteGetNetworkProxySettingsScript().ReturnsForAnyArgs((OperationStatus.Success, new()));
        ScriptHandler.ExecuteGetNetworkNTPSettingsScript().ReturnsForAnyArgs((OperationStatus.Success, new()));
        ScriptHandler.ExecuteGetSystemdUnitStateScript(Arg.Any<ScriptOptions>()).ReturnsForAnyArgs((OperationStatus.Success, new() { Name = ServiceName, }));
        ScriptHandler.ExecuteSetInterfaceIPv4SettingsScript(Arg.Any<ScriptOptions>()).ReturnsForAnyArgs(OperationStatus.Success);
        ScriptHandler.ExecuteSetNetworkDNSSettingsScript(Arg.Any<ScriptOptions>()).ReturnsForAnyArgs(OperationStatus.Success);
        ScriptHandler.ExecuteSetStaticHostsScript(Arg.Any<ScriptOptions>()).ReturnsForAnyArgs(OperationStatus.Success);
        ScriptHandler.ExecuteSetNetworkMulticastDNSSettingsScript(Arg.Any<ScriptOptions>()).ReturnsForAnyArgs(OperationStatus.Success);
        ScriptHandler.ExecuteSetHostnameScript(Arg.Any<string>()).ReturnsForAnyArgs(OperationStatus.Success);
        ScriptHandler.ExecuteSetNetworkProxySettingsScript(Arg.Any<ScriptOptions>()).ReturnsForAnyArgs(OperationStatus.Success);
        ScriptHandler.ExecuteSetNetworkNTPSettingsScript(Arg.Any<ScriptOptions>()).ReturnsForAnyArgs(OperationStatus.Success);
        ScriptHandler.ExecuteSetSystemdUnitStateScript(Arg.Any<ScriptOptions>()).ReturnsForAnyArgs(OperationStatus.Success);
        ScriptHandler.ExecuteApplyNetplanConfigurationScript().ReturnsForAnyArgs(OperationStatus.Success);
        Sut = new(FileSystem, Persistence, ScriptHandler);

        Environment.SetEnvironmentVariable("CONFIGURABLE_SERVICES", ServiceName);
    }
}
