using System.Text.Json;
using System.Text.Json.Serialization;
using AwesomeAssertions;
using HostManagement.Communication.NamedPipe.RequestHandlers;
using HostManagement.Shared;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Serialization;
using HostManagement.SystemConfig;
using HostManagement.Tests.SystemConfig;
using NSubstitute;
using Xunit;
using InternalSystemConfiguration = HostManagement.SystemConfig.Contracts.SystemConfiguration;

namespace HostManagement.Tests.Communication.NamedPipe.RequestHandlers;

/// <summary>
/// Tests a configuration round trip using a client contract that predates newer server settings.
/// </summary>
[Collection("Non Parallel Tests")]
public sealed class SystemConfigurationForwardCompatibilityTests : IDisposable
{
    // TODO This is an E2E test. We have to restructure tests to reflect the test scopes (unit, integration, e2e).

    private const string InitialHostname = "original-host";
    private const string UpdatedHostname = "updated-host";
    private const string PrimaryDNSSuffix = "example.test";
    private const string SyslogAddress = "192.0.2.10:514";
    private const int SyslogRetrySeconds = 45;

    public SystemConfigurationForwardCompatibilityTests() => SystemConfigurationCache.Instance.ResetForTests();

    public void Dispose() => SystemConfigurationCache.Instance.ResetForTests();

    [Fact]
    public async Task ShouldPreserveUnknownSettings_WhenOlderClientReadsModifiesAndSubmitsConfiguration()
    {
        // Arrange
        var context = CreateContext();
        var getHandler = new GetSystemConfigurationRequestHandler(context.Sut);
        var setHandler = new SetSystemConfigurationRequestHandler(context.Sut);
        var correlationId = Guid.NewGuid();

        PublishEventRequestedEventArgs? publishedEvent = null;
        setHandler.PublishEventRequested += (_, args) => publishedEvent = args;

        var getResponseJson = await getHandler.HandleAsync(
            string.Empty,
            Guid.NewGuid(),
            TestContext.Current.CancellationToken);

        // The server exposes settings unknown to the simulated older client.
        using (var serverResponse = JsonDocument.Parse(getResponseJson))
        {
            var configuration = serverResponse.RootElement.GetProperty("Configuration");
            configuration.GetProperty("Syslog").GetProperty("ConnectionRetrySeconds").GetInt32()
                .Should().Be(SyslogRetrySeconds);
            configuration.GetProperty("NetworkDNSSettings").GetProperty("PrimaryDNSSuffix")
                .GetProperty("Suffix").GetString().Should().Be(PrimaryDNSSuffix);
        }

        // Act

        // Deserialize with the older contract, modify a known property and serialize it back.
        var olderResponse = JsonSerializer.Deserialize(
            getResponseJson,
            OlderClientJsonContext.Default.OlderClientGetConfigurationResult)
            ?? throw new InvalidOperationException("The older client could not read the response.");

        olderResponse.Status.Should().Be(OperationStatus.Success);

        var olderConfiguration = olderResponse.Configuration
            ?? throw new InvalidOperationException("The response contains no configuration.");

        olderConfiguration.NetworkDNSSettings.Hostname = UpdatedHostname;

        var requestJson = JsonSerializer.Serialize(
            olderConfiguration,
            OlderClientJsonContext.Default.OlderClientConfiguration);

        using (var olderRequest = JsonDocument.Parse(requestJson))
        {
            olderRequest.RootElement.TryGetProperty("Syslog", out _).Should().BeFalse();
            olderRequest.RootElement.TryGetProperty("NetworkInterfaces", out _).Should().BeFalse();
            olderRequest.RootElement.GetProperty("NetworkDNSSettings")
                .TryGetProperty("PrimaryDNSSuffix", out _).Should().BeFalse();
        }

        var setResponseJson = await setHandler.HandleAsync(
            requestJson,
            correlationId,
            TestContext.Current.CancellationToken);

        // Assert
        var setResponse = MessageSerializer.Deserialize<SetSystemConfigurationResult>(setResponseJson);
        setResponse.Status.Should().Be(OperationStatus.Success);

        // The effective configuration, not the reduced client payload, is persisted and published.
        await context.Persistence.Received(1).SaveUserConfiguration(
            Arg.Is<InternalSystemConfiguration>(config =>
                config.Version == 2 &&
                config.NetworkDNSSettings != null &&
                config.NetworkDNSSettings.Hostname == UpdatedHostname &&
                config.NetworkDNSSettings.PrimaryDNSSuffix != null &&
                config.NetworkDNSSettings.PrimaryDNSSuffix.Enabled == true &&
                config.NetworkDNSSettings.PrimaryDNSSuffix.Suffix == PrimaryDNSSuffix &&
                config.Syslog != null &&
                config.Syslog.Enabled == true &&
                config.Syslog.Address == SyslogAddress &&
                config.Syslog.ConnectionRetrySeconds == SyslogRetrySeconds &&
                config.NetworkInterfaces != null &&
                config.NetworkInterfaces.Count == 1 &&
                config.NetworkInterfaces[0].CommonInformation != null &&
                config.NetworkInterfaces[0].CommonInformation!.Name == "lan1"),
            TestContext.Current.CancellationToken);

        publishedEvent.Should().NotBeNull();
        publishedEvent.Value.Topic.Should().Be(EventTopics.SystemConfigurationChanged);
        publishedEvent.Value.CorrelationId.Should().Be(correlationId);
        publishedEvent.Value.ExcludeRequestingClient.Should().BeTrue();

        var changed = MessageSerializer.Deserialize<GetSystemConfigurationResult>(publishedEvent.Value.Content);
        changed.Status.Should().Be(OperationStatus.Success);
        changed.Configuration.Should().NotBeNull();
        changed.Configuration!.NetworkDNSSettings.Hostname.Should().Be(UpdatedHostname);
        changed.Configuration.NetworkDNSSettings.PrimaryDNSSuffix.Suffix.Should().Be(PrimaryDNSSuffix);
        changed.Configuration.Syslog.Enabled.Should().BeTrue();
        changed.Configuration.Syslog.Address.Should().Be(SyslogAddress);
        changed.Configuration.Syslog.ConnectionRetrySeconds.Should().Be(SyslogRetrySeconds);
        changed.Configuration.NetworkInterfaces.Should().ContainSingle();
    }

    [Fact]
    public async Task ShouldRejectInvalidKnownSetting_WhenOlderClientSubmitsConfiguration()
    {
        // Arrange
        var context = CreateContext();
        var getHandler = new GetSystemConfigurationRequestHandler(context.Sut);
        var setHandler = new SetSystemConfigurationRequestHandler(context.Sut);
        var eventCount = 0;
        setHandler.PublishEventRequested += (_, _) => eventCount++;

        var getResponseJson = await getHandler.HandleAsync(
            string.Empty,
            Guid.NewGuid(),
            TestContext.Current.CancellationToken);

        var olderResponse = JsonSerializer.Deserialize(
            getResponseJson,
            OlderClientJsonContext.Default.OlderClientGetConfigurationResult)
            ?? throw new InvalidOperationException("The older client could not read the response.");

        var olderConfiguration = olderResponse.Configuration
            ?? throw new InvalidOperationException("The response contains no configuration.");

        olderConfiguration.NetworkDNSSettings.Hostname = "invalid hostname";

        var requestJson = JsonSerializer.Serialize(
            olderConfiguration,
            OlderClientJsonContext.Default.OlderClientConfiguration);

        // Act
        var setResponseJson = await setHandler.HandleAsync(
            requestJson,
            Guid.NewGuid(),
            TestContext.Current.CancellationToken);

        // Assert
        var setResponse = MessageSerializer.Deserialize<SetSystemConfigurationResult>(setResponseJson);
        setResponse.Status.Should().Be(OperationStatus.Error);

        eventCount.Should().Be(0);

        await context.Persistence.DidNotReceive().SaveUserConfiguration(
            Arg.Any<InternalSystemConfiguration>(),
            Arg.Any<CancellationToken>());
    }

    private static SystemConfigurationServiceContext CreateContext()
    {
        var context = new SystemConfigurationServiceContext("lan1", InitialHostname);
        context.ConfigureValidNetworkInterface();

        context.ScriptHandler.ExecuteGetNetworkDNSSettingsScript()
            .ReturnsForAnyArgs((Result.SuccessResult, new HostManagement.SystemConfig.Contracts.NetworkDNSSettings
            {
                Hostname = InitialHostname,
                PrimaryDNSSuffix = new() { Enabled = true, Suffix = PrimaryDNSSuffix }
            }));

        context.ScriptHandler.ExecuteGetSyslogSettingsScript()
            .ReturnsForAnyArgs((Result.SuccessResult, new HostManagement.SystemConfig.Contracts.SyslogSettings
            {
                Enabled = true,
                Address = SyslogAddress,
                ConnectionRetrySeconds = SyslogRetrySeconds
            }));

        return context;
    }
}

// Test-only DTOs deliberately omit Syslog, network interfaces and newer DNS properties.
// These mimic an older client whose serializer has been updated to skip unknown response fields.
internal sealed class OlderClientGetConfigurationResult
{
    public OperationStatus Status { get; set; }

    public OlderClientConfiguration? Configuration { get; set; }
}

internal sealed class OlderClientConfiguration
{
    public int Version { get; set; }

    public OlderClientNetworkDNSSettings NetworkDNSSettings { get; set; } = new();
}

internal sealed class OlderClientNetworkDNSSettings
{
    public string Hostname { get; set; } = string.Empty;
}

[JsonSourceGenerationOptions(
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
    UseStringEnumConverter = true)]
[JsonSerializable(typeof(OlderClientGetConfigurationResult))]
[JsonSerializable(typeof(OlderClientConfiguration))]
internal partial class OlderClientJsonContext : JsonSerializerContext;
