using System.Net;
using System.Text.Json;
using System.Text;
using AwesomeAssertions;
using Xunit;
using HostManagement.Shared.Contracts.Network;

namespace HostManagement.Shared.Tests.Converters;

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

    public class CanConvert : IPAddressConverterTests
    {
        [Fact]
        public void ShouldReturnFalse_WhenTypeIsNotIPAddress()
        {
            // Act
            var result = _sut.CanConvert(typeof(string));

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnTrue_WhenTypeIsIPAddress()
        {
            // Act
            var result = _sut.CanConvert(typeof(IPAddress));

            // Assert
            result.Should().BeTrue();
        }

    }

    public class Read : IPAddressConverterTests
    {
        [Fact]
        public void ShouldThrow_WhenTokenIsNotString()
        {
            // Arrange
            var expectedMessage = "Expected string as the JSON token type.";
            var json = "123";

            // Act
            var result = () =>
            {
                var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
                reader.Read();

                return _sut.Read(ref reader, typeof(IPAddress), _options);
            };

            // Assert
            result.Should()
                .Throw<JsonException>()
                .WithMessage(expectedMessage);
        }

        [Fact]
        public void ShouldThrow_WhenStringIsEmpty()
        {
            // Arrange
            var jsonTokenString = "  ";
            var json =
                $"""
                "{jsonTokenString}"
                """;
            var expectedMessage = $"IPAddress '{jsonTokenString}' can not be empty.";

            // Act
            var result = () =>
            {
                var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
                reader.Read();

                return _sut.Read(ref reader, typeof(IPAddress), _options);
            };

            // Assert
            result.Should()
                .Throw<JsonException>()
                .WithMessage(expectedMessage);
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
            var expectedMessage = $"Invalid IP address format: '{jsonTokenString}'";

            // Act
            var result = () =>
            {
                var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
                reader.Read();

                return _sut.Read(ref reader, typeof(IPAddress), _options);
            };

            // Assert
            result.Should()
                .Throw<JsonException>()
                .WithMessage(expectedMessage);
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
    }

    public class Write : IPAddressConverterTests
    {
        [Fact]
        public void ShouldWriteStringEmpty_WhenValueIsNull()
        {
            // Arrange
            IPAddress? value = null;
            var expectedJson =
                $"""
                "{string.Empty}"
                """;

            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream);

            // Act
            _sut.Write(writer, value!, _options);
            writer.Flush();

            // Assert
            var result = Encoding.UTF8.GetString(stream.ToArray());
            result.Should().Be(expectedJson);
        }

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

