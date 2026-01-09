using FluentAssertions;
using NSubstitute;
using Xunit;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Service;

namespace HostManagement.Tests.Service;

public class ServiceControlAdapterTests
{
    public class RestartService
    {
        [Fact]
        public async Task ShouldReturnError_WhenSetServiceReturnsErrors()
        {
            // Arrange
            var restartServiceSubstitute = Substitute.For<IScriptHandler>();
            restartServiceSubstitute
                .ExecuteSetServiceStateScript(Arg.Any<ScriptOptions>())
                .Returns(OperationStatus.Error);

            var sut = new ServiceControlAdapter(restartServiceSubstitute);

            // Act
            var status = await sut.RestartService("ServiceName");

            // Assert
            status.Should().Be(OperationStatus.Error);
        }

        [Fact]
        public async Task ShouldReturnError_WhenSetServiceReturnsWarnings()
        {
            // Arrange
            var restartServiceSubstitute = Substitute.For<IScriptHandler>();
            restartServiceSubstitute
                .ExecuteSetServiceStateScript(Arg.Any<ScriptOptions>())
                .Returns(OperationStatus.Warning);

            var sut = new ServiceControlAdapter(restartServiceSubstitute);

            // Act
            var status = await sut.RestartService("ServiceName");

            // Assert
            status.Should().Be(OperationStatus.Warning);
        }

        [Fact]
        public async Task ShouldReturnSuccess_WhenSetServiceReturnsSuccess()
        {
            // Arrange
            var restartServiceSubstitute = Substitute.For<IScriptHandler>();
            restartServiceSubstitute
                .ExecuteSetServiceStateScript(Arg.Any<ScriptOptions>())
                .Returns(OperationStatus.Success);

            var sut = new ServiceControlAdapter(restartServiceSubstitute);

            // Act
            var status = await sut.RestartService("ServiceName");

            // Assert
            status.Should().Be(OperationStatus.Success);
        }
    }
}
