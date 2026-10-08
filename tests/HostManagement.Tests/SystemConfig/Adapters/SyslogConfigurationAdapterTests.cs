using AwesomeAssertions;
using HostManagement.Scripts;
using HostManagement.Shared.Communication.Enums;
using HostManagement.SystemConfig;
using HostManagement.SystemConfig.Adapters;
using HostManagement.SystemConfig.Contracts;
using NSubstitute;
using Xunit;

namespace HostManagement.Tests.SystemConfig.Adapters;

public sealed class SyslogConfigurationAdapterTests
{
    private const string NetlogdServiceName = "systemd-netlogd.service";
    private const bool SyslogEnabled = true;
    private const string SyslogAddress = "192.168.1.100:514";
    private const SyslogTransportProtocol SyslogProtocol = SyslogTransportProtocol.Tcp;
    private const SyslogLogFormat SyslogFormat = SyslogLogFormat.Rfc5424;
    private const int SyslogRetrySec = 21;
    private const SyslogFacility Facility = SyslogFacility.Auth;
    private const SyslogLevel Level = SyslogLevel.Alert;

    private static SyslogConfigurationAdapter CreateSut(
        IScriptHandler handler,
        bool bypassHasChanges = false,
        bool bypassApply = false)
           => new(handler, bypassHasChanges, bypassApply);

    private static SystemConfiguration CreateSystemConfig(SyslogSettings? syslog = null, bool isServiceEnabled = true)
    {
        return new()
        {
            Syslog = syslog,
            Services =
            [
                new ServiceDetail()
                {
                    Name = NetlogdServiceName,
                    State = isServiceEnabled ? ServiceState.Enabled : ServiceState.Disabled
                }
            ]
        };
    }

    public sealed class GetConfiguration
    {
        [Fact]
        public async Task ShouldReturnError_WhenGetSyslogSettingsFails()
        {
            // Arrange
            var handler = Substitute.For<IScriptHandler>();
            handler.ExecuteGetSyslogSettingsScript().Returns((Result.TestErrorResult, new SyslogSettings()));

            var currentSyslog = new SyslogSettings()
            {
                Enabled = SyslogEnabled,
                Address = SyslogAddress,
                TransportProtocol = SyslogProtocol,
                LogFormat = SyslogFormat,
                ConnectionRetrySeconds = SyslogRetrySec,
                ExcludeSyslogFacilities = [Facility],
                ExcludeSyslogLevels = [Level]
            };

            var currentConfig = CreateSystemConfig(currentSyslog);
            var sut = CreateSut(handler);

            // Act
            var result = await sut.GetConfiguration(currentConfig, CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Error);
            result.Messages.Should().Contain(Result.TestErrorMessage);
            currentConfig.Syslog.Should().BeSameAs(currentSyslog);

            await handler.Received(1).ExecuteGetSyslogSettingsScript();
        }

        [Fact]
        public async Task ShouldGetConfig_WhenGetSyslogSettingsSucceeds()
        {
            // Arrange
            var currentSyslog = new SyslogSettings();

            var gotSyslog = new SyslogSettings()
            {
                Enabled = SyslogEnabled,
                Address = SyslogAddress,
                TransportProtocol = SyslogProtocol,
                LogFormat = SyslogFormat,
                ConnectionRetrySeconds = SyslogRetrySec,
                ExcludeSyslogFacilities = [Facility],
                ExcludeSyslogLevels = [Level]
            };

            var handler = Substitute.For<IScriptHandler>();
            handler.ExecuteGetSyslogSettingsScript().Returns((Result.SuccessResult, gotSyslog));

            var currentConfig = CreateSystemConfig(currentSyslog);
            var sut = CreateSut(handler);

            // Act
            var result = await sut.GetConfiguration(currentConfig, CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Success);
            currentConfig.Syslog.Should().BeSameAs(gotSyslog);

            await handler.Received(1).ExecuteGetSyslogSettingsScript();
        }
    }

    public sealed class HasChanges
    {
        [Fact]
        internal void ShouldReturnFalse_WhenSettingsEqual()
        {
            // Arrange
            var handler = Substitute.For<IScriptHandler>();
            var current = CreateSystemConfig(new() { Enabled = true, Address = "192.168.1.100:514" });
            var proposed = CreateSystemConfig(new() { Enabled = true, Address = "192.168.1.100:514" });
            var sut = CreateSut(handler);

            // Act
            var result = sut.HasChanges(current, proposed);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        internal void ShouldReturnTrue_WhenSettingsDiffer()
        {
            // Arrange
            var handler = Substitute.For<IScriptHandler>();
            var current = CreateSystemConfig(new() { Enabled = true, Address = "192.168.1.100:514" });
            var proposed = CreateSystemConfig(new() { Enabled = false, Address = "192.168.1.100:514" });
            var sut = CreateSut(handler);

            // Act
            var result = sut.HasChanges(current, proposed);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        internal void ShouldReturnTrue_WhenChangeCheckBypassed()
        {
            // Arrange
            var handler = Substitute.For<IScriptHandler>();
            var current = CreateSystemConfig(new() { Enabled = true, Address = "192.168.1.100:514" });
            var proposed = CreateSystemConfig(new() { Enabled = true, Address = "192.168.1.100:514" });
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
        internal async Task ShouldReturnError_WhenSetSyslogSettingsFails()
        {
            // Arrange
            var handler = Substitute.For<IScriptHandler>();
            handler.ExecuteSetSyslogSettingsScript(Arg.Any<ScriptOptions>()).Returns(Result.TestErrorResult);

            var proposed = CreateSystemConfig(new SyslogSettings()
            {
                Enabled = SyslogEnabled,
                Address = SyslogAddress,
                TransportProtocol = SyslogProtocol,
                LogFormat = SyslogFormat,
                ConnectionRetrySeconds = SyslogRetrySec,
                ExcludeSyslogFacilities = [Facility],
                ExcludeSyslogLevels = [Level]
            });

            var sut = CreateSut(handler/*, bypassHasChanges: true*/);

            // Act
            var result = await sut.SetConfiguration(proposed, CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Error);
            result.Messages.Should().Contain(Result.TestErrorMessage);

            // PrepareConfiguration failed
            await handler.Received(1).ExecuteSetSyslogSettingsScript(Arg.Any<ScriptOptions>());

            // ApplyConfiguration was not called
            await handler.DidNotReceive().ExecuteSetSystemdUnitStateScript(Arg.Any<ScriptOptions>());
        }

        [Fact]
        internal async Task ShouldPassNoOptions_WhenSyslogSettingsAreNull()
        {
            // Arrange
            List<ScriptOptions> capturedOptions = [];

            var handler = Substitute.For<IScriptHandler>();
            handler.ExecuteSetSyslogSettingsScript(Arg.Do<ScriptOptions>(capturedOptions.Add))
                .Returns(Result.SuccessResult);

            var proposed = CreateSystemConfig(syslog: null);
            var sut = CreateSut(handler, bypassApply: true);

            // Act
            var result = await sut.SetConfiguration(proposed, CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Success);
            result.Messages.Should().BeEmpty();

            await handler.DidNotReceive().ExecuteSetSyslogSettingsScript(Arg.Any<ScriptOptions>());
            capturedOptions.Should().BeEmpty();
        }

        [Fact]
        internal async Task ShouldPassClearOption_WhenSettingsAreDefault()
        {
            // Arrange
            List<ScriptOptions> capturedOptions = [];

            var handler = Substitute.For<IScriptHandler>();
            handler.ExecuteSetSyslogSettingsScript(Arg.Do<ScriptOptions>(capturedOptions.Add))
                .Returns(Result.SuccessResult);

            var sharedProposed = new Shared.Contracts.SystemConfiguration();
            var proposedAfterContractsTranslation = sharedProposed.ToInternalContracts();

            var sut = CreateSut(handler, bypassApply: true);

            // Act
            var result = await sut.SetConfiguration(proposedAfterContractsTranslation, CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Success);
            result.Messages.Should().BeEmpty();

            await handler.Received(1).ExecuteSetSyslogSettingsScript(Arg.Any<ScriptOptions>());

            capturedOptions.Should().SatisfyRespectively(
                so => so.Options.Should().ContainSingle().Which.Should().Be(ScriptOptionNames.Syslog.Clear));
        }

        [Fact]
        internal async Task ShouldPassNoOptions_WhenSyslogSettingsAreIncomplete()
        {
            // Arrange
            List<ScriptOptions> capturedOptions = [];

            var handler = Substitute.For<IScriptHandler>();
            handler.ExecuteSetSyslogSettingsScript(Arg.Do<ScriptOptions>(capturedOptions.Add))
                .Returns(Result.SuccessResult);

            var proposed = CreateSystemConfig(new SyslogSettings()
            {
                Enabled = SyslogEnabled,
                Address = SyslogAddress
            });

            var sut = CreateSut(handler, bypassApply: true);

            // Act
            var result = await sut.SetConfiguration(proposed, CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Success);
            result.Messages.Should().BeEmpty();

            await handler.DidNotReceive().ExecuteSetSyslogSettingsScript(Arg.Any<ScriptOptions>());
            capturedOptions.Should().BeEmpty();
        }

        [Fact]
        internal async Task ShouldOmitExclusionOptions_WhenExclusionCollectionsAreEmpty()
        {
            // Arrange
            List<ScriptOptions> capturedOptions = [];

            var handler = Substitute.For<IScriptHandler>();
            handler.ExecuteSetSyslogSettingsScript(Arg.Do<ScriptOptions>(capturedOptions.Add))
                .Returns(Result.SuccessResult);

            var proposed = CreateSystemConfig(new SyslogSettings()
            {
                Enabled = SyslogEnabled,
                Address = SyslogAddress,
                TransportProtocol = SyslogProtocol,
                LogFormat = SyslogFormat,
                ConnectionRetrySeconds = SyslogRetrySec,
                ExcludeSyslogFacilities = [],
                ExcludeSyslogLevels = []
            });

            var sut = CreateSut(handler, bypassApply: true);

            // Act
            var result = await sut.SetConfiguration(proposed, CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Success);
            result.Messages.Should().BeEmpty();

            await handler.Received(1).ExecuteSetSyslogSettingsScript(Arg.Any<ScriptOptions>());

            capturedOptions.Should().SatisfyRespectively(
                so => so.Options.Should().ContainInOrder(
                    ScriptOptionNames.Syslog.Enabled, "true",
                    ScriptOptionNames.Syslog.Address, SyslogAddress,
                    ScriptOptionNames.Syslog.Protocol, "tcp",
                    ScriptOptionNames.Syslog.LogFormat, "rfc5424",
                    ScriptOptionNames.Syslog.ConnectionRetrySec, SyslogRetrySec.ToString()));
        }

        [Fact]
        internal async Task ShouldPassAllSyslogOptions_WhenSettingsAreComplete()
        {
            // Arrange
            List<ScriptOptions> capturedOptions = [];

            var handler = Substitute.For<IScriptHandler>();
            handler.ExecuteSetSyslogSettingsScript(Arg.Do<ScriptOptions>(capturedOptions.Add))
                .Returns(Result.SuccessResult);

            var proposed = CreateSystemConfig(new SyslogSettings()
            {
                Enabled = SyslogEnabled,
                Address = SyslogAddress,
                TransportProtocol = SyslogProtocol,
                LogFormat = SyslogFormat,
                ConnectionRetrySeconds = SyslogRetrySec,
                ExcludeSyslogFacilities = [Facility, SyslogFacility.SolarisCron],
                ExcludeSyslogLevels = [Level, SyslogLevel.Info]
            });

            var sut = CreateSut(handler, bypassApply: true);

            // Act
            var result = await sut.SetConfiguration(proposed, CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Success);
            result.Messages.Should().BeEmpty();

            await handler.Received(1).ExecuteSetSyslogSettingsScript(Arg.Any<ScriptOptions>());

            await handler.DidNotReceive().ExecuteSetSystemdUnitStateScript(Arg.Any<ScriptOptions>());

            capturedOptions.Should().SatisfyRespectively(
                so => so.Options.Should().ContainInOrder(
                    ScriptOptionNames.Syslog.Enabled, "true",
                    ScriptOptionNames.Syslog.Address, SyslogAddress,
                    ScriptOptionNames.Syslog.Protocol, "tcp",
                    ScriptOptionNames.Syslog.LogFormat, "rfc5424",
                    ScriptOptionNames.Syslog.ConnectionRetrySec, SyslogRetrySec.ToString(),
                    ScriptOptionNames.Syslog.ExcludeSyslogFacility, "auth", "solaris-cron",
                    ScriptOptionNames.Syslog.ExcludeSyslogLevel, "alert", "info"));
        }

        [Fact]
        internal async Task ShouldReturnError_WhenRestartingNetlogdFails()
        {
            // Arrange
            var handler = Substitute.For<IScriptHandler>();
            handler.ExecuteSetSyslogSettingsScript(Arg.Any<ScriptOptions>()).Returns(Result.SuccessResult);
            handler.ExecuteSetSystemdUnitStateScript(Arg.Any<ScriptOptions>()).Returns(Result.TestErrorResult);

            var proposed = CreateSystemConfig(new SyslogSettings()
            {
                Enabled = SyslogEnabled,
                Address = SyslogAddress,
                TransportProtocol = SyslogProtocol,
                LogFormat = SyslogFormat,
                ConnectionRetrySeconds = SyslogRetrySec,
                ExcludeSyslogFacilities = [Facility],
                ExcludeSyslogLevels = [Level]
            });

            var sut = CreateSut(handler);

            // Act
            var result = await sut.SetConfiguration(proposed, CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Error);
            result.Messages.Should().Contain(Result.TestErrorMessage);

            await handler.Received(1).ExecuteSetSyslogSettingsScript(Arg.Any<ScriptOptions>());
            await handler.Received(1).ExecuteSetSystemdUnitStateScript(Arg.Any<ScriptOptions>());
        }

        [Fact]
        internal async Task ShouldNotRestartNetlogd_WhenPreparationSucceedsButServiceDisabled()
        {
            // Arrange
            List<ScriptOptions> capturedOptions = [];

            var handler = Substitute.For<IScriptHandler>();
            handler.ExecuteSetSyslogSettingsScript(Arg.Any<ScriptOptions>()).Returns(Result.SuccessResult);
            handler.ExecuteSetSystemdUnitStateScript(Arg.Do<ScriptOptions>(capturedOptions.Add))
                .Returns(Result.SuccessResult);

            var proposed = CreateSystemConfig(new SyslogSettings()
            {
                Enabled = SyslogEnabled,
                Address = SyslogAddress,
                TransportProtocol = SyslogProtocol,
                LogFormat = SyslogFormat,
                ConnectionRetrySeconds = SyslogRetrySec,
                ExcludeSyslogFacilities = [Facility],
                ExcludeSyslogLevels = [Level]
            },
            isServiceEnabled: false);

            var sut = CreateSut(handler);

            // Act
            var result = await sut.SetConfiguration(proposed, CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Success);
            result.Messages.Should().BeEmpty();

            await handler.Received(1).ExecuteSetSyslogSettingsScript(Arg.Any<ScriptOptions>());
            await handler.DidNotReceive().ExecuteSetSystemdUnitStateScript(Arg.Any<ScriptOptions>());

            capturedOptions.Should().BeEmpty();
        }

        [Fact]
        internal async Task ShouldRestartNetlogd_WhenPreparationSucceedsAndServiceEnabled()
        {
            // Arrange
            List<ScriptOptions> capturedOptions = [];

            var handler = Substitute.For<IScriptHandler>();
            handler.ExecuteSetSyslogSettingsScript(Arg.Any<ScriptOptions>()).Returns(Result.SuccessResult);
            handler.ExecuteSetSystemdUnitStateScript(Arg.Do<ScriptOptions>(capturedOptions.Add))
                .Returns(Result.SuccessResult);

            var proposed = CreateSystemConfig(new SyslogSettings()
            {
                Enabled = SyslogEnabled,
                Address = SyslogAddress,
                TransportProtocol = SyslogProtocol,
                LogFormat = SyslogFormat,
                ConnectionRetrySeconds = SyslogRetrySec,
                ExcludeSyslogFacilities = [Facility],
                ExcludeSyslogLevels = [Level]
            });

            var sut = CreateSut(handler);

            // Act
            var result = await sut.SetConfiguration(proposed, CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Success);
            result.Messages.Should().BeEmpty();

            await handler.Received(1).ExecuteSetSyslogSettingsScript(Arg.Any<ScriptOptions>());
            await handler.Received(1).ExecuteSetSystemdUnitStateScript(Arg.Any<ScriptOptions>());

            capturedOptions.Should().SatisfyRespectively(
                so => so.Options.Should().ContainInOrder(
                    ScriptOptionNames.SetSystemdUnitState.Name, "systemd-netlogd.service",
                    ScriptOptionNames.SetSystemdUnitState.State, Shared.Enums.ServiceControlAction.Restart.ToString()));
        }
    }
}
