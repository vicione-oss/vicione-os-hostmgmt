using System.Net;
using FluentAssertions;
using Xunit;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Validation;
using HostManagement.Shared.Validation.Validators.NetworkInterfaces;

namespace HostManagement.Shared.Tests.Validation.Validators.NetworkInterfaces;

public class IPv4DetailValidatorTests
{
    private static string GetCustomInterfaceNameText(string? interfaceName)
        => interfaceName is null ? "network interface" : $"network interface '{interfaceName}'";

    [Theory]
    [InlineData(null, "ff00::1")]
    [InlineData("lan1", "ff00::1")]
    public async Task ShouldReturnValidationError_WhenIPAddressIsNoValidIPv4Address(
        string? interfaceName,
        string addressString)
    {
        // Arrange
        var address = IPAddress.Parse(addressString);
        var ipv4Detail = new IPv4Detail() { IPAddress = address, Netmask = ValidationTestData.Netmask.ValidIPv4 };

        var expectedValidationEntry = new ValidationEntry(
            $"IP address '{address}' of {GetCustomInterfaceNameText(interfaceName)} is no valid IPv4 address.");

        // Act
        var result = await new IPv4DetailValidator(interfaceName).ValidateAsync(ipv4Detail);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData("lan1", "10.0.0.0", "255.0.0.0", "10.0.0.1 - 10.255.255.254")]                    // Network: 10.0.0.0/8          Broadcast: 10.255.255.255
    [InlineData("lan1", "10.255.255.255", "255.0.0.0", "10.0.0.1 - 10.255.255.254")]              // Network: 10.0.0.0/8          Broadcast: 10.255.255.255
    [InlineData("lan1", "172.16.0.0", "255.255.0.0", "172.16.0.1 - 172.16.255.254")]              // Network: 172.16.0.0/16       Broadcast: 172.16.255.255
    [InlineData("lan1", "172.16.255.255", "255.255.0.0", "172.16.0.1 - 172.16.255.254")]          // Network: 172.16.0.0/16       Broadcast: 172.16.255.255
    [InlineData("lan1", "192.168.0.0", "255.255.255.0", "192.168.0.1 - 192.168.0.254")]           // Network: 192.168.0.0/24      Broadcast: 192.168.0.255
    [InlineData("lan1", "192.168.0.255", "255.255.255.0", "192.168.0.1 - 192.168.0.254")]         // Network: 192.168.0.0/24      Broadcast: 192.168.0.255
    [InlineData("lan1", "198.51.100.0", "255.255.255.128", "198.51.100.1 - 198.51.100.126")]      // Network: 198.51.100.0/25     Broadcast: 198.51.100.127
    [InlineData(null, "198.51.100.127", "255.255.255.128", "198.51.100.1 - 198.51.100.126")]      // Network: 198.51.100.0/25     Broadcast: 198.51.100.127
    [InlineData(null, "192.168.178.120", "255.255.255.252", "192.168.178.121 - 192.168.178.122")] // Network: 192.168.178.120/30  Broadcast: 192.168.178.123
    [InlineData(null, "192.168.178.123", "255.255.255.252", "192.168.178.121 - 192.168.178.122")] // Network: 192.168.178.120/30  Broadcast: 192.168.178.123
    [InlineData(null, "1.2.3.4", "255.255.255.252", "1.2.3.5 - 1.2.3.6")]                         // Network: 1.2.3.4/30          Broadcast: 1.2.3.7
    [InlineData(null, "1.2.3.7", "255.255.255.252", "1.2.3.5 - 1.2.3.6")]                         // Network: 1.2.3.4/30          Broadcast: 1.2.3.7
    [InlineData(null, "192.168.178.123", "255.255.255.254", "0")]                                 // Network: 192.168.178.122/31  Broadcast: 192.168.178.123
    [InlineData(null, "192.168.178.123", "255.255.255.255", "0")]                                 // Network: 192.168.178.123/32  Broadcast: 192.168.178.123
    public async Task ShouldReturnValidationError_WhenIPAddressAndNetmaskConfigInvalid(
        string? interfaceName,
        string addressString,
        string netmaskString,
        string availableHostsString)
    {
        // Arrange
        var address = IPAddress.Parse(addressString);
        var netmask = IPAddress.Parse(netmaskString);
        var ipv4Detail = new IPv4Detail() { IPAddress = address, Netmask = netmask };

        var expectedValidationEntry = new ValidationEntry(
            $"Netmask '{netmask}' of {GetCustomInterfaceNameText(interfaceName)} defines a subnet " +
            $"that does not include the host IP '{address}' (available hosts: {availableHostsString}).");

        // Act
        var result = await new IPv4DetailValidator(interfaceName).ValidateAsync(ipv4Detail);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData("lan1", "192.168.178.123", "248.0.0.0")]     // Network: 192.0.0.0/5         Broadcast: 199.255.255.255
    [InlineData("lan1", "10.0.0.1", "255.0.0.0")]            // Network: 10.0.0.0/8          Broadcast: 10.255.255.255
    [InlineData("lan1", "100.64.0.1", "255.192.0.0")]        // Network: 100.64.0.0/10       Broadcast: 100.127.255.255
    [InlineData("lan1", "192.168.178.123", "255.192.0.0")]   // Network: 192.128.0.0/10      Broadcast: 192.191.255.255
    [InlineData("lan1", "172.16.5.100", "255.255.0.0")]      // Network: 172.16.0.0/16       Broadcast: 172.16.255.255
    [InlineData("lan1", "192.168.0.254", "255.255.255.0")]   // Network: 192.168.0.0/24      Broadcast: 192.168.0.255
    [InlineData(null, "192.168.178.123", "255.255.255.0")]   // Network: 192.168.178.0/24    Broadcast: 192.168.178.255
    [InlineData(null, "198.51.100.10", "255.255.255.128")]   // Network: 198.51.100.0/25     Broadcast: 198.51.100.127
    [InlineData(null, "203.0.113.200", "255.255.255.192")]   // Network: 203.0.113.192/26    Broadcast: 203.0.113.255
    [InlineData(null, "192.0.2.42", "255.255.255.240")]      // Network: 192.0.2.32/28       Broadcast: 192.0.2.47
    [InlineData(null, "192.168.178.123", "255.255.255.248")] // Network: 192.168.178.120/29  Broadcast: 192.168.178.127
    [InlineData(null, "1.2.3.5", "255.255.255.252")]         // Network: 1.2.3.4/30          Broadcast: 1.2.3.7
    public async Task ShouldReturnValid_WhenIPAddressAndNetmaskConfigValid(
        string? interfaceName,
        string addressString,
        string netmaskString)
    {
        // Arrange
        var address = IPAddress.Parse(addressString);
        var netmask = IPAddress.Parse(netmaskString);
        var ipv4Detail = new IPv4Detail() { IPAddress = address, Netmask = netmask };

        // Act
        var result = await new IPv4DetailValidator(interfaceName).ValidateAsync(ipv4Detail);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Entries.Count.Should().Be(0);
    }
}
