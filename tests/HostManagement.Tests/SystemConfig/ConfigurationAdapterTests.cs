using FluentAssertions;
using Xunit;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Contracts;
using HostManagement.SystemConfig;

namespace HostManagement.Tests.SystemConfig;

public class ConfigurationAdapterTests
{
    private sealed class TestConfigurationAdapter(bool bypassHasChangesInternal = false)
        : ConfigurationAdapter(bypassHasChangesInternal)
    {
        protected internal override Task<OperationStatus> ApplyConfiguration(
            SystemConfiguration config,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(OperationStatus.Success);
        }

        protected internal override Task<OperationStatus> LoadConfiguration(
            SystemConfiguration config,
            CancellationToken cancellationToken)
        {
            config.NetworkDNSSettings.Hostname = "LoadConfiguration-Test";

            return Task.FromResult(OperationStatus.Success);
        }

        private protected override bool HasChangesInternal(
            SystemConfiguration currentConfig,
            SystemConfiguration proposedConfig)
        {
            return !currentConfig.Equals(proposedConfig);
        }
    }

    public class HasChanges
    {
        [Fact]
        public void ShouldReturnTrue_WhenBypassIsTrue()
        {
            // Arrange
            var currentConfig = new SystemConfiguration();
            var proposedConfig = new SystemConfiguration();
            var bypass = true;
            var sut = new TestConfigurationAdapter(bypass);

            // Act
            var result = sut.HasChanges(currentConfig, proposedConfig);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldReturnFalse_WhenBypassIsFalseAndConfigsAreEqual()
        {
            // Arrange
            var currentConfig = new SystemConfiguration();
            var proposedConfig = new SystemConfiguration();
            var bypass = false;
            var sut = new TestConfigurationAdapter(bypass);

            // Act
            var result = sut.HasChanges(currentConfig, proposedConfig);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnTrue_WhenBypassIsFalseAndConfigsAreNotEqual()
        {
            // Arrange
            var currentConfig = new SystemConfiguration();
            var proposedConfig = new SystemConfiguration();
            proposedConfig.NetworkDNSSettings.Hostname = "Bypass-HasChanges-Test";

            var bypass = false;
            var sut = new TestConfigurationAdapter(bypass);

            // Act
            var result = sut.HasChanges(currentConfig, proposedConfig);

            // Assert
            result.Should().BeTrue();
        }
    }

    public class ApplyConfiguration
    {
        [Fact]
        public async Task ShouldExecuteOverwrite_WhenBaseCalled()
        {
            // Arrange
            var proposedConfig = new SystemConfiguration();
            var sut = new TestConfigurationAdapter();

            // Act
            using var cts = new CancellationTokenSource();
            var result = await sut.ApplyConfiguration(proposedConfig, cts.Token);

            // Assert
            result.Should().Be(OperationStatus.Success);
        }
    }

    public class LoadConfiguration
    {
        [Fact]
        public async Task ShouldExecuteOverwrite_WhenBaseCalled()
        {
            // Arrange
            var config = new SystemConfiguration();
            var sut = new TestConfigurationAdapter();

            // Act
            using var cts = new CancellationTokenSource();
            async Task<OperationStatus> LoadAction() => await sut.LoadConfiguration(config, cts.Token);

            // Assert
            config.NetworkDNSSettings.Hostname.Should().Be(string.Empty);

            var result = await LoadAction();

            result.Should().Be(OperationStatus.Success);
            config.NetworkDNSSettings.Hostname.Should().Be("LoadConfiguration-Test");
        }
    }
}
