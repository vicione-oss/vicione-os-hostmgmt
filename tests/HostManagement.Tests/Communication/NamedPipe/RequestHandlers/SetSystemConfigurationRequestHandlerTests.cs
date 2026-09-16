using System.Text.Json;
using AwesomeAssertions;
using HostManagement.Communication.NamedPipe.RequestHandlers;
using HostManagement.Shared;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Communication.Serialization;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Serialization;
using HostManagement.SystemConfig;
using HostManagement.Tests.SystemConfig;
using NSubstitute;
using Xunit;
using CommunicationJsonContext = HostManagement.Shared.Communication.Contracts.SourceGenerationContext;
using InternalSystemConfiguration = HostManagement.SystemConfig.Contracts.SystemConfiguration;

namespace HostManagement.Tests.Communication.NamedPipe.RequestHandlers;

[Collection("Non Parallel Tests")]
public sealed class SetSystemConfigurationRequestHandlerTests : IDisposable
{
    private const string Lan1 = "lan1";
    private const string TestHostname = "test-host";

    public SetSystemConfigurationRequestHandlerTests() => SystemConfigurationCache.Instance.ResetForTests();

    public void Dispose() => SystemConfigurationCache.Instance.ResetForTests();

    [Fact]
    public void ShouldExposeExpectedTopicAndJsonTypeInfo()
    {
        // Arrange
        var context = GetSystemConfigurationServiceContext();
        var sut = new SetSystemConfigurationRequestHandler(context.Sut);

        // Assert
        sut.Topic.Should().Be(Topics.SetSystemConfiguration);
        sut.TypedJsonTypeInfo.Should().BeSameAs(CommunicationJsonContext.Default.SetSystemConfigurationResult);
    }

    [Fact]
    public async Task ShouldReturnSuccessAndPublishConfigurationChangedEvent_WhenConfigurationSucceeds()
    {
        // Arrange
        var context = GetSystemConfigurationServiceContext();

        context.Persistence.SaveUserConfiguration(default!, Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(Task.CompletedTask);

        var sut = new SetSystemConfigurationRequestHandler(context.Sut);
        var proposedConfiguration = CreateConfiguration();
        var request = SystemConfigurationSerializer.Serialize(proposedConfiguration);
        var correlationId = Guid.NewGuid();

        PublishEventRequestedEventArgs? publishedEvent = null;
        sut.PublishEventRequested += (_, args) => publishedEvent = args;

        using var cts = new CancellationTokenSource();

        // Act
        var response = await sut.HandleAsync(request, correlationId, cts.Token);

        // Assert
        var result = SourceGeneratedTypeSerializer.Deserialize(
            response,
            CommunicationJsonContext.Default.SetSystemConfigurationResult);

        result.Status.Should().Be(OperationStatus.Success);

        await context.Persistence.Received(1).SaveUserConfiguration(
            Arg.Any<InternalSystemConfiguration>(),
            cts.Token);

        publishedEvent.Should().NotBeNull();

        var eventArgs = publishedEvent.Value;
        eventArgs.Topic.Should().Be(EventTopics.SystemConfigurationChanged);
        eventArgs.CorrelationId.Should().Be(correlationId);
        eventArgs.ExcludeRequestingClient.Should().BeTrue();

        var eventResult = SourceGeneratedTypeSerializer.Deserialize(
            eventArgs.Content,
            CommunicationJsonContext.Default.GetSystemConfigurationResult);

        eventResult.Status.Should().Be(OperationStatus.Success);
        eventResult.Message.Should().Be("System configuration changed");
        eventResult.Configuration.Should().BeEquivalentTo(proposedConfiguration);
    }

    [Fact]
    public async Task ShouldPublishConfigurationChangedEvent_WhenConfigurationCompletesWithWarning()
    {
        // Arrange
        var context = GetSystemConfigurationServiceContext();

        context.Persistence.SaveUserConfiguration(default!, Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(Task.CompletedTask);

        var warningResult = new Result();
        warningResult.AddWarning("Unit-Test-Warning");

        context.ScriptHandler.ExecuteGetNetworkInterfacesCommonInformationScript(Arg.Any<ScriptOptions>())
            .ReturnsForAnyArgs((warningResult, []));

        var sut = new SetSystemConfigurationRequestHandler(context.Sut);
        var request = SystemConfigurationSerializer.Serialize(CreateConfiguration());

        PublishEventRequestedEventArgs? publishedEvent = null;
        sut.PublishEventRequested += (_, args) => publishedEvent = args;

        // Act
        var response = await sut.HandleAsync(request, Guid.NewGuid(), CancellationToken.None);

        // Assert
        var result = SourceGeneratedTypeSerializer.Deserialize(
            response,
            CommunicationJsonContext.Default.SetSystemConfigurationResult);

        result.Status.Should().Be(OperationStatus.Warning);
        publishedEvent.Should().NotBeNull();
        publishedEvent.Value.Topic.Should().Be(EventTopics.SystemConfigurationChanged);
    }

    [Fact]
    public async Task ShouldNotPublishConfigurationChangedEvent_WhenConfigurationFails()
    {
        // Arrange
        var context = GetSystemConfigurationServiceContext();
        var sut = new SetSystemConfigurationRequestHandler(context.Sut);

        var proposedConfiguration = CreateConfiguration();
        proposedConfiguration.NetworkInterfaces.Clear();

        var request = SystemConfigurationSerializer.Serialize(proposedConfiguration);

        PublishEventRequestedEventArgs? publishedEvent = null;
        sut.PublishEventRequested += (_, args) => publishedEvent = args;

        // Act
        var response = await sut.HandleAsync(request, Guid.NewGuid(), CancellationToken.None);

        // Assert
        var result = SourceGeneratedTypeSerializer.Deserialize(
            response,
            CommunicationJsonContext.Default.SetSystemConfigurationResult);

        result.Status.Should().Be(OperationStatus.Error);
        publishedEvent.Should().BeNull();

        await context.Persistence.DidNotReceive().SaveUserConfiguration(
            Arg.Any<InternalSystemConfiguration>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ShouldThrow_WhenMessageContentContainsInvalidJson()
    {
        // Arrange
        var context = GetSystemConfigurationServiceContext();
        var sut = new SetSystemConfigurationRequestHandler(context.Sut);

        // Act
        var act = async () => await sut.HandleAsync("{", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<JsonException>();
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
