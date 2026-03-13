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
    private static NetworkNTPConfigurationAdapter CreateSut(
        IScriptHandler scriptHandler,
        bool bypassHasChanges = false,
        bool bypassApplyConfig = false)
        => new(scriptHandler, bypassHasChanges, bypassApplyConfig);

    private static SystemConfiguration CreateConfig(bool enabled, params string[] servers)
       => new()
       {
           NetworkNTPSettings = new()
           {
               NTPServersEnabled = enabled,
               NTPServers = [.. servers],
           },
       };

    public sealed class GetConfiguration
    {
        [Fact]
        public async Task ShouldReturnError_WhenScriptFails()
        {
            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler.ExecuteGetNetworkNTPSettingsScript()
                .Returns((OperationStatus.Error, new()));
            var config = CreateConfig(false, "orig.server");
            var originalNetworkSettings = config.NetworkNTPSettings;
            var sut = CreateSut(scriptHandler);

            var status = await sut.GetConfiguration(config, CancellationToken.None);

            status.Should().Be(OperationStatus.Error);
            config.NetworkNTPSettings.Should().BeSameAs(originalNetworkSettings);
        }

        [Fact]
        public async Task ShouldPopulateSettings_WhenSuccessful()
        {
            var scriptHandler = Substitute.For<IScriptHandler>();
            NetworkNTPSettings scriptResult = new()
            {
                NTPServersEnabled = true,
                NTPServers = ["0.pool.ntp.org", "1.pool.ntp.org",],
            };
            scriptHandler.ExecuteGetNetworkNTPSettingsScript()
                .Returns((OperationStatus.Success, scriptResult));
            var sut = CreateSut(scriptHandler);
            SystemConfiguration config = new();

            var status = await sut.GetConfiguration(config, CancellationToken.None);

            status.Should().Be(OperationStatus.Success);
            config.NetworkNTPSettings.Should().BeSameAs(scriptResult);
        }
    }

    public sealed class HasChanges
    {
        [Fact]
        public void ShouldBeFalse_WhenSettingsEqual()
        {
            var scriptHandler = Substitute.For<IScriptHandler>();
            var sut = CreateSut(scriptHandler);
            var current = CreateConfig(true, "0.pool.ntp.org");
            var proposed = CreateConfig(true, "0.pool.ntp.org");

            sut.HasChanges(current, proposed).Should().BeFalse();
        }

        [Fact]
        public void ShouldBeTrue_WhenSettingsDifferent()
        {
            var scriptHandler = Substitute.For<IScriptHandler>();
            var sut = CreateSut(scriptHandler);
            var current = CreateConfig(true, "0.pool.ntp.org");
            var proposed = CreateConfig(true, "1.pool.ntp.org");

            sut.HasChanges(current, proposed).Should().BeTrue();
        }

        [Fact]
        public void ShouldBeTrue_WhenBypassFlagEnabled()
        {
            var scriptHandler = Substitute.For<IScriptHandler>();
            var sut = CreateSut(scriptHandler, bypassHasChanges: true);
            var current = CreateConfig(true, "0.pool.ntp.org");
            var proposed = CreateConfig(true, "0.pool.ntp.org");

            sut.HasChanges(current, proposed).Should().BeTrue();
        }
    }

    public sealed class SetConfiguration
    {
        [Fact]
        public async Task ShouldPrepareAndApply_WhenNotBypassed()
        {
            var scriptHandler = Substitute.For<IScriptHandler>();
            ScriptOptions? network = null;
            scriptHandler.ExecuteSetNetworkNTPSettingsScript(Arg.Do<ScriptOptions>(so => network = so))
                .Returns(Task.FromResult(OperationStatus.Success));
            ScriptOptions? systemd = null;
            scriptHandler.ExecuteSetSystemdUnitStateScript(Arg.Do<ScriptOptions>(so => systemd = so))
                .Returns(Task.FromResult(OperationStatus.Success));
            var sut = CreateSut(scriptHandler, bypassHasChanges: true, bypassApplyConfig: false);
            var config = CreateConfig(true, "2.pool.ntp.org");

            var status = await sut.SetConfiguration(config, CancellationToken.None);

            status.Should().Be(OperationStatus.Success);
            await scriptHandler.Received(1).ExecuteSetNetworkNTPSettingsScript(Arg.Any<ScriptOptions>());
            network.Should().NotBeNull();
            network!.Options.Should().ContainInOrder(
                ScriptOptionNames.NetworkNTP.NTPServersEnabled,
                "true",
                ScriptOptionNames.NetworkNTP.NTPServers,
                "2.pool.ntp.org");
            await scriptHandler.Received(1).ExecuteSetSystemdUnitStateScript(Arg.Any<ScriptOptions>());
            systemd.Should().NotBeNull();
            systemd!.Options.Should().ContainInOrder(
                ScriptOptionNames.SetSystemdUnitState.Name,
                "systemd-timesyncd",
                ScriptOptionNames.SetSystemdUnitState.State,
                "Restart");
        }

        [Fact]
        public async Task ShouldReturnErrorAndSkipApply_WhenPrepareFails()
        {
            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler.ExecuteSetNetworkNTPSettingsScript(Arg.Any<ScriptOptions>())
                .Returns(Task.FromResult(OperationStatus.Error));
            var sut = CreateSut(scriptHandler, bypassHasChanges: true, bypassApplyConfig: false);
            var config = CreateConfig(true, "srv");

            var status = await sut.SetConfiguration(config, CancellationToken.None);

            status.Should().Be(OperationStatus.Error);
            await scriptHandler.DidNotReceive().ExecuteSetSystemdUnitStateScript(Arg.Any<ScriptOptions>());
        }

        [Fact]
        public async Task ShouldSkipApply_WhenBypassApplyConfigTrue()
        {
            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler.ExecuteSetNetworkNTPSettingsScript(Arg.Any<ScriptOptions>())
                .Returns(Task.FromResult(OperationStatus.Success));
            var sut = CreateSut(scriptHandler, bypassHasChanges: true, bypassApplyConfig: true);
            var config = CreateConfig(true, "3.pool.ntp.org");

            var status = await sut.SetConfiguration(config, CancellationToken.None);

            status.Should().Be(OperationStatus.Success);
            await scriptHandler.Received(1).ExecuteSetNetworkNTPSettingsScript(Arg.Any<ScriptOptions>());
            await scriptHandler.DidNotReceive().ExecuteSetSystemdUnitStateScript(Arg.Any<ScriptOptions>());
        }

        [Fact]
        public async Task PrepareShouldUseNonePlaceholder_WhenServerListEmpty()
        {
            var scriptHandler = Substitute.For<IScriptHandler>();
            ScriptOptions? network = null;
            scriptHandler.ExecuteSetNetworkNTPSettingsScript(Arg.Do<ScriptOptions>(so => network = so))
                .Returns(Task.FromResult(OperationStatus.Success));
            var sut = CreateSut(scriptHandler, bypassHasChanges: true, bypassApplyConfig: true);
            var config = CreateConfig(true); // keine Server

            var status = await sut.SetConfiguration(config, CancellationToken.None);

            network.Should().NotBeNull();
            network!.Options.Should().ContainInOrder(
                ScriptOptionNames.NetworkNTP.NTPServersEnabled,
                "true",
                ScriptOptionNames.NetworkNTP.NTPServers,
                "none");
        }
    }
}
