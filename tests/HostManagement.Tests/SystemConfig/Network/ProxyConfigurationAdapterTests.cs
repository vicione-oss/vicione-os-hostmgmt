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

    private const string NoProxyEntry = "localhost";
    private const string NoProxyEntryChanged = "127.0.0.1";

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
                .Returns((Result.ErrorResult, new NetworkProxySettings()));

            var originalProxySettings = new NetworkProxySettings()
            {
                NoProxy = new NoProxySettings()
                {
                    Enabled = true,
                    Entries = [NoProxyEntry]
                }
            };

            var config = CreateSystemConfig(originalProxySettings);
            var sut = CreateSut(CreateSystemConfig(new()), scriptHandler);

            // Act
            var result = await sut.GetConfiguration(config, CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Error);
            result.Messages.Should().Contain(Result.ErrorMessage);
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
                .Returns((Result.SuccessResult, proxySettings));

            var config = CreateSystemConfig();
            var sut = CreateSut(CreateSystemConfig(new()), scriptHandler);

            // Act
            var result = await sut.GetConfiguration(config, CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Success);
            result.Messages.Should().BeEmpty();

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
        public async Task ShouldUnescapeLinkLocalIPv6ZoneIdentifier_ForNoProxyEntries(
            string escapedNoProxyEntry,
            string expectedNoProxyEntry)
        {
            // Arrange
            var proxySettings = new NetworkProxySettings()
            {
                NoProxy = new NoProxySettings()
                {
                    Enabled = true,
                    Entries = [escapedNoProxyEntry]
                }
            };

            var handler = Substitute.For<IScriptHandler>();
            handler
                .ExecuteGetNetworkProxySettingsScript()
                .Returns((Result.SuccessResult, proxySettings));

            var config = CreateSystemConfig();
            var sut = CreateSut(CreateSystemConfig(new()), handler);

            // Act
            var result = await sut.GetConfiguration(config, CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Success);
            result.Messages.Should().BeEmpty();

            config.NetworkProxySettings.Should().NotBeNull();
            config.NetworkProxySettings.NoProxy.Should().NotBeNull();
            config.NetworkProxySettings.NoProxy.Entries.Should().NotBeNull();
            config.NetworkProxySettings.NoProxy.Entries.Should().ContainSingle(expectedNoProxyEntry);

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

        internal sealed class NoProxyEntriesData : TheoryData<NetworkProxySettings?, NetworkProxySettings, int>
        {
            public NoProxyEntriesData()
            {
                var currentProxySettings = new NetworkProxySettings()
                {
                    NoProxy = new() { Enabled = true, Entries = [NoProxyEntry] }
                };

                // Enabled null, Entries same
                Add(
                    currentProxySettings,
                    new() { NoProxy = new() { Enabled = null, Entries = [NoProxyEntry] } },
                    0);

                // Enabled same, Entries == null
                Add(
                    currentProxySettings,
                    new() { NoProxy = new() { Enabled = true, Entries = null } },
                    0);

                // Enabled null, Entries == null
                Add(
                    currentProxySettings,
                    new() { NoProxy = new() { Enabled = null, Entries = null } },
                    0);

                // Enabled same, Entries same
                Add(
                    currentProxySettings,
                    new() { NoProxy = new() { Enabled = true, Entries = [NoProxyEntry] } },
                    0);

                // Enabled changed, Entries changed, currentProxySettings null
                Add(
                    null,
                    new() { NoProxy = new() { Enabled = false, Entries = [NoProxyEntryChanged] } },
                    1);

                // Enabled changed, Entries same
                Add(
                    currentProxySettings,
                    new() { NoProxy = new() { Enabled = false, Entries = [NoProxyEntry] } },
                    1);

                // Enabled same, Entries changed
                Add(
                    currentProxySettings,
                    new() { NoProxy = new() { Enabled = true, Entries = [NoProxyEntryChanged] } },
                    1);

                // Enabled changed, Entries changed
                Add(
                    currentProxySettings,
                    new() { NoProxy = new() { Enabled = false, Entries = [NoProxyEntryChanged] } },
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
            scriptHandler
                .ExecuteSetNetworkProxySettingsScript(Arg.Any<ScriptOptions>())
                .Returns(Result.SuccessResult);

            var current = CreateSystemConfig(currentProxySettings);
            var proposed = CreateSystemConfig(proposedProxySettings);

            var sut = CreateSut(current, scriptHandler, bypassHasChanges: true);

            // Act + Assert
            var result = await sut.SetConfiguration(proposed, CancellationToken.None);

            result.OperationStatus.Should().Be(OperationStatus.Success);
            result.Messages.Should().BeEmpty();

            await scriptHandler
                .Received(expectedReceiveCount)
                .ExecuteSetNetworkProxySettingsScript(Arg.Any<ScriptOptions>());
        }

        [Theory]
        [ClassData(typeof(NoProxyEntriesData))]
        internal async Task ShouldOnlyCallScript_WhenNoProxySettingsConfigured(
            NetworkProxySettings? currentProxySettings,
            NetworkProxySettings proposedProxySettings,
            int expectedReceiveCount)
        {
            // Arrange
            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler
                .ExecuteSetNetworkProxySettingsScript(Arg.Any<ScriptOptions>())
                .Returns(Result.SuccessResult);

            var current = CreateSystemConfig(currentProxySettings);
            var proposed = CreateSystemConfig(proposedProxySettings);

            var sut = CreateSut(current, scriptHandler, bypassHasChanges: true);

            // Act + Assert
            var result = await sut.SetConfiguration(proposed, CancellationToken.None);

            result.OperationStatus.Should().Be(OperationStatus.Success);
            result.Messages.Should().BeEmpty();

            await scriptHandler
                .Received(expectedReceiveCount)
                .ExecuteSetNetworkProxySettingsScript(Arg.Any<ScriptOptions>());
        }

        [Fact]
        public async Task ShouldCallScriptWithNonePlaceholder_WhenEmptyNoProxyEntries()
        {
            // Arrange
            ScriptOptions? captured = null;
            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler
                .ExecuteSetNetworkProxySettingsScript(Arg.Do<ScriptOptions>(so => captured = so))
                .Returns(Result.SuccessResult);

            var current = CreateSystemConfig(new() { NoProxy = new() { Enabled = true, Entries = [NoProxyEntry] } });
            var proposed = CreateSystemConfig(new() { NoProxy = new() { Enabled = true, Entries = [] } });
            var sut = CreateSut(current, scriptHandler, bypassHasChanges: true);

            // Act
            var result = await sut.SetConfiguration(proposed, CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Success);
            result.Messages.Should().BeEmpty();

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
                .Returns(Result.SuccessResult);

            var current = CreateSystemConfig(new());
            var proposed = CreateSystemConfig(new()
            {
                HTTP = CreateProxyDetail(true, HTTPProxy, ProxyPort, username, password)
            });
            var sut = CreateSut(current, scriptHandler, bypassHasChanges: true);

            // Act
            var result = await sut.SetConfiguration(proposed, CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Success);
            result.Messages.Should().BeEmpty();

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
        public async Task ShouldPercentEncodeLinkLocalIPv6ZoneIdentifier_ForNoProxyEntry(
            string noProxyEntry,
            string escapedNoProxyEntry)
        {
            // Arrange
            List<ScriptOptions> captured = [];
            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler
                .ExecuteSetNetworkProxySettingsScript(Arg.Do<ScriptOptions>(captured.Add))
                .Returns(Result.SuccessResult);

            var current = CreateSystemConfig(new());
            var proposed = CreateSystemConfig(new() { NoProxy = new() { Enabled = true, Entries = [noProxyEntry] } });
            var sut = CreateSut(current, scriptHandler, bypassHasChanges: true);

            // Act
            var result = await sut.SetConfiguration(proposed, CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Success);
            result.Messages.Should().BeEmpty();

            await scriptHandler.Received(1).ExecuteSetNetworkProxySettingsScript(Arg.Any<ScriptOptions>());

            captured.Should().SatisfyRespectively(
                so => so.Options.Should().ContainInOrder(
                    ScriptOptionNames.Proxy.NoProxyList, escapedNoProxyEntry,
                    ScriptOptionNames.Proxy.NoProxyListEnabled, "true"));
        }
    }
}
