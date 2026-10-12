using System.Net;
using System.Net.NetworkInformation;
using AwesomeAssertions;
using HostManagement.Communication.NamedPipe.RequestHandlers;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Communication.Serialization;
using HostManagement.SystemConfig;
using HostManagement.SystemConfig.Contracts;
using HostManagement.Tests.SystemConfig;
using NSubstitute;
using Xunit;
using InternalSystemConfiguration = HostManagement.SystemConfig.Contracts.SystemConfiguration;

namespace HostManagement.Tests.Communication.NamedPipe.RequestHandlers;

[Collection("Non Parallel Tests")]
public sealed class ResetNetworkInterfaceRequestHandlerTests : IDisposable
{
    private const string Lan1 = "lan1";
    private const string TestHostname = "test-host";

    public ResetNetworkInterfaceRequestHandlerTests() => SystemConfigurationCache.Instance.ResetForTests();

    public void Dispose() => SystemConfigurationCache.Instance.ResetForTests();

    [Fact]
    public void ShouldExposeExpectedTopicAndJsonTypeInfo()
    {
        // Arrange
        var context = GetSystemConfigurationServiceContext();
        var sut = new ResetNetworkInterfaceRequestHandler(context.Sut);

        // Assert
        sut.Topic.Should().Be(Topics.ResetNetworkInterface);
        sut.TypedJsonTypeInfo.Should().BeSameAs(SourceGenerationContext.Default.SystemControlResult);
    }

    [Fact]
    public async Task ShouldNotPublishEvent_WhenResetFails()
    {
        // Arrange
        var context = GetSystemConfigurationServiceContext();

        ConfigureCurrentNetworkInterface(context);

        context.Persistence.ExistsPreparationConfiguration().Returns(false);

        var sut = new ResetNetworkInterfaceRequestHandler(context.Sut);

        PublishEventRequestedEventArgs? publishedEvent = null;
        sut.PublishEventRequested += (_, args) => publishedEvent = args;

        // Act
        var response = await sut.HandleAsync(Lan1, Guid.NewGuid(), CancellationToken.None);

        // Assert
        var result = SourceGeneratedTypeSerializer.Deserialize(
            response,
            SourceGenerationContext.Default.SystemControlResult);

        result.Status.Should().Be(OperationStatus.Error);
        publishedEvent.Should().BeNull();

        await context.ScriptHandler.Received(1)
            .ExecuteGetNetworkInterfacesCommonInformationScript(Arg.Any<ScriptOptions>());
    }

    [Fact]
    public async Task ShouldPublishConfigurationChangedEvent_WhenResetAndRefreshSucceed()
    {
        // Arrange
        var context = GetSystemConfigurationServiceContext();

        ConfigureSuccessfulReset(context);

        var sut = new ResetNetworkInterfaceRequestHandler(context.Sut);
        var correlationId = Guid.NewGuid();

        PublishEventRequestedEventArgs? publishedEvent = null;
        sut.PublishEventRequested += (_, args) => publishedEvent = args;

        // Act
        var response = await sut.HandleAsync(Lan1, correlationId, CancellationToken.None);

        // Assert
        var result = SourceGeneratedTypeSerializer.Deserialize(
            response,
            SourceGenerationContext.Default.SystemControlResult);

        result.Status.Should().Be(OperationStatus.Success);

        publishedEvent.Should().NotBeNull();

        var eventArgs = publishedEvent.Value;
        eventArgs.Topic.Should().Be(EventTopics.SystemConfigurationChanged);
        eventArgs.CorrelationId.Should().Be(correlationId);
        eventArgs.ExcludeRequestingClient.Should().BeTrue();

        var configurationResult = SourceGeneratedTypeSerializer.Deserialize(
            eventArgs.Content,
            SourceGenerationContext.Default.GetSystemConfigurationResult);

        configurationResult.Status.Should().Be(OperationStatus.Success);
        configurationResult.Configuration.Should().NotBeNull();

        configurationResult.Configuration.NetworkInterfaces.Should()
            .ContainSingle(@interface => @interface.CommonInformation.Name == Lan1);

        await context.ScriptHandler.Received(2)
            .ExecuteGetNetworkInterfacesCommonInformationScript(Arg.Any<ScriptOptions>());
    }

    [Fact]
    public async Task ShouldNotPublishEvent_WhenResetSucceedsButRefreshFails()
    {
        // Arrange
        var context = GetSystemConfigurationServiceContext();

        ConfigureSuccessfulReset(context);

        context.ScriptHandler.ExecuteGetNetworkInterfacesCommonInformationScript(Arg.Any<ScriptOptions>())
            .Returns(
                (
                    Result.SuccessResult,
                    CreateCurrentNetworkInterfaces()
                ),
                (
                    Result.TestErrorResult,
                    []
                ));

        var sut = new ResetNetworkInterfaceRequestHandler(context.Sut);

        PublishEventRequestedEventArgs? publishedEvent = null;
        sut.PublishEventRequested += (_, args) => publishedEvent = args;

        // Act
        var response = await sut.HandleAsync(Lan1, Guid.NewGuid(), CancellationToken.None);

        // Assert
        var result = SourceGeneratedTypeSerializer.Deserialize(
            response,
            SourceGenerationContext.Default.SystemControlResult);

        result.Status.Should().Be(OperationStatus.Success);

        publishedEvent.Should().BeNull();

        await context.ScriptHandler.Received(2)
            .ExecuteGetNetworkInterfacesCommonInformationScript(Arg.Any<ScriptOptions>());
    }

    private static SystemConfigurationServiceContext GetSystemConfigurationServiceContext()
        => new(Lan1, TestHostname);

    private static void ConfigureSuccessfulReset(SystemConfigurationServiceContext context)
    {
        ConfigureCurrentNetworkInterface(context);

        context.Persistence.ExistsPreparationConfiguration().Returns(true);

        context.Persistence
            .LoadPreparationConfiguration(Arg.Any<CancellationToken>())
            .Returns(CreatePreparationConfiguration());

        context.Persistence
            .SaveUserConfiguration(
                Arg.Any<InternalSystemConfiguration>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
    }

    private static void ConfigureCurrentNetworkInterface(SystemConfigurationServiceContext context)
    {
        context.ScriptHandler.ExecuteGetNetworkInterfacesCommonInformationScript(Arg.Any<ScriptOptions>())
            .ReturnsForAnyArgs((Result.SuccessResult, CreateCurrentNetworkInterfaces()));

        context.ScriptHandler
            .ExecuteGetInterfaceSettingsScript(Arg.Any<string>())
            .ReturnsForAnyArgs((
                Result.SuccessResult,
                new IPv4Settings
                {
                    DHCPEnabled = true,
                    Gateway = IPAddress.Parse("10.10.10.1"),
                    IPv4Details =
                    [
                        new()
                        {
                            IPAddress = IPAddress.Parse("10.10.10.2"),
                            Netmask = IPAddress.Parse("255.255.255.0")
                        }
                    ]
                },
                new VLANSettings()));
    }

    private static List<NetworkInterfaceCommonInformation> CreateCurrentNetworkInterfaces()
        => [
            new()
            {
                Name = Lan1,
                Enabled = true,
                UserDefinedMACAddress = new()
                {
                    Enabled = true,
                    Address = PhysicalAddress.Parse("11:22:33:44:55:66")
                }
            }
        ];

    private static LoadedSystemConfiguration CreatePreparationConfiguration() => new()
    {
        SourceVersion = InternalSystemConfiguration.CurrentVersion,
        Configuration = new()
        {
            Version = InternalSystemConfiguration.CurrentVersion,
            NetworkDNSSettings = new()
            {
                Hostname = TestHostname
            },
            NetworkInterfaces =
            [
                new()
                {
                    CommonInformation = new()
                    {
                        Name = Lan1,
                        Enabled = false,
                        UserDefinedMACAddress = new()
                        {
                            Enabled = true,
                            Address = PhysicalAddress.Parse("aa:bb:cc:dd:ee:ff")
                        }
                    },
                    IPv4 = new()
                    {
                        DHCPEnabled = false,
                        Gateway = IPAddress.Parse("192.168.1.1"),
                        IPv4Details =
                        [
                            new()
                            {
                                IPAddress = IPAddress.Parse("192.168.1.2"),
                                Netmask = IPAddress.Parse("255.255.255.0")
                            }
                        ]
                    },
                    VLAN = new()
                    {
                        Enabled = true,
                        ID = 100
                    }
                }
            ]
        }
    };
}
