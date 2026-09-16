using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using HostManagement.Communication.NamedPipe.RequestHandlers;
using HostManagement.Configuration;
using HostManagement.Services;
using HostManagement.Shared;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Communication.Serialization;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace HostManagement.Tests.Communication.NamedPipe.RequestHandlers;

public sealed class GetSupportedCapabilitiesRequestHandlerTests
{
    private const string ValidCapabilitiesIni =
        """
        [Settings]
        [Topics]
        """;

    [Fact]
    public void ShouldExposeExpectedTopicAndJsonTypeInfo()
    {
        // Arrange
        using var service = CreateCapabilitiesService();
        var sut = new GetSupportedCapabilitiesRequestHandler(service);

        // Assert
        sut.Topic.Should().Be(Topics.GetSupportedCapabilities);
        sut.TypedJsonTypeInfo.Should().BeSameAs(SourceGenerationContext.Default.GetSupportedCapabilitiesResult);
    }

    [Fact]
    public async Task ShouldReturnSerializedErrorResult_WhenCapabilitiesNotInitialized()
    {
        // Arrange
        using var capabilitiesService = CreateCapabilitiesService();
        var sut = new GetSupportedCapabilitiesRequestHandler(capabilitiesService);

        // Act
        var serializedResult = await sut.HandleAsync(string.Empty, Guid.Empty, CancellationToken.None);

        // Assert
        var result = SourceGeneratedTypeSerializer.Deserialize(serializedResult, sut.TypedJsonTypeInfo);

        result.Status.Should().Be(OperationStatus.Error);
        result.SupportedCapabilities.Should().NotBeNull();
        result.Message.Should().ContainAll(
            "Getting supported capabilities failed.",
            "Supported capabilities were not loaded.");
    }

    [Fact]
    public async Task ShouldReturnSerializedSupportedCapabilities_WhenGettingCapabilitiesSucceeds()
    {
        // Arrange
        using var capabilitiesService = CreateCapabilitiesService();
        _ = await capabilitiesService.InitializeSupportedCapabilities(CancellationToken.None);
        var sut = new GetSupportedCapabilitiesRequestHandler(capabilitiesService);

        // Act
        var serializedResult = await sut.HandleAsync(string.Empty, Guid.Empty, CancellationToken.None);

        // Assert
        var result = SourceGeneratedTypeSerializer.Deserialize(serializedResult, sut.TypedJsonTypeInfo);

        result.Status.Should().Be(OperationStatus.Success);
        result.SupportedCapabilities.Should().NotBeNull();
    }

    private static SupportedCapabilitiesService CreateCapabilitiesService() => new(CreateFileSystem(), Substitute.For<ILogger>());

    private static MockFileSystem CreateFileSystem(string capabilitiesIni = ValidCapabilitiesIni)
        => new(
            new Dictionary<string, MockFileData>
            {
                [AppPaths.SupportedCapabilitiesFilePath] = new(capabilitiesIni)
            });
}
