using CurrentContracts = HostManagement.SystemConfig.Contracts;
using V1Contracts = HostManagement.SystemConfig.Migrations.Contracts.V1;

namespace HostManagement.SystemConfig.Migrations;

/// <summary>
/// Migrates version 1 system configuration contracts to version 2.
/// </summary>
/// <remarks>
/// Version 2 is currently represented by <see cref="CurrentContracts.SystemConfiguration"/>.
/// When a newer current contract version is introduced, version 2 contracts must be copied to
/// <c>HostManagement.SystemConfig.Migrations.Contracts.V2</c>, and this migration must target that
/// dedicated version 2 contract instead of <see cref="CurrentContracts.SystemConfiguration"/>.
/// </remarks>
internal sealed class V1ToV2SystemConfigurationMigration
    : SystemConfigurationMigration<V1Contracts.SystemConfiguration, CurrentContracts.SystemConfiguration>
{
    public override int SourceVersion => 1;

    public override int TargetVersion => 2;

    protected override CurrentContracts.SystemConfiguration Migrate(V1Contracts.SystemConfiguration source)
        => new()
        {
            Version = TargetVersion,
            NetworkInterfacesSettings = source.NetworkInterfacesSettings.MapOrNull(MigrateNetworkInterfacesSettings),
            NetworkDNSSettings = source.NetworkDNSSettings.MapOrNull(MigrateNetworkDNSSettings),
            NetworkProxySettings = source.NetworkProxySettings.MapOrNull(MigrateNetworkProxySettings),
            NetworkNTPSettings = source.NetworkNTPSettings.MapOrNull(MigrateNetworkNTPSettings),
            Services = source.Services.MapListOrNull(MigrateServiceDetail)
        };

    private static CurrentContracts.NetworkInterfacesSettings MigrateNetworkInterfacesSettings(
        V1Contracts.NetworkInterfacesSettings source)
        => new()
        {
            NetworkInterfaces = source.NetworkInterfaces.MapListOrNull(MigrateNetworkInterfaceDetail)
        };

    private static CurrentContracts.NetworkInterfaceDetail MigrateNetworkInterfaceDetail(
        V1Contracts.NetworkInterfaceDetail source)
        => new()
        {
            CommonInformation = source.CommonInformation.MapOrNull(MigrateNetworkInterfaceCommonInformation),
            IPv4 = source.IPv4.MapOrNull(MigrateIPv4Settings)
        };

    private static CurrentContracts.NetworkInterfaceCommonInformation MigrateNetworkInterfaceCommonInformation(
        V1Contracts.NetworkInterfaceCommonInformation source)
        => new()
        {
            Name = source.Name,
            Enabled = source.Enabled,
            PhysicalAddress = source.PhysicalAddress
        };

    private static CurrentContracts.IPv4Settings MigrateIPv4Settings(V1Contracts.IPv4Settings source)
        => new()
        {
            Gateway = source.Gateway,
            DHCPEnabled = source.DHCPEnabled,
            // DHCPLease was removed in version 2 and was not persisted previously either.
            IPv4Details = source.IPv4Details.MapListOrNull(MigrateIPv4Detail),
            VLANEnabled = source.VLANEnabled,
            VLANID = source.VLANID
        };

    private static CurrentContracts.IPv4Detail MigrateIPv4Detail(V1Contracts.IPv4Detail source)
        => new()
        {
            IPAddress = source.IPAddress,
            Netmask = source.Netmask
        };

    private static CurrentContracts.NetworkDNSSettings MigrateNetworkDNSSettings(V1Contracts.NetworkDNSSettings source)
        => new()
        {
            Hostname = source.Hostname,
            MulticastDNSEnabled = source.MulticastDNSEnabled,
            StaticHostsEnabled = source.StaticHostsEnabled,
            StaticHosts = source.StaticHosts.MapListOrNull(MigrateStaticHostDetail),
            NameServersEnabled = source.NameServersEnabled,
            NameServers = source.NameServers.CopyListOrNull(),
            DNSSuffixEnabled = source.DNSSuffixEnabled,
            DNSSuffix = source.DNSSuffix,
            SearchDomainsEnabled = source.SearchDomainsEnabled,
            SearchDomains = source.SearchDomains.CopyListOrNull()
        };

    private static CurrentContracts.StaticHostDetail MigrateStaticHostDetail(V1Contracts.StaticHostDetail source)
        => new()
        {
            IPAddress = source.IPAddress,
            Hostname = source.Hostname
        };

    private static CurrentContracts.NetworkProxySettings MigrateNetworkProxySettings(
        V1Contracts.NetworkProxySettings source)
        => new()
        {
            HTTP = source.HTTP.MapOrNull(MigrateNetworkProxyDetail),
            HTTPS = source.HTTPS.MapOrNull(MigrateNetworkProxyDetail),
            FTP = source.FTP.MapOrNull(MigrateNetworkProxyDetail),
            SFTP = source.SFTP.MapOrNull(MigrateNetworkProxyDetail),
            SOCKS = source.SOCKS.MapOrNull(MigrateNetworkProxyDetail),

            // DoNotProxyListEnabled and DoNotProxyList were grouped into NoProxy.
            NoProxy = source.MapOrNull(MigrateNoProxySettings)
        };

    private static CurrentContracts.NetworkProxyDetail MigrateNetworkProxyDetail(V1Contracts.NetworkProxyDetail source)
        => new()
        {
            Enabled = source.Enabled,
            Server = source.Server,
            Port = source.Port,
            Username = source.Username,
            Password = source.Password
        };

    private static CurrentContracts.NoProxySettings MigrateNoProxySettings(V1Contracts.NetworkProxySettings source)
        => new()
        {
            Enabled = source.DoNotProxyListEnabled,
            Entries = source.DoNotProxyList.CopyListOrNull()
        };

    private static CurrentContracts.NetworkNTPSettings MigrateNetworkNTPSettings(V1Contracts.NetworkNTPSettings source)
        => new()
        {
            // FallbackNTPServers was removed in version 2 and was not persisted previously either.
            Enabled = source.NTPServersEnabled,
            Servers = source.NTPServers.CopyListOrNull()
        };

    private static CurrentContracts.ServiceDetail MigrateServiceDetail(V1Contracts.ServiceDetail source)
        => new()
        {
            Name = source.Name,
            State = MigrateServiceState(source.State),
        };

    private static CurrentContracts.ServiceState MigrateServiceState(V1Contracts.ServiceState source)
        => source switch
        {
            V1Contracts.ServiceState.Unknown => CurrentContracts.ServiceState.Unknown,
            V1Contracts.ServiceState.Enabled => CurrentContracts.ServiceState.Enabled,
            V1Contracts.ServiceState.Disabled => CurrentContracts.ServiceState.Disabled,
            _ => CurrentContracts.ServiceState.Unknown
        };
}
