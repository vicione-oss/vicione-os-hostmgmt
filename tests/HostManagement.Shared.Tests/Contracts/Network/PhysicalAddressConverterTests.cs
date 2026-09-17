using System.Net.NetworkInformation;
using System.Text.Json;
using AwesomeAssertions;
using HostManagement.Shared.Contracts.Network;
using Xunit;

namespace HostManagement.Shared.Tests.Contracts.Network;

public sealed class PhysicalAddressConverterTests
{
    private static readonly JsonSerializerOptions s_options = new()
    {
        Converters = { new PhysicalAddressConverter() }
    };

    public sealed class Read
    {
        [Theory]
        [InlineData("0223456789AB")]
        [InlineData("02:23:45:67:89:AB")]
        [InlineData("02-23-45-67-89-ab")]
        [InlineData("02:23-45:67-89:ab")]
        public void ShouldReturnPhysicalAddress_WhenSupportedStringFormatProvided(string value)
        {
            // Act
            var result = JsonSerializer.Deserialize<PhysicalAddress>($"\"{value}\"", s_options);

            // Assert
            result.Should().Be(PhysicalAddress.Parse("02:23:45:67:89:AB"));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void ShouldReturnNone_WhenStringEmptyOrWhitespace(string value)
        {
            var result = JsonSerializer.Deserialize<PhysicalAddress>($"\"{value}\"", s_options);

            result.Should().Be(PhysicalAddress.None);
        }

        [Theory]
        [InlineData("0011.2233.4455")]
        [InlineData("02:23:45:67:89")]
        [InlineData("invalid")]
        public void ShouldThrow_WhenUnsupportedStringFormatProvided(string value)
        {
            var action = () => JsonSerializer.Deserialize<PhysicalAddress>($"\"{value}\"", s_options);

            action.Should().Throw<JsonException>();
        }

        [Fact]
        public void ShouldThrow_WhenTokenNotString()
        {
            var action = () => JsonSerializer.Deserialize<PhysicalAddress>("42", s_options);

            action.Should().Throw<JsonException>();
        }
    }

    public sealed class Write
    {
        [Fact]
        public void ShouldWriteColonSeparatedUppercaseString_WhenPhysicalAddressProvided()
        {
            // Arrange
            var physicalAddress = PhysicalAddress.Parse("02-23-45-67-89-ab");

            // Act
            var result = JsonSerializer.Serialize(physicalAddress, s_options);

            // Assert
            result.Should().Be("\"02:23:45:67:89:AB\"");
        }

        [Fact]
        public void ShouldWriteEmptyString_WhenPhysicalAddressNoneProvided()
        {
            var result = JsonSerializer.Serialize(PhysicalAddress.None, s_options);

            result.Should().Be("\"\"");
        }
    }
}
