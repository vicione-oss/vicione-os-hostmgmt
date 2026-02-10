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
    private static InternalSystemConfiguration CreateInternalConfig()
        => new()
        {
            Version = 1,
            NetworkDNSSettings = new() { Hostname = "test-host" },
        };

    private static SystemConfiguration CreateSharedConfig()
        => new()
        {
            Version = 1,
            NetworkDNSSettings = new() { Hostname = "test-host", },
            NetworkInterfacesSettings = new()
            {

                NetworkInterfaces =
                [
                    new()
                    {
                        CommonInformation = new()
                        {
                            Name = "eth0",
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

            var result = await context.Sut.GetConfiguration();

            result.Status.Should().Be(OperationStatus.Success);
            result.Configuration.Should().NotBeNull();
            result.Message.Should().NotBeEmpty();
        }

        [Fact]
        public async Task ShouldReturnError_WhenScriptsFail()
        {
            var context = new SystemConfigurationServiceContext();
            context.ScriptHandler.ExecuteGetNetworkInterfacesCommonInformationScript().ReturnsForAnyArgs((OperationStatus.Error, []));

            var result = await context.Sut.GetConfiguration();

            result.Status.Should().Be(OperationStatus.Error);
            result.Configuration.Should().BeNull();
            result.Message.Should().Contain("Get system configuration from scripts failed");
        }

        [Fact]
        public async Task ShouldReturnWarning_WhenScriptsReturnWarning()
        {
            var context = new SystemConfigurationServiceContext();
            context.ScriptHandler.ExecuteGetNetworkInterfacesCommonInformationScript().ReturnsForAnyArgs((OperationStatus.Warning, []));

            var result = await context.Sut.GetConfiguration();

            result.Status.Should().Be(OperationStatus.Warning);
            result.Configuration.Should().NotBeNull();
            result.Message.Should().Contain("Get system configuration from scripts completed with warnings");
        }

        [Fact]
        public async Task ShouldReturnError_WhenExceptionThrown()
        {
            var context = new SystemConfigurationServiceContext();
            context.ScriptHandler.ExecuteGetNetworkInterfacesCommonInformationScript()
                .ThrowsAsync(new InvalidOperationException("Test exception"));

            var result = await context.Sut.GetConfiguration();

            result.Status.Should().Be(OperationStatus.Error);
            result.Configuration.Should().BeNull();
            result.Message.Should().Contain("Test exception");
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

            var result = await context.Sut.GetConfigurationFromJson();

            result.Status.Should().Be(OperationStatus.Error);
            result.Configuration.Should().BeNull();
            result.Message.Should().Contain("SystemConfiguration.json");
            result.Message.Should().Contain("DefaultSystemConfiguration.json");
        }

        [Fact]
        public async Task ShouldLoadSystemConfiguration_WhenSystemConfigExists()
        {
            var context = new SystemConfigurationServiceContext();
            context.Persistence.UserConfigurationExists().Returns(true);
            context.Persistence.GetUserConfigurationPath().Returns(AppPaths.SystemConfigurationFilePath);
            var config = CreateInternalConfig();
            context.Persistence.LoadUserConfiguration(Arg.Any<CancellationToken>())
                .Returns(config);

            var result = await context.Sut.GetConfigurationFromJson();

            result.Status.Should().Be(OperationStatus.Success);
            result.Configuration.Should().NotBeNull();
            result.Configuration!.NetworkDNSSettings!.Hostname.Should().Be("test-host");
            result.Message.Should().Contain("SystemConfiguration.json");
        }

        [Fact]
        public async Task ShouldLoadDefaultConfiguration_WhenOnlyDefaultConfigExists()
        {
            var context = new SystemConfigurationServiceContext();
            context.Persistence.PreparationConfigurationExists().Returns(true);
            context.Persistence.GetPreparationConfigurationPath().Returns(AppPaths.DefaultSystemConfigurationFilePath);
            var config = CreateInternalConfig();
            context.Persistence.LoadPreparationConfiguration(Arg.Any<CancellationToken>())
                .Returns(config);

            var result = await context.Sut.GetConfigurationFromJson();

            result.Status.Should().Be(OperationStatus.Success);
            result.Configuration.Should().NotBeNull();
            result.Configuration!.NetworkDNSSettings!.Hostname.Should().Be("test-host");
            result.Message.Should().Contain("DefaultSystemConfiguration.json");
        }

        [Fact]
        public async Task ShouldReturnError_WhenPersistenceThrowsException()
        {
            var context = new SystemConfigurationServiceContext();
            context.Persistence.PreparationConfigurationExists().Returns(true);
            context.Persistence.UserConfigurationExists().Returns(true);
            context.Persistence.LoadUserConfiguration(default)
                .ThrowsAsyncForAnyArgs(new InvalidOperationException("Load failed"));

            var result = await context.Sut.GetConfigurationFromJson();

            result.Status.Should().Be(OperationStatus.Error);
            result.Configuration.Should().BeNull();
            result.Message.Should().Contain("Load failed");
        }

        [Fact]
        public async Task ShouldPassCancellationToken()
        {
            var context = new SystemConfigurationServiceContext();
            context.Persistence.PreparationConfigurationExists().Returns(true);
            context.Persistence.UserConfigurationExists().Returns(true);
            var config = CreateInternalConfig();
            context.Persistence.LoadUserConfiguration(default)
                .ReturnsForAnyArgs(config);
            using var cts = new CancellationTokenSource();

            await context.Sut.GetConfigurationFromJson(cts.Token);

            _ = context.Persistence.Received(0).LoadPreparationConfiguration(cts.Token);
            _ = context.Persistence.Received(1).LoadUserConfiguration(cts.Token);
        }
    }

    public sealed class PrepareConfiguration
    {
        [Fact]
        public async Task ShouldReturnSuccess_WhenNoConfigFilesExist()
        {
            var context = new SystemConfigurationServiceContext();

            var result = await context.Sut.PrepareConfiguration();

            result.Status.Should().Be(OperationStatus.Success);
            result.Message.Should().Contain("SystemConfiguration.json");
            result.Message.Should().Contain("DefaultSystemConfiguration.json");
        }

        [Fact]
        public async Task ShouldPrepareSystemConfig_WhenSystemConfigExists()
        {
            var context = new SystemConfigurationServiceContext();
            context.FileSystem.AddFile(AppPaths.SystemConfigurationFilePath, new MockFileData(string.Empty));
            context.Persistence.UserConfigurationExists().Returns(true);
            context.Persistence.GetUserConfigurationPath().Returns(AppPaths.SystemConfigurationFilePath);
            var config = CreateInternalConfig();
            context.Persistence.LoadUserConfiguration(Arg.Any<CancellationToken>())
                .Returns(config);

            var result = await context.Sut.PrepareConfiguration();

            result.Status.Should().Be(OperationStatus.Success);
            context.ScriptHandler.Received().UseResult(Arg.Any<Result>());
        }

        [Fact]
        public async Task ShouldPrepareDefaultConfig_WhenSystemConfigFailsAndDefaultExists()
        {
            var context = new SystemConfigurationServiceContext();
            context.FileSystem.AddFile(AppPaths.SystemConfigurationFilePath, new MockFileData(string.Empty));
            context.Persistence.UserConfigurationExists().Returns(true);
            context.Persistence.GetUserConfigurationPath().Returns(AppPaths.SystemConfigurationFilePath);
            context.FileSystem.AddFile(AppPaths.DefaultSystemConfigurationFilePath, new MockFileData(string.Empty));
            context.Persistence.PreparationConfigurationExists().Returns(true);
            context.Persistence.GetPreparationConfigurationPath().Returns(AppPaths.DefaultSystemConfigurationFilePath);
            context.Persistence.LoadUserConfiguration(Arg.Any<CancellationToken>())
                .ThrowsAsync(new InvalidOperationException("System config error"));
            var config = CreateInternalConfig();
            context.Persistence.LoadPreparationConfiguration(Arg.Any<CancellationToken>())
                .Returns(config);

            var result = await context.Sut.PrepareConfiguration();

            result.Status.Should().Be(OperationStatus.Success);
            result.Message.Should().Contain("System config error");
        }

        [Fact]
        public async Task ShouldReturnError_WhenBothConfigsFail()
        {
            var context = new SystemConfigurationServiceContext();
            context.FileSystem.AddFile(AppPaths.SystemConfigurationFilePath, new MockFileData(string.Empty));
            context.Persistence.UserConfigurationExists().Returns(true);
            context.Persistence.GetUserConfigurationPath().Returns(AppPaths.SystemConfigurationFilePath);
            context.FileSystem.AddFile(AppPaths.DefaultSystemConfigurationFilePath, new MockFileData(string.Empty));
            context.Persistence.PreparationConfigurationExists().Returns(true);
            context.Persistence.GetPreparationConfigurationPath().Returns(AppPaths.DefaultSystemConfigurationFilePath);
            context.Persistence.LoadUserConfiguration(default)
                .ThrowsAsyncForAnyArgs(new InvalidOperationException("Load failed"));
            context.Persistence.LoadPreparationConfiguration(default)
                .ThrowsAsyncForAnyArgs(new InvalidOperationException("Load failed"));

            var result = await context.Sut.PrepareConfiguration();

            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().Contain("Load failed");
        }

        [Fact]
        public async Task ShouldReturnError_WhenAdapterReturnsWarning()
        {
            var context = new SystemConfigurationServiceContext();
            context.Persistence.UserConfigurationExists().Returns(true);
            context.Persistence.PreparationConfigurationExists().Returns(true);
            var config = CreateInternalConfig();
            context.Persistence.LoadUserConfiguration(default).ReturnsForAnyArgs(config);
            context.Persistence.LoadPreparationConfiguration(default).ReturnsForAnyArgs(config);
            context.ScriptHandler.ExecuteSetNetworkDNSSettingsScript(default!)
                .ReturnsForAnyArgs(OperationStatus.Warning);

            var result = await context.Sut.PrepareConfiguration();

            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().Contain("Prepare system configuration not possible.");
        }

        [Fact]
        public async Task ShouldReturnError_WhenAdapterReturnsError()
        {
            var context = new SystemConfigurationServiceContext();
            context.Persistence.UserConfigurationExists().Returns(true);
            context.Persistence.PreparationConfigurationExists().Returns(true);
            var config = CreateInternalConfig();
            context.Persistence.LoadUserConfiguration(default).ReturnsForAnyArgs(config);
            context.Persistence.LoadPreparationConfiguration(default).ReturnsForAnyArgs(config);
            context.ScriptHandler.ExecuteSetNetworkDNSSettingsScript(default!)
                .ReturnsForAnyArgs(OperationStatus.Error);

            var result = await context.Sut.PrepareConfiguration();

            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().Contain("Prepare system configuration not possible.");
        }

        [Fact]
        public async Task ShouldReturnError_WhenExceptionThrownDuringPrepare()
        {
            var context = new SystemConfigurationServiceContext();
            context.FileSystem.AddFile(AppPaths.SystemConfigurationFilePath, new MockFileData(string.Empty));
            context.Persistence.UserConfigurationExists().Returns(true);
            context.Persistence.GetUserConfigurationPath().Returns(AppPaths.SystemConfigurationFilePath);
            context.FileSystem.AddFile(AppPaths.DefaultSystemConfigurationFilePath, new MockFileData(string.Empty));
            context.Persistence.PreparationConfigurationExists().Returns(true);
            context.Persistence.GetPreparationConfigurationPath().Returns(AppPaths.DefaultSystemConfigurationFilePath);
            var config = CreateInternalConfig();
            context.Persistence.LoadUserConfiguration(default).ReturnsForAnyArgs(config);
            context.Persistence.LoadPreparationConfiguration(default).ReturnsForAnyArgs(config);
            context.ScriptHandler.ExecuteSetNetworkDNSSettingsScript(default!)
                .ThrowsAsyncForAnyArgs(new InvalidOperationException("Exception during prepare"));

            var result = await context.Sut.PrepareConfiguration();

            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().Contain("Exception during prepare");
        }
    }

    public sealed class SetConfiguration
    {
        [Fact]
        public async Task ShouldReturnError_WhenGetConfigurationFails()
        {
            var context = new SystemConfigurationServiceContext();
            context.ScriptHandler.ExecuteGetNetworkInterfacesCommonInformationScript()
                .ThrowsAsync(new InvalidOperationException("Test exception"));
            var proposedConfig = CreateSharedConfig();

            var result = await context.Sut.SetConfiguration(proposedConfig);

            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().Contain("Test exception");
        }

        [Fact]
        public async Task ShouldReturnError_WhenGetConfigurationReturnsError()
        {
            var context = new SystemConfigurationServiceContext();
            context.ScriptHandler.ExecuteGetNetworkInterfacesCommonInformationScript()
                .ReturnsForAnyArgs((OperationStatus.Error, []));
            var proposedConfig = CreateSharedConfig();

            var result = await context.Sut.SetConfiguration(proposedConfig);

            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().Contain("Get system configuration from scripts failed");
        }

        [Fact]
        public async Task ShouldReturnWarning_WhenGetConfigurationReturnsWarning()
        {
            var context = new SystemConfigurationServiceContext();
            context.ScriptHandler.ExecuteGetNetworkInterfacesCommonInformationScript()
                .ReturnsForAnyArgs((OperationStatus.Warning, []));
            var proposedConfig = CreateSharedConfig();

            var result = await context.Sut.SetConfiguration(proposedConfig);

            result.Status.Should().Be(OperationStatus.Warning);
            result.Message.Should().Contain("Get system configuration from scripts completed with warnings.");
        }

        [Fact]
        public async Task ShouldReturnError_WhenValidationFails()
        {
            var context = new SystemConfigurationServiceContext();
            var proposedConfig = CreateSharedConfig();
            proposedConfig.NetworkInterfacesSettings.NetworkInterfaces.Clear();

            var result = await context.Sut.SetConfiguration(proposedConfig);

            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().Contain("No network interfaces specified.");
        }

        [Fact]
        public async Task ShouldReturnSuccess_WhenSetSucceeds()
        {
            var context = new SystemConfigurationServiceContext();
            context.Persistence.SaveUserConfiguration(default!, default)
                .ReturnsForAnyArgs(Task.CompletedTask);
            var proposedConfig = CreateSharedConfig();

            var result = await context.Sut.SetConfiguration(proposedConfig);

            result.Status.Should().Be(OperationStatus.Success);
            result.Message.Should().NotBeEmpty();
        }

        [Fact]
        public async Task ShouldPersistConfiguration_WhenShouldPersistIsTrue()
        {
            var context = new SystemConfigurationServiceContext();
            context.Persistence.SaveUserConfiguration(default!, default)
                .ReturnsForAnyArgs(Task.CompletedTask);
            var proposedConfig = CreateSharedConfig();

            await context.Sut.SetConfiguration(proposedConfig, shouldPersistConfiguration: true);

            await context.Persistence.Received(1).SaveUserConfiguration(
                Arg.Any<InternalSystemConfiguration>(),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ShouldNotPersistConfiguration_WhenShouldPersistIsFalse()
        {
            var context = new SystemConfigurationServiceContext();
            var proposedConfig = CreateSharedConfig();

            await context.Sut.SetConfiguration(proposedConfig, shouldPersistConfiguration: false);

            await context.Persistence.DidNotReceive().SaveUserConfiguration(
                Arg.Any<InternalSystemConfiguration>(),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ShouldReturnError_WhenPersistenceFails()
        {
            var context = new SystemConfigurationServiceContext();
            context.Persistence.SaveUserConfiguration(default!, default)
                .ThrowsAsyncForAnyArgs(new InvalidOperationException("Persist failed"));
            var proposedConfig = CreateSharedConfig();

            var result = await context.Sut.SetConfiguration(proposedConfig);

            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().Contain("Persist failed");
        }

        [Fact]
        public async Task ShouldRevertConfiguration_WhenSetConfigurationFails()
        {
            var context = new SystemConfigurationServiceContext();
            context.ScriptHandler.ExecuteSetNetworkDNSSettingsScript(default!)
                .ReturnsForAnyArgs(x => throw new InvalidOperationException("test exception"), x => OperationStatus.Success);
            var proposedConfig = CreateSharedConfig();

            var result = await context.Sut.SetConfiguration(proposedConfig);

            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().Contain("test exception");
            result.Message.Should().Contain("Revert");
        }

        [Fact]
        public async Task ShouldRevertConfiguration_WhenSetConfigurationReturnsError()
        {
            var context = new SystemConfigurationServiceContext();
            context.ScriptHandler.ExecuteSetNetworkDNSSettingsScript(default!)
                .ReturnsForAnyArgs(OperationStatus.Error);
            var proposedConfig = CreateSharedConfig();

            var result = await context.Sut.SetConfiguration(proposedConfig);

            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().Contain("Revert");
            result.Message.Should().Contain("setting system configuration failed");
        }

        [Fact]
        public async Task ShouldReturnWarning_WhenSetConfigurationReturnsWarning()
        {
            var context = new SystemConfigurationServiceContext();
            context.ScriptHandler.ExecuteSetNetworkDNSSettingsScript(default!)
                .ReturnsForAnyArgs(OperationStatus.Warning);
            var proposedConfig = CreateSharedConfig();

            var result = await context.Sut.SetConfiguration(proposedConfig);

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
        ScriptHandler.ExecuteGetInterfaceIPv4SettingsScript(default!).ReturnsForAnyArgs((OperationStatus.Success, new()));
        ScriptHandler.ExecuteGetInterfaceIPv4DHCPLeaseScript(default!).ReturnsForAnyArgs((OperationStatus.Success, new()));
        ScriptHandler.ExecuteGetNetworkDNSSettingsScript().ReturnsForAnyArgs((OperationStatus.Success, new()));
        ScriptHandler.ExecuteGetNetworkProxySettingsScript().ReturnsForAnyArgs((OperationStatus.Success, new()));
        ScriptHandler.ExecuteGetNetworkNTPSettingsScript().ReturnsForAnyArgs((OperationStatus.Success, new()));
        ScriptHandler.ExecuteGetSystemdUnitStateScript(default!).ReturnsForAnyArgs((OperationStatus.Success, new() { Name = ServiceName, }));
        ScriptHandler.ExecuteSetInterfaceIPv4SettingsScript(default!).ReturnsForAnyArgs(OperationStatus.Success);
        ScriptHandler.ExecuteSetNetworkDNSSettingsScript(default!).ReturnsForAnyArgs(OperationStatus.Success);
        ScriptHandler.ExecuteSetStaticHostsScript(default!).ReturnsForAnyArgs(OperationStatus.Success);
        ScriptHandler.ExecuteSetNetworkMulticastDNSSettingsScript(default!).ReturnsForAnyArgs(OperationStatus.Success);
        ScriptHandler.ExecuteSetHostnameScript(default!).ReturnsForAnyArgs(OperationStatus.Success);
        ScriptHandler.ExecuteSetNetworkProxySettingsScript(default!).ReturnsForAnyArgs(OperationStatus.Success);
        ScriptHandler.ExecuteSetNetworkNTPSettingsScript(default!).ReturnsForAnyArgs(OperationStatus.Success);
        ScriptHandler.ExecuteSetSystemdUnitStateScript(default!).ReturnsForAnyArgs(OperationStatus.Success);
        ScriptHandler.ExecuteApplyNetplanConfigurationScript().ReturnsForAnyArgs(OperationStatus.Success);
        Sut = new(FileSystem, Persistence, ScriptHandler);

        Environment.SetEnvironmentVariable("CONFIGURABLE_SERVICES", ServiceName);
    }
}
