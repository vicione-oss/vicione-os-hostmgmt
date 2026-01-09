using AwesomeAssertions;
using Xunit;

namespace HostManagement.Shared.Communication.Tests;

public class UtilitiesTests
{
    public class BytesToHexString
    {
        [Fact]
        public void ShouldThrow_WhenBytesIsNull()
        {
            // Act
            var result = () => Utilities.BytesToHexString(null!);

            // Assert
            result.Should()
                .Throw<ArgumentNullException>()
                .WithParameterName("bytes");
        }

        [Fact]
        public void ShouldReturnEmptyString_WhenBytesIsEmpty()
        {
            // Act
            var result = Utilities.BytesToHexString([]);

            // Assert
            result.Should().BeEmpty();
        }

        [Theory]
        [InlineData(new byte[] { 0x00 }, "00")]
        [InlineData(new byte[] { 0x0F }, "0F")]
        [InlineData(new byte[] { 0xAB }, "AB")]
        [InlineData(new byte[] { 0x12, 0x34 }, "1234")]
        [InlineData(new byte[] { 0xDE, 0xAD, 0xBE, 0xEF }, "DEADBEEF")]
        [InlineData(new byte[] { 0x01, 0x02, 0x03 }, "010203")]
        public void ShouldReturnHexString_WhenBytesAreGiven(byte[] bytes, string expected)
        {
            // Act
            var result = Utilities.BytesToHexString(bytes);

            // Assert
            result.Should().Be(expected);
        }
    }

    public class GetVarInt
    {
        [Fact]
        public void ShouldThrow_WhenNumberNegative()
        {
            // Act
            var result = () => Utilities.GetVarInt(-1);

            // Assert
            result.Should()
                .Throw<ArgumentOutOfRangeException>()
                .WithMessage("*Cannot write negative value as varint.*")
                .WithParameterName("number");
        }

        [Theory]
        [InlineData(0L, new byte[] { 0x00 })]
        [InlineData(1L, new byte[] { 0x01 })]
        [InlineData(127L, new byte[] { 0x7F })]
        [InlineData(128L, new byte[] { 0x80, 0x01 })]
        [InlineData(255L, new byte[] { 0xFF, 0x01 })]
        [InlineData(300L, new byte[] { 0xAC, 0x02 })]
        [InlineData(16384L, new byte[] { 0x80, 0x80, 0x01 })]
        [InlineData(ulong.MaxValue >> 1, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x7F })]
        public void ShouldReturnExpectedBytes_WhenNumberValid(long value, byte[] expected)
        {
            // Act
            var result = Utilities.GetVarInt(value);

            // Assert
            result.Should().BeEquivalentTo(expected, options => options.WithStrictOrdering());
        }
    }
}
