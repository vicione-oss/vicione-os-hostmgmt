using System.Net.NetworkInformation;
using AwesomeAssertions;
using HostManagement.Shared.Extensions;
using Xunit;

namespace HostManagement.Shared.Tests.Extensions;

public sealed class PhysicalAddressExtensionsTests
{
    public sealed class ToColonNotation
    {
        [Theory]
        [InlineData("112233445566", "11:22:33:44:55:66")]
        [InlineData("a1b2c3d4e5f6", "A1:B2:C3:D4:E5:F6")]
        public void ShouldReturnColonSeparatedMacAddress_WhenMacAddressSpecified(
            string macAddress,
            string expectedMacAddress)
        {
            // Arrange
            var physicalAddress = PhysicalAddress.Parse(macAddress);

            // Act
            var result = physicalAddress.ToColonNotation();

            // Assert
            result.Should().Be(expectedMacAddress);
        }
    }
}
