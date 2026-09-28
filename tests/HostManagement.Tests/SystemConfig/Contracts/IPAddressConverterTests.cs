using System.Net;
using System.Text;
using System.Text.Json;
using AwesomeAssertions;
using HostManagement.SystemConfig.Contracts;
using Xunit;

namespace HostManagement.Tests.SystemConfig.Contracts;

public class IPAddressConverterTests
{
    private readonly IPAddressConverter _sut = new();
    private readonly JsonSerializerOptions _options;

    public IPAddressConverterTests()
    {
        _options = new JsonSerializerOptions
        {
            Converters = { _sut }
        };
    }

    public class Read : IPAddressConverterTests
    {
        [Fact]
        public void ShouldThrow_WhenTokenIsNotStringOrNull()
        {
            // Arrange
            var expectedMessage = "Expected string or null as the JSON token type.";
            var json = "123";

            // Act
            var result = () =>
            {
                var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
                reader.Read();

                return _sut.Read(ref reader, typeof(IPAddress), _options);
            };

            // Assert
            result.Should().Throw<JsonException>().WithMessage(expectedMessage);
        }

        [Fact]
        public void ShouldThrow_WhenStringIsInvalidIPAddressFormat()
        {
            // Arrange
            var jsonTokenString = "x192.168.178.42";
            var json =
                $"""
                "{jsonTokenString}"
                """;

            // Act
            var result = () =>
            {
                var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
                reader.Read();

                return _sut.Read(ref reader, typeof(IPAddress), _options);
            };

            // Assert
            result.Should().Throw<FormatException>();
        }

        [Fact]
        public void ShouldReturnIPAddress_WhenValidIPAddressStringProvided()
        {
            // Arrange
            var expectedIPAddress = IPAddress.Parse("192.168.178.42");
            var json =
                $"""
                "{expectedIPAddress}"
                """;

            var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));

            reader.Read();

            // Act
            var result = _sut.Read(ref reader, typeof(IPAddress), _options);

            // Assert
            result.Should().Be(expectedIPAddress);
        }

        [Fact]
        public void ShouldReturnNull_WhenTokenIsNull()
        {
            // Arrange
            var json = "null";
            var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));

            reader.Read();

            // Act
            var result = _sut.Read(ref reader, typeof(IPAddress), _options);

            // Assert
            result.Should().Be(null);
        }
    }

    public class Write : IPAddressConverterTests
    {
        [Fact]
        public void ShouldWriteIPAddressString_WhenValueIsNotNull()
        {
            // Arrange
            var value = IPAddress.Parse("192.168.178.42");
            var expectedJson =
                $"""
                "{value}"
                """;

            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream);

            // Act
            _sut.Write(writer, value, _options);
            writer.Flush();

            // Assert
            var result = Encoding.UTF8.GetString(stream.ToArray());
            result.Should().Be(expectedJson);
        }
    }
}
