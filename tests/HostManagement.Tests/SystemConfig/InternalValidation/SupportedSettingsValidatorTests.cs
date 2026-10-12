using AwesomeAssertions;
using HostManagement.Shared.Capabilities;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Enums;
using HostManagement.SystemConfig.InternalValidation;
using Xunit;

namespace HostManagement.Tests.SystemConfig.InternalValidation;

public sealed class SupportedSettingsValidatorTests
{
    private const string Hostname = "host";
    private const string ChangedHostname = "changed-host";
    private const string ServiceName = "test.service";

    public sealed class Validate
    {
        [Fact]
        public void ShouldReturnInvalid_WhenIPv4ChangedAndCapabilityDisabled()
        {
            // Arrange
            var current = CreateConfiguration();
            var proposed = CreateConfiguration(dhcpEnabled: false);
            var support = new SupportedSettings();

            // Act
            var result = SupportedSettingsValidator.Validate(current, proposed, support);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Messages.Should()
                .ContainSingle()
                .Which.Should().Be(GetDisabledCapabilityMessage("IPv4"));
        }

        [Fact]
        public void ShouldReturnInvalid_WhenVLANChangedAndCapabilityDisabled()
        {
            // Arrange
            var current = CreateConfiguration();
            var proposed = CreateConfiguration(vlanID: 200);
            var support = new SupportedSettings();

            // Act
            var result = SupportedSettingsValidator.Validate(current, proposed, support);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Messages.Should()
                .ContainSingle()
                .Which.Should().Be(GetDisabledCapabilityMessage("VLAN"));
        }

        [Fact]
        public void ShouldReturnInvalid_WhenHostnameChangedAndCapabilityDisabled()
        {
            // Arrange
            var current = CreateConfiguration();
            var proposed = CreateConfiguration(hostname: ChangedHostname);
            var support = new SupportedSettings();

            // Act
            var result = SupportedSettingsValidator.Validate(current, proposed, support);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Messages.Should()
                .ContainSingle()
                .Which.Should().Be(GetDisabledCapabilityMessage("Hostname"));
        }

        [Fact]
        public void ShouldReturnInvalid_WhenProxyChangedAndCapabilityDisabled()
        {
            // Arrange
            var current = CreateConfiguration();
            var proposed = CreateConfiguration(proxyEnabled: true);
            var support = new SupportedSettings();

            // Act
            var result = SupportedSettingsValidator.Validate(current, proposed, support);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Messages.Should()
                .ContainSingle()
                .Which.Should().Be(GetDisabledCapabilityMessage("Proxy"));
        }

        [Fact]
        public void ShouldReturnInvalid_WhenNTPChangedAndCapabilityDisabled()
        {
            // Arrange
            var current = CreateConfiguration();
            var proposed = CreateConfiguration(ntpEnabled: true);
            var support = new SupportedSettings();

            // Act
            var result = SupportedSettingsValidator.Validate(current, proposed, support);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Messages.Should()
                .ContainSingle()
                .Which.Should().Be(GetDisabledCapabilityMessage("NTP"));
        }

        [Fact]
        public void ShouldReturnInvalid_WhenServicesChangedAndCapabilityDisabled()
        {
            // Arrange
            var current = CreateConfiguration();
            var proposed = CreateConfiguration(serviceState: ServiceState.Disabled);
            var support = new SupportedSettings();

            // Act
            var result = SupportedSettingsValidator.Validate(current, proposed, support);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Messages.Should()
                .ContainSingle()
                .Which.Should().Be(GetDisabledCapabilityMessage("Services"));
        }

        [Fact]
        public void ShouldReturnAllErrors_WhenMultipleDisabledSettingsChanged()
        {
            // Arrange
            var current = CreateConfiguration();
            var proposed = CreateConfiguration(
                dhcpEnabled: false,
                vlanID: 200,
                hostname: ChangedHostname,
                proxyEnabled: true,
                ntpEnabled: true,
                serviceState: ServiceState.Disabled);

            var support = new SupportedSettings();

            // Act
            var result = SupportedSettingsValidator.Validate(current, proposed, support);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Messages.Should().Equal(
                GetDisabledCapabilityMessage("IPv4"),
                GetDisabledCapabilityMessage("VLAN"),
                GetDisabledCapabilityMessage("Hostname"),
                GetDisabledCapabilityMessage("Proxy"),
                GetDisabledCapabilityMessage("NTP"),
                GetDisabledCapabilityMessage("Services"));
        }

        [Fact]
        public void ShouldReturnValid_WhenNetworkInterfacesOnlyReordered()
        {
            // Arrange
            var current = CreateConfiguration();
            var proposed = CreateConfiguration(reverseInterfaces: true);
            var support = new SupportedSettings();

            // Act
            var result = SupportedSettingsValidator.Validate(current, proposed, support);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Messages.Should().BeEmpty();
        }

        [Fact]
        public void ShouldReturnValid_WhenHostnameChangedAndCapabilityEnabled()
        {
            // Arrange
            var current = CreateConfiguration();
            var proposed = CreateConfiguration(hostname: ChangedHostname);

            var support = new SupportedSettings();
            support.DNS.Hostname.Capability = CapabilityStatus.Enabled;

            // Act
            var result = SupportedSettingsValidator.Validate(current, proposed, support);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Messages.Should().BeEmpty();
        }

        [Fact]
        public void ShouldReturnValid_WhenServicesChangedAndCapabilityEnabled()
        {
            // Arrange
            var current = CreateConfiguration();
            var proposed = CreateConfiguration(serviceState: ServiceState.Disabled);

            var support = new SupportedSettings();
            support.Services.Capability = CapabilityStatus.Enabled;

            // Act
            var result = SupportedSettingsValidator.Validate(current, proposed, support);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Messages.Should().BeEmpty();
        }

        [Fact]
        public void ShouldReturnValid_WhenConfigurationUnchanged()
        {
            // Arrange
            var current = CreateConfiguration();
            var proposed = CreateConfiguration();
            var support = new SupportedSettings();

            // Act
            var result = SupportedSettingsValidator.Validate(current, proposed, support);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Messages.Should().BeEmpty();
        }
    }

    private static SystemConfiguration CreateConfiguration(
        bool dhcpEnabled = true,
        int vlanID = 100,
        bool reverseInterfaces = false,
        string hostname = Hostname,
        bool proxyEnabled = false,
        bool ntpEnabled = false,
        ServiceState serviceState = ServiceState.Enabled)
    {
        var lan0 = CreateNetworkInterface("lan0", dhcpEnabled, vlanID);
        var lan1 = CreateNetworkInterface("lan1", dhcpEnabled: true, vlanID: 101);

        List<NetworkInterfaceDetail> networkInterfaces = reverseInterfaces ? [lan1, lan0] : [lan0, lan1];

        return new()
        {
            NetworkInterfaces = networkInterfaces,
            NetworkDNSSettings = new()
            {
                Hostname = hostname
            },
            NetworkProxySettings = new()
            {
                HTTP = new()
                {
                    Enabled = proxyEnabled,
                    Server = "proxy.example.com",
                    Port = 8080
                }
            },
            NetworkNTPSettings = new()
            {
                Enabled = ntpEnabled,
                Servers = ["pool.ntp.org"]
            },
            Services =
            [
                new()
                {
                    Name = ServiceName,
                    State = serviceState
                }
            ]
        };
    }

    private static NetworkInterfaceDetail CreateNetworkInterface(string name, bool dhcpEnabled, int vlanID)
        => new()
        {
            CommonInformation = new()
            {
                Name = name,
                Enabled = true
            },
            IPv4 = new()
            {
                DHCPEnabled = dhcpEnabled
            },
            VLAN = new()
            {
                Enabled = true,
                ID = vlanID
            }
        };

    private static string GetDisabledCapabilityMessage(string setting)
        => $"The setting '{setting}' cannot be changed because its capability is disabled.";
}
