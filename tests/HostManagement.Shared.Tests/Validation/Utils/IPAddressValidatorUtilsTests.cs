using System.Net;
using AwesomeAssertions;
using Xunit;
using HostManagement.Shared.Validation.Utils;

namespace HostManagement.Shared.Tests.Validation.Utils;

public class IPAddressValidatorUtilsTests
{
    public class NetworkCIDRProperties
    {
        [Fact]
        public void ShouldReturnNetworkCIDR_WhenPropertyCalled()
        {
            IPAddressValidatorUtils.IPv4NotRoutableNetworkCIDR.Should().Be("0.0.0.0/32");
            IPAddressValidatorUtils.IPv4LoopbackNetworkCIDR.Should().Be("127.0.0.0/8");
            IPAddressValidatorUtils.IPv6LoopbackNetworkCIDR.Should().Be("::1/128");
            IPAddressValidatorUtils.IPv4LinkLocalNetworkCIDR.Should().Be("169.254.0.0/16");
            IPAddressValidatorUtils.IPv6LinkLocalNetworkCIDR.Should().Be("fe80::/10");
            IPAddressValidatorUtils.IPv4MulticastNetworkCIDR.Should().Be("224.0.0.0/4");
            IPAddressValidatorUtils.IPv4BroadcastNetworkCIDR.Should().Be("255.255.255.255/32");
            IPAddressValidatorUtils.IPv6MulticastNetworkCIDR.Should().Be("ff00::/8");
        }
    }

    public class IsIPv4NotRoutable
    {
        [Fact]
        public void ShouldReturnFalse_WhenAddressIsNotIPv4()
        {
            // Arrange
            var ipAddress = IPAddress.Parse("fe80::1");

            // Act
            var result = IPAddressValidatorUtils.IsIPv4NotRoutable(ipAddress);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenAddressIsNotIPv4NotRoutable()
        {
            // Arrange
            var ipAddress = IPAddress.Parse("192.168.42.21");

            // Act
            var result = IPAddressValidatorUtils.IsIPv4NotRoutable(ipAddress);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnTrue_WhenAddressIsIPv4NotRoutable()
        {
            // Act
            var result = IPAddressValidatorUtils
                .IsIPv4NotRoutable(ValidationTestData.Gateway.InvalidNotRoutableIPv4);

            // Assert
            result.Should().BeTrue();
        }
    }

    public class IsNotRoutable
    {
        [Fact]
        public void ShouldReturnFalse_WhenAddressIsNotTheNotRoutableAddress()
        {
            // Arrange
            var ipAddress = IPAddress.Parse("192.168.42.21");

            // Act
            var result = IPAddressValidatorUtils.IsNotRoutable(ipAddress, out var notRoutableCIDRString);

            // Assert
            result.Should().BeFalse();
            notRoutableCIDRString.Should().BeNull();
        }

        [Fact]
        public void ShouldReturnTrue_WhenAddressIsTheNotRoutableAddress()
        {
            // Arrange
            var ipAddress = ValidationTestData.Gateway.InvalidNotRoutableIPv4;

            // Act
            var result = IPAddressValidatorUtils.IsNotRoutable(ipAddress, out var notRoutableCIDRString);

            // Assert
            result.Should().BeTrue();
            notRoutableCIDRString.Should().Be("0.0.0.0/32");
        }
    }

    public class IsIPv4Loopback
    {
        [Fact]
        public void ShouldReturnFalse_WhenAddressIsNotIPv4()
        {
            // Arrange
            var ipAddress = IPAddress.Parse("fe80::1");

            // Act
            var result = IPAddressValidatorUtils.IsIPv4Loopback(ipAddress);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenAddressIsNotIPv4Loopback()
        {
            // Arrange
            var ipAddress = IPAddress.Parse("192.168.42.21");

            // Act
            var result = IPAddressValidatorUtils.IsIPv4Loopback(ipAddress);

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData("127.0.0.0")]
        [InlineData("127.0.0.1")]
        [InlineData("127.0.21.42")]
        [InlineData("127.128.255.254")]
        [InlineData("127.255.255.255")]
        public void ShouldReturnTrue_WhenAddressIsIPv4Loopback(string loopbackIPAddress)
        {
            // Arrange
            var ipAddress = IPAddress.Parse(loopbackIPAddress);

            // Act
            var result = IPAddressValidatorUtils.IsIPv4Loopback(ipAddress);

            // Assert
            result.Should().BeTrue();
        }
    }

    public class IsIPv6Loopback
    {
        [Fact]
        public void ShouldReturnFalse_WhenAddressIsNotIPv6()
        {
            // Arrange
            var ipAddress = IPAddress.Parse("1.1.1.1");

            // Act
            var result = IPAddressValidatorUtils.IsIPv6Loopback(ipAddress);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenAddressIsNotIPv6Loopback()
        {
            // Arrange
            var ipAddress = IPAddress.Parse("::42");

            // Act
            var result = IPAddressValidatorUtils.IsIPv6Loopback(ipAddress);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnTrue_WhenAddressIsIPv6Loopback()
        {
            // Arrange
            var ipAddress = IPAddress.Parse("::1");

            // Act
            var result = IPAddressValidatorUtils.IsIPv6Loopback(ipAddress);

            // Assert
            result.Should().BeTrue();
        }
    }

    public class IsLoopback
    {
        [Theory]
        [InlineData("192.168.42.21")]
        [InlineData("::42")]
        public void ShouldReturnFalse_WhenAddressIsNotLoopback(string ipString)
        {
            // Arrange
            var ipAddress = IPAddress.Parse(ipString);

            // Act
            var result = IPAddressValidatorUtils.IsLoopback(ipAddress, out var loopbackCIDRString);

            // Assert
            result.Should().BeFalse();
            loopbackCIDRString.Should().BeNull();
        }

        [Theory]
        [InlineData("127.0.0.1", "127.0.0.0/8")]
        [InlineData("::1", "::1/128")]
        public void ShouldReturnTrue_WhenAddressIsLoopback(
            string ipString,
            string expectedLoopbackCIDRString)
        {
            // Arrange
            var ipAddress = IPAddress.Parse(ipString);

            // Act
            var result = IPAddressValidatorUtils.IsLoopback(ipAddress, out var loopbackCIDRString);

            // Assert
            result.Should().BeTrue();
            loopbackCIDRString.Should().Be(expectedLoopbackCIDRString);
        }
    }

    public class IsIPv4LinkLocal
    {
        [Fact]
        public void ShouldReturnFalse_WhenAddressIsNotIPv4()
        {
            // Arrange
            var ipAddress = IPAddress.Parse("fe80::1");

            // Act
            var result = IPAddressValidatorUtils.IsIPv4LinkLocal(ipAddress);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenAddressIsNotIPv4LinkLocal()
        {
            // Arrange
            var ipAddress = IPAddress.Parse("192.168.42.21");

            // Act
            var result = IPAddressValidatorUtils.IsIPv4LinkLocal(ipAddress);

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData("169.254.0.0")]
        [InlineData("169.254.0.1")]
        [InlineData("169.254.42.21")]
        [InlineData("169.254.255.254")]
        [InlineData("169.254.255.255")]
        public void ShouldReturnTrue_WhenAddressIsIPv4LinkLocal(string linkLocalIPAddress)
        {
            // Arrange
            var ipAddress = IPAddress.Parse(linkLocalIPAddress);

            // Act
            var result = IPAddressValidatorUtils.IsIPv4LinkLocal(ipAddress);

            // Assert
            result.Should().BeTrue();
        }
    }

    public class IsIPv6LinkLocal
    {
        [Fact]
        public void ShouldReturnFalse_WhenAddressIsNotIPv6()
        {
            // Arrange
            var ipAddress = IPAddress.Parse("1.1.1.1");

            // Act
            var result = IPAddressValidatorUtils.IsIPv6LinkLocal(ipAddress);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenAddressIsNotIPv6LinkLocal()
        {
            // Arrange
            var ipAddress = IPAddress.Parse("2606:4700:4700::1111");

            // Act
            var result = IPAddressValidatorUtils.IsIPv6LinkLocal(ipAddress);

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData("fe80::")]
        [InlineData("fe80::1")]
        [InlineData("fe80::1234:5678:9abc:def0")]
        [InlineData("febf:ffff:ffff:ffff:ffff:ffff:ffff:fffe")]
        [InlineData("febf:ffff:ffff:ffff:ffff:ffff:ffff:ffff")]
        public void ShouldReturnTrue_WhenAddressIsIPv6LinkLocal(string linkLocalIPAddress)
        {
            // Arrange
            var ipAddress = IPAddress.Parse(linkLocalIPAddress);

            // Act
            var result = IPAddressValidatorUtils.IsIPv6LinkLocal(ipAddress);

            // Assert
            result.Should().BeTrue();
        }
    }

    public class IsLinkLocal
    {
        [Theory]
        [InlineData("192.168.42.21")]
        [InlineData("2606:4700:4700::1111")]
        public void ShouldReturnFalse_WhenAddressIsNotLinkLocal(string ipString)
        {
            // Arrange
            var ipAddress = IPAddress.Parse(ipString);

            // Act
            var result = IPAddressValidatorUtils.IsLinkLocal(ipAddress, out var linkLocalCIDRString);

            // Assert
            result.Should().BeFalse();
            linkLocalCIDRString.Should().BeNull();
        }

        [Theory]
        [InlineData("169.254.0.0", "169.254.0.0/16")]
        [InlineData("fe80::", "fe80::/10")]
        public void ShouldReturnTrue_WhenAddressIsLinkLocal(
            string ipString,
            string expectedLinkLocalCIDRString)
        {
            // Arrange
            var ipAddress = IPAddress.Parse(ipString);

            // Act
            var result = IPAddressValidatorUtils.IsLinkLocal(ipAddress, out var linkLocalCIDRString);

            // Assert
            result.Should().BeTrue();
            linkLocalCIDRString.Should().Be(expectedLinkLocalCIDRString);
        }
    }

    public class IsIPv4Multicast
    {
        [Fact]
        public void ShouldReturnFalse_WhenAddressIsNotIPv4()
        {
            // Arrange
            var ipAddress = IPAddress.Parse("fe80::1");

            // Act
            var result = IPAddressValidatorUtils.IsIPv4Multicast(ipAddress);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenAddressIsNotIPv4Multicast()
        {
            // Arrange
            var ipAddress = IPAddress.Parse("192.168.42.21");

            // Act
            var result = IPAddressValidatorUtils.IsIPv4Multicast(ipAddress);

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData("224.0.0.0")]
        [InlineData("224.0.0.1")]
        [InlineData("231.84.42.21")]
        [InlineData("239.255.255.254")]
        [InlineData("239.255.255.255")]
        public void ShouldReturnTrue_WhenAddressIsIPv4Multicast(string multicastIPAddress)
        {
            // Arrange
            var ipAddress = IPAddress.Parse(multicastIPAddress);

            // Act
            var result = IPAddressValidatorUtils.IsIPv4Multicast(ipAddress);

            // Assert
            result.Should().BeTrue();
        }
    }

    public class IsIPv6Multicast
    {
        [Fact]
        public void ShouldReturnFalse_WhenAddressIsNotIPv6()
        {
            // Arrange
            var ipAddress = IPAddress.Parse("1.1.1.1");

            // Act
            var result = IPAddressValidatorUtils.IsIPv6Multicast(ipAddress);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenAddressIsNotIPv6Multicast()
        {
            // Arrange
            var ipAddress = IPAddress.Parse("2606:4700:4700::1111");

            // Act
            var result = IPAddressValidatorUtils.IsIPv6LinkLocal(ipAddress);

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData("ff00::")]
        [InlineData("ff00::1")]
        [InlineData("ff05::1234")]
        [InlineData("ff0e::1a2b:3c4d")]
        [InlineData("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")]
        public void ShouldReturnTrue_WhenAddressIsIPv6Multicast(string multicastIPAddress)
        {
            // Arrange
            var ipAddress = IPAddress.Parse(multicastIPAddress);

            // Act
            var result = IPAddressValidatorUtils.IsIPv6Multicast(ipAddress);

            // Assert
            result.Should().BeTrue();
        }
    }

    public class IsMulticast
    {
        [Theory]
        [InlineData("192.168.42.21")]
        [InlineData("2606:4700:4700::1111")]
        public void ShouldReturnFalse_WhenAddressIsNotMulticast(string ipString)
        {
            // Arrange
            var ipAddress = IPAddress.Parse(ipString);

            // Act
            var result = IPAddressValidatorUtils.IsMulticast(ipAddress, out var multicastCIDRString);

            // Assert
            result.Should().BeFalse();
            multicastCIDRString.Should().BeNull();
        }

        [Theory]
        [InlineData("224.0.0.0", "224.0.0.0/4")]
        [InlineData("ff00::", "ff00::/8")]
        public void ShouldReturnTrue_WhenAddressIsMulticast(
            string ipString,
            string expectedMulticastCIDRString)
        {
            // Arrange
            var ipAddress = IPAddress.Parse(ipString);

            // Act
            var result = IPAddressValidatorUtils.IsMulticast(ipAddress, out var multicastCIDRString);

            // Assert
            result.Should().BeTrue();
            multicastCIDRString.Should().Be(expectedMulticastCIDRString);
        }
    }

    public class IsIpv4Broadcast
    {
        [Fact]
        public void ShouldReturnFalse_WhenAddressIsNotIPv4()
        {
            // Arrange
            var ipAddress = IPAddress.Parse("fe80::1");

            // Act
            var result = IPAddressValidatorUtils.IsIPv4Broadcast(ipAddress);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenAddressIsNotIPv4Broadcast()
        {
            // Arrange
            var ipAddress = IPAddress.Parse("192.168.42.21");

            // Act
            var result = IPAddressValidatorUtils.IsIPv4Broadcast(ipAddress);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnTrue_WhenAddressIsIPv4Broadcast()
        {
            // Arrange
            var ipAddress = IPAddress.Parse("255.255.255.255");

            // Act
            var result = IPAddressValidatorUtils.IsIPv4Broadcast(ipAddress);

            // Assert
            result.Should().BeTrue();
        }
    }

    public class IsBroadcast
    {
        [Fact]
        public void ShouldReturnFalse_WhenAddressIsNotBroadcast()
        {
            // Arrange
            var ipAddress = IPAddress.Parse("192.168.42.21");

            // Act
            var result = IPAddressValidatorUtils.IsBroadcast(ipAddress, out var broadcastCIDRString);

            // Assert
            result.Should().BeFalse();
            broadcastCIDRString.Should().BeNull();
        }

        [Fact]
        public void ShouldReturnTrue_WhenAddressIsBroadcast()
        {
            // Arrange
            var ipAddress = IPAddress.Parse("255.255.255.255");

            // Act
            var result = IPAddressValidatorUtils.IsBroadcast(ipAddress, out var broadcastCIDRString);

            // Assert
            result.Should().BeTrue();
            broadcastCIDRString.Should().Be("255.255.255.255/32");
        }
    }

    public class IsPotentialIPv4Address
    {
        [Theory]
        [InlineData("192.168.42")]
        [InlineData("192.168.178.42.21")]
        [InlineData("my.company.com")]
        public void ShouldReturnFalse_WhenAddressLengthNotEqualTo4(string potentialIPv4Address)
        {
            // Act
            var result = IPAddressValidatorUtils.IsPotentialIPv4Address(potentialIPv4Address);

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData("my.company.domain.com")]
        [InlineData("192.168.178.F8")]
        [InlineData("192.168.178.4٢")] // with ARABIC-INDIC DIGIT TWO
        public void ShouldReturnFalse_WhenAddressHasNonAsciiDigits(string potentialIPv4Address)
        {
            // Act
            var result = IPAddressValidatorUtils.IsPotentialIPv4Address(potentialIPv4Address);

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData("192.168.178.12345")]
        [InlineData("192.168.178.42")]
        public void ShouldReturnTrue_WhenAddressIsPotentialIPv4Address(string potentialIPv4Address)
        {
            // Act
            var result = IPAddressValidatorUtils.IsPotentialIPv4Address(potentialIPv4Address);

            // Assert
            result.Should().BeTrue();
        }

    }

    public class IsPotentialIPv6Address
    {
        [Theory]
        [InlineData("192.168")]
        [InlineData("2606:4700")]
        [InlineData("my.company")]
        public void ShouldReturnFalse_WhenAddressLengthLessThan3(string potentialIPv6Address)
        {
            // Act
            var result = IPAddressValidatorUtils.IsPotentialIPv6Address(potentialIPv6Address);

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData("1.2.3.4.5.6.7.8.9")]
        [InlineData("2001:0db8:85a3:08d3:1319:8a2e:0370:7344:4242")]
        [InlineData("my.12.company.34.56.78.90.domain.com")]
        public void ShouldReturnFalse_WhenAddressLengthGreaterThan8(string potentialIPv6Address)
        {
            // Act
            var result = IPAddressValidatorUtils.IsPotentialIPv6Address(potentialIPv6Address);

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData("my.company.domain.com")]
        [InlineData("٢001:0db8:85a3:08d3:1319:8a2e:0370:7344")] // with ARABIC-INDIC DIGIT TWO
        public void ShouldReturnFalse_WhenAddressHasNonAsciiDigitOrLetter(string potentialIPv6Address)
        {
            // Act
            var result = IPAddressValidatorUtils.IsPotentialIPv6Address(potentialIPv6Address);

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData("2001:0db8:85a3:08d3:1319:8a2e:0370:7344")]
        [InlineData("2001:db8::1428:57ab")]
        [InlineData("fe80::")]
        public void ShouldReturnTrue_WhenAddressIsPotentialIPv6Address(string potentialIPv6Address)
        {
            // Act
            var result = IPAddressValidatorUtils.IsPotentialIPv6Address(potentialIPv6Address);

            // Assert
            result.Should().BeTrue();
        }

    }

    public class IsInDifferentSubnet
    {
        [Fact]
        public void ShouldReturnFalse_WhenAddressIsInSameSubnet()
        {
            // Arrange
            var firstAddress = IPAddress.Parse("192.168.42.21");
            var secondAddress = IPAddress.Parse("192.168.42.22");
            var netmask = IPAddress.Parse("255.255.255.0");

            // Act
            var result = IPAddressValidatorUtils.IsInDifferentSubnet(
                firstAddress,
                secondAddress,
                netmask,
                out var firstNetworkAddressString,
                out var secondNetworkAddressString);

            // Assert
            result.Should().BeFalse();
            firstNetworkAddressString.Should().BeNull();
            secondNetworkAddressString.Should().BeNull();
        }

        [Fact]
        public void ShouldReturnTrue_WhenAddressIsInDifferentSubnet()
        {
            // Arrange
            var firstAddress = IPAddress.Parse("192.168.42.21");
            var secondAddress = IPAddress.Parse("192.168.32.22");
            var netmask = IPAddress.Parse("255.255.255.0");

            // Act
            var result = IPAddressValidatorUtils.IsInDifferentSubnet(
                firstAddress,
                secondAddress,
                netmask,
                out var firstNetworkAddressString,
                out var secondNetworkAddressString);

            // Assert
            result.Should().BeTrue();
            firstNetworkAddressString.Should().Be("192.168.42.0");
            secondNetworkAddressString.Should().Be("192.168.32.0");
        }
    }

    public class IsSubnetNetworkAddress
    {
        [Fact]
        public void ShouldReturnFalse_WhenAddressIsNotSubnetNetworkAddress()
        {
            // Arrange
            var address = IPAddress.Parse("192.168.42.10");
            var addressToCheckAgainst = IPAddress.Parse("192.168.42.21");
            var netmaskToCheckAgainst = IPAddress.Parse("255.255.255.0");

            // Act
            var result = IPAddressValidatorUtils.IsSubnetNetworkAddress(
                address,
                addressToCheckAgainst,
                netmaskToCheckAgainst,
                out var networkAddressString);

            // Assert
            result.Should().BeFalse();
            networkAddressString.Should().BeNull();
        }

        [Fact]
        public void ShouldReturnTrue_WhenAddressIsSubnetNetworkAddress()
        {
            // Arrange
            var address = IPAddress.Parse("192.168.42.0");
            var addressToCheckAgainst = IPAddress.Parse("192.168.42.21");
            var netmaskToCheckAgainst = IPAddress.Parse("255.255.255.0");

            // Act
            var result = IPAddressValidatorUtils.IsSubnetNetworkAddress(
                address,
                addressToCheckAgainst,
                netmaskToCheckAgainst,
                out var networkAddressString);

            // Assert
            result.Should().BeTrue();
            networkAddressString.Should().Be("192.168.42.0");
        }
    }

    public class IsSubnetBroadcastAddress
    {
        [Fact]
        public void ShouldReturnFalse_WhenAddressIsNotSubnetBroadcastAddress()
        {
            // Arrange
            var address = IPAddress.Parse("192.168.42.10");
            var addressToCheckAgainst = IPAddress.Parse("192.168.42.21");
            var netmaskToCheckAgainst = IPAddress.Parse("255.255.255.0");

            // Act
            var result = IPAddressValidatorUtils.IsSubnetBroadcastAddress(
                address,
                addressToCheckAgainst,
                netmaskToCheckAgainst,
                out var broadcastAddressString);

            // Assert
            result.Should().BeFalse();
            broadcastAddressString.Should().BeNull();
        }

        [Fact]
        public void ShouldReturnTrue_WhenAddressIsSubnetBroadcastAddress()
        {
            // Arrange
            var address = IPAddress.Parse("192.168.42.255");
            var addressToCheckAgainst = IPAddress.Parse("192.168.42.21");
            var netmaskToCheckAgainst = IPAddress.Parse("255.255.255.0");

            // Act
            var result = IPAddressValidatorUtils.IsSubnetBroadcastAddress(
                address,
                addressToCheckAgainst,
                netmaskToCheckAgainst,
                out var broadcastAddressString);

            // Assert
            result.Should().BeTrue();
            broadcastAddressString.Should().Be("192.168.42.255");
        }
    }

    public class IsSubnetHostAddress
    {
        [Theory]
        [InlineData("192.168.42.128", "255.255.255.254", "0")]
        [InlineData("192.168.42.128", "255.255.255.128", "192.168.42.129 - 192.168.42.254")]
        public void ShouldReturnFalse_WhenAddressIsNotSubnetHostAddress(
            string addressString,
            string netmaskString,
            string expectedAvailableHostsString)
        {
            // Arrange
            var address = IPAddress.Parse(addressString);
            var netmask = IPAddress.Parse(netmaskString);

            // Act
            var result = IPAddressValidatorUtils.IsSubnetHostAddress(
                address,
                netmask,
                out var availableHostsString);

            // Assert
            result.Should().BeFalse();
            availableHostsString.Should().Be(expectedAvailableHostsString);
        }

        [Fact]
        public void ShouldReturnTrue_WhenAddressIsSubnetHostAddress()
        {
            // Arrange
            var address = IPAddress.Parse("192.168.42.123");
            var netmask = IPAddress.Parse("255.255.255.128");
            var expectedAvailableHostsString = "192.168.42.1 - 192.168.42.126";

            // Act
            var result = IPAddressValidatorUtils.IsSubnetHostAddress(
                address,
                netmask,
                out var availableHostsString);

            // Assert
            result.Should().BeTrue();
            availableHostsString.Should().Be(expectedAvailableHostsString);
        }
    }

    public class GetIPv4AddressAsUInt
    {
        [Fact]
        public void ShouldThrow_WhenAddressIsNotIPv4Address()
        {
            // Arrange
            var nonIPv4Address = IPAddress.Parse("ff00::");
            var expectedMessage = $"IP address must be an IPv4 address.*";

            // Act
            var result = () => IPAddressValidatorUtils.GetIPv4AddressAsUInt(nonIPv4Address);

            // Assert
            result.Should()
                .Throw<ArgumentException>()
                .WithMessage(expectedMessage);
        }

        [Theory]
        [InlineData("255.255.255.0", 0xFFFFFF00)]
        [InlineData("255.255.0.0", 0xFFFF0000)]
        [InlineData("255.0.0.0", 0xFF000000)]
        [InlineData("0.0.0.0", 0x00000000)]
        [InlineData("192.168.1.1", 0xC0A80101)]
        [InlineData("10.0.0.1", 0x0A000001)]
        [InlineData("172.16.0.1", 0xAC100001)]
        [InlineData("1.2.3.4", 0x01020304)]
        [InlineData("127.0.0.1", 0x7F000001)]
        [InlineData("8.8.8.8", 0x08080808)]
        public void ShouldReturnUInt_WhenAddressIsValidIPv4Address(
            string address,
            uint addressAsUInt)
        {
            // Arrange
            var validIPv4Address = IPAddress.Parse(address);

            // Act
            var result = IPAddressValidatorUtils.GetIPv4AddressAsUInt(validIPv4Address);

            // Assert
            result.Should().Be(addressAsUInt);
        }
    }
}
