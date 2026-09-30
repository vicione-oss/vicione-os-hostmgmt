using AwesomeAssertions;
using HostManagement.Communication.NamedPipe.RequestHandlers;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Communication.Serialization;
using HostManagement.SystemConfig;
using HostManagement.Tests.SystemConfig;
using NSubstitute;
using Xunit;
using CommunicationJsonContext = HostManagement.Shared.Communication.Contracts.SourceGenerationContext;

namespace HostManagement.Tests.Communication.NamedPipe.RequestHandlers;

[Collection("Non Parallel Tests")]
public sealed class GetSystemConfigurationRequestHandlerTests : IDisposable
{
    private const string Lan1 = "lan1";
    private const string TestHostname = "test-host";

    public GetSystemConfigurationRequestHandlerTests() => SystemConfigurationCache.Instance.ResetForTests();

    public void Dispose() => SystemConfigurationCache.Instance.ResetForTests();

    [Fact]
    public void ShouldExposeExpectedTopicAndJsonTypeInfo()
    {
        // Arrange
        var context = GetSystemConfigurationServiceContext();
        var sut = new GetSystemConfigurationRequestHandler(context.Sut);

        // Assert
        sut.Topic.Should().Be(Topics.GetSystemConfiguration);
        sut.TypedJsonTypeInfo.Should().BeSameAs(CommunicationJsonContext.Default.GetSystemConfigurationResult);
    }

    [Fact]
    public async Task ShouldReturnSerializedErrorResult_WhenGettingConfigurationFails()
    {
        // Arrange
        var context = GetSystemConfigurationServiceContext();

        context.ScriptHandler.ExecuteGetNetworkInterfacesCommonInformationScript(Arg.Any<ScriptOptions>())
            .ReturnsForAnyArgs((Result.TestErrorResult, []));

        var sut = new GetSystemConfigurationRequestHandler(context.Sut);

        // Act
        var serializedResult = await sut.HandleAsync(string.Empty, Guid.Empty, CancellationToken.None);

        // Assert
        var result = SourceGeneratedTypeSerializer.Deserialize(serializedResult, sut.TypedJsonTypeInfo);

        result.Status.Should().Be(OperationStatus.Error);
        result.Configuration.Should().BeNull();
        result.Message.Should().Contain(Result.TestErrorMessage);
    }

    [Fact]
    public async Task ShouldReturnSerializedConfiguration_WhenGettingConfigurationSucceeds()
    {
        // Arrange
        var context = GetSystemConfigurationServiceContext();
        var sut = new GetSystemConfigurationRequestHandler(context.Sut);

        // Act
        var serializedResult = await sut.HandleAsync(string.Empty, Guid.Empty, CancellationToken.None);

        // Assert
        var result = SourceGeneratedTypeSerializer.Deserialize(serializedResult, sut.TypedJsonTypeInfo);

        result.Status.Should().Be(OperationStatus.Success);
        result.Configuration.Should().NotBeNull();
    }

    private static SystemConfigurationServiceContext GetSystemConfigurationServiceContext()
        => new(Lan1, TestHostname);
}
