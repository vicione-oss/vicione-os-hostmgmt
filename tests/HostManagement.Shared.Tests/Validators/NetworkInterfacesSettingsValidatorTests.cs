using System.Net;
using FluentAssertions;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Validation;
using HostManagement.Shared.Validation.Validators;
using Xunit;

namespace HostManagement.Shared.Tests.Validators;

public class NetworkInterfacesSettingsValidatorTests
{
    private static readonly NetworkInterfaceDetail s_testInterfaceEns33 = new()
    {
        CommonInformation = new()
        {
            Enabled = true,
            Name = "ens33",
            PhysicalAddress = "33-33-33-33-33-33"
        },
        IPv4 = new()
        {
            IPv4Details =
            [
                new()
                {
                    IPAddress = IPAddress.Parse("192.168.123.33"),
                    Netmask = IPAddress.Parse("255.255.255.0")
                }
            ]
        }
    };

    private static readonly NetworkInterfaceDetail s_testInterfaceEns35 = new()
    {
        CommonInformation = new()
        {
            Enabled = true,
            Name = "ens35",
            PhysicalAddress = "35-35-35-35-35-35"
        },
        IPv4 = new()
        {
            IPv4Details =
            [
                new()
                {
                    IPAddress = IPAddress.Parse("192.168.123.35"),
                    Netmask = IPAddress.Parse("255.255.255.0")
                }
            ]
        }
    };

    public class HasNoInterfaces
    {
        [Fact]
        public void ShouldAddValidationError_WhenNoInterfacesFound()
        {
            // Arrange
            var interfaces = new List<NetworkInterfaceDetail>();
            var validationResult = new ValidationResult();

            var expectedValidationEntry = new ValidationEntry(
                "No network interfaces specified.",
                ErrorCodes.ValidationFailed);

            // Act
            NetworkInterfacesSettingsValidator.HasNoInterfaces(interfaces, validationResult);

            // Assert
            validationResult.IsValid.Should().BeFalse();
            validationResult.Entries.Count.Should().Be(1);
            validationResult.Entries.Should().Contain(expectedValidationEntry);
        }

        [Fact]
        public void ShouldHaveNoValidationError_WhenInterfacesSpecified()
        {
            // Arrange
            var interfaces = new List<NetworkInterfaceDetail>()
            {
                s_testInterfaceEns33
            };

            var validationResult = new ValidationResult();

            // Act
            NetworkInterfacesSettingsValidator.HasNoInterfaces(interfaces, validationResult);

            // Assert
            validationResult.IsValid.Should().BeTrue();
            validationResult.Entries.Count.Should().Be(0);
        }
    }

    public class HasLoopbackInterfaces
    {
        [Fact]
        public void ShouldAddValidationError_WhenLoopbackInterfacesSpecified()
        {
            // Arrange
            List<NetworkInterfaceDetail> interfaces =
            [
                s_testInterfaceEns33,
                s_testInterfaceEns35 with
                {
                    CommonInformation = new()
                    {
                        Enabled = true,
                        Name = "lo",
                        PhysicalAddress = "00-00-00-00-00-00"
                    }
                }
            ];

            var validationResult = new ValidationResult();

            var expectedValidationEntry = new ValidationEntry(
                "Loopback interface can not be specified.",
                ErrorCodes.ValidationFailed);

            // Act
            NetworkInterfacesSettingsValidator.HasLoopbackInterfaces(interfaces, validationResult);

            // Assert
            validationResult.IsValid.Should().BeFalse();
            validationResult.Entries.Count.Should().Be(1);
            validationResult.Entries.Should().Contain(expectedValidationEntry);
        }

        [Fact]
        public void ShouldHaveNoValidationError_WhenInterfacesSpecified()
        {
            // Arrange
            List<NetworkInterfaceDetail> interfaces =
            [
                s_testInterfaceEns33,
                s_testInterfaceEns35
            ];

            var validationResult = new ValidationResult();

            // Act
            NetworkInterfacesSettingsValidator.HasLoopbackInterfaces(interfaces, validationResult);

            // Assert
            validationResult.IsValid.Should().BeTrue();
            validationResult.Entries.Count.Should().Be(0);
        }
    }

    public class HasUnknownInterfaces
    {
        [Fact]
        public void ShouldAddValidationError_WhenInterfacUnknown()
        {
            // Arrange
            List<NetworkInterfaceDetail> currentInterfaces =
            [
                s_testInterfaceEns33,
                s_testInterfaceEns35
            ];

            List<NetworkInterfaceDetail> proposedInterfaces =
            [
                s_testInterfaceEns33,
                s_testInterfaceEns35 with
                {
                    CommonInformation = new()
                    {
                        Enabled = true,
                        Name = "ens42",
                        PhysicalAddress = "42-42-42-42-42-42"
                    }
                }
            ];

            var validationResult = new ValidationResult();

            var expectedValidationEntry = new ValidationEntry(
                "Network interface 'ens42' was not found.",
                ErrorCodes.ValidationFailed);

            // Act
            NetworkInterfacesSettingsValidator
                .HasUnknownInterfaces(currentInterfaces, proposedInterfaces, validationResult);

            // Assert
            validationResult.IsValid.Should().BeFalse();
            validationResult.Entries.Count.Should().Be(1);
            validationResult.Entries.Should().Contain(expectedValidationEntry);
        }

        [Fact]
        public void ShouldHaveNoValidationError_WhenInterfacesKnown()
        {
            // Arrange
            List<NetworkInterfaceDetail> interfaces =
            [
                s_testInterfaceEns33,
                s_testInterfaceEns35
            ];
            var validationResult = new ValidationResult();

            // Act
            NetworkInterfacesSettingsValidator
                .HasUnknownInterfaces(interfaces, interfaces, validationResult);

            // Assert
            validationResult.IsValid.Should().BeTrue();
            validationResult.Entries.Count.Should().Be(0);
        }
    }

    public class HasNonUniqueInterfaceNames
    {
        [Fact]
        public void ShouldAddValidationError_WhenNonUniqueInterfaceNamesFound()
        {
            // Arrange
            List<NetworkInterfaceDetail> proposedInterfaces =
            [
                s_testInterfaceEns33,
                s_testInterfaceEns33
            ];

            var validationResult = new ValidationResult();

            var expectedValidationEntry = new ValidationEntry(
                "Network interfaces do not have unique names. Affected names are: 'ens33'.",
                ErrorCodes.ValidationFailed);

            // Act
            NetworkInterfacesSettingsValidator
                .HasNonUniqueInterfaceNames(proposedInterfaces, validationResult);

            // Assert
            validationResult.IsValid.Should().BeFalse();
            validationResult.Entries.Count.Should().Be(1);
            validationResult.Entries.Should().Contain(expectedValidationEntry);
        }

        [Fact]
        public void ShouldHaveNoValidationError_WhenInterfaceNamesValid()
        {
            // Arrange
            List<NetworkInterfaceDetail> proposedInterfaces =
            [
                s_testInterfaceEns33,
                s_testInterfaceEns35
            ];

            var validationResult = new ValidationResult();

            // Act
            NetworkInterfacesSettingsValidator
                .HasNonUniqueInterfaceNames(proposedInterfaces, validationResult);

            // Assert
            validationResult.IsValid.Should().BeTrue();
            validationResult.Entries.Count.Should().Be(0);
        }
    }

    public class HasNonUniqueIPAddresses
    {
        [Fact]
        public void ShouldAddValidationError_WhenNonUniqueIPAddressesFound()
        {
            // Arrange
            List<NetworkInterfaceDetail> proposedInterfaces =
            [
                s_testInterfaceEns33,
                s_testInterfaceEns35 with
                {
                    IPv4 = new()
                    {
                        IPv4Details =
                        [
                            new()
                            {
                                IPAddress = IPAddress.Parse("192.168.123.33"),
                                Netmask = IPAddress.Parse("255.255.255.0")
                            }
                        ]
                    }
                }
            ];

            var validationResult = new ValidationResult();

            var expectedValidationEntry = new ValidationEntry(
                "Network interfaces do not have unique IP addresses. " +
                "Affected IP addresses are: '192.168.123.33'.",
                ErrorCodes.ValidationFailed);

            // Act
            NetworkInterfacesSettingsValidator
                .HasNonUniqueIPAddresses(proposedInterfaces, validationResult);

            // Assert
            validationResult.IsValid.Should().BeFalse();
            validationResult.Entries.Count.Should().Be(1);
            validationResult.Entries.Should().Contain(expectedValidationEntry);
        }

        [Fact]
        public void ShouldHaveNoValidationError_WhenIPAddressesValid()
        {
            // Arrange
            List<NetworkInterfaceDetail> proposedInterfaces =
            [
                s_testInterfaceEns33,
                s_testInterfaceEns35
            ];

            var validationResult = new ValidationResult();

            // Act
            NetworkInterfacesSettingsValidator
                .HasNonUniqueIPAddresses(proposedInterfaces, validationResult);

            // Assert
            validationResult.IsValid.Should().BeTrue();
            validationResult.Entries.Count.Should().Be(0);
        }
    }

    public class ValidateVLANID
    {
        [Theory]
        [InlineData(false, 4095)]
        [InlineData(true, 4095)]
        [InlineData(true, -1)]
        [InlineData(true, 0)]
        public void ShouldAddValidationError_WhenVlanIdInvalid(bool isVlanEnabled, int vlanId)
        {
            // Arrange
            var proposedInterface = s_testInterfaceEns35 with
            {
                IPv4 = new()
                {
                    VLANEnabled = isVlanEnabled,
                    VLANID = vlanId
                }
            };

            var validationResult = new ValidationResult();

            var expectedValidationEntry = new ValidationEntry(
                $"Network interface '{s_testInterfaceEns35.CommonInformation.Name}' has an invalid " +
                $"VLAN ID (expected 1 to 4094). Affected VLAN ID is: {vlanId}.",
                ErrorCodes.ValidationFailed);

            // Act
            NetworkInterfacesSettingsValidator.ValidateVLANID(proposedInterface, validationResult);

            // Assert
            validationResult.IsValid.Should().BeFalse();
            validationResult.Entries.Count.Should().Be(1);
            validationResult.Entries.Should().Contain(expectedValidationEntry);
        }

        [Theory]
        [InlineData(false, 0)]
        [InlineData(false, 1)]
        [InlineData(false, 4094)]
        [InlineData(true, 1)]
        [InlineData(true, 4094)]
        public void ShouldHaveNoValidationError_WhenVlanIdValid(bool isVlanEnabled, int vlanId)
        {
            // Arrange
            var proposedInterface = s_testInterfaceEns35 with
            {
                IPv4 = new()
                {
                    VLANEnabled = isVlanEnabled,
                    VLANID = vlanId
                }
            };

            var validationResult = new ValidationResult();

            // Act
            NetworkInterfacesSettingsValidator.ValidateVLANID(proposedInterface, validationResult);

            // Assert
            validationResult.IsValid.Should().BeTrue();
            validationResult.Entries.Count.Should().Be(0);
        }
    }

    // TODO Add missing unit tests
}
