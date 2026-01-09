using System.Net;
using AwesomeAssertions;
using Xunit;
using HostManagement.Shared.Validation;
using HostManagement.Shared.Validation.Validators.NetworkInterfaces;

namespace HostManagement.Shared.Tests.Validation.Validators.NetworkInterfaces;

public class NetworkInterfaceValidatorTests
{
    [Fact]
    public async Task ShouldReturnValidationError_WhenValidationCancelled()
    {
        // Arrange
        var expectedValidationEntry = new ValidationEntry(ValidationConstants.CancelledResult);

        // Act
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var result = await new NetworkInterfaceValidator()
            .ValidateAsync(ValidationTestData.Interface.Static, cts.Token);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldAddValidationResult_WhenMACAddressConfigured()
    {
        // Arrange
        var interfaceWithInvalidMAC = ValidationTestData.Interface.Static with
        {
            CommonInformation = ValidationTestData.Interface.Static.CommonInformation with
            {
                PhysicalAddress = "01-23-45-67-89-XX"
            }
        };

        // Act
        var result = await new NetworkInterfaceValidator().ValidateAsync(interfaceWithInvalidMAC);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.EntriesString.Should().Contain("MAC");
    }

    [Fact]
    public async Task ShouldAddValidationError_WhenLoopbackInterfaceSpecified()
    {
        // Arrange
        var loopbackInterface = ValidationTestData.Interface.DHCP with
        {
            CommonInformation = ValidationTestData.Interface.DHCP.CommonInformation with
            {
                Name = "lo"
            }
        };

        var expectedValidationEntry = new ValidationEntry("Loopback interface can not be specified.");

        // Act
        var result = await new NetworkInterfaceValidator().ValidateAsync(loopbackInterface);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldAddValidationResult_WhenVLANIDEnabled()
    {
        // Arrange
        var interfaceWithInvalidVLANID = ValidationTestData.Interface.Static with
        {
            IPv4 = ValidationTestData.Interface.Static.IPv4 with
            {
                VLANEnabled = true,
                VLANID = 0
            }
        };

        // Act
        var result = await new NetworkInterfaceValidator().ValidateAsync(interfaceWithInvalidVLANID);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.EntriesString.Should().Contain("VLAN");
    }

    [Fact]
    public async Task ShouldAddValidationError_WhenDHCPConfigInvalid()
    {
        // Arrange
        var interfaceWithInvalidDHCPConfig = ValidationTestData.Interface.DHCP with
        {
            IPv4 = new()
            {
                DHCPEnabled = false,
                IPv4Details = []
            }
        };

        var expectedValidationEntry = new ValidationEntry(
            $"Network interface '{interfaceWithInvalidDHCPConfig.CommonInformation.Name}' " +
            $"has no IP addresses specified although DHCP is disabled.");

        // Act
        var result = await new NetworkInterfaceValidator().ValidateAsync(interfaceWithInvalidDHCPConfig);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData("192.168.123.1", true)]
    [InlineData("127.0.0.1", false)]
    public async Task ShouldAddValidationResult_WhenGatewayValidatorCalled(
        string gatewayAddressString,
        bool isValid)
    {
        // Arrange
        var networkInterface = ValidationTestData.Interface.Static with
        {
            IPv4 = ValidationTestData.Interface.Static.IPv4 with
            {
                IPv4Details =
                [
                    new()
                    {
                        IPAddress = IPAddress.Parse("192.168.123.45"),
                        Netmask = IPAddress.Parse("255.255.255.0")
                    }
                ],
                Gateway = IPAddress.Parse(gatewayAddressString)
            }
        };

        // Act
        var result = await new NetworkInterfaceValidator().ValidateAsync(networkInterface);

        // Assert
        result.IsValid.Should().Be(isValid);
        result.EntriesString.Should().NotContain(ValidationConstants.CancelledResult);
    }

    [Fact]
    public async Task ShouldAddValidationError_WhenGatewayIdenticalWithIP()
    {
        // Arrange
        var interfaceWithInvalidGateway = ValidationTestData.Interface.Static with
        {
            IPv4 = ValidationTestData.Interface.Static.IPv4 with
            {
                IPv4Details =
                [
                    new()
                    {
                        IPAddress = IPAddress.Parse("192.168.123.45"),
                        Netmask = IPAddress.Parse("255.255.255.0")
                    }
                ],
                Gateway = IPAddress.Parse("192.168.123.45")
            }
        };

        var expectedValidationEntry = new ValidationEntry(
            $"Gateway of network interface '{interfaceWithInvalidGateway.CommonInformation.Name}' " +
            "can not be identical to the IP address.");

        // Act
        var result = await new NetworkInterfaceValidator().ValidateAsync(interfaceWithInvalidGateway);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldAddValidationError_WhenGatewayIsInDifferentSubnet()
    {
        // Arrange
        var interfaceWithInvalidGateway = ValidationTestData.Interface.Static with
        {
            IPv4 = ValidationTestData.Interface.Static.IPv4 with
            {
                IPv4Details =
                [
                    new()
                    {
                        IPAddress = IPAddress.Parse("192.168.123.45"),
                        Netmask = IPAddress.Parse("255.255.255.0")
                    }
                ],
                Gateway = IPAddress.Parse("192.168.124.1")
            }
        };

        var expectedValidationEntry = new ValidationEntry(
            $"Gateway of network interface '{interfaceWithInvalidGateway.CommonInformation.Name}' " +
            "can not be in a different subnet. Expected '192.168.123.0' but got '192.168.124.0'.");

        // Act
        var result = await new NetworkInterfaceValidator().ValidateAsync(interfaceWithInvalidGateway);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldAddValidationError_WhenGatewayIstSubnetNetworkAddress()
    {
        // Arrange
        var interfaceWithInvalidGateway = ValidationTestData.Interface.Static with
        {
            IPv4 = ValidationTestData.Interface.Static.IPv4 with
            {
                IPv4Details =
                [
                    new()
                    {
                        IPAddress = IPAddress.Parse("192.168.123.45"),
                        Netmask = IPAddress.Parse("255.255.255.0")
                    }
                ],
                Gateway = IPAddress.Parse("192.168.123.0")
            }
        };

        var expectedValidationEntry = new ValidationEntry(
            $"Gateway of network interface '{interfaceWithInvalidGateway.CommonInformation.Name}' " +
            "can not be the network address of the subnet ('192.168.123.0').");

        // Act
        var result = await new NetworkInterfaceValidator().ValidateAsync(interfaceWithInvalidGateway);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldAddValidationError_WhenGatewayIstSubnetBroadcastAddress()
    {
        // Arrange
        var interfaceWithInvalidGateway = ValidationTestData.Interface.Static with
        {
            IPv4 = ValidationTestData.Interface.Static.IPv4 with
            {
                IPv4Details =
                [
                    new()
                    {
                        IPAddress = IPAddress.Parse("192.168.123.45"),
                        Netmask = IPAddress.Parse("255.255.255.0")
                    }
                ],
                Gateway = IPAddress.Parse("192.168.123.255")
            }
        };

        var expectedValidationEntry = new ValidationEntry(
            $"Gateway of network interface '{interfaceWithInvalidGateway.CommonInformation.Name}' " +
            "can not be the broadcast address of the subnet ('192.168.123.255').");

        // Act
        var result = await new NetworkInterfaceValidator().ValidateAsync(interfaceWithInvalidGateway);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldReturnValid_WhenInterfaceValid()
    {
        // Act
        var result = await new NetworkInterfaceValidator()
            .ValidateAsync(ValidationTestData.Interface.Static);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Entries.Count.Should().Be(0);
    }
}
