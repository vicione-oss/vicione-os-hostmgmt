using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace HostManagement.Tests;

public class ConsoleLifetimeTests
{
    public sealed class TokenProperty
    {
        [Fact]
        public void ShouldNotBeCancelled_AfterCreation()
        {
            // Arrange
            using var sut = new ConsoleLifetime(NullLogger.Instance);

            // Act
            var token = sut.Token;

            // Assert
            token.IsCancellationRequested.Should().BeFalse();
        }
    }

    public sealed class Cancel
    {
        [Fact]
        public void ShouldCancelToken_WhenCanceled()
        {
            // Arrange
            using var sut = new ConsoleLifetime(NullLogger.Instance);

            // Act
            sut.Cancel();

            // Assert
            sut.Token.IsCancellationRequested.Should().BeTrue();
        }
    }

    public sealed class Dispose
    {
        [Fact]
        public void ShouldNotThrow_WhenCalledOnce()
        {
            // Arrange
            using var sut = new ConsoleLifetime(NullLogger.Instance);

            // Act
            var action = sut.Dispose;

            // Assert
            action.Should().NotThrow();
        }

        [Fact]
        public void ShouldNotThrow_AfterCancel()
        {
            // Arrange
            using var sut = new ConsoleLifetime(NullLogger.Instance);
            sut.Cancel();

            // Act
            var action = sut.Dispose;

            // Assert
            action.Should().NotThrow();
        }
    }
}
