using AwesomeAssertions;
using HostManagement.Shared.Communication.Enums;
using HostManagement.SystemConfig;
using HostManagement.SystemConfig.Contracts;
using Xunit;

namespace HostManagement.Tests.SystemConfig;

public sealed class SystemConfigurationAdapterTests
{
    private const string ChangedHostname = "changed-host";

    private static SystemConfigurationAdapter CreateSut(
        SystemConfiguration currentConfig,
        bool bypassHasChanges = false,
        params TestConfigurationAdapter[] adapters)
            => new(currentConfig, adapters, bypassHasChanges, bypassApplyConfig: true);

    private static SystemConfiguration CreateChangedConfiguration()
        => new() { NetworkDNSSettings = new() { Hostname = ChangedHostname } };

    public sealed class GetConfiguration
    {
        [Fact]
        public async Task ShouldReturnErrorAndStopProcessing_WhenAdapterReturnsError()
        {
            // Arrange
            var warningAdapter = new TestConfigurationAdapter(getResult: Result.WarningResult);
            var errorAdapter = new TestConfigurationAdapter(getResult: Result.ErrorResult);
            var skippedAdapter = new TestConfigurationAdapter();
            var sut = CreateSut(new(), adapters: [warningAdapter, errorAdapter, skippedAdapter]);
            var config = new SystemConfiguration();

            // Act
            var result = await sut.GetConfiguration(config, CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Error);
            result.Messages.Should().ContainInOrder(Result.WarningMessage, Result.ErrorMessage);

            warningAdapter.GetConfigurationCallCount.Should().Be(1);
            errorAdapter.GetConfigurationCallCount.Should().Be(1);
            skippedAdapter.GetConfigurationCallCount.Should().Be(0);
        }

        [Fact]
        public async Task ShouldReturnWarningAndProcessAllAdapters_WhenAdapterReturnsWarning()
        {
            // Arrange
            using var cancellationTokenSource = new CancellationTokenSource();

            var warningAdapter = new TestConfigurationAdapter(getResult: Result.WarningResult);
            var successAdapter = new TestConfigurationAdapter();
            var sut = CreateSut(new(), adapters: [warningAdapter, successAdapter]);
            var config = new SystemConfiguration();

            // Act
            var result = await sut.GetConfiguration(config, cancellationTokenSource.Token);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Warning);
            result.Messages.Should().ContainSingle().Which.Should().Be(Result.WarningMessage);

            warningAdapter.GetConfigurationCallCount.Should().Be(1);
            successAdapter.GetConfigurationCallCount.Should().Be(1);
            warningAdapter.GetCancellationToken.Should().Be(cancellationTokenSource.Token);
            successAdapter.GetCancellationToken.Should().Be(cancellationTokenSource.Token);
        }

        [Fact]
        public async Task ShouldReturnSuccess_WhenAllAdaptersSucceed()
        {
            // Arrange
            var firstAdapter = new TestConfigurationAdapter();
            var secondAdapter = new TestConfigurationAdapter();
            var sut = CreateSut(new(), adapters: [firstAdapter, secondAdapter]);

            // Act
            var result = await sut.GetConfiguration(new(), CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Success);
            result.Messages.Should().BeEmpty();
            firstAdapter.GetConfigurationCallCount.Should().Be(1);
            secondAdapter.GetConfigurationCallCount.Should().Be(1);
        }
    }

    public sealed class HasChanges
    {
        [Fact]
        public void ShouldReturnFalse_WhenConfigurationsEqual()
        {
            // Arrange
            var currentConfig = new SystemConfiguration();
            var proposedConfig = new SystemConfiguration();
            var sut = CreateSut(currentConfig);

            // Act
            var result = sut.HasChanges(currentConfig, proposedConfig);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnTrue_WhenConfigurationsDiffer()
        {
            // Arrange
            var currentConfig = new SystemConfiguration();
            var proposedConfig = CreateChangedConfiguration();
            var sut = CreateSut(currentConfig);

            // Act
            var result = sut.HasChanges(currentConfig, proposedConfig);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldReturnTrue_WhenChangeCheckBypassed()
        {
            // Arrange
            var currentConfig = new SystemConfiguration();
            var proposedConfig = new SystemConfiguration();
            var sut = CreateSut(currentConfig, bypassHasChanges: true);

            // Act
            var result = sut.HasChanges(currentConfig, proposedConfig);

            // Assert
            result.Should().BeTrue();
        }
    }

    public sealed class SetConfiguration
    {
        [Fact]
        public async Task ShouldSetOnlyChangedAdapters_WhenConfigurationContainsChanges()
        {
            // Arrange
            using var cancellationTokenSource = new CancellationTokenSource();

            var currentConfig = new SystemConfiguration();
            var proposedConfig = CreateChangedConfiguration();
            var unchangedAdapter = new TestConfigurationAdapter(hasChanges: false);
            var changedAdapter = new TestConfigurationAdapter(hasChanges: true);
            var sut = CreateSut(currentConfig, adapters: [unchangedAdapter, changedAdapter]);

            // Act
            var result = await sut.SetConfiguration(proposedConfig, cancellationTokenSource.Token);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Success);
            result.Messages.Should().BeEmpty();

            unchangedAdapter.SetConfigurationCallCount.Should().Be(0);
            changedAdapter.SetConfigurationCallCount.Should().Be(1);
            changedAdapter.CurrentConfig.Should().BeSameAs(currentConfig);
            changedAdapter.ProposedConfig.Should().BeSameAs(proposedConfig);
            changedAdapter.SetCancellationToken.Should().Be(cancellationTokenSource.Token);
        }

        [Fact]
        public async Task ShouldReturnErrorAndStopProcessing_WhenChangedAdapterReturnsError()
        {
            // Arrange
            var currentConfig = new SystemConfiguration();
            var proposedConfig = CreateChangedConfiguration();

            var warningAdapter = new TestConfigurationAdapter(
                hasChanges: true,
                setResult: Result.WarningResult);

            var errorAdapter = new TestConfigurationAdapter(
                hasChanges: true,
                setResult: Result.ErrorResult);

            var skippedAdapter = new TestConfigurationAdapter(hasChanges: true);

            var sut = CreateSut(
                currentConfig,
                adapters: [warningAdapter, errorAdapter, skippedAdapter]);

            // Act
            var result = await sut.SetConfiguration(proposedConfig, CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Error);
            result.Messages.Should().ContainInOrder(Result.WarningMessage, Result.ErrorMessage);

            warningAdapter.SetConfigurationCallCount.Should().Be(1);
            errorAdapter.SetConfigurationCallCount.Should().Be(1);
            skippedAdapter.SetConfigurationCallCount.Should().Be(0);
        }

        [Fact]
        public async Task ShouldSkipAllAdapters_WhenConfigurationHasNoChanges()
        {
            // Arrange
            var firstAdapter = new TestConfigurationAdapter(hasChanges: false);
            var secondAdapter = new TestConfigurationAdapter(hasChanges: false);
            var sut = CreateSut(new(), adapters: [firstAdapter, secondAdapter]);

            // Act
            var result = await sut.SetConfiguration(new(), CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Success);
            result.Messages.Should().BeEmpty();
            firstAdapter.SetConfigurationCallCount.Should().Be(0);
            secondAdapter.SetConfigurationCallCount.Should().Be(0);
        }
    }

    private sealed class TestConfigurationAdapter : ConfigurationAdapter<SystemConfigurationAdapter>
    {
        private readonly Result _getResult;
        private readonly bool _hasChanges;
        private readonly Result _setResult;

        public TestConfigurationAdapter(Result? getResult = null, bool hasChanges = false, Result? setResult = null)
            : base(bypassApplyConfig: true)
        {
            _getResult = getResult ?? Result.SuccessResult;
            _hasChanges = hasChanges;
            _setResult = setResult ?? Result.SuccessResult;
        }

        public CancellationToken GetCancellationToken { get; private set; }
        public int GetConfigurationCallCount { get; private set; }
        public SystemConfiguration? CurrentConfig { get; private set; }
        public SystemConfiguration? ProposedConfig { get; private set; }
        public CancellationToken SetCancellationToken { get; private set; }
        public int SetConfigurationCallCount { get; private set; }

        protected internal override Task<Result> GetConfiguration(
            SystemConfiguration config,
            CancellationToken cancellationToken)
        {
            GetConfigurationCallCount++;
            GetCancellationToken = cancellationToken;
            return Task.FromResult(_getResult);
        }

        private protected override bool HasConfigurationChanges(
            SystemConfiguration currentConfig,
            SystemConfiguration proposedConfig)
        {
            CurrentConfig = currentConfig;
            ProposedConfig = proposedConfig;
            return _hasChanges;
        }

        private protected override Task<Result> PrepareConfiguration(
            SystemConfiguration config,
            CancellationToken cancellationToken)
        {
            SetConfigurationCallCount++;
            SetCancellationToken = cancellationToken;
            return Task.FromResult(_setResult);
        }
    }
}
