using System.Net;
using AwesomeAssertions;
using HostManagement.SystemConfig.Contracts;
using Xunit;

namespace HostManagement.Tests.SystemConfig.Contracts;

public sealed class NetworkDNSSettingsTests
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
            var settings1 = new NetworkDNSSettings();
            var settings2 = new NetworkDNSSettings();

            var result = settings1.Equals(settings2);

            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDifferentHostname()
        {
            var settings1 = CreateSampleSettings();
            var settings2 = CreateSampleSettings();
            settings2.Hostname = "different-hostname";

            var result = settings1.Equals(settings2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenNullVsNonNullHostname()
        {
            var settings1 = new NetworkDNSSettings { Hostname = null };
            var settings2 = new NetworkDNSSettings { Hostname = "test-host" };

            var result = settings1.Equals(settings2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDifferentMulticastDNSEnabled()
        {
            var settings1 = CreateSampleSettings();
            var settings2 = CreateSampleSettings();
            settings2.MulticastDNSEnabled = !settings1.MulticastDNSEnabled;

            var result = settings1.Equals(settings2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenNullVsNonNullMulticastDNSEnabled()
        {
            var settings1 = new NetworkDNSSettings { MulticastDNSEnabled = null };
            var settings2 = new NetworkDNSSettings { MulticastDNSEnabled = true };

            var result = settings1.Equals(settings2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenNullVsNonNullStaticHostsEnabled()
        {
            var settings1 = new NetworkDNSSettings { StaticHostsEnabled = null };
            var settings2 = new NetworkDNSSettings { StaticHostsEnabled = true };

            var result = settings1.Equals(settings2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDifferentStaticHosts()
        {
            var settings1 = CreateSampleSettings();
            var settings2 = CreateSampleSettings();
            settings2.StaticHosts!.Add(new StaticHostDetail
            {
                IPAddress = IPAddress.Parse("192.168.1.200"),
                Hostname = "extra-host"
            });

            var result = settings1.Equals(settings2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenNullVsNonNullStaticHosts()
        {
            var settings1 = new NetworkDNSSettings { StaticHosts = null };
            var settings2 = new NetworkDNSSettings
            {
                StaticHosts =
                [
                    new StaticHostDetail
                    {
                        IPAddress = IPAddress.Parse("192.168.1.100"),
                        Hostname = "host1"
                    }
                ]
            };

            var result = settings1.Equals(settings2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDifferentStaticHostOrder()
        {
            var host1 = new StaticHostDetail { IPAddress = IPAddress.Parse("192.168.1.100"), Hostname = "host1" };
            var host2 = new StaticHostDetail { IPAddress = IPAddress.Parse("192.168.1.101"), Hostname = "host2" };

            var settings1 = new NetworkDNSSettings { StaticHosts = [host1, host2] };
            var settings2 = new NetworkDNSSettings { StaticHosts = [host2, host1] };

            var result = settings1.Equals(settings2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenNullVsNonNullNameServersEnabled()
        {
            var settings1 = new NetworkDNSSettings { NameServersEnabled = null };
            var settings2 = new NetworkDNSSettings { NameServersEnabled = true };

            var result = settings1.Equals(settings2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDifferentNameServers()
        {
            var settings1 = CreateSampleSettings();
            var settings2 = CreateSampleSettings();
            settings2.NameServers!.Add(IPAddress.Parse("1.1.1.1"));

            var result = settings1.Equals(settings2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenNullVsNonNullNameServers()
        {
            var settings1 = new NetworkDNSSettings { NameServers = null };
            var settings2 = new NetworkDNSSettings
            {
                NameServers = [IPAddress.Parse("8.8.8.8")]
            };

            var result = settings1.Equals(settings2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDifferentNameServerOrder()
        {
            var settings1 = new NetworkDNSSettings
            {
                NameServers = [IPAddress.Parse("8.8.8.8"), IPAddress.Parse("8.8.4.4")]
            };
            var settings2 = new NetworkDNSSettings
            {
                NameServers = [IPAddress.Parse("8.8.4.4"), IPAddress.Parse("8.8.8.8")]
            };

            var result = settings1.Equals(settings2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenNullVsNonNullDNSSuffixEnabled()
        {
            var settings1 = new NetworkDNSSettings { DNSSuffixEnabled = null };
            var settings2 = new NetworkDNSSettings { DNSSuffixEnabled = true };

            var result = settings1.Equals(settings2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDifferentDNSSuffix()
        {
            var settings1 = CreateSampleSettings();
            var settings2 = CreateSampleSettings();
            settings2.DNSSuffix = "different.local";

            var result = settings1.Equals(settings2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenNullVsNonNullDNSSuffix()
        {
            var settings1 = new NetworkDNSSettings { DNSSuffix = null };
            var settings2 = new NetworkDNSSettings { DNSSuffix = "example.local" };

            var result = settings1.Equals(settings2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenNullVsNonNullSearchDomainsEnabled()
        {
            var settings1 = new NetworkDNSSettings { SearchDomainsEnabled = null };
            var settings2 = new NetworkDNSSettings { SearchDomainsEnabled = true };

            var result = settings1.Equals(settings2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDifferentSearchDomains()
        {
            var settings1 = CreateSampleSettings();
            var settings2 = CreateSampleSettings();
            settings2.SearchDomains!.Add("extra.local");

            var result = settings1.Equals(settings2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenNullVsNonNullSearchDomains()
        {
            var settings1 = new NetworkDNSSettings { SearchDomains = null };
            var settings2 = new NetworkDNSSettings
            {
                SearchDomains = ["domain.local"]
            };

            var result = settings1.Equals(settings2);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDifferentSearchDomainOrder()
        {
            var settings1 = new NetworkDNSSettings
            {
                SearchDomains = ["domain1.local", "domain2.local"]
            };
            var settings2 = new NetworkDNSSettings
            {
                SearchDomains = ["domain2.local", "domain1.local"]
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
        public void ShouldReturnFalse_WhenComplexNestedDifferences()
        {
            var settings1 = CreateSampleSettings();
            var settings2 = CreateSampleSettings();
            settings2.StaticHosts![0].Hostname = "modified-hostname";

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
            settings2.Hostname = "different-hostname";

            var hash1 = settings1.GetHashCode();
            var hash2 = settings2.GetHashCode();

            hash1.Should().NotBe(hash2);
        }
    }

    private static NetworkDNSSettings CreateSampleSettings()
    {
        return new NetworkDNSSettings
        {
            Hostname = "test-host",
            MulticastDNSEnabled = true,
            StaticHostsEnabled = true,
            StaticHosts =
            [
                new StaticHostDetail
                {
                    IPAddress = IPAddress.Parse("192.168.1.100"),
                    Hostname = "static-host1"
                }
            ],
            NameServersEnabled = true,
            NameServers = [IPAddress.Parse("8.8.8.8"), IPAddress.Parse("8.8.4.4")],
            DNSSuffixEnabled = true,
            DNSSuffix = "example.local",
            SearchDomainsEnabled = true,
            SearchDomains = ["search1.local", "search2.local"]
        };
    }
}
