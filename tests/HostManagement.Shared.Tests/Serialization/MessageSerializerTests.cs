using System.Text.Json;
using AwesomeAssertions;
using HostManagement.Shared.Contracts.System;
using HostManagement.Shared.Serialization;
using Xunit;

namespace HostManagement.Shared.Tests.Serialization;

public class MessageSerializerTests
{
    private const string PackagePath = "/tmp/hostmanagement.deb";
    private const string SignaturePath = "/tmp/hostmanagement.deb.minisig";

    public class Serialize
    {
        [Fact]
        public void ShouldThrow_WhenValueNull()
        {
            // Act
            var act = () => MessageSerializer.Serialize<SignedDebianPackage>(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("value");
        }

        [Fact]
        public void ShouldThrow_WhenContractNotRegistered()
        {
            // Arrange
            var value = new UnregisteredContract { Name = "test" };

            // Act
            var act = () => MessageSerializer.Serialize(value);

            // Assert
            act.Should().Throw<InvalidOperationException>();
        }

        [Theory]
        [InlineData("ping")]
        [InlineData("")]
        [InlineData("{\"NotJson\": ")]
        public void ShouldReturnValueUnchanged_WhenValueIsRawText(string rawText)
        {
            // Act
            var result = MessageSerializer.Serialize(rawText);

            // Assert
            result.Should().Be(rawText);
        }

        [Fact]
        public void ShouldReturnJson_WhenContractRegistered()
        {
            // Arrange
            var package = new SignedDebianPackage { FilePath = PackagePath, SignatureFilePath = SignaturePath };

            // Act
            var result = MessageSerializer.Serialize(package);

            // Assert
            result.Should().Contain(PackagePath).And.Contain(SignaturePath);
        }
    }

    public class Deserialize
    {
        [Fact]
        public void ShouldThrow_WhenContentNull()
        {
            // Act
            var act = () => MessageSerializer.Deserialize<SignedDebianPackage>(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("content");
        }

        [Fact]
        public void ShouldThrow_WhenContractNotRegistered()
        {
            // Act
            var act = () => MessageSerializer.Deserialize<UnregisteredContract>("{}");

            // Assert
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void ShouldThrow_WhenContentIsNotValidJson()
        {
            // Act
            var act = () => MessageSerializer.Deserialize<SignedDebianPackage>("not json");

            // Assert
            act.Should().Throw<JsonException>();
        }

        [Fact]
        public void ShouldThrow_WhenContentIsJsonNull()
        {
            // Act
            var act = () => MessageSerializer.Deserialize<SignedDebianPackage>("null");

            // Assert
            act.Should().Throw<JsonException>().WithMessage("*could not be deserialized*");
        }

        [Theory]
        [InlineData("pong")]
        [InlineData("")]
        [InlineData("not json at all")]
        public void ShouldReturnContentUnchanged_WhenContractIsRawText(string rawText)
        {
            // Act
            var result = MessageSerializer.Deserialize<string>(rawText);

            // Assert
            result.Should().Be(rawText);
        }

        [Fact]
        public void ShouldReturnContract_WhenContentIsValidJson()
        {
            // Arrange
            var package = new SignedDebianPackage { FilePath = PackagePath, SignatureFilePath = SignaturePath };
            var json = MessageSerializer.Serialize(package);

            // Act
            var result = MessageSerializer.Deserialize<SignedDebianPackage>(json);

            // Assert
            result.FilePath.Should().Be(PackagePath);
            result.SignatureFilePath.Should().Be(SignaturePath);
        }
    }

    public class RoundTrip
    {
        [Theory]
        [InlineData("ping")]
        [InlineData("\"already quoted\"")]
        [InlineData("{}")]
        public void ShouldNotAddQuotes_WhenRawTextRoundTripped(string rawText)
        {
            // Act
            var serialized = MessageSerializer.Serialize(rawText);
            var result = MessageSerializer.Deserialize<string>(serialized);

            // Assert
            result.Should().Be(rawText);
        }
    }
}
