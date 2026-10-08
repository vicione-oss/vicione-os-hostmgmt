using AwesomeAssertions;
using HostManagement.Shared.Contracts.Network;
using Xunit;

namespace HostManagement.Shared.Tests.Contracts.Network;

public sealed class NetworkNTPSettingsTests
{
    public sealed class EqualsMethod
    {
        [Theory]
        [ClassData(typeof(DifferentNetworkNTPSettingsData))]
        public void ShouldReturnFalse_WhenDifferent(NetworkNTPSettings first, NetworkNTPSettings second)
            => first.Equals(second).Should().BeFalse();

        [Theory]
        [ClassData(typeof(EqualNetworkNTPSettingsData))]
        public void ShouldReturnTrue_WhenEqual(NetworkNTPSettings first, NetworkNTPSettings second)
            => first.Equals(second).Should().BeTrue();
    }

    public sealed class GetHashCodeMethod
    {
        [Theory]
        [ClassData(typeof(DifferentNetworkNTPSettingsData))]
        public void ShouldReturnDifferentHashCode_WhenDifferent(NetworkNTPSettings first, NetworkNTPSettings second)
        {
            // Act
            var hash1 = first.GetHashCode();
            var hash2 = second.GetHashCode();

            // Assert
            hash1.Should().NotBe(hash2);
        }

        [Theory]
        [ClassData(typeof(EqualNetworkNTPSettingsData))]
        public void ShouldReturnSameHashCode_WhenEqual(NetworkNTPSettings first, NetworkNTPSettings second)
        {
            // Act
            var hash1 = first.GetHashCode();
            var hash2 = second.GetHashCode();

            // Assert
            hash1.Should().Be(hash2);
        }
    }

#pragma warning disable CA1812
    internal sealed class DifferentNetworkNTPSettingsData : TheoryData<NetworkNTPSettings, NetworkNTPSettings>
    {
        public DifferentNetworkNTPSettingsData()
        {
            var networkNTPSettings = new NetworkNTPSettings()
            {
                Enabled = true,
                Servers =
                [
                    "192.168.1.1",
                    "ntp.example.com"
                ]
            };

            // Enabled changed
            Add(
                networkNTPSettings,
                networkNTPSettings with
                {
                    Enabled = false
                });

            // Servers order changed
            Add(
                networkNTPSettings,
                networkNTPSettings with
                {
                    Servers =
                    [
                        "ntp.example.com",
                        "192.168.1.1"
                    ]
                });

            // Second server changed
            Add(
                networkNTPSettings,
                networkNTPSettings with
                {
                    Servers =
                    [
                    "192.168.1.1",
                    "other.ntp.example.com"
                    ]
                });
        }
    }

    internal sealed class EqualNetworkNTPSettingsData : TheoryData<NetworkNTPSettings, NetworkNTPSettings>
    {
        public EqualNetworkNTPSettingsData()
        {
            Add(
                new NetworkNTPSettings()
                {
                    Enabled = true,
                    Servers =
                    [
                        "192.168.1.1",
                        "ntp.example.com"
                    ]
                },
                new NetworkNTPSettings()
                {
                    Enabled = true,
                    Servers =
                    [
                        "192.168.1.1",
                        "ntp.example.com"
                    ]
                });

            Add(new NetworkNTPSettings(), new NetworkNTPSettings());
        }
    }
}
