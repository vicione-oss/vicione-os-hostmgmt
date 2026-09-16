using System.Text.Json;
using AwesomeAssertions;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Communication.NamedPipe.Client;
using HostManagement.Shared.Contracts.System;
using HostManagement.Shared.Serialization;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace HostManagement.Shared.Communication.Tests.NamedPipe.Client;

public class PipeClientExtensionsTests
{
    private const string TestTopic = "GetServiceState";
    private const string TestServiceName = "ssh";
    private const string ActiveState = "active";

    public class SendRequestAsyncWithResponseContract
    {
        [Fact]
        public async Task ShouldThrow_WhenClientNull()
        {
            // Act
            var act = async () => await ((IPipeClient)null!)
                .SendRequestAsync<GetServiceStateResult>(TestTopic, "", cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("client");
        }

        [Fact]
        public async Task ShouldThrow_WhenResponseContentMalformed()
        {
            // Arrange
            await using var client = CreateClientReturning("not json");

            // Act
            var act = async () => await client.SendRequestAsync<GetServiceStateResult>(
                TestTopic,
                TestServiceName,
                cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            await act.Should().ThrowAsync<JsonException>();
        }

        [Fact]
        public async Task ShouldReturnDeserializedResponse_WhenResponseContentValid()
        {
            // Arrange
            await using var client = CreateClientReturning(SerializedServiceStateResult());

            // Act
            var result = await client.SendRequestAsync<GetServiceStateResult>(
                TestTopic,
                TestServiceName,
                cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.ActiveState.Should().Be(ActiveState);
            result.Status.Should().Be(OperationStatus.Success);
        }

        [Fact]
        public async Task ShouldReturnRawContent_WhenResponseContractIsString()
        {
            // Arrange
            const string RawResponse = "pong";
            await using var client = CreateClientReturning(RawResponse);

            // Act
            var result = await client.SendRequestAsync<string>(
                "echo",
                "ping",
                cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.Should().Be(RawResponse);
        }

        [Fact]
        public async Task ShouldForwardCorrelationId_WhenCorrelationIdGiven()
        {
            // Arrange
            var correlationId = Guid.NewGuid();
            await using var client = Substitute.For<IPipeClient>();

            client
                .SendRequestAsync(TestTopic, TestServiceName, correlationId, Arg.Any<CancellationToken>())
                .Returns(CreateResponse(SerializedServiceStateResult()));

            // Act
            var result = await client.SendRequestAsync<GetServiceStateResult>(
                TestTopic,
                TestServiceName,
                correlationId,
                cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.ActiveState.Should().Be(ActiveState);

            await client
                .Received(1)
                .SendRequestAsync(TestTopic, TestServiceName, correlationId, Arg.Any<CancellationToken>());
        }
    }

    public class SendRequestAsyncWithRequestContract
    {
        [Fact]
        public async Task ShouldThrow_WhenClientNull()
        {
            // Arrange
            var package = CreateSignedDebianPackage();

            // Act
            var act = async () => await ((IPipeClient)null!)
                .SendRequestAsync<SignedDebianPackage, SystemControlResult>(
                    TestTopic,
                    package,
                    cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("client");
        }

        [Fact]
        public async Task ShouldSendSerializedContent_WhenRequestContractGiven()
        {
            // Arrange
            var package = CreateSignedDebianPackage();
            await using var client = CreateClientReturning(SerializedServiceStateResult());

            // Act
            _ = await client.SendRequestAsync<SignedDebianPackage, GetServiceStateResult>(
                TestTopic,
                package,
                cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            await client.Received(1).SendRequestAsync(
                TestTopic,
                Arg.Is<string>(content => content.Contains(package.FilePath, StringComparison.Ordinal)),
                cancellationToken: Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ShouldSendContentUnchanged_WhenRequestContractIsString()
        {
            // Arrange
            const string RawRequest = "ping";
            await using var client = CreateClientReturning("pong");

            // Act
            _ = await client.SendRequestAsync<string, string>(
                "echo",
                RawRequest,
                cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            await client.Received(1).SendRequestAsync("echo", RawRequest, cancellationToken: Arg.Any<CancellationToken>());
        }
    }

    public class SendOperationAsync
    {
        [Fact]
        public async Task ShouldThrow_WhenClientNull()
        {
            // Act
            var act = async () => await ((IPipeClient)null!)
                .SendOperationAsync<GetServiceStateResult>(TestTopic, "", cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("client");
        }

        [Theory]
        [InlineData("not json")]
        [InlineData("")]
        [InlineData("{\"ResultType\":\"SomeOtherResult\"}")]
        public async Task ShouldReturnErrorResult_WhenResponseContentMalformed(string responseContent)
        {
            // Arrange
            await using var client = CreateClientReturning(responseContent);

            // Act
            var result = await client.SendOperationAsync<GetServiceStateResult>(
                TestTopic,
                TestServiceName,
                cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().Contain("could not be deserialized");
            result.ActiveState.Should().Be("unknown");
        }

        [Fact]
        public async Task ShouldReturnDeserializedResult_WhenResponseContentValid()
        {
            // Arrange
            await using var client = CreateClientReturning(SerializedServiceStateResult());

            // Act
            var result = await client.SendOperationAsync<GetServiceStateResult>(
                TestTopic,
                TestServiceName,
                cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.Status.Should().Be(OperationStatus.Success);
            result.ActiveState.Should().Be(ActiveState);
        }

        [Fact]
        public async Task ShouldPropagate_WhenSendingFails()
        {
            // Arrange
            await using var client = Substitute.For<IPipeClient>();

            client
                .SendRequestAsync(TestTopic, Arg.Any<string>(), cancellationToken: Arg.Any<CancellationToken>())
                .ThrowsAsync(new IOException("Pipe faulted."));

            // Act
            var act = async () => await client.SendOperationAsync<GetServiceStateResult>(
                TestTopic,
                "",
                cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            await act.Should().ThrowAsync<IOException>();
        }

        [Fact]
        public async Task ShouldSendSerializedContent_WhenRequestContractGiven()
        {
            // Arrange
            var package = CreateSignedDebianPackage();
            await using var client = CreateClientReturning(SerializedServiceStateResult());

            // Act
            _ = await client.SendOperationAsync<SignedDebianPackage, GetServiceStateResult>(
                TestTopic,
                package,
                cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            await client.Received(1).SendRequestAsync(
                TestTopic,
                Arg.Is<string>(content => content.Contains(package.FilePath, StringComparison.Ordinal)),
                cancellationToken: Arg.Any<CancellationToken>());
        }
    }

    private static IPipeClient CreateClientReturning(string responseContent)
    {
        var client = Substitute.For<IPipeClient>();

        client
            .SendRequestAsync(Arg.Any<string>(), Arg.Any<string>(), cancellationToken: Arg.Any<CancellationToken>())
            .Returns(CreateResponse(responseContent));

        return client;
    }

    private static NamedPipeMessage CreateResponse(string content) => new()
    {
        Topic = TestTopic,
        Type = MessageType.Response,
        Content = content
    };

    private static string SerializedServiceStateResult() => MessageSerializer.Serialize(new GetServiceStateResult
    {
        Status = OperationStatus.Success,
        Message = "Service state read.",
        ActiveState = ActiveState
    });

    private static SignedDebianPackage CreateSignedDebianPackage() => new()
    {
        FilePath = "/tmp/hostmanagement.deb",
        SignatureFilePath = "/tmp/hostmanagement.deb.minisig"
    };
}
