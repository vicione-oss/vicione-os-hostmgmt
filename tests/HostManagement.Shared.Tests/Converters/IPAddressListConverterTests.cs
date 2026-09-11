using System.Net;
using System.Text;
using System.Text.Json;
using AwesomeAssertions;
using HostManagement.Shared.Contracts.Network;
using Xunit;

namespace HostManagement.Shared.Tests.Converters;

public class IPAddressListConverterTests
{
    private readonly IPAddressListConverter _sut = new();
    private readonly JsonSerializerOptions _options;

    public IPAddressListConverterTests()
    {
        _options = new JsonSerializerOptions
        {
            Converters = { _sut }
        };
    }

    public class CanConvert : IPAddressListConverterTests
    {
        [Fact]
        public void ShouldReturnFalse_WhenTypeIsNotIPAddressList()
        {
            // Act
            var result = _sut.CanConvert(typeof(string));

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenTypeIsIPAddressArray()
        {
            // Act
            var result = _sut.CanConvert(typeof(IPAddress[]));

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnTrue_WhenTypeIsIPAddressList()
        {
            // Act
            var result = _sut.CanConvert(typeof(List<IPAddress>));

            // Assert
            result.Should().BeTrue();
        }
    }

    public class Read : IPAddressListConverterTests
    {
        [Fact]
        public void ShouldThrow_WhenTokenIsNotStartArray()
        {
            // Arrange
            var expectedMessage = "Expected start of array as the JSON token type.";
            var json =
                """
                "192.168.178.42"
                """;

            // Act
            var result = () =>
            {
                var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
                reader.Read();

                return _sut.Read(ref reader, typeof(List<IPAddress>), _options);
            };

            // Assert
            result.Should().Throw<JsonException>().WithMessage(expectedMessage);
        }

        [Fact]
        public void ShouldThrow_WhenArrayItemIsNotString()
        {
            // Arrange
            var expectedMessage = "Expected string as the JSON token type.";
            var json =
                """
                ["192.168.178.42", 123]
                """;

            // Act
            var result = () =>
            {
                var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
                reader.Read();

                return _sut.Read(ref reader, typeof(List<IPAddress>), _options);
            };

            // Assert
            result.Should().Throw<JsonException>().WithMessage(expectedMessage);
        }

        [Fact]
        public void ShouldThrow_WhenArrayItemIsNull()
        {
            // Arrange
            var expectedMessage = "Expected string as the JSON token type.";
            var json =
                """
                ["192.168.178.42", null]
                """;

            // Act
            var result = () =>
            {
                var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
                reader.Read();

                return _sut.Read(ref reader, typeof(List<IPAddress>), _options);
            };

            // Assert
            result.Should().Throw<JsonException>().WithMessage(expectedMessage);
        }

        [Fact]
        public void ShouldThrow_WhenStringIsEmpty()
        {
            // Arrange
            var jsonTokenString = string.Empty;
            var expectedMessage = $"Invalid IP address format: {jsonTokenString}";
            var json =
                """
                [""]
                """;

            // Act
            var result = () =>
            {
                var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
                reader.Read();

                return _sut.Read(ref reader, typeof(List<IPAddress>), _options);
            };

            // Assert
            result.Should().Throw<JsonException>().WithMessage(expectedMessage);
        }

        [Fact]
        public void ShouldThrow_WhenStringIsInvalidIPAddressFormat()
        {
            // Arrange
            var jsonTokenString = "x192.168.178.42";
            var expectedMessage = $"Invalid IP address format: {jsonTokenString}";
            var json =
                $$"""
                ["{{jsonTokenString}}"]
                """;

            // Act
            var result = () =>
            {
                var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
                reader.Read();

                return _sut.Read(ref reader, typeof(List<IPAddress>), _options);
            };

            // Assert
            result.Should().Throw<JsonException>().WithMessage(expectedMessage);
        }

        [Fact]
        public void ShouldReturnEmptyList_WhenArrayIsEmpty()
        {
            // Arrange
            var json = "[]";
            var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));

            reader.Read();

            // Act
            var result = _sut.Read(ref reader, typeof(List<IPAddress>), _options);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void ShouldReturnIPAddressList_WhenValidIPv4AddressesProvided()
        {
            // Arrange
            var expected = new List<IPAddress>
            {
                IPAddress.Parse("192.168.178.42"),
                IPAddress.Parse("10.0.0.1")
            };

            var json =
                """
                ["192.168.178.42", "10.0.0.1"]
                """;

            var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));

            reader.Read();

            // Act
            var result = _sut.Read(ref reader, typeof(List<IPAddress>), _options);

            // Assert
            result.Should().Equal(expected);
        }

        [Fact]
        public void ShouldReturnIPAddressList_WhenValidIPv6AddressesProvided()
        {
            // Arrange
            var expected = new List<IPAddress>
            {
                IPAddress.Parse("2001:db8::1"),
                IPAddress.Parse("fe80::1")
            };

            var json =
                """
                ["2001:db8::1", "fe80::1"]
                """;

            var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));

            reader.Read();

            // Act
            var result = _sut.Read(ref reader, typeof(List<IPAddress>), _options);

            // Assert
            result.Should().Equal(expected);
        }

        [Fact]
        public void ShouldReturnIPAddressListInOriginalOrder_WhenMultipleAddressesProvided()
        {
            // Arrange
            var expected = new List<IPAddress>
            {
                IPAddress.Parse("192.168.178.42"),
                IPAddress.Parse("2001:db8::1"),
                IPAddress.Parse("10.0.0.1")
            };

            var json =
                """
                ["192.168.178.42", "2001:db8::1", "10.0.0.1"]
                """;

            var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));

            reader.Read();

            // Act
            var result = _sut.Read(ref reader, typeof(List<IPAddress>), _options);

            // Assert
            result.Should().Equal(expected);
        }
    }

    public class Write : IPAddressListConverterTests
    {
        [Fact]
        public void ShouldWriteEmptyArray_WhenListIsEmpty()
        {
            // Arrange
            var value = new List<IPAddress>();
            var expectedJson = "[]";

            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream);

            // Act
            _sut.Write(writer, value, _options);
            writer.Flush();

            // Assert
            var result = Encoding.UTF8.GetString(stream.ToArray());
            result.Should().Be(expectedJson);
        }

        [Fact]
        public void ShouldWriteIPAddressArray_WhenIPv4AddressesProvided()
        {
            // Arrange
            var value = new List<IPAddress>
            {
                IPAddress.Parse("192.168.178.42"),
                IPAddress.Parse("10.0.0.1")
            };

            var expectedJson =
                """
                ["192.168.178.42","10.0.0.1"]
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

        [Fact]
        public void ShouldWriteIPAddressArray_WhenIPv6AddressesProvided()
        {
            // Arrange
            var value = new List<IPAddress>
            {
                IPAddress.Parse("2001:db8::1"),
                IPAddress.Parse("fe80::1")
            };

            var expectedJson =
                """
                ["2001:db8::1","fe80::1"]
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

        [Fact]
        public void ShouldWriteIPAddressArrayInOriginalOrder_WhenMultipleAddressesProvided()
        {
            // Arrange
            var value = new List<IPAddress>
            {
                IPAddress.Parse("192.168.178.42"),
                IPAddress.Parse("2001:db8::1"),
                IPAddress.Parse("10.0.0.1")
            };

            var expectedJson =
                """
                ["192.168.178.42","2001:db8::1","10.0.0.1"]
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
