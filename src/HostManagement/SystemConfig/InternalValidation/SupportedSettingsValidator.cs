using HostManagement.Shared.Capabilities;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Contracts.Service;
using HostManagement.Shared.Contracts.Syslog;

namespace HostManagement.SystemConfig.InternalValidation;

/// <summary>
/// Validates configuration changes against the supported capabilities. 
/// </summary>
internal static class SupportedSettingsValidator
{
    /// <summary>
    /// Validates whether the proposed configuration changes are supported.
    /// </summary>
    /// <param name="current">The current system configuration.</param>
    /// <param name="proposed">The proposed system configuration.</param>
    /// <param name="support">The available support for the settings.</param>
    /// <returns>The validation result.</returns>
    internal static ValidationResult Validate(
        SystemConfiguration current,
        SystemConfiguration proposed,
        SupportedSettings support)
    {
        var context = new SupportedSettingsValidationContext();

        context.ValidateInterfaces(support.NetworkInterfaces, current.NetworkInterfaces, proposed.NetworkInterfaces);
        context.ValidateDNS(support.DNS, current.NetworkDNSSettings, proposed.NetworkDNSSettings);
        context.ValidateProxy(support.Proxy, current.NetworkProxySettings, proposed.NetworkProxySettings);
        context.ValidateNTP(support.NTP, current.NetworkNTPSettings, proposed.NetworkNTPSettings);
        context.ValidateSyslog(support.Syslog, current.Syslog, proposed.Syslog);
        context.ValidateServices(support.Services, current.Services, proposed.Services);

        return context.Result;
    }

    private sealed class SupportedSettingsValidationContext
    {
        internal ValidationResult Result { get; } = new();

        internal void ValidateInterfaces(
            NetworkInterfacesSupport interfacesSupport,
            IReadOnlyList<NetworkInterfaceDetail> current,
            IReadOnlyList<NetworkInterfaceDetail> proposed)
        {
            if (!SystemConfigurationChangeDetector.HasSequenceChanges(current, proposed))
                return;

            var interfaceDetailSupport = interfacesSupport.NetworkInterfaceDetail;

            var currentIpv4 = current.Select(detail => (detail.CommonInformation.Name, detail.IPv4));
            var proposedIpv4 = proposed.Select(detail => (detail.CommonInformation.Name, detail.IPv4));

            ValidateSequence(interfaceDetailSupport.IPv4, currentIpv4, proposedIpv4, @interface => @interface.Name);

            var currentVlan = current.Select(detail => (detail.CommonInformation.Name, detail.VLAN));
            var proposedVlan = proposed.Select(detail => (detail.CommonInformation.Name, detail.VLAN));

            ValidateSequence(interfaceDetailSupport.VLAN, currentVlan, proposedVlan, @interface => @interface.Name);
        }

        internal void ValidateDNS(
            NetworkDNSSettingsSupport dnsSupport,
            NetworkDNSSettings current,
            NetworkDNSSettings proposed)
        {
            if (!SystemConfigurationChangeDetector.HasChanges(current, proposed))
                return;

            Validate(dnsSupport.Hostname, current.Hostname, proposed.Hostname);
            Validate(dnsSupport.MulticastDNS, current.MulticastDNSEnabled, proposed.MulticastDNSEnabled);
            Validate(dnsSupport.StaticHosts, current.StaticHosts, proposed.StaticHosts);
            Validate(dnsSupport.NameServers, current.NameServers, proposed.NameServers);
            Validate(dnsSupport.PrimaryDNSSuffix, current.PrimaryDNSSuffix, proposed.PrimaryDNSSuffix);
            Validate(dnsSupport.SearchDomains, current.SearchDomains, proposed.SearchDomains);
        }

        internal void ValidateProxy(
            NetworkProxySettingsSupport proxySupport,
            NetworkProxySettings current,
            NetworkProxySettings proposed)
                => Validate(proxySupport, current, proposed);

        internal void ValidateNTP(
            NetworkNTPSettingsSupport ntpSupport,
            NetworkNTPSettings current,
            NetworkNTPSettings proposed)
                => Validate(ntpSupport, current, proposed);

        internal void ValidateSyslog(
            SyslogSupport syslogSupport,
            SyslogSettings current,
            SyslogSettings proposed)
                => Validate(syslogSupport, current, proposed);

        internal void ValidateServices(
            ServicesSupport servicesSupport,
            IReadOnlyList<ServiceDetail> current,
            IReadOnlyList<ServiceDetail> proposed)
                => ValidateSequence(servicesSupport, current, proposed, service => service.Name);

        private void Validate<T>(LeafSettingSupport settingSupport, T? current, T? proposed)
        {
            if (!SystemConfigurationChangeDetector.HasChanges(current, proposed))
                return;

            if (settingSupport.Capability is CapabilityStatus.Disabled)
                Result.AddError(GetErrorMessage(settingSupport));
        }

        private void ValidateSequence<T, TKey>(
            LeafSettingSupport settingSupport,
            IEnumerable<T>? current,
            IEnumerable<T>? proposed,
            Func<T, TKey>? order)
        {
            if (!SystemConfigurationChangeDetector.HasSequenceChanges(current, proposed, order))
                return;

            if (settingSupport.Capability is CapabilityStatus.Disabled)
                Result.AddError(GetErrorMessage(settingSupport));
        }

        private static string GetErrorMessage(LeafSettingSupport settingSupport)
            => $"The setting '{settingSupport.Name}' cannot be changed because its capability is disabled.";
    }
}
