using System.Net;
using AwesomeAssertions;
using HostManagement.Shared.Contracts.Network;
using Xunit;

namespace HostManagement.Shared.Tests.Contracts.Network;

public sealed class IPv4SettingsTests
{
    public sealed class EqualsMethod
    {
        [Theory]
        [ClassData(typeof(DifferentIPv4SettingsData))]
        public void ShouldReturnFalse_WhenDifferent(IPv4Settings first, IPv4Settings second)
            => first.Equals(second).Should().BeFalse();

        [Theory]
        [ClassData(typeof(EqualIPv4SettingsData))]
        public void ShouldReturnTrue_WhenEqual(IPv4Settings first, IPv4Settings second)
            => first.Equals(second).Should().BeTrue();
    }

    public sealed class GetHashCodeMethod
    {
        [Theory]
        [ClassData(typeof(DifferentIPv4SettingsData))]
        public void ShouldReturnDifferentHashCode_WhenDifferent(IPv4Settings first, IPv4Settings second)
        {
            // Act
            var hash1 = first.GetHashCode();
            var hash2 = second.GetHashCode();

            // Assert
            hash1.Should().NotBe(hash2);
        }

        [Theory]
        [ClassData(typeof(EqualIPv4SettingsData))]
        public void ShouldReturnSameHashCode_WhenEqual(IPv4Settings first, IPv4Settings second)
        {
            // Act
            var hash1 = first.GetHashCode();
            var hash2 = second.GetHashCode();

            // Assert
            hash1.Should().Be(hash2);
        }
    }

#pragma warning disable CA1812
    internal sealed class DifferentIPv4SettingsData : TheoryData<IPv4Settings, IPv4Settings>
    {
        public DifferentIPv4SettingsData()
        {
            var firstIPv4Detail = new IPv4Detail
            {
                IPAddress = IPAddress.Parse("192.168.1.101"),
                Netmask = IPAddress.Parse("255.255.255.0")
            };

            var secondIPv4Detail = new IPv4Detail
            {
                IPAddress = IPAddress.Parse("192.168.1.102"),
                Netmask = IPAddress.Parse("255.255.255.0")
            };

            var ipv4Settings = new IPv4Settings()
            {
                Gateway = IPAddress.Parse("192.168.1.1"),
                DHCPEnabled = true,
                IPv4Details = [firstIPv4Detail, secondIPv4Detail]
            };

            // Gateway changed
            Add(
                ipv4Settings,
                ipv4Settings with
                {
                    Gateway = IPAddress.Parse("192.168.1.2")
                });

            // Gateway removed
            Add(
                ipv4Settings,
                ipv4Settings with
                {
                    Gateway = null
                });

            // DHCPEnabled changed
            Add(
                ipv4Settings,
                ipv4Settings with
                {
                    DHCPEnabled = false
                });

            // IPv4Details order changed
            Add(
                ipv4Settings,
                ipv4Settings with
                {
                    IPv4Details =
                    [
                        secondIPv4Detail,
                        firstIPv4Detail
                    ]
                });

            // Second IPv4 address changed
            Add(
                ipv4Settings,
                ipv4Settings with
                {
                    IPv4Details =
                    [
                        firstIPv4Detail,
                        secondIPv4Detail with
                        {
                            IPAddress = IPAddress.Parse("192.168.1.103")
                        }
                    ]
                });

            // Second netmask changed
            Add(
                ipv4Settings,
                ipv4Settings with
                {
                    IPv4Details =
                    [
                        firstIPv4Detail,
                        secondIPv4Detail with
                        {
                            Netmask = IPAddress.Parse("255.255.0.0")
                        }
                    ]
                });

            // Settings completely changed
            Add(
                ipv4Settings,
                new IPv4Settings()
                {
                    Gateway = IPAddress.Parse("10.0.0.1"),
                    DHCPEnabled = false,
                    IPv4Details =
                    [
                        new IPv4Detail
                        {
                            IPAddress = IPAddress.Parse("10.0.0.100"),
                            Netmask = IPAddress.Parse("255.0.0.0")
                        }
                    ]
                });
        }
    }

    internal sealed class EqualIPv4SettingsData : TheoryData<IPv4Settings, IPv4Settings>
    {
        public EqualIPv4SettingsData()
        {
            Add(
                new IPv4Settings()
                {
                    Gateway = IPAddress.Parse("192.168.1.1"),
                    DHCPEnabled = true,
                    IPv4Details =
                    [
                        new IPv4Detail
                        {
                            IPAddress = IPAddress.Parse("192.168.1.100"),
                            Netmask = IPAddress.Parse("255.255.255.0")
                        }
                    ]
                },
                new IPv4Settings()
                {
                    Gateway = IPAddress.Parse("192.168.1.1"),
                    DHCPEnabled = true,
                    IPv4Details =
                    [
                        new IPv4Detail
                        {
                            IPAddress = IPAddress.Parse("192.168.1.100"),
                            Netmask = IPAddress.Parse("255.255.255.0")
                        }
                    ]
                });

            Add(new IPv4Settings(), new IPv4Settings());
        }
    }
#pragma warning restore CA1812
}
