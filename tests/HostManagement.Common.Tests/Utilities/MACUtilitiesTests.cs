using FluentAssertions;
using Xunit;
using HostManagement.Common.Utilities;

namespace HostManagement.Common.Tests.Utilities;

public class MACUtilitiesTests
{
    public class ConvertToColonNotation
    {
        [Theory]
        [InlineData("112233445566", "11:22:33:44:55:66")]
        [InlineData("a1:b2:c3:d4:e5:f6", "a1:b2:c3:d4:e5:f6")]
        [InlineData("a1:bb-c3:dd-e5:ff", "a1:bb:c3:dd:e5:ff")]
        [InlineData("aa-bb-cc-dd-ee-ff", "aa:bb:cc:dd:ee:ff")]
        public void ShouldReturnColonSeparatedMacAddress_WhenMacAddressSpecified(
            string macAddress,
            string expectedMacAddress)
        {
            // Act
            var result = MACUtilities.ConvertToColonNotation(macAddress);

            // Assert
            result.Should().Be(expectedMacAddress);
        }
    }

    public class ConvertToHyphenNotation
    {
        [Theory]
        [InlineData("112233445566", "11-22-33-44-55-66")]
        [InlineData("a1-b2-c3-d4-e5-f6", "a1-b2-c3-d4-e5-f6")]
        [InlineData("a1:bb-c3:dd-e5:ff", "a1-bb-c3-dd-e5-ff")]
        [InlineData("aa:bb:cc:dd:ee:ff", "aa-bb-cc-dd-ee-ff")]
        public void ShouldReturnHyphenSeparatedMacAddress_WhenMacAddressSpecified(
            string macAddress,
            string expectedMacAddress)
        {
            // Act
            var result = MACUtilities.ConvertToHyphenNotation(macAddress);

            // Assert
            result.Should().Be(expectedMacAddress);
        }
    }
}
