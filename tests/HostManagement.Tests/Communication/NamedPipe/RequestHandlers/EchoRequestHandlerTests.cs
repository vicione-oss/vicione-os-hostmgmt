using AwesomeAssertions;
using HostManagement.Communication.NamedPipe.RequestHandlers;
using HostManagement.Shared;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace HostManagement.Tests.Communication.NamedPipe.RequestHandlers;

public sealed class EchoRequestHandlerTests
{
    [Fact]
    public void ShouldReturnEchoTopic_WhenCalled()
        => new EchoRequestHandler(Substitute.For<ILogger>()).Topic.Should().Be(Topics.Echo);

    [Fact]
    public void ShouldThrowNotImplementedException_WhenJsonTypeInfoCalled()
    {
        var sut = new EchoRequestHandler(Substitute.For<ILogger>());
        var func = () => sut.JsonTypeInfo;
        func.Should().Throw<NotImplementedException>();
    }

    [Fact]
    public void ShouldThrowNotImplementedException_WhenCreateErrorInstanceCalled()
    {
        var sut = new EchoRequestHandler(Substitute.For<ILogger>());
        var func = () => sut.CreateErrorInstance("error");
        func.Should().Throw<NotImplementedException>();
    }

    public sealed class HandleAsync
    {
        [Fact]
        public async Task ShouldReturnPong_WhenMessageIsPing()
        {
            // Arrange
            var sut = new EchoRequestHandler(Substitute.For<ILogger>());

            // Act
            var result = await sut.HandleAsync("ping", Guid.NewGuid(), CancellationToken.None);

            // Assert
            result.Should().Be("pong");
        }

        [Theory]
        [InlineData("stress")]
        [InlineData("stress-1")]
        [InlineData("some-stress-message")]
        public async Task ShouldReturnStressResponse_WhenMessageContainsStress(string message)
        {
            // Arrange
            var sut = new EchoRequestHandler(Substitute.For<ILogger>());

            // Act
            var result = await sut.HandleAsync(message, Guid.NewGuid(), CancellationToken.None);

            // Assert
            result.Should().Be($"{message}-response");
        }

        [Theory]
        [InlineData("")]
        [InlineData("hello")]
        [InlineData("PING")]
        public async Task ShouldReturnNo_WhenMessageIsUnknown(string message)
        {
            // Arrange
            var sut = new EchoRequestHandler(Substitute.For<ILogger>());

            // Act
            var result = await sut.HandleAsync(message, Guid.NewGuid(), CancellationToken.None);

            // Assert
            result.Should().Be("No!");
        }

        [Fact]
        public async Task ShouldReturnWaitForIt_WhenMessageIsTrigger()
        {
            // Arrange
            var sut = new EchoRequestHandler(Substitute.For<ILogger>());
            using var cancellationTokenSource = new CancellationTokenSource();
            await cancellationTokenSource.CancelAsync();

            // Act
            var result = await sut.HandleAsync("trigger", Guid.NewGuid(), cancellationTokenSource.Token);

            // Assert
            result.Should().Be("Wait for it!");
        }
    }
}
