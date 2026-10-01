using HostManagement.Shared.Contracts.NetworkStatus;
using HostManagement.SystemConfig.Contracts;

namespace HostManagement;

/// <summary>
/// Represents the script handler.
/// </summary>
internal interface IScriptHandler
{
    /// <summary>
    /// Executes the script to apply the netplan configuration.
    /// </summary>
    /// <returns><see cref="Result"/></returns>
    Task<Result> ExecuteApplyNetplanConfigurationScript();

    /// <summary>
    /// Executes the script to check if a network interface is available.
    /// </summary>
    /// <param name="interfaceName">The interface name.</param>
    /// <returns><see cref="Result"/></returns>
    Task<Result> ExecuteIsNetworkInterfaceAvailableScript(string interfaceName);

    /// <summary>
    /// Executes a script to get the network interfaces depending on the <paramref name="interfaceType"/>.
    /// The default will get physical and virtual interfaces without vlan interfaces.
    /// </summary>
    /// <remarks>
    /// This returns a raw string evaluated via `ip -br link show` command, e.g.:
    /// <para>
    /// <![CDATA[ens33            UP             00:50:56:36:6d:33 <BROADCAST,MULTICAST,UP,LOWER_UP>]]><br/>
    /// <![CDATA[ens36            UP             00:50:56:36:6d:36 <BROADCAST,MULTICAST,UP,LOWER_UP>]]>
    /// </para>
    /// </remarks>
    /// <param name="interfaceType">
    /// The interface type to get.
    /// <list type="bullet">
    /// <item>"" (default) > physical, virtual, no vlan</item>
    /// <item>"all"        > physical, virtual, vlan</item>
    /// <item>"physical"   > physical</item>
    /// <item>"virtual"    > virtual</item>
    /// <item>"vlan"       > vlan</item>
    /// </list>
    /// </param>
    /// <returns><see cref="Result"/> and the string of interfaces.</returns>
    Task<(Result, string)> ExecuteGetNetworkInterfacesScript(string interfaceType = "");

    /// <summary>
    /// Executes the script to get the common information of all network interfaces.
    /// </summary>
    /// <param name="scriptOptions">The script options to execute the script.</param>
    /// <returns><see cref="Result"/> and a list of  <see cref="NetworkInterfaceCommonInformation"/>
    /// of all network interfaces.</returns>
    Task<(Result, List<NetworkInterfaceCommonInformation>)> ExecuteGetNetworkInterfacesCommonInformationScript(
        ScriptOptions scriptOptions);

    /// <summary>
    /// Executes the script to get the network status information.
    /// </summary>
    /// <param name="scriptOptions">The script options to execute the script.</param>
    /// <returns><see cref="Result"/> and <see cref="NetworkStatusInformation"/>.</returns>
    Task<(Result, NetworkStatusInformation)> ExecuteGetNetworkStatusInformationScript(ScriptOptions scriptOptions);

    /// <summary>
    /// Executes the script to get the original physical address of a network interface.
    /// </summary>
    /// <param name="interfaceName">The interface name.</param>
    /// <returns><see cref="Result"/> and the original physical address of the network interface.</returns>
    Task<(Result, string)> ExecuteGetOriginalPhysicalAddressScript(string interfaceName);

    /// <summary>
    /// Executes the script to get settings of a network interface.
    /// </summary>
    /// <param name="interfaceName">The interface name.</param>
    /// <returns><see cref="Result"/> and <see cref="IPv4Settings"/> of the network interface.</returns>
    Task<(Result, IPv4Settings, VLANSettings)> ExecuteGetInterfaceSettingsScript(string interfaceName);

    /// <summary>
    /// Executes the script to set settings of a network interface.
    /// </summary>
    /// <param name="scriptOptions">The script options to execute the script.</param>
    /// <returns><see cref="Result"/></returns>
    Task<Result> ExecuteSetInterfaceSettingsScript(ScriptOptions scriptOptions);

    /// <summary>
    /// Executes the script to get IPv4 DHCP lease information of a network interface.
    /// </summary>
    /// <param name="interfaceName">The interface name.</param>
    /// <returns><see cref="Result"/> and <see cref="DHCPLease"/> of the network interface.</returns>
    Task<(Result, DHCPLease)> ExecuteGetInterfaceIPv4DHCPLeaseScript(string interfaceName);

    /// <summary>
    /// Executes the script to renew the DHCP lease of a network interface.
    /// </summary>
    /// <param name="interfaceName">The interface name.</param>
    /// <returns><see cref="Result"/></returns>
    Task<Result> ExecuteRenewDHCPLeaseScript(string interfaceName);

    /// <summary>
    /// Executes the script to get DNS settings.
    /// </summary>
    /// <returns><see cref="Result"/> and <see cref="NetworkDNSSettings"/>.</returns>
    Task<(Result, NetworkDNSSettings)> ExecuteGetNetworkDNSSettingsScript();

    /// <summary>
    /// Executes the script to set static hosts.
    /// </summary>
    /// <param name="scriptOptions">The script options to execute the script.</param>
    /// <returns><see cref="Result"/></returns>
    Task<Result> ExecuteSetStaticHostsScript(ScriptOptions scriptOptions);

    /// <summary>
    /// Executes the script to set DNS settings.
    /// </summary>
    /// <param name="scriptOptions">The script options to execute the script.</param>
    /// <returns><see cref="Result"/></returns>
    Task<Result> ExecuteSetNetworkDNSSettingsScript(ScriptOptions scriptOptions);

    /// <summary>
    /// Executes the script to set multicast DNS settings.
    /// </summary>
    /// <param name="scriptOptions">The script options to execute the script.</param>
    /// <returns><see cref="Result"/></returns>
    Task<Result> ExecuteSetNetworkMulticastDNSSettingsScript(ScriptOptions scriptOptions);

    /// <summary>
    /// Executes the script to set the hostname.
    /// </summary>
    /// <param name="hostname">The hostname.</param>
    /// <returns><see cref="Result"/></returns>
    Task<Result> ExecuteSetHostnameScript(string hostname);

    /// <summary>
    /// Executes the script to get NTP fallback information.
    /// </summary>
    /// <returns><see cref="Result"/> and a list of fallback NTP servers.</returns>
    Task<(Result, IEnumerable<string>)> ExecuteGetNetworkNTPFallbackInformationScript();

    /// <summary>
    /// Executes the script to get the NTP synchronization status.
    /// </summary>
    /// <returns><see cref="Result"/> and the NTP synchronization status.</returns>
    Task<(Result, NTPSyncStatus)> ExecuteGetNTPSyncStatusScript();

    /// <summary>
    /// Executes the script to get NTP settings.
    /// </summary>
    /// <returns><see cref="Result"/> and <see cref="NetworkNTPSettings"/>.</returns>
    Task<(Result, NetworkNTPSettings)> ExecuteGetNetworkNTPSettingsScript();

    /// <summary>
    /// Executes the script to set NTP settings.
    /// </summary>
    /// <param name="scriptOptions">The script options to execute the script.</param>
    /// <returns><see cref="Result"/></returns>
    Task<Result> ExecuteSetNetworkNTPSettingsScript(ScriptOptions scriptOptions);

    /// <summary>
    /// Executes the script to get proxy settings.
    /// </summary>
    /// <returns><see cref="Result"/> and <see cref="NetworkProxySettings"/>.</returns>
    Task<(Result, NetworkProxySettings)> ExecuteGetNetworkProxySettingsScript();

    /// <summary>
    /// Executes the script to set proxy settings.
    /// </summary>
    /// <param name="scriptOptions">The script options to execute the script.</param>
    /// <returns><see cref="Result"/></returns>
    Task<Result> ExecuteSetNetworkProxySettingsScript(ScriptOptions scriptOptions);

    /// <summary>
    /// Executes the script to get syslog settings.
    /// </summary>
    /// <returns><see cref="Result"/> and <see cref="SyslogSettings"/>.</returns>
    Task<(Result, SyslogSettings)> ExecuteGetSyslogSettingsScript();

    /// <summary>
    /// Executes the script to set syslog settings.
    /// </summary>
    /// <param name="scriptOptions">The script options to execute the script.</param>
    /// <returns><see cref="Result"/></returns>
    Task<Result> ExecuteSetSyslogSettingsScript(ScriptOptions scriptOptions);

    /// <summary>
    /// Executes the script to get the current active state ("active", "inactive" etc.) of a given Linux systemd unit.
    /// </summary>
    /// <param name="scriptOptions">The script options to execute the script.</param>
    /// <returns><see cref="Result"/> and the active state.</returns>
    Task<(Result, string)> ExecuteGetSystemdUnitActiveStateScript(ScriptOptions scriptOptions);

    /// <summary>
    /// Executes the script to get the current state (enabled or disabled) of a given Linux systemd unit.
    /// </summary>
    /// <param name="scriptOptions">The script options to execute the script.</param>
    /// <returns><see cref="Result"/> and <see cref="ServiceDetail"/>.</returns>
    Task<(Result, ServiceDetail)> ExecuteGetSystemdUnitStateScript(ScriptOptions scriptOptions);

    /// <summary>
    /// Executes the script to enable, disable, start, stop or restart a Linux systemd unit.
    /// </summary>
    /// <param name="scriptOptions">The script options to execute the script.</param>
    /// <returns><see cref="Result"/></returns>
    Task<Result> ExecuteSetSystemdUnitStateScript(ScriptOptions scriptOptions);

    /// <summary>
    /// Executes a script to install a Debian package file.
    /// </summary>
    /// <param name="scriptOptions">The script options to execute the script.</param>
    /// <returns><see cref="Result"/></returns>
    Task<Result> ExecuteInstallDebianPackageScript(ScriptOptions scriptOptions);

    /// <summary>
    /// Executes the script to update the system.
    /// </summary>
    /// <param name="scriptOptions">The script options to execute the script.</param>
    /// <returns><see cref="Result"/></returns>
    Task<Result> ExecuteUpdateSystemScript(ScriptOptions scriptOptions);

    /// <summary>
    /// Executes the script to reset the system.
    /// </summary>
    /// <returns><see cref="Result"/></returns>
    Task<Result> ExecuteResetSystemScript();

    /// <summary>
    /// Executes the script to shutdown or restart the system.
    /// </summary>
    /// <param name="scriptOptions">The script options to execute the script.</param>
    /// <returns><see cref="Result"/></returns>
    Task<Result> ExecuteShutdownSystemScript(ScriptOptions scriptOptions);
}
