using AwesomeAssertions;
using HostManagement.Scripts;
using HostManagement.Shared.Communication.Enums;
using HostManagement.SystemConfig.Contracts;
using HostManagement.SystemConfig.Network;
using NSubstitute;
using Xunit;

namespace HostManagement.Tests.SystemConfig.Network;

public sealed class NetworkNTPConfigurationAdapterTests
{
    private const string Server1 = "0.pool.ntp.org";
    private const string Server2 = "1.pool.ntp.org";

    private static NetworkNTPConfigurationAdapter CreateSut(
        IScriptHandler handler,
        bool bypassHasChanges = false,
        bool bypassApplyConfig = false)
           => new(handler, bypassHasChanges, bypassApplyConfig);

    private static SystemConfiguration CreateSystemConfig(bool enabled, params string[] servers)
       => new()
       {
           NetworkNTPSettings = new()
           {
               Enabled = enabled,
               Servers = [.. servers]
           }
       };

    public sealed class GetConfiguration
    {
        [Fact]
        public async Task ShouldReturnError_WhenScriptFails()
        {
            // Arrange
            var handler = Substitute.For<IScriptHandler>();
            handler.ExecuteGetNetworkNTPSettingsScript().Returns((Result.TestErrorResult, new()));

            var config = CreateSystemConfig(false, [Server1, Server2]);
            var originalNTPSettings = config.NetworkNTPSettings;
            var sut = CreateSut(handler);

            // Act
            var result = await sut.GetConfiguration(config, CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Error);
            result.Messages.Should().Contain(Result.TestErrorMessage);
            config.NetworkNTPSettings.Should().BeSameAs(originalNTPSettings);
        }

        [Fact]
        public async Task ShouldPopulateSettings_WhenScriptSucceeds()
        {
            // Arrange
            var ntpSettings = new NetworkNTPSettings()
            {
                Enabled = true,
                Servers = [Server1, Server2]
            };

            var handler = Substitute.For<IScriptHandler>();
            handler.ExecuteGetNetworkNTPSettingsScript().Returns((Result.SuccessResult, ntpSettings));

            var config = new SystemConfiguration();
            var sut = CreateSut(handler);

            // Act
            var result = await sut.GetConfiguration(config, CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Success);
            result.Messages.Should().BeEmpty();
            config.NetworkNTPSettings.Should().BeSameAs(ntpSettings);
        }
    }

    public sealed class HasChanges
    {
        [Fact]
        public void ShouldReturnFalse_WhenSettingsEqual()
        {
            // Arrange
            var handler = Substitute.For<IScriptHandler>();
            var current = CreateSystemConfig(true, Server1);
            var proposed = CreateSystemConfig(true, Server1);
            var sut = CreateSut(handler);

            // Act
            var result = sut.HasChanges(current, proposed);

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData(true, Server2)]
        [InlineData(false, Server1)]
        [InlineData(false, Server2)]
        public void ShouldReturnTrue_WhenSettingsDiffer(bool ntpEnabled, string ntpServer)
        {
            // Arrange
            var handler = Substitute.For<IScriptHandler>();
            var current = CreateSystemConfig(true, Server1);
            var proposed = CreateSystemConfig(ntpEnabled, ntpServer);
            var sut = CreateSut(handler);

            // Act
            var result = sut.HasChanges(current, proposed);

            // Assert
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData(true, Server2)]
        [InlineData(false, Server1)]
        [InlineData(false, Server2)]
        [InlineData(true, Server1)]
        public void ShouldReturnTrue_WhenChangeCheckBypassed(bool ntpEnabled, string ntpServer)
        {
            // Arrange
            var handler = Substitute.For<IScriptHandler>();
            var current = CreateSystemConfig(true, Server1);
            var proposed = CreateSystemConfig(ntpEnabled, ntpServer);
            var sut = CreateSut(handler, bypassHasChanges: true);

            // Act
            var result = sut.HasChanges(current, proposed);

            // Assert
            result.Should().BeTrue();
        }
    }

    public sealed class SetConfiguration
    {
        [Fact]
        public async Task ShouldReturnErrorAndSkipApply_WhenPrepareFails()
        {
            // Arrange
            var handler = Substitute.For<IScriptHandler>();
            handler.ExecuteSetNetworkNTPSettingsScript(Arg.Any<ScriptOptions>()).Returns(Result.TestErrorResult);

            var config = CreateSystemConfig(true, Server1);
            var sut = CreateSut(handler, bypassHasChanges: true, bypassApplyConfig: false);

            // Act
            var result = await sut.SetConfiguration(config, CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Error);
            result.Messages.Should().Contain(Result.TestErrorMessage);

            await handler.DidNotReceive().ExecuteSetSystemdUnitStateScript(Arg.Any<ScriptOptions>());
        }

        [Fact]
        public async Task ShouldNotAddNTPScriptOptions_WhenEnabledNull()
        {
            // Arrange
            ScriptOptions? ntpScriptOptions = null;
            ScriptOptions? systemdScriptOptions = null;

            var handler = Substitute.For<IScriptHandler>();

            handler
                .ExecuteSetNetworkNTPSettingsScript(Arg.Do<ScriptOptions>(so => ntpScriptOptions = so))
                .Returns(Result.SuccessResult);

            handler
                .ExecuteSetSystemdUnitStateScript(Arg.Do<ScriptOptions>(so => systemdScriptOptions = so))
                .Returns(Result.SuccessResult);

            var config = new SystemConfiguration()
            {
                NetworkNTPSettings = new()
                {
                    Enabled = null,
                    Servers = [Server1, Server2]
                }
            };

            var sut = CreateSut(handler, bypassHasChanges: true, bypassApplyConfig: false);

            // Act
            var result = await sut.SetConfiguration(config, CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Success);
            result.Messages.Should().BeEmpty();

            await handler.DidNotReceive().ExecuteSetNetworkNTPSettingsScript(Arg.Any<ScriptOptions>());
            ntpScriptOptions.Should().BeNull();
        }

        [Fact]
        public async Task ShouldPrepareAndApply_WhenApplyNotBypassed()
        {
            // Arrange
            ScriptOptions? ntpScriptOptions = null;
            ScriptOptions? systemdScriptOptions = null;

            var handler = Substitute.For<IScriptHandler>();

            handler
                .ExecuteSetNetworkNTPSettingsScript(Arg.Do<ScriptOptions>(so => ntpScriptOptions = so))
                .Returns(Result.SuccessResult);

            handler
                .ExecuteSetSystemdUnitStateScript(Arg.Do<ScriptOptions>(so => systemdScriptOptions = so))
                .Returns(Result.SuccessResult);

            var config = CreateSystemConfig(true, Server1);
            var sut = CreateSut(handler, bypassHasChanges: true, bypassApplyConfig: false);

            // Act
            var result = await sut.SetConfiguration(config, CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Success);
            result.Messages.Should().BeEmpty();

            // Prepare was executed
            await handler.Received(1).ExecuteSetNetworkNTPSettingsScript(Arg.Any<ScriptOptions>());
            ntpScriptOptions.Should().NotBeNull();
            ntpScriptOptions.Options.Should().ContainInOrder(
                ScriptOptionNames.NetworkNTP.NTPServersEnabled, "true",
                ScriptOptionNames.NetworkNTP.NTPServers, Server1);

            // Apply was executed
            await handler.Received(1).ExecuteSetSystemdUnitStateScript(Arg.Any<ScriptOptions>());
            systemdScriptOptions.Should().NotBeNull();
            systemdScriptOptions.Options.Should().ContainInOrder(
                ScriptOptionNames.SetSystemdUnitState.Name, "systemd-timesyncd",
                ScriptOptionNames.SetSystemdUnitState.State, "Restart");
        }

        [Fact]
        public async Task ShouldPrepareAndSkipApply_WhenApplyBypassed()
        {
            // Arrange
            var handler = Substitute.For<IScriptHandler>();
            handler.ExecuteSetNetworkNTPSettingsScript(Arg.Any<ScriptOptions>()).Returns(Result.SuccessResult);

            var config = CreateSystemConfig(true, Server1);
            var sut = CreateSut(handler, bypassHasChanges: true, bypassApplyConfig: true);

            // Act
            var result = await sut.SetConfiguration(config, CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Success);
            result.Messages.Should().BeEmpty();

            // Prepare was executed
            await handler.Received(1).ExecuteSetNetworkNTPSettingsScript(Arg.Any<ScriptOptions>());

            // Apply was not executed
            await handler.DidNotReceive().ExecuteSetSystemdUnitStateScript(Arg.Any<ScriptOptions>());
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ShouldUseNonePlaceholder_WhenNTPServerListNull(bool bypassApplyConfig)
        {
            // Arrange
            ScriptOptions? capturedOptions = null;

            var handler = Substitute.For<IScriptHandler>();

            handler
                .ExecuteSetNetworkNTPSettingsScript(Arg.Do<ScriptOptions>(so => capturedOptions = so))
                .Returns(Result.SuccessResult);
            handler
                .ExecuteSetSystemdUnitStateScript(Arg.Any<ScriptOptions>())
                .Returns(Result.SuccessResult);

            var config = new SystemConfiguration()
            {
                NetworkNTPSettings = new()
                {
                    Enabled = true,
                    Servers = null
                }
            };

            var sut = CreateSut(handler, bypassHasChanges: true, bypassApplyConfig);

            // Act
            var result = await sut.SetConfiguration(config, CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Success);
            result.Messages.Should().BeEmpty();

            capturedOptions.Should().NotBeNull();
            capturedOptions.Options.Should().ContainInOrder(
                ScriptOptionNames.NetworkNTP.NTPServersEnabled, "true",
                ScriptOptionNames.NetworkNTP.NTPServers, "none");
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ShouldUseNonePlaceholder_WhenNTPServerListEmpty(bool bypassApplyConfig)
        {
            // Arrange
            ScriptOptions? capturedOptions = null;

            var handler = Substitute.For<IScriptHandler>();

            handler
                .ExecuteSetNetworkNTPSettingsScript(Arg.Do<ScriptOptions>(so => capturedOptions = so))
                .Returns(Result.SuccessResult);
            handler
                .ExecuteSetSystemdUnitStateScript(Arg.Any<ScriptOptions>())
                .Returns(Result.SuccessResult);

            var config = CreateSystemConfig(true); // without NTP server
            var sut = CreateSut(handler, bypassHasChanges: true, bypassApplyConfig);

            // Act
            var result = await sut.SetConfiguration(config, CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Success);
            result.Messages.Should().BeEmpty();

            capturedOptions.Should().NotBeNull();
            capturedOptions.Options.Should().ContainInOrder(
                ScriptOptionNames.NetworkNTP.NTPServersEnabled, "true",
                ScriptOptionNames.NetworkNTP.NTPServers, "none");
        }
    }
}
