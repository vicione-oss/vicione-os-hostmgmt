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
    private const int ProxyPort = 8080;
    private const int ProxyPortChanged = 4040;

    private const string ProxyUser = "user";
    private const string ProxyPassword = "pass";

    private const string HTTPProxy = "http://proxy";
    private const string HTTPProxyChanged = "http://proxy-changed";

    private const string HTTPSProxy = "https://proxy";
    private const string HTTPSProxyChanged = "https://proxy-changed";

    private const string FTPProxy = "ftp://proxy";
    private const string FTPProxyChanged = "ftp://proxy-changed";

    private const string SFTPProxy = "sftp://proxy";
    private const string SFTPProxyChanged = "sftp://proxy-changed";

    private const string SocksProxy = "socks://proxy";
    private const string SocksProxyChanged = "socks://proxy-changed";

    private const string DoNotProxyItem = "localhost";
    private const string DoNotProxyItemChanged = "127.0.0.1";

    private static ProxyConfigurationAdapter CreateSut(
        SystemConfiguration current,
        IScriptHandler handler,
        bool bypassHasChanges = false,
        bool bypassApply = false)
        => new(current, handler, bypassHasChanges, bypassApply);

    private static SystemConfiguration CreateSystemConfig(NetworkProxySettings? proxy = null)
        => new() { NetworkProxySettings = proxy };

    private static NetworkProxyDetail CreateProxyDetail(
        bool enabled,
        string server,
        int port,
        string? user = null,
        string? password = null)
        => new() { Enabled = enabled, Server = server, Port = port, Username = user, Password = password };

    public sealed class GetConfiguration
    {
        [Fact]
        public async Task ShouldReturnError_WhenScriptFails()
        {
            // Arrange
            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler
                .ExecuteGetNetworkProxySettingsScript()
                .Returns(Task.FromResult((OperationStatus.Error, new NetworkProxySettings())));

            var originalProxySettings = new NetworkProxySettings()
            {
                DoNotProxyListEnabled = true,
                DoNotProxyList = [DoNotProxyItem]
            };

            var config = CreateSystemConfig(originalProxySettings);
            var sut = CreateSut(CreateSystemConfig(new()), scriptHandler);

            // Act
            var status = await sut.GetConfiguration(config, CancellationToken.None);

            // Assert
            status.Should().Be(OperationStatus.Error);
            config.NetworkProxySettings.Should().BeSameAs(originalProxySettings);

            await scriptHandler.Received(1).ExecuteGetNetworkProxySettingsScript();
        }

        [Theory]
        [InlineData("user%25", "user%", "p%40ss%20word", "p@ss word")]
        [InlineData("user%4042", "user@42", "pa%24%24word", "pa$$word")]
        public async Task ShouldUnescapeSpecialCharacters_ForUsernameAndPassword(
            string escapedUsername,
            string expectedUsername,
            string escapedPassword,
            string expectedPassword)
        {
            // Arrange
            var proxySettings = new NetworkProxySettings()
            {
                HTTPS = new NetworkProxyDetail()
                {
                    Enabled = true,
                    Server = HTTPSProxy,
                    Port = ProxyPort,
                    Username = escapedUsername,
                    Password = escapedPassword
                }
            };

            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler
                .ExecuteGetNetworkProxySettingsScript()
                .Returns(Task.FromResult((OperationStatus.Success, proxySettings)));

            var config = CreateSystemConfig();
            var sut = CreateSut(CreateSystemConfig(new()), scriptHandler);

            // Act
            var status = await sut.GetConfiguration(config, CancellationToken.None);

            // Assert
            status.Should().Be(OperationStatus.Success);

            config.NetworkProxySettings.Should().NotBeNull();
            config.NetworkProxySettings!.HTTPS.Should().NotBeNull();
            config.NetworkProxySettings.HTTPS.Username.Should().Be(expectedUsername);
            config.NetworkProxySettings.HTTPS.Password.Should().Be(expectedPassword);

            await scriptHandler.Received(1).ExecuteGetNetworkProxySettingsScript();
        }

        [Theory]
        [InlineData("fe80::/10%25lan1", "fe80::/10%lan1")]
        [InlineData("fe80::/10%2525", "fe80::/10%25")]
        [InlineData("[fe80::/10%25lan1]:8080", "[fe80::/10%lan1]:8080")]
        [InlineData("[fe80::/10%2525]:8080", "[fe80::/10%25]:8080")]
        public async Task ShouldUnescapeLinkLocalIPv6ZoneIdentifier_ForDoNotProxyListItems(
            string escapedNoProxyItem,
            string expectedNoProxyItem)
        {
            // Arrange
            var proxySettings = new NetworkProxySettings()
            {
                DoNotProxyListEnabled = true,
                DoNotProxyList = [escapedNoProxyItem]
            };

            var handler = Substitute.For<IScriptHandler>();
            handler
                .ExecuteGetNetworkProxySettingsScript()
                .Returns(Task.FromResult((OperationStatus.Success, proxySettings)));

            var config = CreateSystemConfig();
            var sut = CreateSut(CreateSystemConfig(new()), handler);

            // Act
            var status = await sut.GetConfiguration(config, CancellationToken.None);

            // Assert
            status.Should().Be(OperationStatus.Success);

            config.NetworkProxySettings.Should().NotBeNull();
            config.NetworkProxySettings.DoNotProxyList.Should().NotBeNull();
            config.NetworkProxySettings.DoNotProxyList.Should().ContainSingle(expectedNoProxyItem);

            await handler.Received(1).ExecuteGetNetworkProxySettingsScript();
        }
    }

    public sealed class HasChanges
    {
        public static TheoryData<bool, string, int> ProxyDifferentTestData => new TheoryData<bool, string, int>()
        {
            // Single property changed
            {true, HTTPProxy, ProxyPortChanged },
            {true, HTTPProxyChanged, ProxyPort },
            {false, HTTPProxy, ProxyPort },

            // Two properties changed
            {true, HTTPProxyChanged, ProxyPortChanged },
            {false, HTTPProxyChanged, ProxyPort },
            {false, HTTPProxy, ProxyPortChanged },

            // All properties changed
            {false, HTTPProxyChanged, ProxyPortChanged }
        };

        [Fact]
        public void ShouldBeFalse_WhenSettingsEqual()
        {
            var scriptHandler = Substitute.For<IScriptHandler>();
            var current = CreateSystemConfig(new() { HTTP = CreateProxyDetail(true, HTTPProxy, ProxyPort) });
            var proposed = CreateSystemConfig(new() { HTTP = CreateProxyDetail(true, HTTPProxy, ProxyPort) });
            var sut = CreateSut(current, scriptHandler);

            sut.HasChanges(current, proposed).Should().BeFalse();
        }

        [Theory]
        [MemberData(nameof(ProxyDifferentTestData))]
        public void ShouldBeTrue_WhenSettingsDifferent(bool enabled, string server, int port)
        {
            var scriptHandler = Substitute.For<IScriptHandler>();
            var current = CreateSystemConfig(new() { HTTP = CreateProxyDetail(true, HTTPProxy, ProxyPort) });
            var proposed = CreateSystemConfig(new() { HTTP = CreateProxyDetail(enabled, server, port) });
            var sut = CreateSut(current, scriptHandler);

            sut.HasChanges(current, proposed).Should().BeTrue();
        }

        [Theory]
        [MemberData(nameof(ProxyDifferentTestData))]
        [InlineData(true, HTTPProxy, ProxyPort)]
        public void ShouldBeTrue_WhenBypassHasChangesFlagTrue(bool enabled, string server, int port)
        {
            var scriptHandler = Substitute.For<IScriptHandler>();
            var current = CreateSystemConfig(new() { HTTP = CreateProxyDetail(true, HTTPProxy, ProxyPort) });
            var proposed = CreateSystemConfig(new() { HTTP = CreateProxyDetail(enabled, server, port) });
            var sut = CreateSut(current, scriptHandler, bypassHasChanges: true);

            sut.HasChanges(current, proposed).Should().BeTrue();
        }
    }

    public sealed class SetConfiguration
    {
#pragma warning disable CA1812
        internal sealed class NetworkProxyData : TheoryData<NetworkProxySettings?, NetworkProxySettings, int>
        {
            public NetworkProxyData()
            {
                var currentProxySettings = new NetworkProxySettings()
                {
                    HTTP = CreateProxyDetail(true, HTTPProxy, ProxyPort, ProxyUser, ProxyPassword),
                    HTTPS = CreateProxyDetail(false, HTTPSProxy, ProxyPort),
                    FTP = CreateProxyDetail(true, FTPProxy, ProxyPort),
                    SFTP = CreateProxyDetail(false, SFTPProxy, ProxyPort, ProxyUser, ProxyPassword),
                    SOCKS = CreateProxyDetail(true, SocksProxy, ProxyPort)
                };

                // Null proposed proxy details
                Add(
                    currentProxySettings,
                    new()
                    {
                        HTTP = null,
                        HTTPS = null,
                        FTP = null,
                        SFTP = null,
                        SOCKS = null
                    },
                    0);

                // Default proposed proxy details
                Add(
                    currentProxySettings,
                    new()
                    {
                        HTTP = new(),
                        HTTPS = new(),
                        FTP = new(),
                        SFTP = new(),
                        SOCKS = new()
                    },
                    0);

                // Same proposed proxy details
                Add(
                    currentProxySettings,
                    new()
                    {
                        HTTP = currentProxySettings.HTTP,
                        HTTPS = currentProxySettings.HTTPS,
                        FTP = currentProxySettings.FTP,
                        SFTP = currentProxySettings.SFTP,
                        SOCKS = currentProxySettings.SOCKS
                    },
                    0);

                // Different proposed proxy details
                Add(
                    currentProxySettings,
                    new()
                    {
                        HTTPS = CreateProxyDetail(false, HTTPSProxyChanged, ProxyPort)
                    },
                    1);

                Add(
                    currentProxySettings,
                    new()
                    {
                        HTTP = CreateProxyDetail(true, HTTPProxyChanged, ProxyPort, ProxyUser, ProxyPassword),
                        HTTPS = CreateProxyDetail(false, HTTPSProxyChanged, ProxyPort),
                        FTP = CreateProxyDetail(true, FTPProxyChanged, ProxyPort),
                        SFTP = CreateProxyDetail(false, SFTPProxyChanged, ProxyPort, ProxyUser, ProxyPassword),
                        SOCKS = CreateProxyDetail(true, SocksProxyChanged, ProxyPort)
                    },
                    5);

                // currentProxySettings null
                Add(
                    null,
                    new()
                    {
                        HTTP = CreateProxyDetail(true, HTTPProxyChanged, ProxyPort, ProxyUser, ProxyPassword),
                        HTTPS = CreateProxyDetail(false, HTTPSProxyChanged, ProxyPort),
                        FTP = CreateProxyDetail(true, FTPProxyChanged, ProxyPort),
                        SFTP = CreateProxyDetail(false, SFTPProxyChanged, ProxyPort, ProxyUser, ProxyPassword),
                        SOCKS = CreateProxyDetail(true, SocksProxyChanged, ProxyPort)
                    },
                    5);
            }
        }

        internal sealed class NetworkProxyListData : TheoryData<NetworkProxySettings?, NetworkProxySettings, int>
        {
            public NetworkProxyListData()
            {
                var currentProxySettings = new NetworkProxySettings()
                {
                    DoNotProxyListEnabled = true,
                    DoNotProxyList = [DoNotProxyItem]
                };

                // DoNotProxyListEnabled null, DoNotProxyList same
                Add(
                    currentProxySettings,
                    new()
                    {
                        DoNotProxyListEnabled = null,
                        DoNotProxyList = [DoNotProxyItem]
                    },
                    0);

                // DoNotProxyListEnabled same, DoNotProxyList == null
                Add(
                    currentProxySettings,
                    new()
                    {
                        DoNotProxyListEnabled = true,
                        DoNotProxyList = null
                    },
                    0);

                // DoNotProxyListEnabled null, DoNotProxyList == null
                Add(
                    currentProxySettings,
                    new()
                    {
                        DoNotProxyListEnabled = null,
                        DoNotProxyList = null
                    },
                    0);

                // DoNotProxyListEnabled same, DoNotProxyList same
                Add(
                    currentProxySettings,
                    new()
                    {
                        DoNotProxyListEnabled = true,
                        DoNotProxyList = [DoNotProxyItem]
                    },
                    0);

                // DoNotProxyListEnabled changed, DoNotProxyList changed, currentProxySettings null
                Add(
                    null,
                    new()
                    {
                        DoNotProxyListEnabled = false,
                        DoNotProxyList = [DoNotProxyItemChanged]
                    },
                    1);

                // DoNotProxyListEnabled changed, DoNotProxyList same
                Add(
                    currentProxySettings,
                    new()
                    {
                        DoNotProxyListEnabled = false,
                        DoNotProxyList = [DoNotProxyItem]
                    },
                    1);

                // DoNotProxyListEnabled same, DoNotProxyList changed
                Add(
                    currentProxySettings,
                    new()
                    {
                        DoNotProxyListEnabled = true,
                        DoNotProxyList = [DoNotProxyItemChanged]
                    },
                    1);

                // DoNotProxyListEnabled changed, DoNotProxyList changed
                Add(
                    currentProxySettings,
                    new()
                    {
                        DoNotProxyListEnabled = false,
                        DoNotProxyList = [DoNotProxyItemChanged]
                    },
                    1);
            }
        }
#pragma warning restore CA1812

        [Theory]
        [ClassData(typeof(NetworkProxyData))]
        internal async Task ShouldOnlyCallScripts_WhenProxyDetailsConfigured(
            NetworkProxySettings? currentProxySettings,
            NetworkProxySettings proposedProxySettings,
            int expectedReceiveCount)
        {
            // Arrange
            var scriptHandler = Substitute.For<IScriptHandler>();
            var current = CreateSystemConfig(currentProxySettings);
            var proposed = CreateSystemConfig(proposedProxySettings);

            var sut = CreateSut(current, scriptHandler, bypassHasChanges: true);

            // Act + Assert
            var status = await sut.SetConfiguration(proposed, CancellationToken.None);

            status.Should().Be(OperationStatus.Success);
            await scriptHandler.Received(expectedReceiveCount).ExecuteSetNetworkProxySettingsScript(Arg.Any<ScriptOptions>());
        }

        [Theory]
        [ClassData(typeof(NetworkProxyListData))]
        internal async Task ShouldOnlyCallScript_WhenDoNotProxySettingsConfigured(
            NetworkProxySettings? currentProxySettings,
            NetworkProxySettings proposedProxySettings,
            int expectedReceiveCount)
        {
            // Arrange
            var scriptHandler = Substitute.For<IScriptHandler>();
            var current = CreateSystemConfig(currentProxySettings);
            var proposed = CreateSystemConfig(proposedProxySettings);

            var sut = CreateSut(current, scriptHandler, bypassHasChanges: true);

            // Act + Assert
            var status = await sut.SetConfiguration(proposed, CancellationToken.None);

            status.Should().Be(OperationStatus.Success);
            await scriptHandler
                .Received(expectedReceiveCount)
                .ExecuteSetNetworkProxySettingsScript(Arg.Any<ScriptOptions>());
        }

        [Fact]
        public async Task ShouldCallScript_WithNonePlaceholderForEmptyDoNotProxyList()
        {
            // Arrange
            ScriptOptions? captured = null;
            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler
                .ExecuteSetNetworkProxySettingsScript(Arg.Do<ScriptOptions>(so => captured = so))
                .Returns(Task.FromResult(OperationStatus.Success));

            var current = CreateSystemConfig(new() { DoNotProxyListEnabled = true, DoNotProxyList = [DoNotProxyItem] });
            var proposed = CreateSystemConfig(new() { DoNotProxyListEnabled = true, DoNotProxyList = [] });
            var sut = CreateSut(current, scriptHandler, bypassHasChanges: true);

            // Act
            var status = await sut.SetConfiguration(proposed, CancellationToken.None);

            // Assert
            status.Should().Be(OperationStatus.Success);

            captured.Should().NotBeNull();
            captured!.Options.Should().ContainInOrder(
                ScriptOptionNames.Proxy.NoProxyList, "NONE",
                ScriptOptionNames.Proxy.NoProxyListEnabled, "true");
        }

        [Theory]
        [InlineData("user%", "user%25", "p@ss word", "p%40ss%20word")]
        [InlineData("user@42", "user%4042", "pa$$word", "pa%24%24word")]
        public async Task ShouldPercentEncodeSpecialCharacters_ForUsernameAndPassword(
            string username,
            string escapedUsername,
            string password,
            string escapedPassword)
        {
            // Arrange
            List<ScriptOptions> captured = [];
            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler
                .ExecuteSetNetworkProxySettingsScript(Arg.Do<ScriptOptions>(captured.Add))
                .Returns(Task.FromResult(OperationStatus.Success));

            var current = CreateSystemConfig(new());
            var proposed = CreateSystemConfig(new()
            {
                HTTP = CreateProxyDetail(true, HTTPProxy, ProxyPort, username, password)
            });
            var sut = CreateSut(current, scriptHandler, bypassHasChanges: true);

            // Act
            var status = await sut.SetConfiguration(proposed, CancellationToken.None);

            // Assert
            status.Should().Be(OperationStatus.Success);
            await scriptHandler.Received(1).ExecuteSetNetworkProxySettingsScript(Arg.Any<ScriptOptions>());

            captured.Should().SatisfyRespectively(
                so => so.Options.Should().ContainInOrder(
                    ScriptOptionNames.Proxy.Enabled, "true",
                    ScriptOptionNames.Proxy.URL, HTTPProxy,
                    ScriptOptionNames.Proxy.Port, ProxyPort.ToString(),
                    ScriptOptionNames.Proxy.Username, escapedUsername,
                    ScriptOptionNames.Proxy.Password, escapedPassword));
        }

        [Theory]
        [InlineData("fe80::/10%lan1", "fe80::/10%25lan1")]
        [InlineData("fe80::/10%25", "fe80::/10%2525")]
        [InlineData("[fe80::/10%lan1]:8080", "[fe80::/10%25lan1]:8080")]
        [InlineData("[fe80::/10%25]:8080", "[fe80::/10%2525]:8080")]
        public async Task ShouldPercentEncodeLinkLocalIPv6ZoneIdentifier_ForNoProxyItem(
            string noProxyItem,
            string escapedNoProxyItem)
        {
            // Arrange
            List<ScriptOptions> captured = [];
            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler
                .ExecuteSetNetworkProxySettingsScript(Arg.Do<ScriptOptions>(captured.Add))
                .Returns(Task.FromResult(OperationStatus.Success));

            var current = CreateSystemConfig(new());
            var proposed = CreateSystemConfig(new()
            {
                DoNotProxyListEnabled = true,
                DoNotProxyList = [noProxyItem]
            });
            var sut = CreateSut(current, scriptHandler, bypassHasChanges: true);

            // Act
            var status = await sut.SetConfiguration(proposed, CancellationToken.None);

            // Assert
            status.Should().Be(OperationStatus.Success);
            await scriptHandler.Received(1).ExecuteSetNetworkProxySettingsScript(Arg.Any<ScriptOptions>());

            captured.Should().SatisfyRespectively(
                so => so.Options.Should().ContainInOrder(
                    ScriptOptionNames.Proxy.NoProxyList, escapedNoProxyItem,
                    ScriptOptionNames.Proxy.NoProxyListEnabled, "true"));
        }
    }
}
