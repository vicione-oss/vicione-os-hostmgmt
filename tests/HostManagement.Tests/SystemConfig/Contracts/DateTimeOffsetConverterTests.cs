using System.Text.Json;
using AwesomeAssertions;
using HostManagement.SystemConfig.Contracts;
using Xunit;

namespace HostManagement.Tests.SystemConfig.Contracts;

public class DateTimeOffsetConverterTests
{
    private readonly DateTimeOffsetConverter _sut = new();
    private readonly JsonSerializerOptions _options;

    public DateTimeOffsetConverterTests()
    {
        _options = new JsonSerializerOptions
        {
            Converters = { _sut }
        };
    }

    public class Read : DateTimeOffsetConverterTests
    {
        [Fact]
        public void ShouldThrow_WhenTokenIsNotStringOrNull()
        {
            // Arrange
            var expectedMessage = "Expected string or null as the JSON token type.";
            var json = "123";

            // Act
            var result = () => JsonSerializer.Deserialize<DateTimeOffset?>(json, _options);

            // Assert
            result.Should().Throw<JsonException>().WithMessage(expectedMessage);
        }

        [Theory]
        [InlineData("2024-10-30T01:23:45 +0190")]
        [InlineData("2024-10-30T01:23:45 ++0200")]
        [InlineData("2024-10-30T01:23:45 -+0300")]
        [InlineData("2024-10-30T01.23:45 +0500")]
        public void ShouldThrow_WhenStringIsNotUtcDateTimeOffsetFormat(string jsonTokenString)
        {
            // Arrange
            var json =
                $"""
                "{jsonTokenString}"
                """;

            var expectedMessage = $"String '{jsonTokenString}' was not recognized as a valid DateTime.";

            // Act
            var result = () => JsonSerializer.Deserialize<DateTimeOffset?>(json, _options);

            // Assert
            result.Should().Throw<FormatException>().WithMessage(expectedMessage);
        }

        [Theory]
        [InlineData("2024-10-30T01:23:45Z")]
        [InlineData("2024-10-30T01:23:45+0200")]
        [InlineData("2024-10-30T01:23:45 +0300")]
        [InlineData("2024-10-30T01:23:45 -04:00")]
        [InlineData("2024-10-30T01:23:45-14:00")]
        [InlineData("2024-10-30T01:23:45+14:00")]
        public void ShouldReturnDateTimeOffset_WhenValidUtcDateTimeOffsetStringProvided(string expectedDateTimeString)
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
        public void ShouldWriteDateTimeUtcString_WhenValueIsNotNull()
        {
            // Arrange
            var dateTimeOffset = new DateTimeOffset(2024, 10, 30, 1, 23, 45, TimeSpan.FromHours(2));

            // All non-ASCII characters will get escaped by the JSON serializer by default,
            // so to test we need to replace "+" with "\\u002B" too.
            var expectedJson =
                $"""
                "{dateTimeOffset.ToString("yyyy-MM-ddTHH:mm:ss.fffffffK").Replace("+", "\\u002B")}"
                """;

            // Act
            var result = JsonSerializer.Serialize<DateTimeOffset?>(dateTimeOffset, _options);

            // Assert
            result.Should().Be(expectedJson);
        }
    }
}
