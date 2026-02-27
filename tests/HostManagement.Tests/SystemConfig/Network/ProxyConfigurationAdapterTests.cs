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
    private static ProxyConfigurationAdapter CreateSut(SystemConfiguration current, IScriptHandler handler, bool bypassHasChanges = false, bool bypassApply = false)
        => new(current, handler, bypassHasChanges, bypassApply);

    private static SystemConfiguration CreateConfig(NetworkProxySettings? proxy = null)
        => new() { NetworkProxySettings = proxy ?? new() };

    private static NetworkProxyDetail CreateDetail(bool enabled, string server, int port, string? user = null, string? pwd = null)
        => new() { Enabled = enabled, Server = server, Port = port, Username = user, Password = pwd };

    public sealed class HasChanges
    {
        [Fact]
        public void ShouldBeFalse_WhenEqual()
        {
            var handler = Substitute.For<IScriptHandler>();
            var current = CreateConfig();
            var sut = CreateSut(current, handler);
            var proposed = CreateConfig();

            sut.HasChanges(current, proposed).Should().BeFalse();
        }

        [Fact]
        public void ShouldBeTrue_WhenDifferent()
        {
            var handler = Substitute.For<IScriptHandler>();
            var current = CreateConfig();
            var proposed = CreateConfig(new NetworkProxySettings() { HTTP = CreateDetail(true, "proxy", 8080) });
            var sut = CreateSut(current, handler);

            sut.HasChanges(current, proposed).Should().BeTrue();
        }

        [Fact]
        public void ShouldBeTrue_WhenBypass()
        {
            var handler = Substitute.For<IScriptHandler>();
            var current = CreateConfig();
            var proposed = CreateConfig();
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
                    HTTP = CreateDetail(true, "http://proxy", 80, "user%", "p@ss word"),
                    HTTPS = CreateDetail(false, "https://proxy", 81),
                    FTP = CreateDetail(true, "ftp://proxy", 21),
                    SFTP = CreateDetail(false, "sftp://proxy", 22, "us-er", "pass_word"),
                    SOCKS = CreateDetail(true, "socks://proxy", 1080),
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
                        HTTPS = CreateDetail(false, "https://proxy-changed", 81),
                        DoNotProxyListEnabled = true,
                        DoNotProxyList = ["changed",]
                    },
                    2);

                Add(
                    currentProxySettings,
                    new()
                    {
                        HTTP = CreateDetail(true, "http://proxy-changed", 80, "user%", "p@ss word"),
                        HTTPS = CreateDetail(false, "https://proxy-changed", 81),
                        FTP = CreateDetail(true, "ftp://proxy-changed", 21),
                        SFTP = CreateDetail(false, "sftp://proxy-changed", 22, "us-er", "pass_word"),
                        SOCKS = CreateDetail(true, "socks://proxy-changed", 1080),
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
            var current = CreateConfig(currentProxySettings);
            var proposed = CreateConfig(proposedProxySettings);

            var sut = CreateSut(current, handler, bypassHasChanges: true);

            // Act + Assert
            var status = await sut.SetConfiguration(proposed);

            status.Should().Be(OperationStatus.Success);
            await handler.Received(expectedReceiveCount).ExecuteSetNetworkProxySettingsScript(Arg.Any<ScriptOptions>());
        }

        [Fact]
        public async Task ShouldCallScripts_ForChangedProxies()
        {
            var handler = Substitute.For<IScriptHandler>();
            List<ScriptOptions> captured = [];
            handler.ExecuteSetNetworkProxySettingsScript(Arg.Do<ScriptOptions>(captured.Add))
                .Returns(Task.FromResult(OperationStatus.Success));
            var current = CreateConfig();
            var proposed = CreateConfig(new()
            {
                HTTP = CreateDetail(true, "http://proxy", 80, "user%", "p@ss word"),
                HTTPS = CreateDetail(false, "https://proxy", 81),
                FTP = CreateDetail(true, "ftp://proxy", 21),
                SFTP = CreateDetail(false, "sftp://proxy", 22, "us-er", "pass_word"),
                SOCKS = CreateDetail(true, "socks://proxy", 1080),
                DoNotProxyList = ["localhost",],
                DoNotProxyListEnabled = true,
            });
            var sut = CreateSut(current, handler, bypassHasChanges: true);

            var status = await sut.SetConfiguration(proposed);

            status.Should().Be(OperationStatus.Success);
            await handler.Received(6).ExecuteSetNetworkProxySettingsScript(Arg.Any<ScriptOptions>());
            captured.Should().SatisfyRespectively(
                so => so.Options.Should().ContainInOrder(
                    ScriptOptionNames.Proxy.Enabled,
                    "true",
                    ScriptOptionNames.Proxy.URL,
                    "http://proxy",
                    ScriptOptionNames.Proxy.Port,
                    "80",
                    ScriptOptionNames.Proxy.Username,
                    "user%25",
                    ScriptOptionNames.Proxy.Password,
                    "p%40ss%20word"),
                so => so.Options.Should().ContainInOrder(
                    ScriptOptionNames.Proxy.Enabled,
                    "false",
                    ScriptOptionNames.Proxy.URL,
                    "https://proxy",
                    ScriptOptionNames.Proxy.Port,
                    "81"),
                so => so.Options.Should().ContainInOrder(
                    ScriptOptionNames.Proxy.Enabled,
                    "true",
                    ScriptOptionNames.Proxy.URL,
                    "ftp://proxy",
                    ScriptOptionNames.Proxy.Port,
                    "21"),
                so => so.Options.Should().ContainInOrder(
                    ScriptOptionNames.Proxy.Enabled,
                    "false",
                    ScriptOptionNames.Proxy.URL,
                    "sftp://proxy",
                    ScriptOptionNames.Proxy.Port,
                    "22",
                    ScriptOptionNames.Proxy.Username,
                    "us-er",
                    ScriptOptionNames.Proxy.Password,
                    "pass_word"),
                so => so.Options.Should().ContainInOrder(
                    ScriptOptionNames.Proxy.Enabled,
                    "true",
                    ScriptOptionNames.Proxy.URL,
                    "socks://proxy",
                    ScriptOptionNames.Proxy.Port,
                    "1080"),
                so => so.Options.Should().ContainInOrder(
                    ScriptOptionNames.Proxy.NoProxyList,
                    "localhost",
                    ScriptOptionNames.Proxy.NoProxyListEnabled,
                    "true"));
        }

        [Fact]
        public async Task ShouldCallScript_WithNonePlaceholderForEmptyDoNotProxyList()
        {
            var handler = Substitute.For<IScriptHandler>();
            ScriptOptions? captured = null;
            handler.ExecuteSetNetworkProxySettingsScript(Arg.Do<ScriptOptions>(so => captured = so))
                .Returns(Task.FromResult(OperationStatus.Success));
            var current = CreateConfig(new() { DoNotProxyListEnabled = true, DoNotProxyList = ["fe80::1%eth0",], });
            var proposed = CreateConfig(new() { DoNotProxyListEnabled = true, DoNotProxyList = [], });
            var sut = CreateSut(current, handler, bypassHasChanges: true);

            var status = await sut.SetConfiguration(proposed);

            status.Should().Be(OperationStatus.Success);
            captured.Should().NotBeNull();
            captured!.Options.Should().ContainInOrder(
                ScriptOptionNames.Proxy.NoProxyList,
                "NONE",
                ScriptOptionNames.Proxy.NoProxyListEnabled,
                "true");
        }
    }
}
