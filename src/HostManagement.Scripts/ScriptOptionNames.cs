using System.Diagnostics.CodeAnalysis;

namespace HostManagement.Scripts;

/// <summary>
/// Provides all script option names for shell scripts.
/// </summary>
[SuppressMessage(
    "Design",
    "CA1034:Nested types should not be visible",
    Justification = "Nested classes provide logical grouping of script option names")]
public static class ScriptOptionNames
{
    private const string EnabledFlag = "--enabled";
    private const string FileFlag = "--file";
    private const string InterfacesFlag = "--interfaces";
    private const string InterfaceNameFlag = "--interface-name";
    private const string IpAddressFlag = "--ip-address";
    private const string NameFlag = "--name";

    /// <summary>
    /// Option names for the get network status information script.
    /// </summary>
    public static class GetNetworkStatusInformation
    {
        /// <summary>
        /// The script argument for the interface name whose status is to be determined.
        /// </summary>
        public const string InterfaceName = InterfaceNameFlag;

        /// <summary>
        /// The script argument for the interfaces.
        /// </summary>
        public const string Interfaces = InterfacesFlag;
    }

    /// <summary>
    /// Option names for the get systemd unit active state script.
    /// </summary>
    public static class GetSystemdUnitActiveState
    {
        /// <summary>
        /// The script argument for the systemd unit name whose status is to be determined.
        /// </summary>
        public const string Name = NameFlag;
    }

    /// <summary>
    /// Option names for the get systemd unit state script.
    /// </summary>
    public static class GetSystemdUnitState
    {
        /// <summary>
        /// The script argument for the systemd unit name whose status is to be determined.
        /// </summary>
        public const string Name = "--name";
    }

    /// <summary>
    /// Option names for the install_deb_file script.
    /// </summary>
    public static class InstallDebianPackage
    {
        /// <summary>
        /// The script argument for the Debian package path.
        /// </summary>
        public const string DebianPackage = "--deb-package";
    }

    /// <summary>
    /// Option names for the network script.
    /// </summary>
    public static class Network
    {
        /// <summary>
        /// The script argument for the VLAN enabled state.
        /// </summary>
        public const string VLANEnabled = "--vlan-enabled";

        /// <summary>
        /// The script argument for the VLAN ID.
        /// </summary>
        public const string VLANID = "--vlan-id";

        /// <summary>
        /// The script argument for the interface name.
        /// </summary>
        public const string InterfaceName = InterfaceNameFlag;

        /// <summary>
        /// The script argument for the MAC address.
        /// </summary>
        public const string MACAddress = "--mac-address";

        /// <summary>
        /// The script argument for the interface enabled state.
        /// </summary>
        public const string Enabled = EnabledFlag;

        /// <summary>
        /// The script argument for the interface DHCP enabled state.
        /// </summary>
        public const string DHCPEnabled = "--dhcp-enabled";

        /// <summary>
        /// The script argument for an IP address in CIDR notation.
        /// </summary>
        public const string IPAddress = IpAddressFlag;

        /// <summary>
        /// The script argument for the gateway.
        /// </summary>
        public const string Gateway = "--gateway";

        /// <summary>
        /// The script argument for the interfaces.
        /// </summary>
        public const string Interfaces = InterfacesFlag;
    }

    /// <summary>
    /// Option names for the network DNS script.
    /// </summary>
    public static class NetworkDNS
    {
        /// <summary>
        /// The script argument for the name servers enabled state.
        /// </summary>
        public const string NameServersEnabled = "--nameservers-enabled";

        /// <summary>
        /// The script argument for the name servers.
        /// </summary>
        public const string NameServers = "--nameservers";

        /// <summary>
        /// The script argument for the DNS suffix enabled state.
        /// </summary>
        public const string DNSSuffixEnabled = "--dns-suffix-enabled";

        /// <summary>
        /// The script argument for the DNS suffix.
        /// </summary>
        public const string DNSSuffix = "--dns-suffix";

        /// <summary>
        /// The script argument for the search domains enabled state.
        /// </summary>
        public const string SearchDomainsEnabled = "--search-domains-enabled";

        /// <summary>
        /// The script argument for the search domains.
        /// </summary>
        public const string SearchDomains = "--search-domains";

        /// <summary>
        /// The script argument for the interfaces.
        /// </summary>
        public const string Interfaces = InterfacesFlag;
    }

    /// <summary>
    /// Option names for the network DNS static hosts script.
    /// </summary>
    public static class NetworkDNSStaticHosts
    {
        /// <summary>
        /// The script argument for the enabled state.
        /// </summary>
        public const string Enabled = EnabledFlag;

        /// <summary>
        /// The script argument for the IP address.
        /// </summary>
        public const string IPAddress = IpAddressFlag;

        /// <summary>
        /// The script argument for the hostname.
        /// </summary>
        public const string Hostname = "--hostname";
    }

    /// <summary>
    /// Option names for the network multicast DNS script.
    /// </summary>
    public static class NetworkMulticastDNS
    {
        /// <summary>
        /// The script argument for the multicast DNS (mDNS) enabled.
        /// </summary>
        public const string MulticastDNSEnabled = "--multicast-dns-enabled";

        /// <summary>
        /// The script argument for the interfaces.
        /// </summary>
        public const string Interfaces = InterfacesFlag;
    }

    /// <summary>
    /// Option names for the network NTP script.
    /// </summary>
    public static class NetworkNTP
    {
        /// <summary>
        /// The script argument for the NTP servers enabled state.
        /// </summary>
        public const string NTPServersEnabled = "--ntpservers-enabled";

        /// <summary>
        /// The script argument for the NTP servers.
        /// </summary>
        public const string NTPServers = "--ntpservers";
    }

    /// <summary>
    /// Option names for the proxy script.
    /// </summary>
    public static class Proxy
    {
        /// <summary>
        /// The script argument for the proxy enabled state.
        /// </summary>
        public const string Enabled = EnabledFlag;

        /// <summary>
        /// The script argument for the proxy server URL.
        /// </summary>
        public const string URL = "--url";

        /// <summary>
        /// The script argument for the proxy port.
        /// </summary>
        public const string Port = "--port";

        /// <summary>
        /// The script argument for the proxy username.
        /// </summary>
        public const string Username = "--username";

        /// <summary>
        /// The script argument for the proxy password.
        /// </summary>
        public const string Password = "--password";

        /// <summary>
        /// The script argument for the no proxy list.
        /// </summary>
        public const string NoProxyList = "--no-proxy-list";

        /// <summary>
        /// The script argument for the no proxy list enabled.
        /// </summary>
        public const string NoProxyListEnabled = "--no-proxy-list-enabled";

        /// <summary>
        /// The script argument to clear all proxy settings.
        /// </summary>
        public const string Clear = "--clear";
    }

    /// <summary>
    /// Option names for the set execute permission script.
    /// </summary>
    [SuppressMessage(
        "Naming",
        "CA1711:Identifiers should not have incorrect suffix",
        Justification = "Permission is part of the script's purpose, not a .NET permission type")]
    public static class SetExecutePermission
    {
        /// <summary>
        /// The script argument for the permission (chmod).
        /// </summary>
        public const string Permission = "--permission";

        /// <summary>
        /// The script argument for the file.
        /// </summary>
        public const string File = FileFlag;
    }

    /// <summary>
    /// Option names for the set pipe ownership script.
    /// </summary>
    public static class SetPipeOwnership
    {
        /// <summary>
        /// The script argument for the file.
        /// </summary>
        public const string File = FileFlag;
    }

    /// <summary>
    /// Option names for the set systemd unit state script.
    /// </summary>
    public static class SetSystemdUnitState
    {
        /// <summary>
        /// The script argument for the systemd unit name.
        /// </summary>
        public const string Name = NameFlag;

        /// <summary>
        /// The script argument for the systemd unit state.
        /// </summary>
        public const string State = "--state";
    }

    /// <summary>
    /// Option names for the shutdown system script.
    /// </summary>
    /// <remarks>The script can be used to shutdown or restart the system.</remarks>
    public static class ShutdownSystem
    {
        /// <summary>
        /// The script argument for the shutdown delay in seconds (optional).
        /// </summary>
        public const string Delay = "--delay";

        /// <summary>
        /// The script argument to specify whether to restart the system after shutdown (optional).
        /// </summary>
        public const string Restart = "--restart";
    }

    /// <summary>
    /// Option names for the update system script.
    /// </summary>
    public static class UpdateSystem
    {
        /// <summary>
        /// The script argument for the system file path.
        /// </summary>
        public const string FilePath = FileFlag;
    }
}
