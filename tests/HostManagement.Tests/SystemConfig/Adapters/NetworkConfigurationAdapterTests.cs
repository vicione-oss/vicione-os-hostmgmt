using AwesomeAssertions;
using HostManagement.Shared.Communication.Enums;
using HostManagement.SystemConfig.Adapters;
using HostManagement.SystemConfig.Contracts;
using NSubstitute;
using Xunit;

namespace HostManagement.Tests.SystemConfig.Adapters;

public sealed class NetworkConfigurationAdapterTests
{
    private static NetworkConfigurationAdapter CreateSut(
        SystemConfiguration current,
        IScriptHandler handler,
        bool bypassHasChanges = false)
            => new(current, handler, bypassHasChanges, false);

    public sealed class HasChanges
    {
        [Fact]
        public void ShouldReturnFalse_WhenConfigurationsEqual()
        {
            // Arrange
            var handler = Substitute.For<IScriptHandler>();
            var current = new SystemConfiguration();
            var proposed = new SystemConfiguration();
            var sut = CreateSut(current, handler);

            // Act
            var result = sut.HasChanges(current, proposed);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnTrue_WhenNetworkInterfacesDiffer()
        {
            // Arrange
            var handler = Substitute.For<IScriptHandler>();

            var current = new SystemConfiguration();
            var proposed = new SystemConfiguration()
            {
                NetworkInterfaces =
                [
                    new()
                    {
                        CommonInformation = new() { Enabled = true, Name = "eth0" },
                        IPv4 = new(),
                        VLAN = new()
                    }
                ]
            };

            var sut = CreateSut(current, handler);

            // Act
            var result = sut.HasChanges(current, proposed);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldReturnTrue_WhenNetworkDNSSettingsDiffer()
        {
            // Arrange
            var handler = Substitute.For<IScriptHandler>();

            var current = new SystemConfiguration();
            var proposed = new SystemConfiguration()
            {
                NetworkDNSSettings = new()
                {
                    Hostname = "other"
                }
            };

            var sut = CreateSut(current, handler);

            // Act
            var result = sut.HasChanges(current, proposed);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldReturnTrue_WhenChangeCheckBypassed()
        {
            // Arrange
            var handler = Substitute.For<IScriptHandler>();
            var current = new SystemConfiguration();
            var proposed = new SystemConfiguration();
            var sut = CreateSut(current, handler, bypassHasChanges: true);

            // Act
            var result = sut.HasChanges(current, proposed);

            // Assert
            result.Should().BeTrue();
        }
    }

    public sealed class SetConfiguration
    {
        [Fact]
        public async Task ShouldApplyNetplan_WhenAnySubAdapterHasChanges()
        {
            // Arrange
            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler.ExecuteSetHostnameScript("new").Returns(Result.SuccessResult);
            scriptHandler.ExecuteApplyNetplanConfigurationScript().Returns(Result.SuccessResult);

            var current = new SystemConfiguration();
            var proposed = new SystemConfiguration()
            {
                NetworkDNSSettings = new()
                {
                    Hostname = "new" // force change
                }
            };

            var sut = CreateSut(current, scriptHandler);

            // Act
            var result = await sut.SetConfiguration(proposed, CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Success);
            result.Messages.Should().BeEmpty();

            _ = await scriptHandler.Received(1).ExecuteSetHostnameScript("new");
            await scriptHandler.Received(1).ExecuteApplyNetplanConfigurationScript();
        }

        [Fact]
        public async Task ShouldNotApplyNetplan_WhenConfigurationUnchanged()
        {
            // Arrange
            var handler = Substitute.For<IScriptHandler>();
            handler.ExecuteApplyNetplanConfigurationScript().Returns(Result.SuccessResult);

            var current = new SystemConfiguration();
            var proposed = new SystemConfiguration();
            var sut = CreateSut(current, handler);

            // Act
            var result = await sut.SetConfiguration(proposed, CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Success);
            result.Messages.Should().BeEmpty();

            await handler.DidNotReceive().ExecuteApplyNetplanConfigurationScript();
        }
    }
}
