using System.Text.Json;
using FluentAssertions;
using Xunit;
using HostManagement.Shared.Contracts.Network;

namespace HostManagement.Shared.Tests.Converters;

public class DateTimeOffsetConverterTests
{
    private readonly DateTimeOffsetConverter _sut = new();
    private readonly JsonSerializerOptions _options;

    public DateTimeOffsetConverterTests()
    {
        _options = new JsonSerializerOptions
        {
            RespectNullableAnnotations = true,
            Converters = { _sut }
        };
    }

    public class CanConvert : DateTimeOffsetConverterTests
    {
        [Fact]
        public void ShouldReturnFalse_WhenTypeIsNotDateTimeOffset()
        {
            // Act
            var result = _sut.CanConvert(typeof(string));

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnTrue_WhenTypeIsDateTimeOffsetNullable()
        {
            // Act
            var result = _sut.CanConvert(typeof(DateTimeOffset?));

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldReturnTrue_WhenTypeIsDateTimeOffset()
        {
            // Act
            var result = _sut.CanConvert(typeof(DateTimeOffset));

            // Assert
            result.Should().BeTrue();
        }
    }

    public class Read : DateTimeOffsetConverterTests
    {
        [Fact]
        public void ShouldReturnNull_WhenTokenIsNull()
        {
            // This is default behavior of the JsonSerializer and therefor needs no implementation
            // in the custom converter.

            // Act
            var expectedDateTimeOffset =
                JsonSerializer.Deserialize<DateTimeOffset?>("null", _options);

            // Assert
            expectedDateTimeOffset.Should().BeNull();
        }

        [Fact]
        public void ShouldThrow_WhenTokenIsNotString()
        {
            // Arrange
            var expectedMessage = "Expected string as the JSON token type.";
            var json = "123";

            // Act
            var result = () => JsonSerializer.Deserialize<DateTimeOffset?>(json, _options);

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
            var expectedMessage = $"DateTimeOffset '{jsonTokenString}' can not be empty.";

            // Act
            var result = () => JsonSerializer.Deserialize<DateTimeOffset?>(json, _options);

            // Assert
            result.Should()
                .Throw<JsonException>()
                .WithMessage(expectedMessage);
        }

        [Theory]
        [InlineData("2024-10-30 01:23:45")]
        [InlineData("2024-10-30 01:23:45Z")]
        [InlineData("2024-10-30 01:23:45+0200")]
        [InlineData("2024-10-30 01:23:45 +0300")]
        public void ShouldThrow_WhenStringIsMissingTSeparator(string jsonTokenString)
        {
            // Arrange
            var json =
                $"""
                "{jsonTokenString}"
                """;

            var expectedMessage =
                $"DateTimeOffset '{jsonTokenString}' is missing 'T' separator " +
                $"(e.g. '2024-10-30T01:23:45+0200').";

            // Act
            var result = () => JsonSerializer.Deserialize<DateTimeOffset?>(json, _options);

            // Assert
            result.Should()
                .Throw<JsonException>()
                .WithMessage(expectedMessage);
        }

        [Theory]
        [InlineData("2024-10-40T01:23:45 +0100")]
        [InlineData("2024-10-30T01:23:45 +0190")]
        [InlineData("2024-10-30T01:23:45 ++0200")]
        [InlineData("2024-10-30T01:23:45 -+0300")]
        [InlineData("x2024-10-30T01:23:45 -0400")]
        [InlineData("2024-10-30T01.23:45 +0500")]
        [InlineData("2024-10-30T01:23:45 +1401")]
        [InlineData("DateTimeOffset")]
        public void ShouldThrow_WhenStringIsNotUtcDateTimeOffsetFormat(
            string jsonTokenString)
        {
            // Arrange
            var json =
                $"""
                "{jsonTokenString}"
                """;

            var expectedMessage =
                $"DateTimeOffset '{jsonTokenString}' has to be in DateTimeOffset format " +
                $"(e.g. '2024-10-30T01:23:45+0200').";

            // Act
            var result = () => JsonSerializer.Deserialize<DateTimeOffset?>(json, _options);

            // Assert
            result.Should()
                .Throw<JsonException>()
                .WithMessage(expectedMessage);
        }

        [Theory]
        [InlineData("2024-10-30T01:23:45Z")]
        [InlineData("2024-10-30T01:23:45+0200")]
        [InlineData("2024-10-30T01:23:45 +0300")]
        [InlineData("2024-10-30T01:23:45 -04:00")]
        [InlineData("2024-10-30T01:23:45-14:00")]
        [InlineData("2024-10-30T01:23:45+14:00")]
        public void ShouldReturnDateTimeOffset_WhenValidUtcDateTimeOffsetStringProvided(
            string expectedDateTimeString)
        {
            // Arrange
            var expectedDateTime = DateTimeOffset.Parse(expectedDateTimeString);
            var json =
                $"""
                "{expectedDateTimeString}"
                """;

            // Act
            var result = JsonSerializer.Deserialize<DateTimeOffset?>(json, _options);

            // Assert
            result.Should().Be(expectedDateTime);
        }
    }

    public class Write : DateTimeOffsetConverterTests
    {
        [Fact]
        public void ShouldThrow_WhenValueIsNotDateTimeOffset()
        {
            // Arrange
            object? invalidType = null;

            var expectedMessage =
                $"Invalid DateTimeOffset value: '{invalidType}'.";

            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream);

            // Act
            var result = () => _sut.Write(writer, (DateTimeOffset?)invalidType, _options);

            // Assert
            result.Should()
                .Throw<JsonException>()
                .WithMessage(expectedMessage);
        }

        [Fact]
        public void ShouldWriteNull_WhenValueIsNull()
        {
            // This is default behavior of the JsonSerializer and therefor needs no implementation
            // in the custom converter.

            // Arrange
            var expectedJson =
                """
                null
                """;

            // Act
            var result = JsonSerializer.Serialize<DateTimeOffset?>(null, _options);

            // Assert
            result.Should().Be(expectedJson);
        }

        [Fact]
        public void ShouldWriteDateTimeUtcString_WhenValueIsNotNull()
        {
            // Arrange
            var dateTimeOffset = new DateTimeOffset(2024, 10, 30, 1, 23, 45, TimeSpan.FromHours(2));

            // All non-ASCII characters will get escaped by the JSON serializer by default,
            // so to test we need to replace "+" with "\\u002B" too.
            var expectedJson =
                $"""
                "{dateTimeOffset.ToString("yyyy-MM-ddTHH:mm:ssK").Replace("+", "\\u002B")}"
                """;

            // Act
            var result = JsonSerializer.Serialize<DateTimeOffset?>(dateTimeOffset, _options);

            // Assert
            result.Should().Be(expectedJson);
        }
    }
}

