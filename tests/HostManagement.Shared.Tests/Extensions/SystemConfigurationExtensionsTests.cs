using AwesomeAssertions;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Extensions;
using Xunit;

namespace HostManagement.Shared.Tests.Extensions;

public sealed class SystemConfigurationExtensionsTests
{
    public sealed class GetNetworkInterface
    {
        [Fact]
        public void ShouldReturnMatchingNetworkInterface_WhenInterfaceExists()
        {
            // Arrange
            var expected = CreateNetworkInterface("lan1");

            var config = new SystemConfiguration
            {
                NetworkInterfaces =
                [
                    CreateNetworkInterface("lan0"),
                    expected,
                    CreateNetworkInterface("lan2")
                ]
            };

            // Act
            var result = config.GetNetworkInterface("lan1");

            // Assert
            result.Should().BeSameAs(expected);
        }

        [Fact]
        public void ShouldReturnNull_WhenInterfaceDoesNotExist()
        {
            // Arrange
            var config = new SystemConfiguration
            {
                NetworkInterfaces =
                [
                    CreateNetworkInterface("lan0"),
                    CreateNetworkInterface("lan1")
                ]
            };

            // Act
            var result = config.GetNetworkInterface("lan2");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void ShouldReturnNull_WhenInterfaceNameUsesDifferentCasing()
        {
            // Arrange
            var config = new SystemConfiguration
            {
                NetworkInterfaces =
                [
                    CreateNetworkInterface("lan1")
                ]
            };

            // Act
            var result = config.GetNetworkInterface("LAN1");

            // Assert
            result.Should().BeNull();
        }

        private static NetworkInterfaceDetail CreateNetworkInterface(string name)
            => new()
            {
                CommonInformation = new()
                {
                    Name = name,
                    Enabled = true
                },
                IPv4 = new()
            };
    }
}
