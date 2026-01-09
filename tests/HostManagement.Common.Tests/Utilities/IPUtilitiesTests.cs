using System.Net;
using System.Net.Sockets;
using AwesomeAssertions;
using Xunit;
using HostManagement.Common.Utilities;

namespace HostManagement.Common.Tests.Utilities;

public class IPUtilitiesTests
{
    public static TheoryData<int, IPAddress> IPv4CIDRData => new()
    {
        { 0, IPAddress.Parse("0.0.0.0") },
        { 1, IPAddress.Parse("128.0.0.0") },
        { 2, IPAddress.Parse("192.0.0.0") },
        { 3, IPAddress.Parse("224.0.0.0") },
        { 4, IPAddress.Parse("240.0.0.0") },
        { 5, IPAddress.Parse("248.0.0.0") },
        { 6, IPAddress.Parse("252.0.0.0") },
        { 7, IPAddress.Parse("254.0.0.0") },
        { 8, IPAddress.Parse("255.0.0.0") },
        { 9, IPAddress.Parse("255.128.0.0") },
        { 10, IPAddress.Parse("255.192.0.0") },
        { 11, IPAddress.Parse("255.224.0.0") },
        { 12, IPAddress.Parse("255.240.0.0") },
        { 13, IPAddress.Parse("255.248.0.0") },
        { 14, IPAddress.Parse("255.252.0.0") },
        { 15, IPAddress.Parse("255.254.0.0") },
        { 16, IPAddress.Parse("255.255.0.0") },
        { 17, IPAddress.Parse("255.255.128.0") },
        { 18, IPAddress.Parse("255.255.192.0") },
        { 19, IPAddress.Parse("255.255.224.0") },
        { 20, IPAddress.Parse("255.255.240.0") },
        { 21, IPAddress.Parse("255.255.248.0") },
        { 22, IPAddress.Parse("255.255.252.0") },
        { 23, IPAddress.Parse("255.255.254.0") },
        { 24, IPAddress.Parse("255.255.255.0") },
        { 25, IPAddress.Parse("255.255.255.128") },
        { 26, IPAddress.Parse("255.255.255.192") },
        { 27, IPAddress.Parse("255.255.255.224") },
        { 28, IPAddress.Parse("255.255.255.240") },
        { 29, IPAddress.Parse("255.255.255.248") },
        { 30, IPAddress.Parse("255.255.255.252") },
        { 31, IPAddress.Parse("255.255.255.254") },
        { 32, IPAddress.Parse("255.255.255.255") }
    };

    public static TheoryData<int, IPAddress> IPv6CIDRData => new()
    {
        { 0, IPAddress.Parse("::") },
        { 1, IPAddress.Parse("8000::") },
        { 2, IPAddress.Parse("c000::") },
        { 3, IPAddress.Parse("e000::") },
        { 4, IPAddress.Parse("f000::") },
        { 5, IPAddress.Parse("f800::") },
        { 6, IPAddress.Parse("fc00::") },
        { 7, IPAddress.Parse("fe00::") },
        { 8, IPAddress.Parse("ff00::") },
        { 9, IPAddress.Parse("ff80::") },
        { 10, IPAddress.Parse("ffc0::") },
        { 11, IPAddress.Parse("ffe0::") },
        { 12, IPAddress.Parse("fff0::") },
        { 13, IPAddress.Parse("fff8::") },
        { 14, IPAddress.Parse("fffc::") },
        { 15, IPAddress.Parse("fffe::") },
        { 16, IPAddress.Parse("ffff::") },
        { 17, IPAddress.Parse("ffff:8000::") },
        { 18, IPAddress.Parse("ffff:c000::") },
        { 19, IPAddress.Parse("ffff:e000::") },
        { 20, IPAddress.Parse("ffff:f000::") },
        { 21, IPAddress.Parse("ffff:f800::") },
        { 22, IPAddress.Parse("ffff:fc00::") },
        { 23, IPAddress.Parse("ffff:fe00::") },
        { 24, IPAddress.Parse("ffff:ff00::") },
        { 25, IPAddress.Parse("ffff:ff80::") },
        { 26, IPAddress.Parse("ffff:ffc0::") },
        { 27, IPAddress.Parse("ffff:ffe0::") },
        { 28, IPAddress.Parse("ffff:fff0::") },
        { 29, IPAddress.Parse("ffff:fff8::") },
        { 30, IPAddress.Parse("ffff:fffc::") },
        { 31, IPAddress.Parse("ffff:fffe::") },
        { 32, IPAddress.Parse("ffff:ffff::") },
        { 33, IPAddress.Parse("ffff:ffff:8000::") },
        { 34, IPAddress.Parse("ffff:ffff:c000::") },
        { 35, IPAddress.Parse("ffff:ffff:e000::") },
        { 36, IPAddress.Parse("ffff:ffff:f000::") },
        { 37, IPAddress.Parse("ffff:ffff:f800::") },
        { 38, IPAddress.Parse("ffff:ffff:fc00::") },
        { 39, IPAddress.Parse("ffff:ffff:fe00::") },
        { 40, IPAddress.Parse("ffff:ffff:ff00::") },
        { 41, IPAddress.Parse("ffff:ffff:ff80::") },
        { 42, IPAddress.Parse("ffff:ffff:ffc0::") },
        { 43, IPAddress.Parse("ffff:ffff:ffe0::") },
        { 44, IPAddress.Parse("ffff:ffff:fff0::") },
        { 45, IPAddress.Parse("ffff:ffff:fff8::") },
        { 46, IPAddress.Parse("ffff:ffff:fffc::") },
        { 47, IPAddress.Parse("ffff:ffff:fffe::") },
        { 48, IPAddress.Parse("ffff:ffff:ffff::") },
        { 49, IPAddress.Parse("ffff:ffff:ffff:8000::") },
        { 50, IPAddress.Parse("ffff:ffff:ffff:c000::") },
        { 51, IPAddress.Parse("ffff:ffff:ffff:e000::") },
        { 52, IPAddress.Parse("ffff:ffff:ffff:f000::") },
        { 53, IPAddress.Parse("ffff:ffff:ffff:f800::") },
        { 54, IPAddress.Parse("ffff:ffff:ffff:fc00::") },
        { 55, IPAddress.Parse("ffff:ffff:ffff:fe00::") },
        { 56, IPAddress.Parse("ffff:ffff:ffff:ff00::") },
        { 57, IPAddress.Parse("ffff:ffff:ffff:ff80::") },
        { 58, IPAddress.Parse("ffff:ffff:ffff:ffc0::") },
        { 59, IPAddress.Parse("ffff:ffff:ffff:ffe0::") },
        { 60, IPAddress.Parse("ffff:ffff:ffff:fff0::") },
        { 61, IPAddress.Parse("ffff:ffff:ffff:fff8::") },
        { 62, IPAddress.Parse("ffff:ffff:ffff:fffc::") },
        { 63, IPAddress.Parse("ffff:ffff:ffff:fffe::") },
        { 64, IPAddress.Parse("ffff:ffff:ffff:ffff::") },
        { 65, IPAddress.Parse("ffff:ffff:ffff:ffff:8000::") },
        { 66, IPAddress.Parse("ffff:ffff:ffff:ffff:c000::") },
        { 67, IPAddress.Parse("ffff:ffff:ffff:ffff:e000::") },
        { 68, IPAddress.Parse("ffff:ffff:ffff:ffff:f000::") },
        { 69, IPAddress.Parse("ffff:ffff:ffff:ffff:f800::") },
        { 70, IPAddress.Parse("ffff:ffff:ffff:ffff:fc00::") },
        { 71, IPAddress.Parse("ffff:ffff:ffff:ffff:fe00::") },
        { 72, IPAddress.Parse("ffff:ffff:ffff:ffff:ff00::") },
        { 73, IPAddress.Parse("ffff:ffff:ffff:ffff:ff80::") },
        { 74, IPAddress.Parse("ffff:ffff:ffff:ffff:ffc0::") },
        { 75, IPAddress.Parse("ffff:ffff:ffff:ffff:ffe0::") },
        { 76, IPAddress.Parse("ffff:ffff:ffff:ffff:fff0::") },
        { 77, IPAddress.Parse("ffff:ffff:ffff:ffff:fff8::") },
        { 78, IPAddress.Parse("ffff:ffff:ffff:ffff:fffc::") },
        { 79, IPAddress.Parse("ffff:ffff:ffff:ffff:fffe::") },
        { 80, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff::") },
        { 81, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:8000::") },
        { 82, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:c000::") },
        { 83, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:e000::") },
        { 84, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:f000::") },
        { 85, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:f800::") },
        { 86, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:fc00::") },
        { 87, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:fe00::") },
        { 88, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ff00::") },
        { 89, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ff80::") },
        { 90, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffc0::") },
        { 91, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffe0::") },
        { 92, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:fff0::") },
        { 93, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:fff8::") },
        { 94, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:fffc::") },
        { 95, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:fffe::") },
        { 96, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff::") },
        { 97, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:8000::") },
        { 98, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:c000::") },
        { 99, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:e000::") },
        { 100, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:f000::") },
        { 101, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:f800::") },
        { 102, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:fc00::") },
        { 103, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:fe00::") },
        { 104, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ff00::") },
        { 105, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ff80::") },
        { 106, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffc0::") },
        { 107, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffe0::") },
        { 108, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:fff0::") },
        { 109, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:fff8::") },
        { 110, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:fffc::") },
        { 111, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:fffe::") },
        { 112, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffff::") },
        { 113, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffff:8000") },
        { 114, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffff:c000") },
        { 115, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffff:e000") },
        { 116, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffff:f000") },
        { 117, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffff:f800") },
        { 118, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffff:fc00") },
        { 119, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffff:fe00") },
        { 120, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ff00") },
        { 121, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ff80") },
        { 122, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffc0") },
        { 123, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffe0") },
        { 124, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffff:fff0") },
        { 125, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffff:fff8") },
        { 126, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffff:fffc") },
        { 127, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffff:fffe") },
        { 128, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff") }
    };

    public class ConvertToCIDR
    {
        [Theory]
        [InlineData("10.0.0.1", "255.0.0.0", "10.0.0.1/8")]
        [InlineData("172.16.0.1", "255.255.0.0", "172.16.0.1/16")]
        [InlineData("192.168.42.21", "255.255.255.0", "192.168.42.21/24")]
        [InlineData("192.168.1.1", "255.255.255.255", "192.168.1.1/32")]
        [InlineData("fc00::1", "fc00::", "fc00::1/6")]
        [InlineData("64:ffff:1::", "ffff:ffff:ffff::", "64:ffff:1::/48")]
        [InlineData("2001:db8::", "ffff:ffff:ffff:ffff::", "2001:db8::/64")]
        [InlineData("2001:db8::1", "ffff:ffff:ffff:ffff:ffff::", "2001:db8::1/80")]
        [InlineData("2001:db8::2", "ffff:ffff:ffff:ffff:ffff:ffff::", "2001:db8::2/96")]
        [InlineData("2001:db8::3", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff", "2001:db8::3/128")]
        public void ShouldReturnCIDR_WhenIPAndNetmaskSpecified(
            string ipAddress,
            string netmask,
            string expectedCIDR)
        {
            // Arrange
            var ip = IPAddress.Parse(ipAddress);
            var mask = IPAddress.Parse(netmask);

            // Act
            var result = IPUtilities.ConvertToCIDR(ip, mask);

            // Assert
            result.Should().Be(expectedCIDR);
        }
    }

    public class GetCIDRLengthFromNetmask : IPUtilitiesTests
    {
        [Theory]
        [MemberData(nameof(IPv4CIDRData))]
        [MemberData(nameof(IPv6CIDRData))]
        public void ShouldReturnCIDRLength_WhenNetmaskSpecified(
            int expectedCIDRLength,
            IPAddress netmask)
        {
            // Act
            var result = IPUtilities.GetCIDRLengthFromNetmask(netmask);

            // Assert
            result.Should().Be(expectedCIDRLength);
        }
    }

    public class ConvertFromCIDR
    {
        [Theory]
        [InlineData("192.168.42,21/24")]
        [InlineData("192.168.421.21/24")]
        [InlineData("localhost/8")]
        [InlineData("64:ffff:1:;")]
        [InlineData("64:feffef:1::")]
        [InlineData("64:gggg:1::")]
        public void ShouldThrow_WhenIPInvalid(string cidr)
        {
            // Act
            var result = () => IPUtilities.ConvertFromCIDR(cidr);

            // Assert
            result.Should()
                .Throw<FormatException>()
                .WithMessage("Could not parse IP address of provided CIDR string.");
        }

        [Theory]
        [InlineData("192.168.42.21/24,4")]
        [InlineData("192.168.42.21/abc")]
        [InlineData("127.0.0.1/")]
        [InlineData("64:ffff:1::/xyz")]
        [InlineData("64:ffff:1::/")]
        public void ShouldThrow_WhenCIDRLengthInvalid(string cidr)
        {
            // Act
            var result = () => IPUtilities.ConvertFromCIDR(cidr);

            // Assert
            result.Should()
                .Throw<FormatException>()
                .WithMessage("Could not parse netmask of provided CIDR string.");
        }

        [Theory]
        [InlineData("10.0.0.1", "255.0.0.0", "10.0.0.1/8")]
        [InlineData("172.16.0.1", "255.255.0.0", "172.16.0.1/16")]
        [InlineData("192.168.42.21", "255.255.255.0", "192.168.42.21/24")]
        [InlineData("192.168.1.1", "255.255.255.255", "192.168.1.1/32")]
        [InlineData("fc00::1", "fc00::", "fc00::1/6")]
        [InlineData("64:ffff:1::", "ffff:ffff:ffff::", "64:ffff:1::/48")]
        [InlineData("2001:db8::", "ffff:ffff:ffff:ffff::", "2001:db8::/64")]
        [InlineData("2001:db8::1", "ffff:ffff:ffff:ffff:ffff::", "2001:db8::1/80")]
        [InlineData("2001:db8::2", "ffff:ffff:ffff:ffff:ffff:ffff::", "2001:db8::2/96")]
        [InlineData("2001:db8::3", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff", "2001:db8::3/128")]
        public void ShouldReturnIPAndNetmask_WhenCIDRSpecified(
            string expectedIPAddress,
            string expectedNetmask,
            string cidr)
        {
            // Arrange
            var expectedIP = IPAddress.Parse(expectedIPAddress);
            var expectedMask = IPAddress.Parse(expectedNetmask);

            // Act
            var (ipAddress, netmask) = IPUtilities.ConvertFromCIDR(cidr);

            // Assert
            ipAddress.Should().Be(expectedIP);
            netmask.Should().Be(expectedMask);
        }
    }

    public class GetNetmaskFromCIDRLength : IPUtilitiesTests
    {
        [Theory]
        [InlineData(-32, AddressFamily.InterNetwork)]
        [InlineData(-1, AddressFamily.InterNetwork)]
        [InlineData(33, AddressFamily.InterNetwork)]
        [InlineData(64, AddressFamily.InterNetwork)]

        [InlineData(-32, AddressFamily.InterNetworkV6)]
        [InlineData(-1, AddressFamily.InterNetworkV6)]
        [InlineData(129, AddressFamily.InterNetworkV6)]
        [InlineData(256, AddressFamily.InterNetworkV6)]
        public void ShouldThrow_WhenCIDRLengthOutOfRange(int cidrLength, AddressFamily addressFamily)
        {
            // Arrange
            var maxCidrLength = addressFamily == AddressFamily.InterNetworkV6 ? 128 : 32;
            var expectedParameterName = "cidrLength";

            // Act
            var result = () => IPUtilities.GetNetmaskFromCIDRLength(cidrLength, addressFamily);

            // Assert
            result.Should()
                .Throw<ArgumentOutOfRangeException>()
                .WithMessage($"CIDR length must be between 0 and {maxCidrLength}.*")
                .WithParameterName(expectedParameterName);
        }

        [Fact]
        public void ShouldThrow_WhenUnsupportedAddressFamily()
        {
            // Act
            var result = () => IPUtilities.GetNetmaskFromCIDRLength(24, AddressFamily.ControllerAreaNetwork);

            // Assert
            result.Should()
                .Throw<ArgumentException>()
                .WithMessage("Unsupported address family.*")
                .WithParameterName("addressFamily");
        }

        [Theory]
        [MemberData(nameof(IPv4CIDRData))]
        public void ShouldReturnNetmask_WhenIPv4CIDRLengthSpecified(
            int cidrLength,
            IPAddress expectedNetmask)
        {
            // Act
            var result = IPUtilities.GetNetmaskFromCIDRLength(cidrLength, AddressFamily.InterNetwork);

            // Assert
            result.Should().Be(expectedNetmask);
        }

        [Theory]
        [MemberData(nameof(IPv6CIDRData))]
        public void ShouldReturnNetmask_WhenIPv6CIDRLengthSpecified(
            int cidrLength,
            IPAddress expectedNetmask)
        {
            // Act
            var result = IPUtilities.GetNetmaskFromCIDRLength(cidrLength, AddressFamily.InterNetworkV6);

            // Assert
            result.Should().Be(expectedNetmask);
        }
    }
}
