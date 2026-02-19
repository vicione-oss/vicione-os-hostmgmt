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
    internal static SharedContracts.SystemConfiguration ToSharedContracts(this SystemConfiguration intern)
    {
        SharedContracts.SystemConfiguration systemConfiguration = new(intern.Services?.Select(static s => s.ToSharedContracts()).ToList())
        {
            Version = intern.Version,
        };
        if (intern.NetworkInterfacesSettings is not null)
            systemConfiguration.NetworkInterfacesSettings = intern.NetworkInterfacesSettings.ToSharedContracts();
        if (intern.NetworkDNSSettings is not null)
            systemConfiguration.NetworkDNSSettings = intern.NetworkDNSSettings.ToSharedContracts();
        if (intern.NetworkProxySettings is not null)
            systemConfiguration.NetworkProxySettings = intern.NetworkProxySettings.ToSharedContracts();
        if (intern.NetworkNTPSettings is not null)
            systemConfiguration.NetworkNTPSettings = intern.NetworkNTPSettings.ToSharedContracts();
        return systemConfiguration;
    }

    private static SharedService.ServiceDetail ToSharedContracts(this ServiceDetail intern)
        => new()
        {
            Name = intern.Name,
            State = (Shared.Enums.ServiceState)intern.State,
        };

    private static SharedNetwork.NetworkInterfacesSettings ToSharedContracts(this NetworkInterfacesSettings intern)
        => new(intern.NetworkInterfaces?.Select(static ni => ni.ToSharedContracts()).ToList());

    private static SharedNetwork.NetworkInterfaceDetail ToSharedContracts(this NetworkInterfaceDetail intern)
    {
        return new()
        {
            CommonInformation = (intern.CommonInformation ?? new() { Name = string.Empty, Enabled = false, }).ToSharedContracts(),
            IPv4 = intern.IPv4?.ToSharedContracts() ?? new(),
        };
    }

    private static SharedNetwork.NetworkInterfaceCommonInformation ToSharedContracts(this NetworkInterfaceCommonInformation intern)
    {
        return new()
        {
            Enabled = intern.Enabled ?? false,
            Name = intern.Name,
            PhysicalAddress = intern.PhysicalAddress ?? string.Empty,
        };
    }

    private static SharedNetwork.IPv4Settings ToSharedContracts(this IPv4Settings intern)
    {
        return new(intern.IPv4Details?.Select(d => d.ToSharedContracts()).ToList())
        {
            Gateway = intern.Gateway,
            DHCPEnabled = intern.DHCPEnabled ?? false,
            DHCPLease = intern.DHCPLease?.ToSharedContracts(),
            VLANEnabled = intern.VLANEnabled ?? false,
            VLANID = intern.VLANID ?? 0,
        };
    }

    private static SharedNetwork.IPv4Detail ToSharedContracts(this IPv4Detail intern)
        => new()
        {
            IPAddress = intern.IPAddress,
            Netmask = intern.Netmask,
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

    private static SharedNetwork.NetworkDNSSettings ToSharedContracts(this NetworkDNSSettings intern)
        => new(
            intern.StaticHosts?.Select(sh => sh.ToSharedContracts()).ToList(),
            intern.NameServers?.ToList(),
            intern.SearchDomains?.ToList())
        {
            Hostname = intern.Hostname ?? string.Empty,
            MulticastDNSEnabled = intern.MulticastDNSEnabled ?? false,
            StaticHostsEnabled = intern.StaticHostsEnabled ?? false,
            NameServersEnabled = intern.NameServersEnabled ?? false,
            DNSSuffixEnabled = intern.DNSSuffixEnabled ?? false,
            DNSSuffix = intern.DNSSuffix ?? string.Empty,
            SearchDomainsEnabled = intern.SearchDomainsEnabled ?? false,
        };

    private static SharedNetwork.StaticHostDetail ToSharedContracts(this StaticHostDetail intern)
        => new()
        {
            Hostname = intern.Hostname,
            IPAddress = intern.IPAddress,
        };

    private static SharedNetwork.NetworkProxySettings ToSharedContracts(this NetworkProxySettings intern)
    {
        SharedNetwork.NetworkProxySettings networkProxySettings = new(intern.DoNotProxyList?.ToList())
        {
            DoNotProxyListEnabled = intern.DoNotProxyListEnabled ?? false,
        };
        if (intern.FTP is not null)
            networkProxySettings.FTP = intern.FTP.ToSharedContracts();
        if (intern.HTTP is not null)
            networkProxySettings.HTTP = intern.HTTP.ToSharedContracts();
        if (intern.HTTPS is not null)
            networkProxySettings.HTTPS = intern.HTTPS.ToSharedContracts();
        if (intern.SFTP is not null)
            networkProxySettings.SFTP = intern.SFTP.ToSharedContracts();
        if (intern.SOCKS is not null)
            networkProxySettings.SOCKS = intern.SOCKS.ToSharedContracts();
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

    private static SharedNetwork.NetworkNTPSettings ToSharedContracts(this NetworkNTPSettings intern)
        => new(
            intern.FallbackNTPServers?.ToList(),
            intern.NTPServers?.ToList())
        {
            NTPServersEnabled = intern.NTPServersEnabled ?? false,
        };

    internal static SystemConfiguration ToInternalContracts(this SharedContracts.SystemConfiguration shared)
        => new()
        {
            NetworkInterfacesSettings = shared.NetworkInterfacesSettings.ToInternalContracts(),
            NetworkDNSSettings = shared.NetworkDNSSettings.ToInternalContracts(),
            NetworkProxySettings = shared.NetworkProxySettings.ToInternalContracts(),
            NetworkNTPSettings = shared.NetworkNTPSettings.ToInternalContracts(),
            Services = [.. shared.Services.Select(s => s.ToInternalContracts())],
            Version = shared.Version,
        };

    private static NetworkInterfacesSettings ToInternalContracts(this SharedNetwork.NetworkInterfacesSettings shared)
        => new()
        {
            NetworkInterfaces = [.. shared.NetworkInterfaces.Select(ni => ni.ToInternalContracts())],
        };

    private static NetworkInterfaceDetail ToInternalContracts(this SharedNetwork.NetworkInterfaceDetail shared)
        => new()
        {
            CommonInformation = shared.CommonInformation.ToInternalContracts(),
            IPv4 = shared.IPv4.ToInternalContracts(),
        };

    private static NetworkInterfaceCommonInformation ToInternalContracts(this SharedNetwork.NetworkInterfaceCommonInformation shared)
        => new()
        {
            Enabled = shared.Enabled,
            Name = shared.Name,
            PhysicalAddress = shared.PhysicalAddress,
        };

    private static IPv4Settings ToInternalContracts(this SharedNetwork.IPv4Settings shared)
        => new()
        {
            Gateway = shared.Gateway,
            DHCPEnabled = shared.DHCPEnabled,
            DHCPLease = shared.DHCPLease?.ToInternalContracts(),
            VLANEnabled = shared.VLANEnabled,
            VLANID = shared.VLANID,
            IPv4Details = [.. shared.IPv4Details.Select(d => d.ToInternalContracts())],
        };

    private static DHCPLease ToInternalContracts(this SharedNetwork.DHCPLease shared)
        => new()
        {
            LeaseObtained = shared.LeaseObtained,
            LeaseExpires = shared.LeaseExpires,
            Gateway = shared.Gateway,
            NetworkDNSSettings = shared.NetworkDNSSettings?.ToInternalContracts(),
            IPv4Detail = shared.IPv4Detail?.ToInternalContracts(),
        };

    private static NetworkDNSSettings ToInternalContracts(this SharedNetwork.NetworkDNSSettings shared)
        => new()
        {
            Hostname = shared.Hostname,
            MulticastDNSEnabled = shared.MulticastDNSEnabled,
            StaticHostsEnabled = shared.StaticHostsEnabled,
            StaticHosts = [.. shared.StaticHosts.Select(sh => sh.ToInternalContracts())],
            NameServersEnabled = shared.NameServersEnabled,
            NameServers = [.. shared.NameServers],
            DNSSuffixEnabled = shared.DNSSuffixEnabled,
            DNSSuffix = shared.DNSSuffix,
            SearchDomainsEnabled = shared.SearchDomainsEnabled,
            SearchDomains = [.. shared.SearchDomains],
        };

    private static StaticHostDetail ToInternalContracts(this SharedNetwork.StaticHostDetail shared)
        => new()
        {
            Hostname = shared.Hostname,
            IPAddress = shared.IPAddress,
        };

    private static IPv4Detail ToInternalContracts(this SharedNetwork.IPv4Detail shared)
        => new()
        {
            IPAddress = shared.IPAddress,
            Netmask = shared.Netmask
        };

    private static NetworkProxySettings ToInternalContracts(this SharedNetwork.NetworkProxySettings shared)
        => new()
        {
            FTP = shared.FTP.ToInternalContracts(),
            HTTP = shared.HTTP.ToInternalContracts(),
            HTTPS = shared.HTTPS.ToInternalContracts(),
            SFTP = shared.SFTP.ToInternalContracts(),
            SOCKS = shared.SOCKS.ToInternalContracts(),
            DoNotProxyListEnabled = shared.DoNotProxyListEnabled,
            DoNotProxyList = [.. shared.DoNotProxyList],
        };

    private static NetworkProxyDetail? ToInternalContracts(this SharedNetwork.NetworkProxyDetail shared)
    {
        return new()
        {
            Enabled = shared.Enabled,
            Server = shared.Server,
            Port = shared.Port,
            Username = shared.Username,
            Password = shared.Password,
        };
    }

    private static NetworkNTPSettings ToInternalContracts(this SharedNetwork.NetworkNTPSettings shared)
        => new()
        {
            NTPServersEnabled = shared.NTPServersEnabled,
            NTPServers = [.. shared.NTPServers],
            FallbackNTPServers = shared.FallbackNTPServers?.ToList(),
        };

    private static ServiceDetail ToInternalContracts(this SharedService.ServiceDetail shared)
        => new()
        {
            Name = shared.Name,
            State = (ServiceState)shared.State,
        };
}
