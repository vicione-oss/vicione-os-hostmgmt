using AwesomeAssertions;
using HostManagement.Communication.NamedPipe.RequestHandlers;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Serialization;
using Xunit;

namespace HostManagement.Tests.Communication.NamedPipe.RequestHandlers;

public sealed class RequestHandlerTests
{
    private const string ErrorMessage = "Unit-Test-Error";

    public sealed class CreateErrorResponseContentMethod
    {
        [Fact]
        public void ShouldThrow_WhenResponseTypeNotRegistered()
        {
            // Arrange
            var sut = new DummyUnregisteredHandler();

            // Act
            var act = () => sut.CreateErrorResponseContent(ErrorMessage);

            // Assert
            act.Should()
                .Throw<InvalidOperationException>()
                .WithMessage($"*{typeof(UnregisteredDummyResult).FullName}*");
        }

        [Fact]
        public void ShouldReturnSerializedErrorResult_WhenResponseTypeRegistered()
        {
            // Arrange
            var sut = new DummyRegisteredHandler();

            // Act
            var content = sut.CreateErrorResponseContent(ErrorMessage);

            // Assert
            var result = MessageSerializer.Deserialize<ServiceControlResult>(content);

            result.ResultType.Should().Be(nameof(ServiceControlResult));
            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().Be(ErrorMessage);
        }

        [Fact]
        public void ShouldReturnOverriddenContent_WhenOverridden()
        {
            // Arrange
            var sut = new DummyOverridingHandler();

            // Act
            var content = sut.CreateErrorResponseContent(ErrorMessage);

            // Assert
            content.Should().Be(ErrorMessage);
        }
    }

    private sealed class DummyRegisteredHandler : RequestHandler<ServiceControlResult>
    {
        public override string Topic => "dummy.registered";

        public override Task<string> HandleAsync(
            string messageContent,
            Guid correlationId,
            CancellationToken cancellationToken)
            => Task.FromResult(string.Empty);
    }

    private sealed class DummyUnregisteredHandler : RequestHandler<UnregisteredDummyResult>
    {
        public override string Topic => "dummy.unregistered";

        public override Task<string> HandleAsync(
            string messageContent,
            Guid correlationId,
            CancellationToken cancellationToken)
            => Task.FromResult(string.Empty);
    }

    private sealed class DummyOverridingHandler : RequestHandler<ServiceControlResult>
    {
        public override string Topic => "dummy.overriding";

        public override string CreateErrorResponseContent(string message) => message;

        public override Task<string> HandleAsync(
            string messageContent,
            Guid correlationId,
            CancellationToken cancellationToken)
            => Task.FromResult(string.Empty);
    }

    private sealed record UnregisteredDummyResult : OperationResult, IOperationResultFactory<UnregisteredDummyResult>
    {
        public UnregisteredDummyResult() : base(nameof(UnregisteredDummyResult)) { }

        protected override string ExpectedType => nameof(UnregisteredDummyResult);

        public static UnregisteredDummyResult CreateErrorResult(string message) => new()
        {
            Status = OperationStatus.Error,
            Message = message
        };
    }
}
