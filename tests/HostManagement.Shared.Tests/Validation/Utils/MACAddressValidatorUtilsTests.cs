using FluentAssertions;
using Xunit;
using HostManagement.Shared.Validation.Utils;

namespace HostManagement.Shared.Tests.Validation.Utils;

public class MACAddressValidatorUtilsTests
{
    public class HasNoSeparators
    {
        [Theory]
        [InlineData("aabbccddeeff")]
        [InlineData("A1B2C3D4E5F6")]
        [InlineData("918273645546")]
        public void ShouldReturnTrue_WhenAddressHasNoSeparators(string address)
        {
            // Act
            var result = MACAddressValidatorUtils.HasNoSeparators(address);

            // Assert
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData("aa:bb:cc:dd:ee:ff")]
        [InlineData("A1-B2-C3-D4-E5-F6")]
        [InlineData("91:82:73:64:55:46")]
        public void ShouldReturnFalse_WhenAddressHasSeparators(string address)
        {
            // Act
            var result = MACAddressValidatorUtils.HasNoSeparators(address);

            // Assert
            result.Should().BeFalse();
        }
    }

    public class HasInvalidOctetCount
    {
        [Theory]
        [InlineData("aa:bb:cc:dd:ee")]
        [InlineData("A1-B2-C3-D4")]
        [InlineData("91:82:73:64:55:46:37")]
        public void ShouldReturnTrue_WhenAddressHasInvalidOctetCount(string address)
        {
            // Act
            var result = MACAddressValidatorUtils.HasInvalidOctetCount(address);

            // Assert
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData("aa:bb:cc:dd:ee:ff")]
        [InlineData("A1-B2-C3-D4-E5-F6")]
        [InlineData("91:82:73:64:55:46")]
        public void ShouldReturnFalse_WhenAddressHasValidOctetCount(string address)
        {
            // Act
            var result = MACAddressValidatorUtils.HasInvalidOctetCount(address);

            // Assert
            result.Should().BeFalse();
        }
    }

    public class HasOctetInvalidLength
    {
        [Theory]
        [InlineData("aaa")]
        [InlineData("A11")]
        [InlineData("9")]
        public void ShouldReturnTrue_WhenOctetHasInvalidLength(string octet)
        {
            // Act
            var result = MACAddressValidatorUtils.HasOctetInvalidLength(octet);

            // Assert
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData("aa")]
        [InlineData("A1")]
        [InlineData("91")]
        public void ShouldReturnFalse_WhenOctetHasValidLength(string octet)
        {
            // Act
            var result = MACAddressValidatorUtils.HasOctetInvalidLength(octet);

            // Assert
            result.Should().BeFalse();
        }
    }

    public class HasOctetNonAsciiHexDigit
    {
        [Theory]
        [InlineData("aä")]
        [InlineData("Aø")]
        [InlineData("٩1")]
        public void ShouldReturnTrue_WhenOctetHasNonAsciiHexDigit(string octet)
        {
            // Act
            var result = MACAddressValidatorUtils.HasOctetNonAsciiHexDigit(octet);

            // Assert
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData("aa")]
        [InlineData("A1")]
        [InlineData("91")]
        public void ShouldReturnFalse_WhenOctetHasValidAsciiHexDigits(string octet)
        {
            // Act
            var result = MACAddressValidatorUtils.HasOctetNonAsciiHexDigit(octet);

            // Assert
            result.Should().BeFalse();
        }
    }
}
