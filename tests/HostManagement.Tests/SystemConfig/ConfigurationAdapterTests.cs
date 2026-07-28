using AwesomeAssertions;
using HostManagement.Shared.Communication.Enums;
using HostManagement.SystemConfig;
using HostManagement.SystemConfig.Contracts;
using Xunit;

namespace HostManagement.Tests.SystemConfig;

public class ConfigurationAdapterTests
{
    private sealed class TestConfigurationAdapter(
        bool bypassHasChanges,
        bool bypassApplyConfig)
        : ConfigurationAdapter(bypassHasChanges, bypassApplyConfig)
    {
        public string AdapterPhase { get; private set; } = string.Empty;

        private protected override Task<Result> PrepareConfiguration(
            SystemConfiguration config,
            CancellationToken cancellationToken)
        {
            AdapterPhase = "Prepare";

            return Task.FromResult(Result.SuccessResult);
        }
        private protected override Task<Result> ApplyConfiguration(
            SystemConfiguration config,
            CancellationToken cancellationToken)
        {
            AdapterPhase = "Apply";

            return Task.FromResult(Result.SuccessResult);
        }

        protected internal override Task<Result> GetConfiguration(
            SystemConfiguration config,
            CancellationToken cancellationToken)
        {
            config.NetworkDNSSettings ??= new()
            {
                Hostname = "GetConfiguration-Test",
            };

            return Task.FromResult(Result.SuccessResult);
        }

        private protected override bool HasConfigurationChanges(
            SystemConfiguration currentConfig,
            SystemConfiguration proposedConfig)
                => !currentConfig.Equals(proposedConfig);
    }

    public class HasChanges
    {
        [Fact]
        public void ShouldReturnTrue_WhenBypassIsTrue()
        {
            // Arrange
            var currentConfig = new SystemConfiguration();
            var proposedConfig = new SystemConfiguration();
            var sut = new TestConfigurationAdapter(
                bypassHasChanges: true,
                bypassApplyConfig: false);

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
            var sut = new TestConfigurationAdapter(
                bypassHasChanges: false,
                bypassApplyConfig: false);

            // Act
            var result = sut.HasChanges(currentConfig, proposedConfig);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnTrue_WhenBypassIsFalseAndConfigsAreNotEqual()
        {
            // Arrange
            SystemConfiguration currentConfig = new();
            SystemConfiguration proposedConfig = new()
            {
                NetworkDNSSettings = new()
                {
                    Hostname = "Bypass-HasChanges-Test",
                },
            };

            var sut = new TestConfigurationAdapter(
                bypassHasChanges: false,
                bypassApplyConfig: false);

            // Act
            var result = sut.HasChanges(currentConfig, proposedConfig);

            // Assert
            result.Should().BeTrue();
        }
    }

    public class GetConfiguration
    {
        [Fact]
        public async Task ShouldExecuteOverwrite_WhenBaseCalled()
        {
            // Arrange
            var config = new SystemConfiguration();
            var sut = new TestConfigurationAdapter(
                bypassHasChanges: false,
                bypassApplyConfig: false);

            // Act
            using var cts = new CancellationTokenSource();
            async Task<Result> GetAction() => await sut.GetConfiguration(config, cts.Token);

            // Assert
            config.NetworkDNSSettings.Should().BeNull();

            var result = await GetAction();

            result.OperationStatus.Should().Be(OperationStatus.Success);
            config.NetworkDNSSettings.Hostname.Should().Be("GetConfiguration-Test");
        }
    }

    public class SetConfiguration
    {
        [Fact]
        public async Task ShouldExecuteOverwrite_WhenBaseCalled()
        {
            // Arrange
            var config = new SystemConfiguration();
            var sut = new TestConfigurationAdapter(
                bypassHasChanges: false,
                bypassApplyConfig: false);

            // Act
            using var cts = new CancellationTokenSource();
            var result = await sut.SetConfiguration(config, cts.Token);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Success);
            sut.AdapterPhase.Should().Be("Apply");
        }

        [Fact]
        public async Task ShouldExecutePrepareConfiguration_WhenApplyBypassed()
        {
            // Arrange
            var config = new SystemConfiguration();
            var sut = new TestConfigurationAdapter(
                bypassHasChanges: false,
                bypassApplyConfig: true);

            // Act
            using var cts = new CancellationTokenSource();
            var result = await sut.SetConfiguration(config, cts.Token);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Success);
            sut.AdapterPhase.Should().Be("Prepare");
        }
    }
}
