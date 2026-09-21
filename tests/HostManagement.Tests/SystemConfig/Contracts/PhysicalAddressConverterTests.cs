using System.Net.NetworkInformation;
using System.Text;
using System.Text.Json;
using AwesomeAssertions;
using HostManagement.SystemConfig.Contracts;
using Xunit;


namespace HostManagement.Tests.SystemConfig.Contracts;

public class PhysicalAddressConverterTests
{
    private readonly JsonSerializerOptions _options;
    private readonly PhysicalAddressConverter _sut = new();

    public PhysicalAddressConverterTests()
    {
        _options = new JsonSerializerOptions
        {
            Converters = { _sut }
        };
    }

    public sealed class Read : PhysicalAddressConverterTests
    {
        [Fact]
        public void ShouldThrow_WhenTokenIsNotString()
        {
            // Arrange
            var expectedMessage = "Expected string as the JSON token type.";

            // Act
            var action = () =>
            {
                var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes("123"));
                reader.Read();

                return _sut.Read(ref reader, typeof(PhysicalAddress), _options);
            };

            // Assert
            action.Should().Throw<JsonException>().WithMessage(expectedMessage);
        }

        [Theory]
        [InlineData("0011.2233.4455")]
        [InlineData("invalid")]
        public void ShouldThrow_WhenStringFormatUnsupported(string value)
        {
            // Arrange
            var expectedMessage = $"Invalid physical address format: '{value}'";

            // Act
            var action = () =>
            {
                var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes($"\"{value}\""));
                reader.Read();

                return _sut.Read(ref reader, typeof(PhysicalAddress), _options);
            };

            // Assert
            action.Should().Throw<JsonException>().WithMessage(expectedMessage);
        }

        [Theory]
        [InlineData("001122334455")]
        [InlineData("00:11:22:33:44:55")]
        [InlineData("00-11-22-33-44-55")]
        [InlineData("00:11-22:33-44:55")]
        public void ShouldReturnPhysicalAddress_WhenSupportedStringProvided(string value)
        {
            // Arrange
            var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes($"\"{value}\""));
            reader.Read();

            // Act
            var result = _sut.Read(ref reader, typeof(PhysicalAddress), _options);

            // Assert
            result.Should().Be(PhysicalAddress.Parse("00:11:22:33:44:55"));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void ShouldReturnNone_WhenStringEmptyOrWhitespace(string value)
        {
            // Arrange
            var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes($"\"{value}\""));
            reader.Read();

            // Act
            var result = _sut.Read(ref reader, typeof(PhysicalAddress), _options);

            // Assert
            result.Should().Be(PhysicalAddress.None);
        }
    }

    public sealed class Write : PhysicalAddressConverterTests
    {
        [Fact]
        public void ShouldWriteEmptyString_WhenValueNone()
        {
            // Arrange
            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream);

            // Act
            _sut.Write(writer, PhysicalAddress.None, _options);
            writer.Flush();

            // Assert
            Encoding.UTF8.GetString(stream.ToArray()).Should().Be("\"\"");
        }

        [Fact]
        public void ShouldWriteColonSeparatedPhysicalAddressString_WhenValueProvided()
        {
            // Arrange
            var value = PhysicalAddress.Parse("00-11-22-33-44-55");
            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream);

            // Act
            _sut.Write(writer, value, _options);
            writer.Flush();

            // Assert
            Encoding.UTF8.GetString(stream.ToArray()).Should().Be("\"00:11:22:33:44:55\"");
        }
    }
}
