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
    private const string NTPServer1 = "0.pool.ntp.org";
    private const string NTPServer2 = "1.pool.ntp.org";

    private static NetworkNTPConfigurationAdapter CreateSut(
        IScriptHandler scriptHandler,
        bool bypassHasChanges = false,
        bool bypassApplyConfig = false)
        => new(scriptHandler, bypassHasChanges, bypassApplyConfig);

    private static SystemConfiguration CreateSystemConfig(bool ntpServersEnabled, params string[] ntpServers)
       => new()
       {
           NetworkNTPSettings = new()
           {
               NTPServersEnabled = ntpServersEnabled,
               NTPServers = [.. ntpServers],
           },
       };

    public sealed class GetConfiguration
    {
        [Fact]
        public async Task ShouldReturnError_WhenScriptFails()
        {
            // Arrange
            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler.ExecuteGetNetworkNTPSettingsScript().Returns((OperationStatus.Error, new()));

            var config = CreateSystemConfig(false, [NTPServer1, NTPServer2]);
            var originalNTPSettings = config.NetworkNTPSettings;
            var sut = CreateSut(scriptHandler);

            // Act + Assert
            var status = await sut.GetConfiguration(config, CancellationToken.None);

            status.Should().Be(OperationStatus.Error);
            config.NetworkNTPSettings.Should().BeSameAs(originalNTPSettings);
        }

        [Fact]
        public async Task ShouldPopulateSettings_WhenSuccessful()
        {
            // Arrange
            var ntpSettings = new NetworkNTPSettings()
            {
                NTPServersEnabled = true,
                NTPServers = [NTPServer1, NTPServer2]
            };

            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler.ExecuteGetNetworkNTPSettingsScript().Returns((OperationStatus.Success, ntpSettings));

            var sut = CreateSut(scriptHandler);

            // Act + Assert
            var config = new SystemConfiguration();
            var status = await sut.GetConfiguration(config, CancellationToken.None);

            status.Should().Be(OperationStatus.Success);
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
            var current = CreateSystemConfig(true, NTPServer1);
            var proposed = CreateSystemConfig(true, NTPServer1);

            sut.HasChanges(current, proposed).Should().BeFalse();
        }

        [Theory]
        [InlineData(true, NTPServer2)]
        [InlineData(false, NTPServer1)]
        [InlineData(false, NTPServer2)]
        public void ShouldBeTrue_WhenSettingsDifferent(bool ntpEnabled, string ntpServer)
        {
            var scriptHandler = Substitute.For<IScriptHandler>();
            var sut = CreateSut(scriptHandler);
            var current = CreateSystemConfig(true, NTPServer1);
            var proposed = CreateSystemConfig(ntpEnabled, ntpServer);

            sut.HasChanges(current, proposed).Should().BeTrue();
        }

        [Theory]
        [InlineData(true, NTPServer2)]
        [InlineData(false, NTPServer1)]
        [InlineData(false, NTPServer2)]
        [InlineData(true, NTPServer1)]
        public void ShouldBeTrue_WhenBypassHasChangesFlagTrue(bool ntpEnabled, string ntpServer)
        {
            var scriptHandler = Substitute.For<IScriptHandler>();
            var sut = CreateSut(scriptHandler, bypassHasChanges: true);
            var current = CreateSystemConfig(true, NTPServer1);
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
                .Returns(Task.FromResult(OperationStatus.Error));

            var sut = CreateSut(scriptHandler, bypassHasChanges: true, bypassApplyConfig: false);
            var config = CreateSystemConfig(true, NTPServer1);

            // Act + Assert
            var status = await sut.SetConfiguration(config, CancellationToken.None);

            status.Should().Be(OperationStatus.Error);

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
                .Returns(Task.FromResult(OperationStatus.Success));

            scriptHandler
                .ExecuteSetSystemdUnitStateScript(Arg.Do<ScriptOptions>(so => systemdScriptOptions = so))
                .Returns(Task.FromResult(OperationStatus.Success));

            var sut = CreateSut(scriptHandler, bypassHasChanges: true, bypassApplyConfig: false);
            var config = CreateSystemConfig(true, NTPServer1);

            // Act
            var status = await sut.SetConfiguration(config, CancellationToken.None);

            // Assert
            status.Should().Be(OperationStatus.Success);

            // Prepare was executed
            await scriptHandler.Received(1).ExecuteSetNetworkNTPSettingsScript(Arg.Any<ScriptOptions>());
            ntpScriptOptions.Should().NotBeNull();
            ntpScriptOptions!.Options.Should().ContainInOrder(
                ScriptOptionNames.NetworkNTP.NTPServersEnabled, "true",
                ScriptOptionNames.NetworkNTP.NTPServers, NTPServer1);

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
                .Returns(Task.FromResult(OperationStatus.Success));

            var sut = CreateSut(scriptHandler, bypassHasChanges: true, bypassApplyConfig: true);
            var config = CreateSystemConfig(true, NTPServer1);

            // Act + Assert
            var status = await sut.SetConfiguration(config, CancellationToken.None);

            status.Should().Be(OperationStatus.Success);

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
                .Returns(Task.FromResult(OperationStatus.Success));

            var sut = CreateSut(scriptHandler, bypassHasChanges: true, bypassApplyConfig);
            var config = CreateSystemConfig(true); // without NTP server

            // Act + Assert
            var status = await sut.SetConfiguration(config, CancellationToken.None);

            ntpScriptOptions.Should().NotBeNull();
            ntpScriptOptions!.Options.Should().ContainInOrder(
                ScriptOptionNames.NetworkNTP.NTPServersEnabled, "true",
                ScriptOptionNames.NetworkNTP.NTPServers, "none");
        }
    }
}
