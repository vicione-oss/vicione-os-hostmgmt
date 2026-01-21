using System.Text.Json.Serialization.Metadata;
using AwesomeAssertions;
using HostManagement.Communication.NamedPipe.RequestHandlers;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Enums;
using Xunit;

namespace HostManagement.Tests.Communication.NamedPipe.RequestHandlers;

public sealed class RequestHandlerTests
{
    private const string ErrorMessage = "Unit-Test-Error";

    public sealed class TypedJsonTypeInfoProperty
    {
        [Fact]
        public void ShouldReturnJsonTypeInfo_WhenTypeRegistered()
        {
            // Arrange
            var sut = new DummyRegisteredHandler();

            // Act
            var jsonTypeInfo = sut.TypedJsonTypeInfo;

            // Assert
            jsonTypeInfo.Should().NotBeNull();
            jsonTypeInfo.Type.Should().Be<ServiceControlResult>(); // Dummy uses ServiceControlResult
        }
    }

    public sealed class JsonTypeInfoProperty
    {
        [Fact]
        public void ShouldReturnSameInstance_AsTypedJsonTypeInfo_WhenTypeRegistered()
        {
            // Arrange
            var sut = new DummyRegisteredHandler();

            // Act
            var untypedJsonTypeInfo = sut.JsonTypeInfo;
            var typedJsonTypeInfo = sut.TypedJsonTypeInfo;

            // Assert
            ReferenceEquals(untypedJsonTypeInfo, typedJsonTypeInfo).Should().BeTrue();
        }
    }

    public sealed class CreateTypedErrorInstanceMethod
    {
        [Fact]
        public void ShouldCreateErrorInstance_WhenCalled()
        {
            // Arrange
            var sut = new DummyRegisteredHandler();

            // Act
            var result = sut.CreateTypedErrorInstance(ErrorMessage);

            // Assert
            result.Should().NotBeNull();
            result.ResultType.Should().Be(nameof(ServiceControlResult)); // Dummy uses ServiceControlResult
            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().Be(ErrorMessage);

            var asInterface = (IRequestHandler)sut;
            var boxed = asInterface.CreateErrorInstance(ErrorMessage);
            boxed.Should().BeOfType<ServiceControlResult>(); // Dummy uses ServiceControlResult
        }
    }

    private sealed class DummyRegisteredHandler : RequestHandler<ServiceControlResult>
    {
        public override string Topic => "dummy.registered";

        public override JsonTypeInfo<ServiceControlResult> TypedJsonTypeInfo
            => SourceGenerationContext.Default.ServiceControlResult;

        public override Task<string> HandleAsync(string messageContent, CancellationToken cancellationToken)
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
