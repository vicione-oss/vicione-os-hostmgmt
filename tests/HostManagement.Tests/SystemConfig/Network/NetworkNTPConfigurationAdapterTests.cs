using AwesomeAssertions;
using HostManagement.Scripts;
using HostManagement.Shared.Communication.Enums;
using HostManagement.SystemConfig.Contracts;
using HostManagement.SystemConfig.Network;
using NSubstitute;
using Xunit;

namespace HostManagement.Tests.SystemConfig.Network;

public class NetworkNTPConfigurationAdapterTests
{
    private const string Server1 = "0.pool.ntp.org";
    private const string Server2 = "1.pool.ntp.org";

    private static NetworkNTPConfigurationAdapter CreateSut(
        IScriptHandler scriptHandler,
        bool bypassHasChanges = false,
        bool bypassApplyConfig = false)
            => new(scriptHandler, bypassHasChanges, bypassApplyConfig);

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
            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler.ExecuteGetNetworkNTPSettingsScript().Returns((Result.ErrorResult, new()));

            var config = CreateSystemConfig(false, [Server1, Server2]);
            var originalNTPSettings = config.NetworkNTPSettings;
            var sut = CreateSut(scriptHandler);

            // Act + Assert
            var result = await sut.GetConfiguration(config, CancellationToken.None);

            result.OperationStatus.Should().Be(OperationStatus.Error);
            result.Messages.Should().Contain(Result.ErrorMessage);
            config.NetworkNTPSettings.Should().BeSameAs(originalNTPSettings);
        }

        [Fact]
        public async Task ShouldPopulateSettings_WhenSuccessful()
        {
            // Arrange
            var ntpSettings = new NetworkNTPSettings()
            {
                Enabled = true,
                Servers = [Server1, Server2]
            };

            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler.ExecuteGetNetworkNTPSettingsScript().Returns((Result.SuccessResult, ntpSettings));

            var sut = CreateSut(scriptHandler);

            // Act + Assert
            var config = new SystemConfiguration();
            var result = await sut.GetConfiguration(config, CancellationToken.None);

            result.OperationStatus.Should().Be(OperationStatus.Success);
            result.Messages.Should().BeEmpty();
            config.NetworkNTPSettings.Should().BeSameAs(ntpSettings);
        }
    }

    public sealed class HasChanges
    {
        [Fact]
        public void ShouldBeFalse_WhenSettingsEqual()
        {
            var scriptHandler = Substitute.For<IScriptHandler>();
            var sut = CreateSut(scriptHandler);
            var current = CreateSystemConfig(true, Server1);
            var proposed = CreateSystemConfig(true, Server1);

            sut.HasChanges(current, proposed).Should().BeFalse();
        }

        [Theory]
        [InlineData(true, Server2)]
        [InlineData(false, Server1)]
        [InlineData(false, Server2)]
        public void ShouldBeTrue_WhenSettingsDifferent(bool ntpEnabled, string ntpServer)
        {
            var scriptHandler = Substitute.For<IScriptHandler>();
            var sut = CreateSut(scriptHandler);
            var current = CreateSystemConfig(true, Server1);
            var proposed = CreateSystemConfig(ntpEnabled, ntpServer);

            sut.HasChanges(current, proposed).Should().BeTrue();
        }

        [Theory]
        [InlineData(true, Server2)]
        [InlineData(false, Server1)]
        [InlineData(false, Server2)]
        [InlineData(true, Server1)]
        public void ShouldBeTrue_WhenBypassHasChangesFlagTrue(bool ntpEnabled, string ntpServer)
        {
            var scriptHandler = Substitute.For<IScriptHandler>();
            var sut = CreateSut(scriptHandler, bypassHasChanges: true);
            var current = CreateSystemConfig(true, Server1);
            var proposed = CreateSystemConfig(ntpEnabled, ntpServer);

            sut.HasChanges(current, proposed).Should().BeTrue();
        }
    }

    public sealed class SetConfiguration
    {
        [Fact]
        public async Task ShouldReturnErrorAndSkipApply_WhenPrepareFails()
        {
            // Arrange
            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler
                .ExecuteSetNetworkNTPSettingsScript(Arg.Any<ScriptOptions>())
                .Returns(Result.ErrorResult);

            var sut = CreateSut(scriptHandler, bypassHasChanges: true, bypassApplyConfig: false);
            var config = CreateSystemConfig(true, Server1);

            // Act + Assert
            var result = await sut.SetConfiguration(config, CancellationToken.None);

            result.OperationStatus.Should().Be(OperationStatus.Error);
            result.Messages.Should().Contain(Result.ErrorMessage);

            // Apply was not executed
            await scriptHandler.DidNotReceive().ExecuteSetSystemdUnitStateScript(Arg.Any<ScriptOptions>());
        }

        [Fact]
        public async Task ShouldPrepareAndApply_WhenBypassApplyConfigFlagFalse()
        {
            ScriptOptions? ntpScriptOptions = null;
            ScriptOptions? systemdScriptOptions = null;

            var scriptHandler = Substitute.For<IScriptHandler>();

            scriptHandler
                .ExecuteSetNetworkNTPSettingsScript(Arg.Do<ScriptOptions>(so => ntpScriptOptions = so))
                .Returns(Result.SuccessResult);

            scriptHandler
                .ExecuteSetSystemdUnitStateScript(Arg.Do<ScriptOptions>(so => systemdScriptOptions = so))
                .Returns(Result.SuccessResult);

            var sut = CreateSut(scriptHandler, bypassHasChanges: true, bypassApplyConfig: false);
            var config = CreateSystemConfig(true, Server1);

            // Act
            var result = await sut.SetConfiguration(config, CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Success);
            result.Messages.Should().BeEmpty();

            // Prepare was executed
            await scriptHandler.Received(1).ExecuteSetNetworkNTPSettingsScript(Arg.Any<ScriptOptions>());
            ntpScriptOptions.Should().NotBeNull();
            ntpScriptOptions!.Options.Should().ContainInOrder(
                ScriptOptionNames.NetworkNTP.NTPServersEnabled, "true",
                ScriptOptionNames.NetworkNTP.NTPServers, Server1);

            // Apply was executed
            await scriptHandler.Received(1).ExecuteSetSystemdUnitStateScript(Arg.Any<ScriptOptions>());
            systemdScriptOptions.Should().NotBeNull();
            systemdScriptOptions!.Options.Should().ContainInOrder(
                ScriptOptionNames.SetSystemdUnitState.Name, "systemd-timesyncd",
                ScriptOptionNames.SetSystemdUnitState.State, "Restart");
        }

        [Fact]
        public async Task ShouldPrepareAndSkipApply_WhenBypassApplyConfigFlagTrue()
        {
            // Arrange
            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler
                .ExecuteSetNetworkNTPSettingsScript(Arg.Any<ScriptOptions>())
                .Returns(Result.SuccessResult);

            var sut = CreateSut(scriptHandler, bypassHasChanges: true, bypassApplyConfig: true);
            var config = CreateSystemConfig(true, Server1);

            // Act + Assert
            var result = await sut.SetConfiguration(config, CancellationToken.None);

            result.OperationStatus.Should().Be(OperationStatus.Success);
            result.Messages.Should().BeEmpty();

            // Prepare was executed
            await scriptHandler.Received(1).ExecuteSetNetworkNTPSettingsScript(Arg.Any<ScriptOptions>());

            // Apply was not executed
            await scriptHandler.DidNotReceive().ExecuteSetSystemdUnitStateScript(Arg.Any<ScriptOptions>());
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ShouldUseNonePlaceholder_WhenServerListEmpty(bool bypassApplyConfig)
        {
            // Arrange
            ScriptOptions? ntpScriptOptions = null;

            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler
                .ExecuteSetNetworkNTPSettingsScript(Arg.Do<ScriptOptions>(so => ntpScriptOptions = so))
                .Returns(Result.SuccessResult);
            scriptHandler
                .ExecuteSetSystemdUnitStateScript(Arg.Any<ScriptOptions>())
                .Returns(Result.SuccessResult);

            var sut = CreateSut(scriptHandler, bypassHasChanges: true, bypassApplyConfig);
            var config = CreateSystemConfig(true); // without NTP server

            // Act + Assert
            var result = await sut.SetConfiguration(config, CancellationToken.None);

            result.OperationStatus.Should().Be(OperationStatus.Success);
            result.Messages.Should().BeEmpty();

            ntpScriptOptions.Should().NotBeNull();
            ntpScriptOptions!.Options.Should().ContainInOrder(
                ScriptOptionNames.NetworkNTP.NTPServersEnabled, "true",
                ScriptOptionNames.NetworkNTP.NTPServers, "none");
        }
    }
}
