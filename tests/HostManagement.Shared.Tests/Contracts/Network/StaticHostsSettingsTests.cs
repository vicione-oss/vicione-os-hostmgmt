using System.Net;
using AwesomeAssertions;
using HostManagement.Shared.Contracts.Network;
using Xunit;

namespace HostManagement.Shared.Tests.Contracts.Network;

public sealed class StaticHostsSettingsTests
{
    public sealed class EqualsMethod
    {
        [Theory]
        [ClassData(typeof(DifferentStaticHostsSettingsData))]
        public void ShouldReturnFalse_WhenDifferent(StaticHostsSettings first, StaticHostsSettings second)
            => first.Equals(second).Should().BeFalse();

        [Theory]
        [ClassData(typeof(EqualStaticHostsSettingsData))]
        public void ShouldReturnTrue_WhenEqual(StaticHostsSettings first, StaticHostsSettings second)
            => first.Equals(second).Should().BeTrue();
    }

    public sealed class GetHashCodeMethod
    {
        [Theory]
        [ClassData(typeof(DifferentStaticHostsSettingsData))]
        public void ShouldReturnDifferentHashCode_WhenDifferent(StaticHostsSettings first, StaticHostsSettings second)
        {
            // Act
            var hash1 = first.GetHashCode();
            var hash2 = second.GetHashCode();

            // Assert
            hash1.Should().NotBe(hash2);
        }

        [Theory]
        [ClassData(typeof(EqualStaticHostsSettingsData))]
        public void ShouldReturnSameHashCode_WhenEqual(StaticHostsSettings first, StaticHostsSettings second)
        {
            // Act
            var hash1 = first.GetHashCode();
            var hash2 = second.GetHashCode();

            // Assert
            hash1.Should().Be(hash2);
        }
    }

#pragma warning disable CA1812
    internal sealed class DifferentStaticHostsSettingsData : TheoryData<StaticHostsSettings, StaticHostsSettings>
    {
        public DifferentStaticHostsSettingsData()
        {
            var firstStaticHostDetail = new StaticHostDetail
            {
                IPAddress = IPAddress.Parse("192.168.1.101"),
                Hostname = "static-host-1"
            };

            var secondStaticHostDetail = new StaticHostDetail
            {
                IPAddress = IPAddress.Parse("192.168.1.102"),
                Hostname = "static-host-2"
            };

            var staticHostsSettings = new StaticHostsSettings()
            {
                Enabled = true,
                Hosts = [firstStaticHostDetail, secondStaticHostDetail]
            };

            // Enabled changed
            Add(
                staticHostsSettings,
                staticHostsSettings with
                {
                    Enabled = false
                });

            // Hosts order changed
            Add(
                staticHostsSettings,
                staticHostsSettings with
                {
                    Hosts =
                    [
                        secondStaticHostDetail,
                        firstStaticHostDetail
                    ]
                });

            // Second host IP changed
            Add(
                staticHostsSettings,
                staticHostsSettings with
                {
                    Hosts =
                    [
                        firstStaticHostDetail,
                        secondStaticHostDetail with { IPAddress = IPAddress.Parse("192.168.1.103") }
                    ]
                });

            // Second host hostname changed
            Add(
                staticHostsSettings,
                staticHostsSettings with
                {
                    Hosts =
                    [
                        firstStaticHostDetail,
                        secondStaticHostDetail with { Hostname = "static-host-3" }
                    ]
                });

            // Second host completely changed
            Add(
                staticHostsSettings,
                new StaticHostsSettings()
                {
                    Enabled = false,
                    Hosts =
                    [
                        new StaticHostDetail
                            {
                                IPAddress = IPAddress.Parse("192.168.1.104"),
                                Hostname = "static-host-4"
                            }
                    ]
                });
        }
    }

    internal sealed class EqualStaticHostsSettingsData : TheoryData<StaticHostsSettings, StaticHostsSettings>
    {
        public EqualStaticHostsSettingsData()
        {
            Add(
                new StaticHostsSettings()
                {
                    Enabled = true,
                    Hosts =
                    [
                        new StaticHostDetail
                        {
                            IPAddress = IPAddress.Parse("192.168.1.100"),
                            Hostname = "static-host1"
                        }
                    ]
                },
                new StaticHostsSettings()
                {
                    Enabled = true,
                    Hosts =
                    [
                        new StaticHostDetail
                        {
                            IPAddress = IPAddress.Parse("192.168.1.100"),
                            Hostname = "static-host1"
                        }
                    ]
                });

            Add(new StaticHostsSettings(), new StaticHostsSettings());
        }
    }
#pragma warning restore CA1812
}
