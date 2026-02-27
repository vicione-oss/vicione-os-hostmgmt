using AwesomeAssertions;
using HostManagement.Scripts;
using HostManagement.Shared.Communication.Enums;
using HostManagement.SystemConfig.Contracts;
using HostManagement.SystemConfig.Network;
using NSubstitute;
using Xunit;

namespace HostManagement.Tests.SystemConfig.Network;

public class ProxyConfigurationAdapterTests
{
    private static ProxyConfigurationAdapter CreateSut(
        SystemConfiguration current,
        IScriptHandler handler,
        bool bypassHasChanges = false,
        bool bypassApply = false)
        => new(current, handler, bypassHasChanges, bypassApply);

    private static SystemConfiguration CreateSystemConfig(NetworkProxySettings? proxy = null)
        => new() { NetworkProxySettings = proxy ?? new() };

    private static NetworkProxyDetail CreateProxyDetail(
        bool enabled,
        string server,
        int port,
        string? user = null,
        string? password = null)
        => new() { Enabled = enabled, Server = server, Port = port, Username = user, Password = password };

    public sealed class HasChanges
    {
        [Fact]
        public void ShouldBeFalse_WhenEqual()
        {
            var handler = Substitute.For<IScriptHandler>();
            var current = CreateSystemConfig();
            var sut = CreateSut(current, handler);
            var proposed = CreateSystemConfig();

            sut.HasChanges(current, proposed).Should().BeFalse();
        }

        [Fact]
        public void ShouldBeTrue_WhenDifferent()
        {
            var handler = Substitute.For<IScriptHandler>();
            var current = CreateSystemConfig();
            var proposed = CreateSystemConfig(new NetworkProxySettings() { HTTP = CreateProxyDetail(true, "proxy", 8080) });
            var sut = CreateSut(current, handler);

            sut.HasChanges(current, proposed).Should().BeTrue();
        }

        [Fact]
        public void ShouldBeTrue_WhenBypass()
        {
            var handler = Substitute.For<IScriptHandler>();
            var current = CreateSystemConfig();
            var proposed = CreateSystemConfig();
            var sut = CreateSut(current, handler, bypassHasChanges: true);

            sut.HasChanges(current, proposed).Should().BeTrue();
        }
    }

    public sealed class SetConfiguration
    {
#pragma warning disable CA1812
        internal sealed class NetworkProxySettingsData : TheoryData<NetworkProxySettings, NetworkProxySettings, int>
        {
            public NetworkProxySettingsData()
            {
                var currentProxySettings = new NetworkProxySettings()
                {
                    HTTP = CreateProxyDetail(true, "http://proxy", 80, "user%", "p@ss word"),
                    HTTPS = CreateProxyDetail(false, "https://proxy", 81),
                    FTP = CreateProxyDetail(true, "ftp://proxy", 21),
                    SFTP = CreateProxyDetail(false, "sftp://proxy", 22, "us-er", "pass_word"),
                    SOCKS = CreateProxyDetail(true, "socks://proxy", 1080),
                    DoNotProxyListEnabled = true,
                    DoNotProxyList = ["localhost",]
                };

                Add(
                    currentProxySettings,
                    new(),
                    0);

                Add(
                    currentProxySettings,
                    new()
                    {
                        DoNotProxyListEnabled = true,
                        DoNotProxyList = ["changed",]
                    },
                    1);

                Add(
                    currentProxySettings,
                    new()
                    {
                        HTTPS = CreateProxyDetail(false, "https://proxy-changed", 81),
                        DoNotProxyListEnabled = true,
                        DoNotProxyList = ["changed",]
                    },
                    2);

                Add(
                    currentProxySettings,
                    new()
                    {
                        HTTP = CreateProxyDetail(true, "http://proxy-changed", 80, "user%", "p@ss word"),
                        HTTPS = CreateProxyDetail(false, "https://proxy-changed", 81),
                        FTP = CreateProxyDetail(true, "ftp://proxy-changed", 21),
                        SFTP = CreateProxyDetail(false, "sftp://proxy-changed", 22, "us-er", "pass_word"),
                        SOCKS = CreateProxyDetail(true, "socks://proxy-changed", 1080),
                        DoNotProxyListEnabled = true,
                        DoNotProxyList = ["changed",]
                    },
                    6);
            }
        }
#pragma warning restore CA1812


        [Theory]
        [ClassData(typeof(NetworkProxySettingsData))]
        internal async Task ShouldOnlyCallScripts_WhenProxyDetailIsConfigured(
            NetworkProxySettings currentProxySettings,
            NetworkProxySettings proposedProxySettings,
            int expectedReceiveCount)
        {
            // Arrange
            var handler = Substitute.For<IScriptHandler>();
            var current = CreateSystemConfig(currentProxySettings);
            var proposed = CreateSystemConfig(proposedProxySettings);

            var sut = CreateSut(current, handler, bypassHasChanges: true);

            // Act + Assert
            var status = await sut.SetConfiguration(proposed);

            status.Should().Be(OperationStatus.Success);
            await handler.Received(expectedReceiveCount).ExecuteSetNetworkProxySettingsScript(Arg.Any<ScriptOptions>());
        }

        [Fact]
        public async Task ShouldCallScripts_ForChangedProxies()
        {
            // Arrange
            List<ScriptOptions> captured = [];
            var handler = Substitute.For<IScriptHandler>();
            handler
                .ExecuteSetNetworkProxySettingsScript(Arg.Do<ScriptOptions>(captured.Add))
                .Returns(Task.FromResult(OperationStatus.Success));

            var current = CreateSystemConfig();
            var proposed = CreateSystemConfig(new()
            {
                HTTP = CreateProxyDetail(true, "http://proxy", 80, "user%", "p@ss word"),
                HTTPS = CreateProxyDetail(false, "https://proxy", 81),
                FTP = CreateProxyDetail(true, "ftp://proxy", 21),
                SFTP = CreateProxyDetail(false, "sftp://proxy", 22, "us-er", "pass_word"),
                SOCKS = CreateProxyDetail(true, "socks://proxy", 1080),
                DoNotProxyList = ["localhost",],
                DoNotProxyListEnabled = true,
            });
            var sut = CreateSut(current, handler, bypassHasChanges: true);

            // Act
            var status = await sut.SetConfiguration(proposed);

            // Assert
            status.Should().Be(OperationStatus.Success);
            await handler.Received(6).ExecuteSetNetworkProxySettingsScript(Arg.Any<ScriptOptions>());

            captured.Should().SatisfyRespectively(
                so => so.Options.Should().ContainInOrder(
                    ScriptOptionNames.Proxy.Enabled, "true",
                    ScriptOptionNames.Proxy.URL, "http://proxy",
                    ScriptOptionNames.Proxy.Port, "80",
                    ScriptOptionNames.Proxy.Username, "user%25",
                    ScriptOptionNames.Proxy.Password, "p%40ss%20word"),
                so => so.Options.Should().ContainInOrder(
                    ScriptOptionNames.Proxy.Enabled, "false",
                    ScriptOptionNames.Proxy.URL, "https://proxy",
                    ScriptOptionNames.Proxy.Port, "81"),
                so => so.Options.Should().ContainInOrder(
                    ScriptOptionNames.Proxy.Enabled, "true",
                    ScriptOptionNames.Proxy.URL, "ftp://proxy",
                    ScriptOptionNames.Proxy.Port, "21"),
                so => so.Options.Should().ContainInOrder(
                    ScriptOptionNames.Proxy.Enabled, "false",
                    ScriptOptionNames.Proxy.URL, "sftp://proxy",
                    ScriptOptionNames.Proxy.Port, "22",
                    ScriptOptionNames.Proxy.Username, "us-er",
                    ScriptOptionNames.Proxy.Password, "pass_word"),
                so => so.Options.Should().ContainInOrder(
                    ScriptOptionNames.Proxy.Enabled, "true",
                    ScriptOptionNames.Proxy.URL, "socks://proxy",
                    ScriptOptionNames.Proxy.Port, "1080"),
                so => so.Options.Should().ContainInOrder(
                    ScriptOptionNames.Proxy.NoProxyList, "localhost",
                    ScriptOptionNames.Proxy.NoProxyListEnabled, "true"));
        }

        [Fact]
        public async Task ShouldCallScript_WithNonePlaceholderForEmptyDoNotProxyList()
        {
            // Arrange
            ScriptOptions? captured = null;
            var handler = Substitute.For<IScriptHandler>();
            handler
                .ExecuteSetNetworkProxySettingsScript(Arg.Do<ScriptOptions>(so => captured = so))
                .Returns(Task.FromResult(OperationStatus.Success));

            var current = CreateSystemConfig(new() { DoNotProxyListEnabled = true, DoNotProxyList = ["fe80::1%eth0",], });
            var proposed = CreateSystemConfig(new() { DoNotProxyListEnabled = true, DoNotProxyList = [], });
            var sut = CreateSut(current, handler, bypassHasChanges: true);

            // Act
            var status = await sut.SetConfiguration(proposed);

            // Assert
            status.Should().Be(OperationStatus.Success);

            captured.Should().NotBeNull();
            captured!.Options.Should().ContainInOrder(
                ScriptOptionNames.Proxy.NoProxyList, "NONE",
                ScriptOptionNames.Proxy.NoProxyListEnabled, "true");
        }
    }
}
