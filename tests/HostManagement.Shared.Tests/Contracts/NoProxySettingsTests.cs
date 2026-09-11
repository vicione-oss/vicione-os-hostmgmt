using AwesomeAssertions;
using HostManagement.Shared.Contracts.Network;
using Xunit;

namespace HostManagement.Shared.Tests.Contracts;

public sealed class NoProxySettingsTests
{
    public sealed class EqualsMethod
    {
        [Theory]
        [ClassData(typeof(DifferentNoProxySettingsData))]
        public void ShouldReturnFalse_WhenDifferent(NoProxySettings first, NoProxySettings second)
            => first.Equals(second).Should().BeFalse();

        [Theory]
        [ClassData(typeof(EqualNoProxySettingsData))]
        public void ShouldReturnTrue_WhenEqual(NoProxySettings first, NoProxySettings second)
            => first.Equals(second).Should().BeTrue();
    }

    public sealed class GetHashCodeMethod
    {
        [Theory]
        [ClassData(typeof(DifferentNoProxySettingsData))]
        public void ShouldReturnDifferentHashCode_WhenDifferent(NoProxySettings first, NoProxySettings second)
        {
            // Act
            var hash1 = first.GetHashCode();
            var hash2 = second.GetHashCode();

            // Assert
            hash1.Should().NotBe(hash2);
        }

        [Theory]
        [ClassData(typeof(EqualNoProxySettingsData))]
        public void ShouldReturnSameHashCode_WhenEqual(NoProxySettings first, NoProxySettings second)
        {
            // Act
            var hash1 = first.GetHashCode();
            var hash2 = second.GetHashCode();

            // Assert
            hash1.Should().Be(hash2);
        }
    }

#pragma warning disable CA1812
    internal sealed class DifferentNoProxySettingsData : TheoryData<NoProxySettings, NoProxySettings>
    {
        public DifferentNoProxySettingsData()
        {
            var noProxySettings = new NoProxySettings()
            {
                Enabled = true,
                Entries =
                [
                    "192.168.1.1",
                    "internal.example.com"
                ]
            };

            // Enabled changed
            Add(
                noProxySettings,
                noProxySettings with
                {
                    Enabled = false
                });

            // Entries order changed
            Add(
                noProxySettings,
                noProxySettings with
                {
                    Entries =
                    [
                        "internal.example.com",
                        "192.168.1.1"
                    ]
                });

            // Second entry changed
            Add(
                noProxySettings,
                noProxySettings with
                {
                    Entries =
                    [
                        "192.168.1.1",
                        "other.example.com"
                    ]
                });

            // Settings completely changed
            Add(
                noProxySettings,
                new NoProxySettings()
                {
                    Enabled = false,
                    Entries = ["[2001:db8::1]:8080"]
                });
        }
    }

    internal sealed class EqualNoProxySettingsData : TheoryData<NoProxySettings, NoProxySettings>
    {
        public EqualNoProxySettingsData()
        {
            Add(
                new NoProxySettings()
                {
                    Enabled = true,
                    Entries =
                    [
                        "192.168.1.1",
                        "[2001:db8::1]:8080",
                        "internal.example.com"
                    ]
                },
                new NoProxySettings()
                {
                    Enabled = true,
                    Entries =
                    [
                        "192.168.1.1",
                        "[2001:db8::1]:8080",
                        "internal.example.com"
                    ]
                });

            Add(new NoProxySettings(), new NoProxySettings());
        }
    }
#pragma warning restore CA1812
}
