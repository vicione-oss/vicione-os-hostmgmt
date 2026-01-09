using System.Net;
using FluentAssertions;
using Xunit;
using HostManagement.Shared.Validation.Utils;

namespace HostManagement.Shared.Tests.Validation.Utils;

public class IPAddressValidatorUtilsTests
{
    public class IPv4LoopbackNetworkCIDRProperty
    {
        [Fact]
        public void ShouldReturnIPv4LoopbackNetworkCIDR_WhenCalled()
            => IPAddressValidatorUtils.IPv4LoopbackNetworkCIDR.Should().Be("127.0.0.0/8");
    }

    public class IPv6LoopbackNetworkCIDRProperty
    {
        [Fact]
        public void ShouldReturnIPv6LoopbackNetworkCIDR_WhenCalled()
            => IPAddressValidatorUtils.IPv6LoopbackNetworkCIDR.Should().Be("::1/128");
    }

    public class IPv4LinkLocalNetworkCIDRProperty
    {
        [Fact]
        public void ShouldReturnIPv4LinkLocalNetworkCIDR_WhenCalled()
            => IPAddressValidatorUtils.IPv4LinkLocalNetworkCIDR.Should().Be("169.254.0.0/16");
    }

    public class IPv6LinkLocalNetworkCIDRProperty
    {
        [Fact]
        public void ShouldReturnIPv6LinkLocalNetworkCIDR_WhenCalled()
            => IPAddressValidatorUtils.IPv6LinkLocalNetworkCIDR.Should().Be("fe80::/10");
    }

    public class IPv4MulticastNetworkCIDRProperty
    {
        [Fact]
        public void ShouldReturnIPv4MulticastNetworkCIDR_WhenCalled()
            => IPAddressValidatorUtils.IPv4MulticastNetworkCIDR.Should().Be("224.0.0.0/4");
    }

    public class IPv6MulticastNetworkCIDRProperty
    {
        [Fact]
        public void ShouldReturnIPv6MulticastNetworkCIDR_WhenCalled()
            => IPAddressValidatorUtils.IPv6MulticastNetworkCIDR.Should().Be("ff00::/8");
    }

    public class IPv4BroadcastNetworkCIDRProperty
    {
        [Fact]
        public void ShouldReturnIPv4BroadcastNetworkCIDR_WhenCalled()
            => IPAddressValidatorUtils.IPv4BroadcastNetworkCIDR.Should().Be("255.255.255.255/32");
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
        public void ShouldReturnTrue_WhenAddressIsIPv6Loopback()
        {
            // Arrange
            var ipAddress = IPAddress.Parse("::1");

            // Act
            var result = IPAddressValidatorUtils.IsIPv6Loopback(ipAddress);

            // Assert
            result.Should().BeTrue();
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
        public void ShouldReturnTrue_WhenAddressIsIPv4Broadcast()
        {
            // Arrange
            var ipAddress = IPAddress.Parse("255.255.255.255");

            // Act
            var result = IPAddressValidatorUtils.IsIPv4Broadcast(ipAddress);

            // Assert
            result.Should().BeTrue();
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
}
