using AwesomeAssertions;
using HostManagement.SystemConfig.Contracts;
using Xunit;

namespace HostManagement.Tests.SystemConfig.Contracts;

public class NetworkProxySettingsTests
{
    public sealed class EqualsMethod
    {
        [Fact]
        public void ShouldReturnTrue_WhenIdenticalSettings()
        {
            var settings1 = CreateSampleSettings();
            var settings2 = CreateSampleSettings();

            var result = settings1.Equals(settings2);

            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldReturnTrue_WhenEmptySettings()
        {
            var settings1 = new NetworkProxySettings();
            var settings2 = new NetworkProxySettings();

            var result = settings1.Equals(settings2);

            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDifferentHTTPProxy()
        {
            var settings1 = CreateSampleSettings();
            var settings2 = CreateSampleSettings();
            settings2.HTTP!.Enabled = !settings1.HTTP!.Enabled;

            var result = settings1.Equals(settings2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenNullVsNonNullHTTPProxy()
        {
            var settings1 = new NetworkProxySettings { HTTP = null };
            var settings2 = new NetworkProxySettings
            {
                HTTP = new NetworkProxyDetail { Enabled = true, Server = "proxy.example.com", Port = 8080 }
            };

            var result = settings1.Equals(settings2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDifferentHTTPSProxy()
        {
            var settings1 = CreateSampleSettings();
            var settings2 = CreateSampleSettings();
            settings2.HTTPS!.Server = "different-server.com";

            var result = settings1.Equals(settings2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenNullVsNonNullHTTPSProxy()
        {
            var settings1 = new NetworkProxySettings { HTTPS = null };
            var settings2 = new NetworkProxySettings
            {
                HTTPS = new NetworkProxyDetail { Enabled = true, Server = "proxy.example.com", Port = 8443 }
            };

            var result = settings1.Equals(settings2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDifferentFTPProxy()
        {
            var settings1 = CreateSampleSettings();
            var settings2 = CreateSampleSettings();
            settings2.FTP!.Port = 8080;

            var result = settings1.Equals(settings2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenNullVsNonNullFTPProxy()
        {
            var settings1 = new NetworkProxySettings { FTP = null };
            var settings2 = new NetworkProxySettings
            {
                FTP = new NetworkProxyDetail { Enabled = false, Server = "ftp-proxy.example.com", Port = 8021 }
            };

            var result = settings1.Equals(settings2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDifferentSFTPProxy()
        {
            var settings1 = CreateSampleSettings();
            var settings2 = CreateSampleSettings();
            settings2.SFTP!.Username = "different-user";

            var result = settings1.Equals(settings2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenNullVsNonNullSFTPProxy()
        {
            var settings1 = new NetworkProxySettings { SFTP = null };
            var settings2 = new NetworkProxySettings
            {
                SFTP = new NetworkProxyDetail { Enabled = false, Server = "sftp-proxy.example.com", Port = 8022 }
            };

            var result = settings1.Equals(settings2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDifferentSOCKSProxy()
        {
            var settings1 = CreateSampleSettings();
            var settings2 = CreateSampleSettings();
            settings2.SOCKS!.Password = "different-password";

            var result = settings1.Equals(settings2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenNullVsNonNullSOCKSProxy()
        {
            var settings1 = new NetworkProxySettings { SOCKS = null };
            var settings2 = new NetworkProxySettings
            {
                SOCKS = new NetworkProxyDetail { Enabled = false, Server = "socks-proxy.example.com", Port = 1080 }
            };

            var result = settings1.Equals(settings2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDifferentDoNotProxyListEnabled()
        {
            var settings1 = CreateSampleSettings();
            var settings2 = CreateSampleSettings();
            settings2.DoNotProxyListEnabled = !settings1.DoNotProxyListEnabled;

            var result = settings1.Equals(settings2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenNullVsNonNullDoNotProxyListEnabled()
        {
            var settings1 = new NetworkProxySettings { DoNotProxyListEnabled = null };
            var settings2 = new NetworkProxySettings { DoNotProxyListEnabled = true };

            var result = settings1.Equals(settings2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDifferentDoNotProxyList()
        {
            var settings1 = CreateSampleSettings();
            var settings2 = CreateSampleSettings();
            settings2.DoNotProxyList!.Add("extra.local");

            var result = settings1.Equals(settings2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenNullVsNonNullDoNotProxyList()
        {
            var settings1 = new NetworkProxySettings { DoNotProxyList = null };
            var settings2 = new NetworkProxySettings
            {
                DoNotProxyList = ["localhost"]
            };

            var result = settings1.Equals(settings2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDifferentDoNotProxyListOrder()
        {
            var settings1 = new NetworkProxySettings
            {
                DoNotProxyList = ["localhost", "127.0.0.1"]
            };
            var settings2 = new NetworkProxySettings
            {
                DoNotProxyList = ["127.0.0.1", "localhost"]
            };

            var result = settings1.Equals(settings2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenNull()
        {
            var settings = CreateSampleSettings();

            var result = settings.Equals(null);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenNestedProxyDetailDifferences()
        {
            var settings1 = CreateSampleSettings();
            var settings2 = CreateSampleSettings();
            settings2.HTTP = new NetworkProxyDetail
            {
                Enabled = true,
                Server = "different-proxy.com",
                Port = 8888,
                Username = "different-user",
                Password = "different-pass"
            };

            var result = settings1.Equals(settings2);

            result.Should().BeFalse();
        }
    }

    public sealed class GetHashCodeMethod
    {
        [Fact]
        public void ShouldReturnSameHashCode_WhenIdenticalSettings()
        {
            var settings1 = CreateSampleSettings();
            var settings2 = CreateSampleSettings();

            var hash1 = settings1.GetHashCode();
            var hash2 = settings2.GetHashCode();

            hash1.Should().Be(hash2);
        }

        [Fact]
        public void ShouldReturnDifferentHashCode_WhenDifferentSettings()
        {
            var settings1 = CreateSampleSettings();
            var settings2 = CreateSampleSettings();
            settings2.HTTP!.Enabled = !settings1.HTTP!.Enabled;

            var hash1 = settings1.GetHashCode();
            var hash2 = settings2.GetHashCode();

            hash1.Should().NotBe(hash2);
        }
    }

    private static NetworkProxySettings CreateSampleSettings()
    {
        return new NetworkProxySettings
        {
            HTTP = new()
            {
                Enabled = true,
                Server = "http-proxy.example.com",
                Port = 8080,
                Username = "user1",
                Password = "pass1",
            },
            HTTPS = new()
            {
                Enabled = true,
                Server = "https-proxy.example.com",
                Port = 8443
            },
            FTP = new()
            {
                Enabled = false,
                Server = "ftp-proxy.example.com",
                Port = 8023,
            },
            SFTP = new()
            {
                Enabled = false,
                Server = "sftp-proxy.example.com",
                Port = 8024,
            },
            SOCKS = new()
            {
                Enabled = false,
                Server = "socks-proxy.example.com",
                Port = 8088,
            },
            DoNotProxyListEnabled = true,
            DoNotProxyList = ["localhost", "127.0.0.1", "*.local",],
        };
    }
}
