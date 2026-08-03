using HostManagement.SystemConfig.Contracts;
using SharedContracts = HostManagement.Shared.Contracts;
using SharedNetwork = HostManagement.Shared.Contracts.Network;
using SharedService = HostManagement.Shared.Contracts.Service;

namespace HostManagement.SystemConfig;

/// <summary>
/// Provides bidirectional mapping between internal DTOs and public DTOs (API layer).
/// </summary>
/// <remarks>
/// <para>
/// This translator implements a two-layer DTO architecture:
/// </para>
/// <list type="bullet">
/// <item><description><strong>Internal DTOs</strong>
/// use nullable reference types to enable partial application and to represent optional fields from script outputs.</description></item>
/// <item><description><strong>Public DTOs</strong>
/// use non-nullable reference types with required initialization, providing a cleaner contract for external consumers.</description></item>
/// </list>
/// </remarks>
internal static class ContractsTranslator
{
    // ToSharedContracts

    internal static SharedContracts.SystemConfiguration ToSharedContracts(this SystemConfiguration intern)
    {
        var systemConfiguration = new SharedContracts.SystemConfiguration { Version = intern.Version };

        if (intern.NetworkInterfaces is { } interfaces)
        {
            systemConfiguration.NetworkInterfaces.AddRange(
                interfaces.Select(static @interface => @interface.ToSharedContracts()));
        }

        if (intern.NetworkDNSSettings is { } dnsSettings)
            systemConfiguration.NetworkDNSSettings = dnsSettings.ToSharedContracts();

        if (intern.NetworkProxySettings is { } proxySettings)
            systemConfiguration.NetworkProxySettings = proxySettings.ToSharedContracts();

        if (intern.NetworkNTPSettings is { } ntpSettings)
            systemConfiguration.NetworkNTPSettings = ntpSettings.ToSharedContracts();

        if (intern.Services is { } services)
        {
            systemConfiguration.Services.AddRange(
                services.Select(static service => service.ToSharedContracts()));
        }

        return systemConfiguration;
    }

    private static SharedNetwork.NetworkInterfaceDetail ToSharedContracts(this NetworkInterfaceDetail intern)
    {
        var internalCommonInformation = intern.CommonInformation ?? new() { Enabled = false, Name = string.Empty };

        return new()
        {
            CommonInformation = internalCommonInformation.ToSharedContracts(),
            IPv4 = intern.IPv4?.ToSharedContracts() ?? new(),
            VLAN = intern.VLAN?.ToSharedContracts() ?? new()
        };
    }

    private static SharedNetwork.NetworkInterfaceCommonInformation ToSharedContracts(
        this NetworkInterfaceCommonInformation intern)
    {
        var commonInformation = new SharedNetwork.NetworkInterfaceCommonInformation()
        {
            Enabled = intern.Enabled ?? false,
            Name = intern.Name
        };

        if (intern.PhysicalAddress is { } physicalAddress)
            commonInformation.PhysicalAddress = physicalAddress;

        return commonInformation;
    }

    private static SharedNetwork.IPv4Settings ToSharedContracts(this IPv4Settings intern)
    {
        var ipv4Settings = new SharedNetwork.IPv4Settings
        {
            Gateway = intern.Gateway
        };

        if (intern.DHCPEnabled is { } dhcpEnabled)
            ipv4Settings.DHCPEnabled = dhcpEnabled;

        if (intern.IPv4Details is { } ipv4Details)
        {
            ipv4Settings = ipv4Settings with
            {
                IPv4Details = [.. ipv4Details.Select(static detail => detail.ToSharedContracts())]
            };
        }

        return ipv4Settings;
    }

    private static SharedNetwork.IPv4Detail ToSharedContracts(this IPv4Detail intern)
        => new()
        {
            IPAddress = intern.IPAddress,
            Netmask = intern.Netmask,
        };

    private static SharedNetwork.VLANSettings ToSharedContracts(this VLANSettings intern)
    {
        var vlanSettings = new SharedNetwork.VLANSettings();

        if (intern.Enabled is { } enabled)
            vlanSettings.Enabled = enabled;

        if (intern.ID is { } id)
            vlanSettings.ID = id;

        return vlanSettings;
    }

    private static SharedNetwork.NetworkDNSSettings ToSharedContracts(this NetworkDNSSettings intern)
    {
        var networkDnsSettings = new SharedNetwork.NetworkDNSSettings();

        if (intern.Hostname is { } hostname)
            networkDnsSettings.Hostname = hostname;

        if (intern.MulticastDNSEnabled is { } multicastEnabled)
            networkDnsSettings.MulticastDNSEnabled = multicastEnabled;

        if (intern.StaticHosts is { } staticHosts)
            networkDnsSettings.StaticHosts = staticHosts.ToSharedContracts();

        if (intern.NameServers is { } nameServers)
            networkDnsSettings.NameServers = nameServers.ToSharedContracts();

        if (intern.PrimaryDNSSuffix is { } primaryDnsSuffix)
            networkDnsSettings.PrimaryDNSSuffix = primaryDnsSuffix.ToSharedContracts();

        if (intern.SearchDomains is { } searchDomains)
            networkDnsSettings.SearchDomains = searchDomains.ToSharedContracts();

        return networkDnsSettings;
    }

    private static SharedNetwork.StaticHostsSettings ToSharedContracts(this StaticHostsSettings intern)
    {
        var staticHostsSettings = new SharedNetwork.StaticHostsSettings();

        if (intern.Enabled is { } enabled)
            staticHostsSettings.Enabled = enabled;

        if (intern.Hosts is { } hosts)
        {
            staticHostsSettings = staticHostsSettings with
            {
                Hosts = [.. hosts.Select(static host => host.ToSharedContracts())]
            };
        }

        return staticHostsSettings;
    }

    private static SharedNetwork.StaticHostDetail ToSharedContracts(this StaticHostDetail intern)
        => new()
        {
            Hostname = intern.Hostname,
            IPAddress = intern.IPAddress
        };

    private static SharedNetwork.NameServersSettings ToSharedContracts(this NameServersSettings intern)
    {
        var nameServersSettings = new SharedNetwork.NameServersSettings();

        if (intern.Enabled is { } enabled)
            nameServersSettings.Enabled = enabled;

        if (intern.Addresses is { } addresses)
            nameServersSettings = nameServersSettings with { Addresses = [.. addresses] };

        return nameServersSettings;
    }

    private static SharedNetwork.PrimaryDNSSuffixSettings ToSharedContracts(this PrimaryDNSSuffixSettings intern)
    {
        var primaryDNSSuffixSettings = new SharedNetwork.PrimaryDNSSuffixSettings();

        if (intern.Enabled is { } enabled)
            primaryDNSSuffixSettings.Enabled = enabled;

        if (intern.Suffix is { } suffix)
            primaryDNSSuffixSettings.Suffix = suffix;

        return primaryDNSSuffixSettings;
    }

    private static SharedNetwork.SearchDomainsSettings ToSharedContracts(this SearchDomainsSettings intern)
    {
        var searchDomainsSettings = new SharedNetwork.SearchDomainsSettings();

        if (intern.Enabled is { } enabled)
            searchDomainsSettings.Enabled = enabled;

        if (intern.Domains is { } domains)
            searchDomainsSettings = searchDomainsSettings with { Domains = [.. domains] };

        return searchDomainsSettings;
    }

    private static SharedNetwork.NetworkProxySettings ToSharedContracts(this NetworkProxySettings intern)
    {
        var networkProxySettings = new SharedNetwork.NetworkProxySettings();

        if (intern.FTP is { } ftp)
            networkProxySettings.FTP = ftp.ToSharedContracts();

        if (intern.HTTP is { } http)
            networkProxySettings.HTTP = http.ToSharedContracts();

        if (intern.HTTPS is { } https)
            networkProxySettings.HTTPS = https.ToSharedContracts();

        if (intern.SFTP is { } sftp)
            networkProxySettings.SFTP = sftp.ToSharedContracts();

        if (intern.SOCKS is { } socks)
            networkProxySettings.SOCKS = socks.ToSharedContracts();

        if (intern.NoProxy is { } noProxy)
            networkProxySettings.NoProxy = noProxy.ToSharedContracts();

        return networkProxySettings;
    }

    private static SharedNetwork.NetworkProxyDetail ToSharedContracts(this NetworkProxyDetail intern)
        => new()
        {
            Enabled = intern.Enabled,
            Server = intern.Server,
            Port = intern.Port,
            Username = intern.Username,
            Password = intern.Password,
        };

    private static SharedNetwork.NoProxySettings ToSharedContracts(this NoProxySettings intern)
    {
        var noProxySettings = new SharedNetwork.NoProxySettings();

        if (intern.Enabled is { } enabled)
            noProxySettings.Enabled = enabled;

        if (intern.Entries is { } entries)
            noProxySettings = noProxySettings with { Entries = entries };

        return noProxySettings;
    }

    private static SharedNetwork.NetworkNTPSettings ToSharedContracts(this NetworkNTPSettings intern)
    {
        var networkNTPSettings = new SharedNetwork.NetworkNTPSettings();

        if (intern.Enabled is { } enabled)
            networkNTPSettings.Enabled = enabled;

        if (intern.Servers is { } servers)
            networkNTPSettings = networkNTPSettings with { Servers = servers };

        return networkNTPSettings;
    }

    private static SharedService.ServiceDetail ToSharedContracts(this ServiceDetail intern)
        => new()
        {
            Name = intern.Name,
            State = (Shared.Enums.ServiceState)intern.State,
        };

    internal static SharedNetwork.DHCPLease ToSharedContracts(this DHCPLease intern)
        => new()
        {
            LeaseObtained = intern.LeaseObtained,
            LeaseExpires = intern.LeaseExpires,
            Gateway = intern.Gateway,
            NetworkDNSSettings = intern.NetworkDNSSettings?.ToSharedContracts(),
            IPv4Detail = intern.IPv4Detail?.ToSharedContracts(),
        };

    // ToInternalContracts

    internal static SystemConfiguration ToInternalContracts(this SharedContracts.SystemConfiguration shared)
        => new()
        {
            Version = shared.Version,

            NetworkInterfaces = [.. shared.NetworkInterfaces.Select(
                static @interface => @interface.ToInternalContracts())],

            NetworkDNSSettings = shared.NetworkDNSSettings.ToInternalContracts(),
            NetworkProxySettings = shared.NetworkProxySettings.ToInternalContracts(),
            NetworkNTPSettings = shared.NetworkNTPSettings.ToInternalContracts(),
            Services = [.. shared.Services.Select(static service => service.ToInternalContracts())]
        };

    private static NetworkInterfaceDetail ToInternalContracts(this SharedNetwork.NetworkInterfaceDetail shared)
        => new()
        {
            CommonInformation = shared.CommonInformation.ToInternalContracts(),
            IPv4 = shared.IPv4.ToInternalContracts(),
            VLAN = shared.VLAN.ToInternalContracts()
        };

    private static NetworkInterfaceCommonInformation ToInternalContracts(
        this SharedNetwork.NetworkInterfaceCommonInformation shared)
            => new()
            {
                Enabled = shared.Enabled,
                Name = shared.Name,
                PhysicalAddress = shared.PhysicalAddress
            };

    private static IPv4Settings ToInternalContracts(this SharedNetwork.IPv4Settings shared)
        => new()
        {
            Gateway = shared.Gateway,
            DHCPEnabled = shared.DHCPEnabled,
            IPv4Details = [.. shared.IPv4Details.Select(detail => detail.ToInternalContracts())]
        };

    private static IPv4Detail ToInternalContracts(this SharedNetwork.IPv4Detail shared)
        => new()
        {
            IPAddress = shared.IPAddress,
            Netmask = shared.Netmask
        };

    private static VLANSettings ToInternalContracts(this SharedNetwork.VLANSettings shared)
        => new()
        {
            Enabled = shared.Enabled,
            ID = shared.ID
        };

    private static NetworkDNSSettings ToInternalContracts(this SharedNetwork.NetworkDNSSettings shared)
        => new()
        {
            Hostname = shared.Hostname,
            MulticastDNSEnabled = shared.MulticastDNSEnabled,
            StaticHosts = shared.StaticHosts.ToInternalContracts(),
            NameServers = shared.NameServers.ToInternalContracts(),
            PrimaryDNSSuffix = shared.PrimaryDNSSuffix.ToInternalContracts(),
            SearchDomains = shared.SearchDomains.ToInternalContracts()
        };

    private static StaticHostsSettings ToInternalContracts(this SharedNetwork.StaticHostsSettings shared)
        => new()
        {
            Enabled = shared.Enabled,
            Hosts = [.. shared.Hosts.Select(host => host.ToInternalContracts())]
        };

    private static StaticHostDetail ToInternalContracts(this SharedNetwork.StaticHostDetail shared)
        => new()
        {
            Hostname = shared.Hostname,
            IPAddress = shared.IPAddress
        };

    private static NameServersSettings ToInternalContracts(this SharedNetwork.NameServersSettings shared)
        => new()
        {
            Enabled = shared.Enabled,
            Addresses = [.. shared.Addresses]
        };

    private static PrimaryDNSSuffixSettings ToInternalContracts(this SharedNetwork.PrimaryDNSSuffixSettings shared)
        => new()
        {
            Enabled = shared.Enabled,
            Suffix = shared.Suffix
        };

    private static SearchDomainsSettings ToInternalContracts(this SharedNetwork.SearchDomainsSettings shared)
        => new()
        {
            Enabled = shared.Enabled,
            Domains = [.. shared.Domains]
        };

    private static NetworkProxySettings ToInternalContracts(this SharedNetwork.NetworkProxySettings shared)
        => new()
        {
            FTP = shared.FTP.ToInternalContracts(),
            HTTP = shared.HTTP.ToInternalContracts(),
            HTTPS = shared.HTTPS.ToInternalContracts(),
            SFTP = shared.SFTP.ToInternalContracts(),
            SOCKS = shared.SOCKS.ToInternalContracts(),
            NoProxy = shared.NoProxy.ToInternalContracts()
        };

    private static NetworkProxyDetail? ToInternalContracts(this SharedNetwork.NetworkProxyDetail shared)
    {
        return new()
        {
            Enabled = shared.Enabled,
            Server = shared.Server,
            Port = shared.Port,
            Username = shared.Username,
            Password = shared.Password
        };
    }

    private static NoProxySettings ToInternalContracts(this SharedNetwork.NoProxySettings shared)
        => new()
        {
            Enabled = shared.Enabled,
            Entries = [.. shared.Entries]
        };

    private static NetworkNTPSettings ToInternalContracts(this SharedNetwork.NetworkNTPSettings shared)
        => new()
        {
            Enabled = shared.Enabled,
            Servers = [.. shared.Servers]
        };

    private static ServiceDetail ToInternalContracts(this SharedService.ServiceDetail shared)
        => new()
        {
            Name = shared.Name,
            State = (ServiceState)shared.State
        };
}
