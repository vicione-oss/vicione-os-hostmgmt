using System.Net;
using AwesomeAssertions;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Validation;
using HostManagement.Shared.Validation.Validators.NetworkInterfaces;
using Xunit;

namespace HostManagement.Shared.Tests.Validation.Validators.NetworkInterfaces;

public class NetworkInterfacesValidatorTests
{
    [Fact]
    public async Task ShouldReturnValidationError_WhenValidationCancelled()
    {
        // Arrange
        var interfaces = new List<NetworkInterfaceDetail>();
        var expectedValidationEntry = new ValidationEntry(ValidationConstants.CancelledResult);

        // Act
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var result = await new NetworkInterfacesValidator().ValidateAsync(interfaces, cancellationToken: cts.Token);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldReturnValidationError_WhenNoInterfacesSpecified()
    {
        // Arrange
        var interfaces = new List<NetworkInterfaceDetail>();
        var expectedValidationEntry = new ValidationEntry("No network interfaces specified.");

        // Act
        var result = await new NetworkInterfacesValidator()
            .ValidateAsync(interfaces, cancellationToken: CancellationToken.None);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldReturnValidationError_WhenDuplicateInterfaceNamesFound()
    {
        // Arrange
        var duplicateName = ValidationTestData.Interface.Static.CommonInformation.Name;
        var interfaces = new List<NetworkInterfaceDetail>
        {
            ValidationTestData.Interface.Static,
            ValidationTestData.Interface.DHCP with
            {
                CommonInformation = ValidationTestData.Interface.DHCP.CommonInformation with
                {
                    Name = duplicateName
                }
            }
        };

        var expectedValidationEntry = new ValidationEntry(
            $"Network interfaces have duplicate names. Affected interfaces are: '{duplicateName}'.");

        // Act
        var result = await new NetworkInterfacesValidator()
            .ValidateAsync(interfaces, cancellationToken: CancellationToken.None);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldReturnValidationError_WhenDuplicateMACAddressesFound()
    {
        // Arrange
        var dhcpInterface = ValidationTestData.Interface.DHCP;
        var staticInterface = ValidationTestData.Interface.Static;

        var duplicateMACAddress = staticInterface.CommonInformation.UserDefinedMACAddress.Address;
        var interfaces = new List<NetworkInterfaceDetail>
        {
            staticInterface,
            dhcpInterface with
            {
                CommonInformation = dhcpInterface.CommonInformation with
                {
                    UserDefinedMACAddress = dhcpInterface.CommonInformation.UserDefinedMACAddress with
                    {
                        Enabled = true,
                        Address = duplicateMACAddress
                    }
                }
            }
        };

        var expectedValidationEntry = new ValidationEntry(
            $"Network interfaces have duplicate MAC addresses. Affected MAC addresses are: '{duplicateMACAddress}'.");

        // Act
        var result = await new NetworkInterfacesValidator()
            .ValidateAsync(interfaces, cancellationToken: CancellationToken.None);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldReturnValidationError_WhenReservedMACAddressesFound()
    {
        // Arrange
        var dhcpInterface = ValidationTestData.Interface.DHCP;
        var staticInterface = ValidationTestData.Interface.Static;

        var reservedOriginalMACAddress = "02-23-45-67-89-AD";
        var originalMACAddresses = new List<string>() { reservedOriginalMACAddress };

        var interfaces = new List<NetworkInterfaceDetail>
        {
            staticInterface,
            dhcpInterface with
            {
                CommonInformation = dhcpInterface.CommonInformation with
                {
                    UserDefinedMACAddress = dhcpInterface.CommonInformation.UserDefinedMACAddress with
                    {
                        Enabled = true,
                        Address = reservedOriginalMACAddress
                    }
                }
            }
        };

        var expectedValidationEntry = new ValidationEntry(
            $"Network interfaces have reserved original MAC addresses defined as user-defined MAC address. Affected MAC addresses are: '{reservedOriginalMACAddress}'.");

        // Act
        var result = await new NetworkInterfacesValidator()
            .ValidateAsync(interfaces, originalMACAddresses, cancellationToken: CancellationToken.None);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldReturnValidationError_WhenMultipleDefaultGatewaysFound()
    {
        // Arrange
        var secondDefaultGateway = IPAddress.Parse("192.168.123.2");
        var interfaces = new List<NetworkInterfaceDetail>
        {
            ValidationTestData.Interface.Static,
            ValidationTestData.Interface.DHCP with
            {
                IPv4 = new()
                {
                    DHCPEnabled = false,
                    Gateway = secondDefaultGateway
                }
            }
        };

        var expectedValidationEntry = new ValidationEntry(
            "Network interfaces can not have multiple default gateways. Affected interfaces are: " +
            $"'{interfaces[0].CommonInformation.Name}' ({interfaces[0].IPv4.Gateway}), " +
            $"'{interfaces[1].CommonInformation.Name}' ({secondDefaultGateway}).");

        // Act
        var result = await new NetworkInterfacesValidator()
            .ValidateAsync(interfaces, cancellationToken: CancellationToken.None);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldReturnValidationError_WhenIPAddressesConflictWithGateway()
    {
        // Arrange
        var conflictingIPAddress = ValidationTestData.Interface.Static.IPv4.Gateway;
        var interfaces = new List<NetworkInterfaceDetail>
        {
            ValidationTestData.Interface.Static with
            {
                IPv4 = ValidationTestData.Interface.Static.IPv4 with
                {
                    IPv4Details =
                    [
                        new()
                        {
                            IPAddress = conflictingIPAddress!,
                            Netmask = IPAddress.Parse("255.255.255.0")
                        }
                    ]
                }
            },
            ValidationTestData.Interface.DHCP
        };

        var expectedValidationEntry = new ValidationEntry(
            $"Network interfaces have conflicting IP addresses with the default gateway '{conflictingIPAddress}'. " +
            $"Affected interfaces are: '{interfaces[0].CommonInformation.Name}'.");

        // Act
        var result = await new NetworkInterfacesValidator()
            .ValidateAsync(interfaces, cancellationToken: CancellationToken.None);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldReturnValidationError_WhenDuplicateIPAddressesFound()
    {
        // Arrange
        var duplicateIPAddress = ValidationTestData.Interface.Static.IPv4.IPv4Details[0].IPAddress;
        var interfaces = new List<NetworkInterfaceDetail>
        {
            ValidationTestData.Interface.Static,
            ValidationTestData.Interface.DHCP with
            {
                IPv4 = ValidationTestData.Interface.DHCP.IPv4 with
                {
                    DHCPEnabled = false,
                    IPv4Details =
                    [
                        new()
                        {
                            IPAddress = duplicateIPAddress,
                            Netmask = IPAddress.Parse("255.255.255.0")
                        }
                    ]
                }
            }
        };

        var expectedValidationEntry = new ValidationEntry(
            "Network interfaces have duplicate IP addresses. " +
            $"Affected IP addresses are: '{duplicateIPAddress}'.");

        // Act
        var result = await new NetworkInterfacesValidator()
            .ValidateAsync(interfaces, cancellationToken: CancellationToken.None);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(42)]
    public async Task ShouldHaveNoValidationError_WhenVLANDisabledAndDuplicateVLANIDsFound(int duplicateId)
    {
        // Arrange
        var interfaces = new List<NetworkInterfaceDetail>
        {
            ValidationTestData.Interface.Static with
            {
                VLAN = new()
                {
                    Enabled = false,
                    ID = duplicateId
                }
            },
            ValidationTestData.Interface.DHCP with
            {
                VLAN = new()
                {
                    Enabled = false,
                    ID = duplicateId
                }
            }
        };

        // Act
        var result = await new NetworkInterfacesValidator()
            .ValidateAsync(interfaces, cancellationToken: CancellationToken.None);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Entries.Count.Should().Be(0);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(42)]
    public async Task ShouldReturnValidationError_WhenDuplicateVLANIDsFound(int duplicateId)
    {
        // Arrange
        var interfaces = new List<NetworkInterfaceDetail>
        {
            ValidationTestData.Interface.Static with
            {
                VLAN = new()
                {
                    Enabled = true,
                    ID = duplicateId
                }
            },
            ValidationTestData.Interface.DHCP with
            {
                VLAN = new()
                {
                    Enabled = true,
                    ID = duplicateId
                }
            },
        };

        var expectedValidationEntry = new ValidationEntry(
            $"Network interfaces have duplicate VLAN IDs. Affected interfaces are: " +
            $"'{interfaces[0].CommonInformation.Name}', " +
            $"'{interfaces[1].CommonInformation.Name}'.");

        // Act
        var result = await new NetworkInterfacesValidator()
            .ValidateAsync(interfaces, cancellationToken: CancellationToken.None);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldReturnValid_WhenInterfacesValid()
    {
        // Arrange
        var interfaces = new List<NetworkInterfaceDetail>
        {
            ValidationTestData.Interface.Static,
            ValidationTestData.Interface.DHCP
        };

        // Act
        var result = await new NetworkInterfacesValidator()
            .ValidateAsync(interfaces, cancellationToken: CancellationToken.None);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Entries.Count.Should().Be(0);
    }
}
