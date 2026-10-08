using AwesomeAssertions;
using HostManagement.Shared.Communication.Enums;
using HostManagement.SystemConfig.Adapters;
using HostManagement.SystemConfig.Contracts;
using Xunit;

namespace HostManagement.Tests.SystemConfig.Adapters;

public sealed class ConfigurationAdapterTests
{
    private const string TestHostname = "GetConfiguration-Test";

    public sealed class GetConfiguration
    {
        [Fact]
        public async Task ShouldPopulateConfiguration_WhenCalled()
        {
            // Arrange
            var config = new SystemConfiguration();
            var sut = new TestConfigurationAdapter(bypassHasChanges: false, bypassApplyConfig: false);

            // Act
            var result = await sut.GetConfiguration(config, CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Success);
            config.NetworkDNSSettings.Should().NotBeNull();
            config.NetworkDNSSettings.Hostname.Should().Be(TestHostname);
        }
    }

    public sealed class HasChanges
    {
        [Fact]
        public void ShouldReturnTrue_WhenChangeCheckBypassed()
        {
            // Arrange
            var currentConfig = new SystemConfiguration();
            var proposedConfig = new SystemConfiguration();
            var sut = new TestConfigurationAdapter(bypassHasChanges: true, bypassApplyConfig: false);

            // Act
            var result = sut.HasChanges(currentConfig, proposedConfig);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldReturnFalse_WhenConfigurationsEqual()
        {
            // Arrange
            var currentConfig = new SystemConfiguration();
            var proposedConfig = new SystemConfiguration();
            var sut = new TestConfigurationAdapter(bypassHasChanges: false, bypassApplyConfig: false);

            // Act
            var result = sut.HasChanges(currentConfig, proposedConfig);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnTrue_WhenConfigurationsDiffer()
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

            var sut = new TestConfigurationAdapter(bypassHasChanges: false, bypassApplyConfig: false);

            // Act
            var result = sut.HasChanges(currentConfig, proposedConfig);

            // Assert
            result.Should().BeTrue();
        }
    }

    public sealed class SetConfiguration
    {
        [Fact]
        public async Task ShouldExecuteApplyConfiguration_WhenApplyNotBypassed()
        {
            // Arrange
            var config = new SystemConfiguration();
            var sut = new TestConfigurationAdapter(bypassHasChanges: false, bypassApplyConfig: false);

            // Act
            var result = await sut.SetConfiguration(config, CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Success);
            sut.AdapterPhase.Should().Be("Apply");
        }

        [Fact]
        public async Task ShouldExecutePrepareConfiguration_WhenApplyBypassed()
        {
            // Arrange
            var config = new SystemConfiguration();
            var sut = new TestConfigurationAdapter(bypassHasChanges: false, bypassApplyConfig: true);

            // Act
            var result = await sut.SetConfiguration(config, CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Success);
            sut.AdapterPhase.Should().Be("Prepare");
        }
    }

    private sealed class TestConfigurationAdapter(bool bypassHasChanges, bool bypassApplyConfig)
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
                Hostname = TestHostname
            };

            return Task.FromResult(Result.SuccessResult);
        }

        private protected override bool HasConfigurationChanges(
            SystemConfiguration currentConfig,
            SystemConfiguration proposedConfig)
                => !currentConfig.Equals(proposedConfig);
    }
}
