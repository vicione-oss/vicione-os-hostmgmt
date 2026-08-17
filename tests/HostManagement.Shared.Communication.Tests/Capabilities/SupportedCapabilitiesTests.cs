using AwesomeAssertions;
using HostManagement.Shared.Capabilities;
using HostManagement.Shared.Communication.Capabilities;
using Xunit;

namespace HostManagement.Shared.Communication.Tests.Capabilities;

public sealed class SupportedCapabilitiesTests
{
    [Fact]
    public void ShouldHaveExpectedSupportedSettings_WhenPropertiesOfSystemConfigurationClassAreAnnotated()
    {
        // Act
        var supportedSettings = new SupportedSettings();

        // Assert
        supportedSettings.NetworkInterfaces.GetType().Should().Be<NetworkInterfacesSupport>();
        supportedSettings.NetworkInterfaces.Name.Should().Be("NetworkInterfaces");
        supportedSettings.NetworkInterfaces.Capability.Should().Be(CapabilityStatus.Disabled);

        supportedSettings.NetworkInterfaces.NetworkInterfaceDetail.GetType().Should().Be<NetworkInterfaceDetailSupport>();
        supportedSettings.NetworkInterfaces.NetworkInterfaceDetail.IPv4.GetType().Should().Be<IPv4Support>();
        supportedSettings.NetworkInterfaces.NetworkInterfaceDetail.IPv4.Name.Should().Be("IPv4");
        supportedSettings.NetworkInterfaces.NetworkInterfaceDetail.IPv4.Capability.Should().Be(CapabilityStatus.Disabled);

        supportedSettings.NetworkInterfaces.NetworkInterfaceDetail.VLAN.GetType().Should().Be<VLANSupport>();
        supportedSettings.NetworkInterfaces.NetworkInterfaceDetail.VLAN.Name.Should().Be("VLAN");
        supportedSettings.NetworkInterfaces.NetworkInterfaceDetail.VLAN.Capability.Should().Be(CapabilityStatus.Disabled);

        supportedSettings.DNS.GetType().Should().Be<NetworkDNSSettingsSupport>();
        supportedSettings.DNS.Name.Should().Be("DNS");
        supportedSettings.DNS.Capability.Should().Be(CapabilityStatus.Disabled);

        supportedSettings.DNS.Hostname.GetType().Should().Be<HostnameSupport>();
        supportedSettings.DNS.Hostname.Name.Should().Be("Hostname");
        supportedSettings.DNS.Hostname.Capability.Should().Be(CapabilityStatus.Disabled);

        supportedSettings.DNS.MulticastDNS.GetType().Should().Be<MulticastDNSEnabledSupport>();
        supportedSettings.DNS.MulticastDNS.Name.Should().Be("MulticastDNS");
        supportedSettings.DNS.MulticastDNS.Capability.Should().Be(CapabilityStatus.Disabled);

        supportedSettings.DNS.StaticHosts.GetType().Should().Be<StaticHostsSupport>();
        supportedSettings.DNS.StaticHosts.Capability.Should().Be(CapabilityStatus.Disabled);

        supportedSettings.DNS.NameServers.GetType().Should().Be<NameServersSupport>();
        supportedSettings.DNS.NameServers.Name.Should().Be("NameServers");
        supportedSettings.DNS.NameServers.Capability.Should().Be(CapabilityStatus.Disabled);

        supportedSettings.DNS.PrimaryDNSSuffix.GetType().Should().Be<PrimaryDNSSuffixSupport>();
        supportedSettings.DNS.PrimaryDNSSuffix.Name.Should().Be("PrimaryDNSSuffix");
        supportedSettings.DNS.PrimaryDNSSuffix.Capability.Should().Be(CapabilityStatus.Disabled);

        supportedSettings.DNS.SearchDomains.GetType().Should().Be<SearchDomainsSupport>();
        supportedSettings.DNS.SearchDomains.Name.Should().Be("SearchDomains");
        supportedSettings.DNS.SearchDomains.Capability.Should().Be(CapabilityStatus.Disabled);

        supportedSettings.Proxy.GetType().Should().Be<NetworkProxySettingsSupport>();
        supportedSettings.Proxy.Name.Should().Be("Proxy");
        supportedSettings.Proxy.Capability.Should().Be(CapabilityStatus.Disabled);

        supportedSettings.NTP.GetType().Should().Be<NetworkNTPSettingsSupport>();
        supportedSettings.NTP.Name.Should().Be("NTP");
        supportedSettings.NTP.Capability.Should().Be(CapabilityStatus.Disabled);

        supportedSettings.Services.GetType().Should().Be<ServicesSupport>();
        supportedSettings.Services.Name.Should().Be("Services");
        supportedSettings.Services.Capability.Should().Be(CapabilityStatus.Disabled);
    }

    [Fact]
    public void ShouldHaveExpectedSupportedTopics_WhenFieldsOfTopicsClassAreAnnotated()
    {
        // Act
        var supportedTopics = new SupportedTopics();

        // Assert
        supportedTopics.InstallSignedDebianPackage.Should().Be(CapabilityStatus.Disabled);
        supportedTopics.ResetNetworkInterface.Should().Be(CapabilityStatus.Disabled);
        supportedTopics.RenewDHCPLease.Should().Be(CapabilityStatus.Disabled);
        supportedTopics.StartService.Should().Be(CapabilityStatus.Disabled);
        supportedTopics.StopService.Should().Be(CapabilityStatus.Disabled);
        supportedTopics.RestartService.Should().Be(CapabilityStatus.Disabled);
        supportedTopics.ShutdownSystem.Should().Be(CapabilityStatus.Disabled);
        supportedTopics.RestartSystem.Should().Be(CapabilityStatus.Disabled);
        supportedTopics.ResetSystem.Should().Be(CapabilityStatus.Disabled);
        supportedTopics.UpdateSystem.Should().Be(CapabilityStatus.Disabled);
    }
}
