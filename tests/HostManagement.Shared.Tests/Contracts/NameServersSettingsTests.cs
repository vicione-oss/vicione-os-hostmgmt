using System.Net;
using AwesomeAssertions;
using HostManagement.Shared.Contracts.Network;
using Xunit;

namespace HostManagement.Shared.Tests.Contracts;

public sealed class NameServersSettingsTests
{
    public sealed class EqualsMethod
    {
        [Theory]
        [ClassData(typeof(DifferentNameServersSettingsData))]
        public void ShouldReturnFalse_WhenDifferent(NameServersSettings first, NameServersSettings second)
            => first.Equals(second).Should().BeFalse();

        [Theory]
        [ClassData(typeof(EqualNameServersSettingsData))]
        public void ShouldReturnTrue_WhenEqual(NameServersSettings first, NameServersSettings second)
            => first.Equals(second).Should().BeTrue();
    }

    public sealed class GetHashCodeMethod
    {
        [Theory]
        [ClassData(typeof(DifferentNameServersSettingsData))]
        public void ShouldReturnDifferentHashCode_WhenDifferent(NameServersSettings first, NameServersSettings second)
        {
            // Act
            var hash1 = first.GetHashCode();
            var hash2 = second.GetHashCode();

            // Assert
            hash1.Should().NotBe(hash2);
        }

        [Theory]
        [ClassData(typeof(EqualNameServersSettingsData))]
        public void ShouldReturnSameHashCode_WhenEqual(NameServersSettings first, NameServersSettings second)
        {
            // Act
            var hash1 = first.GetHashCode();
            var hash2 = second.GetHashCode();

            // Assert
            hash1.Should().Be(hash2);
        }
    }

#pragma warning disable CA1812
    internal sealed class DifferentNameServersSettingsData : TheoryData<NameServersSettings, NameServersSettings>
    {
        public DifferentNameServersSettingsData()
        {
            var firstAddress = IPAddress.Parse("192.168.1.1");
            var secondAddress = IPAddress.Parse("2001:db8::1");

            var nameServersSettings = new NameServersSettings()
            {
                Enabled = true,
                Addresses = [firstAddress, secondAddress]
            };

            // Enabled changed
            Add(
                nameServersSettings,
                nameServersSettings with
                {
                    Enabled = false
                });

            // Addresses order changed
            Add(
                nameServersSettings,
                nameServersSettings with
                {
                    Addresses =
                    [
                        secondAddress,
                        firstAddress
                    ]
                });

            // Second address changed
            Add(
                nameServersSettings,
                nameServersSettings with
                {
                    Addresses =
                    [
                        firstAddress,
                        IPAddress.Parse("2001:db8::2")
                    ]
                });

            // Settings completely changed
            Add(
                nameServersSettings,
                new NameServersSettings()
                {
                    Enabled = false,
                    Addresses = [IPAddress.Parse("10.0.0.1")]
                });
        }
    }

    internal sealed class EqualNameServersSettingsData : TheoryData<NameServersSettings, NameServersSettings>
    {
        public EqualNameServersSettingsData()
        {
            Add(
                new NameServersSettings()
                {
                    Enabled = true,
                    Addresses =
                    [
                        IPAddress.Parse("192.168.1.1"),
                        IPAddress.Parse("2001:db8::1")
                    ]
                },
                new NameServersSettings()
                {
                    Enabled = true,
                    Addresses =
                    [
                        IPAddress.Parse("192.168.1.1"),
                        IPAddress.Parse("2001:db8::1")
                    ]
                });

            Add(new NameServersSettings(), new NameServersSettings());
        }
    }
#pragma warning restore CA1812
}
