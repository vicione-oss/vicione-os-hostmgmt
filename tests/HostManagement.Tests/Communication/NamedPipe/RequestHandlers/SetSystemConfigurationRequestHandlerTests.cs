using AwesomeAssertions;
using HostManagement.Communication.NamedPipe.RequestHandlers;
using HostManagement.Shared;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Serialization;
using HostManagement.SystemConfig;
using HostManagement.Tests.SystemConfig;
using NSubstitute;
using Xunit;
using InternalSystemConfiguration = HostManagement.SystemConfig.Contracts.SystemConfiguration;

namespace HostManagement.Tests.Communication.NamedPipe.RequestHandlers;

[Collection("Non Parallel Tests")]
public sealed class SetSystemConfigurationRequestHandlerTests : IDisposable
{
    private const string Lan1 = "lan1";
    private const string TestHostname = "test-host";

    private static readonly int s_currentVersion = new SystemConfiguration().Version;

    public SetSystemConfigurationRequestHandlerTests() => SystemConfigurationCache.Instance.ResetForTests();

    public void Dispose() => SystemConfigurationCache.Instance.ResetForTests();

    [Fact]
    public void ShouldExposeExpectedTopic()
    {
        // Arrange
        var context = GetSystemConfigurationServiceContext();
        var sut = new SetSystemConfigurationRequestHandler(context.Sut);

        // Assert
        sut.Topic.Should().Be(Topics.SetSystemConfiguration);
    }

    [Fact]
    public async Task ShouldReturnErrorAndNotPublishConfigurationChangedEvent_WhenMessageContentContainsInvalidJson()
    {
        // Arrange
        var context = GetSystemConfigurationServiceContext();
        var sut = new SetSystemConfigurationRequestHandler(context.Sut);
        var eventCount = 0;
        sut.PublishEventRequested += (_, args) => eventCount++;

        // Act
        var response = await sut.HandleAsync("{", Guid.NewGuid(), CancellationToken.None);

        // Assert
        var result = MessageSerializer.Deserialize<SetSystemConfigurationResult>(response);
        result.Status.Should().Be(OperationStatus.Error);

        eventCount.Should().Be(0);

        await context.Persistence.DidNotReceive().SaveUserConfiguration(
            Arg.Any<InternalSystemConfiguration>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ShouldReturnErrorAndNotPublishConfigurationChangedEvent_WhenMergeFails()
    {
        // Arrange
        var context = GetSystemConfigurationServiceContext();
        var sut = new SetSystemConfigurationRequestHandler(context.Sut);
        var eventCount = 0;
        sut.PublishEventRequested += (_, _) => eventCount++;

        // Act
        var response = await sut.HandleAsync(
            $$$"""{"Version":{{{s_currentVersion}}},"NetworkDNSSettings":{"UnknownSetting":true}}""",
            Guid.NewGuid(),
            CancellationToken.None);

        // Assert
        var result = MessageSerializer.Deserialize<SetSystemConfigurationResult>(response);
        result.Status.Should().Be(OperationStatus.Error);
        result.Message.Should().Be(
            "Set system configuration failed." +
            Environment.NewLine +
            "Unknown configuration property '/NetworkDNSSettings/UnknownSetting'.");

        eventCount.Should().Be(0);

        await context.Persistence.DidNotReceive().SaveUserConfiguration(
            Arg.Any<InternalSystemConfiguration>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ShouldReturnErrorAndNotPublishConfigurationChangedEvent_WhenConfigurationFails()
    {
        // Arrange
        var context = GetSystemConfigurationServiceContext();
        var sut = new SetSystemConfigurationRequestHandler(context.Sut);
        var eventCount = 0;
        sut.PublishEventRequested += (_, args) => eventCount++;

        var proposedConfiguration = CreateConfiguration();
        proposedConfiguration.NetworkInterfaces.Clear();

        var request = MessageSerializer.Serialize(proposedConfiguration);

        // Act
        var response = await sut.HandleAsync(request, Guid.NewGuid(), CancellationToken.None);

        // Assert
        var result = MessageSerializer.Deserialize<SetSystemConfigurationResult>(response);
        result.Status.Should().Be(OperationStatus.Error);

        eventCount.Should().Be(0);

        await context.Persistence.DidNotReceive().SaveUserConfiguration(
            Arg.Any<InternalSystemConfiguration>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ShouldReturnWarningAndPublishConfigurationChangedEvent_WhenConfigurationCompletesWithWarning()
    {
        // Arrange
        var context = GetSystemConfigurationServiceContext();
        context.ConfigureValidNetworkInterface();

        context.Persistence.SaveUserConfiguration(default!, Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(Task.CompletedTask);

        var warningResult = new Result();
        warningResult.AddWarning("Unit-Test-Warning");

        context.ScriptHandler.ExecuteGetNetworkProxySettingsScript().ReturnsForAnyArgs((warningResult, new()));

        var sut = new SetSystemConfigurationRequestHandler(context.Sut);
        var request = MessageSerializer.Serialize(CreateConfiguration());

        PublishEventRequestedEventArgs? publishedEvent = null;
        sut.PublishEventRequested += (_, args) => publishedEvent = args;

        // Act
        var response = await sut.HandleAsync(request, Guid.NewGuid(), CancellationToken.None);

        // Assert
        var result = MessageSerializer.Deserialize<SetSystemConfigurationResult>(response);
        result.Status.Should().Be(OperationStatus.Warning);

        publishedEvent.Should().NotBeNull();
        publishedEvent.Value.Topic.Should().Be(EventTopics.SystemConfigurationChanged);
    }

    [Fact]
    public async Task ShouldReturnSuccessAndPublishMergedConfiguration_WhenConfigurationSucceeds()
    {
        // Arrange
        var context = GetSystemConfigurationServiceContext();
        context.ConfigureValidNetworkInterface();
        context.ScriptHandler.ExecuteGetSyslogSettingsScript()
            .ReturnsForAnyArgs((Result.SuccessResult, new() { Address = "192.0.2.1:514", ConnectionRetrySeconds = 45 }));

        var sut = new SetSystemConfigurationRequestHandler(context.Sut);
        PublishEventRequestedEventArgs? publishedEvent = null;
        sut.PublishEventRequested += (_, args) => publishedEvent = args;
        var correlationId = Guid.NewGuid();

        // Act
        var responseJson = await sut.HandleAsync(
            $$$"""{"Version":{{{s_currentVersion}}},"NetworkDNSSettings":{"Hostname":"updated-host"}}""",
            correlationId,
            CancellationToken.None);

        // Assert
        var response = MessageSerializer.Deserialize<SetSystemConfigurationResult>(responseJson);
        response.Status.Should().Be(OperationStatus.Success, response.Message);

        publishedEvent.Should().NotBeNull();

        var published = publishedEvent.Value;
        published.Topic.Should().Be(EventTopics.SystemConfigurationChanged);
        published.CorrelationId.Should().Be(correlationId);
        published.ExcludeRequestingClient.Should().BeTrue();

        var changedConfiguration = MessageSerializer.Deserialize<GetSystemConfigurationResult>(published.Content);
        changedConfiguration.Configuration.Should().NotBeNull();

        var mergedConfiguration = changedConfiguration.Configuration!;
        mergedConfiguration.Version.Should().Be(2);
        mergedConfiguration.NetworkDNSSettings.Hostname.Should().Be("updated-host");
        mergedConfiguration.Syslog.ConnectionRetrySeconds.Should().Be(45);
    }

    [Fact]
    public async Task ShouldReturnSuccessAndPublishConfigurationChangedEvent_WhenConfigurationSucceeds()
    {
        // Arrange
        var context = GetSystemConfigurationServiceContext();
        context.ConfigureValidNetworkInterface();
        context.Persistence.SaveUserConfiguration(default!, Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(Task.CompletedTask);

        var sut = new SetSystemConfigurationRequestHandler(context.Sut);
        var proposedConfiguration = CreateConfiguration();
        var request = MessageSerializer.Serialize(proposedConfiguration);
        var correlationId = Guid.NewGuid();

        PublishEventRequestedEventArgs? publishedEvent = null;
        sut.PublishEventRequested += (_, args) => publishedEvent = args;

        using var cts = new CancellationTokenSource();

        // Act
        var response = await sut.HandleAsync(request, correlationId, cts.Token);

        // Assert
        var result = MessageSerializer.Deserialize<SetSystemConfigurationResult>(response);
        result.Status.Should().Be(OperationStatus.Success);

        await context.Persistence.Received(1).SaveUserConfiguration(
            Arg.Any<InternalSystemConfiguration>(),
            cts.Token);

        publishedEvent.Should().NotBeNull();

        var eventArgs = publishedEvent.Value;
        eventArgs.Topic.Should().Be(EventTopics.SystemConfigurationChanged);
        eventArgs.CorrelationId.Should().Be(correlationId);
        eventArgs.ExcludeRequestingClient.Should().BeTrue();

        var eventResult = MessageSerializer.Deserialize<GetSystemConfigurationResult>(eventArgs.Content);
        eventResult.Status.Should().Be(OperationStatus.Success);
        eventResult.Message.Should().Be("System configuration changed");
        eventResult.Configuration.Should().BeEquivalentTo(proposedConfiguration);
    }

    private static SystemConfigurationServiceContext GetSystemConfigurationServiceContext()
        => new(Lan1, TestHostname);

    private static SystemConfiguration CreateConfiguration()
        => new()
        {
            Version = 2,
            NetworkDNSSettings = new()
            {
                Hostname = TestHostname,
                PrimaryDNSSuffix = new()
                {
                    Enabled = false,
                    Suffix = "company"
                }
            },
            NetworkInterfaces =
            [
                new()
                {
                    CommonInformation = new()
                    {
                        Name = Lan1,
                        Enabled = true
                    },
                    IPv4 = new()
                    {
                        DHCPEnabled = true
                    }
                }
            ]
        };
}
