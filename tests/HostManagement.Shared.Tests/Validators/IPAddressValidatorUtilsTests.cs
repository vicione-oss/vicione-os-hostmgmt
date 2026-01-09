using Xunit;
using FluentAssertions;
using HostManagement.Shared.Validation.Utils;
using System.Net;

namespace HostManagement.Shared.Tests.Validators;

public class IPAddressValidatorUtilsTests
{
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
}
