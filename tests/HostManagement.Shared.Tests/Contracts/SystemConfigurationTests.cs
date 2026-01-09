using FluentAssertions;
using Xunit;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Service;
using HostManagement.Shared.Enums;

namespace HostManagement.Shared.Tests.Contracts;

public class SystemConfigurationTests
{
    public class ServicesProperty
    {
        private static readonly SystemConfiguration s_emptyConfig = new();

        private static readonly SystemConfiguration s_config = new(
            [
                CreateService("svc1", ServiceState.Enabled),
                CreateService("svc2", ServiceState.Enabled)
            ]);

        private static readonly SystemConfiguration s_configServicesDisabled = new(
            [
                CreateService("svc1", ServiceState.Disabled),
                CreateService("svc2", ServiceState.Disabled)
            ]);

        private static readonly SystemConfiguration s_configServicesReversed = new(
            [
                CreateService("svc2", ServiceState.Enabled),
                CreateService("svc1", ServiceState.Enabled)
            ]);

        private static ServiceDetail CreateService(string name, ServiceState state) =>
            new() { Name = name, State = state };

        [Fact]
        public void ShouldReturnFalse_WhenOneSystemConfigurationIsNull()
            => s_config.Equals(null).Should().BeFalse();

        [Fact]
        public void ShouldReturnFalse_WhenOneServiceListIsEmpty()
        {
            // Assert
            s_emptyConfig.Equals(s_config).Should().BeFalse();
            s_emptyConfig.GetHashCode().Should().NotBe(s_config.GetHashCode());
        }

        [Fact]
        public void ShouldReturnFalse_WhenServicesAreDifferent()
        {
            // Assert
            s_config.Equals(s_configServicesDisabled).Should().BeFalse();
            s_config.GetHashCode().Should().NotBe(s_configServicesDisabled.GetHashCode());
        }

        [Fact]
        public void ShouldReturnTrue_WhenBothSystemConfigurationsAreEmpty()
        {
            // Arrange
            var config2 = new SystemConfiguration();

            // Assert
            s_emptyConfig.Equals(config2).Should().BeTrue();
            s_emptyConfig.GetHashCode().Should().Be(config2.GetHashCode());
        }

        [Fact]
        public void ShouldReturnTrue_WhenServicesReversed()
        {
            // Assert
            s_config.Equals(s_configServicesReversed).Should().BeTrue();
            s_config.GetHashCode().Should().Be(s_configServicesReversed.GetHashCode());
        }
    }
}
