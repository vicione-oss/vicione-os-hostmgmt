using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Contracts.NetworkStatus;
using HostManagement.SystemConfig.Contracts;

namespace HostManagement;

/// <summary>
/// Represents the script handler.
/// </summary>
internal interface IScriptHandler
{
    /// <summary>
    /// Use the specified <see cref="Result"/> to express the outcome of operations.
    /// </summary>
    /// <param name="result">The result of the operations.</param>
    void UseResult(Result result);

    /// <summary>
    /// Executes the script to apply the netplan configuration.
    /// </summary>
    /// <returns><see cref="OperationStatus"/></returns>
    Task<OperationStatus> ExecuteApplyNetplanConfigurationScript();

    /// <summary>
    /// Executes the script to check if a network interface is available.
    /// </summary>
    /// <param name="interfaceName">The interface name.</param>
    /// <returns><see cref="OperationStatus"/></returns>
    Task<OperationStatus> ExecuteIsNetworkInterfaceAvailableScript(string interfaceName);

    /// <summary>
    /// Executes a script to get the system architecture (e.g. amd64, arm64).
    /// </summary>
    /// <returns><see cref="OperationStatus"/> and the string of the architecture.</returns>
    Task<(OperationStatus, string)> ExecuteGetArchitectureScript();

    /// <summary>
    /// Executes a script to get the network interfaces depending on <paramref name="interfaceType"/>.
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
    /// <returns><see cref="OperationStatus"/> and the string of interfaces.</returns>
    Task<(OperationStatus, string)> ExecuteGetNetworkInterfacesScript(string interfaceType = "");

    /// <summary>
    /// Executes the script to get the common information of all network interfaces.
    /// </summary>
    /// <returns><see cref="OperationStatus"/> and a list of
    /// <see cref="NetworkInterfaceCommonInformation"/> of all network interfaces.</returns>
    Task<(OperationStatus, List<NetworkInterfaceCommonInformation>)> ExecuteGetNetworkInterfacesCommonInformationScript();

    /// <summary>
    /// Executes the script to get the network status information.
    /// </summary>
    /// <param name="scriptOptions">The script options to execute the script.</param>
    /// <returns><see cref="OperationStatus"/> and <see cref="NetworkStatusInformation"/>.</returns>
    Task<(OperationStatus, NetworkStatusInformation)> ExecuteGetNetworkStatusInformationScript(ScriptOptions scriptOptions);

    /// <summary>
    /// Executes the script to get the original physical address of a network interface.
    /// </summary>
    /// <param name="interfaceName">The interface name.</param>
    /// <returns><see cref="OperationStatus"/> and the original physical address of the network interface.</returns>
    Task<(OperationStatus, string)> ExecuteGetOriginalPhysicalAddressScript(string interfaceName);

    /// <summary>
    /// Executes the script to get IPv4 settings of a network interface.
    /// </summary>
    /// <param name="interfaceName">The interface name.</param>
    /// <returns><see cref="OperationStatus"/> and <see cref="IPv4Settings"/> of the network interface.</returns>
    Task<(OperationStatus, IPv4Settings)> ExecuteGetInterfaceIPv4SettingsScript(string interfaceName);

    /// <summary>
    /// Executes the script to set IPv4 settings of a network interface.
    /// </summary>
    /// <param name="scriptOptions">The script options to execute the script.</param>
    /// <returns><see cref="OperationStatus"/></returns>
    Task<OperationStatus> ExecuteSetInterfaceIPv4SettingsScript(ScriptOptions scriptOptions);

    /// <summary>
    /// Executes the script to get IPv4 DHCP lease information of a network interface.
    /// </summary>
    /// <param name="interfaceName">The interface name.</param>
    /// <returns><see cref="OperationStatus"/> and <see cref="DHCPLease"/> of the network interface.</returns>
    Task<(OperationStatus, DHCPLease)> ExecuteGetInterfaceIPv4DHCPLeaseScript(string interfaceName);

    /// <summary>
    /// Executes the script to renew the DHCP lease of a network interface.
    /// </summary>
    /// <param name="interfaceName">The interface name.</param>
    /// <returns><see cref="OperationStatus"/></returns>
    Task<OperationStatus> ExecuteRenewDHCPLeaseScript(string interfaceName);

    /// <summary>
    /// Executes the script to get DNS settings.
    /// </summary>
    /// <returns><see cref="OperationStatus"/> and <see cref="NetworkDNSSettings"/>.</returns>
    Task<(OperationStatus, NetworkDNSSettings)> ExecuteGetNetworkDNSSettingsScript();

    /// <summary>
    /// Executes the script to set static hosts.
    /// </summary>
    /// <param name="scriptOptions">The script options to execute the script.</param>
    /// <returns><see cref="OperationStatus"/></returns>
    Task<OperationStatus> ExecuteSetStaticHostsScript(ScriptOptions scriptOptions);

    /// <summary>
    /// Executes the script to set DNS settings.
    /// </summary>
    /// <param name="scriptOptions">The script options to execute the script.</param>
    /// <returns><see cref="OperationStatus"/></returns>
    Task<OperationStatus> ExecuteSetNetworkDNSSettingsScript(ScriptOptions scriptOptions);

    /// <summary>
    /// Executes the script to set multicast DNS settings.
    /// </summary>
    /// <param name="scriptOptions">The script options to execute the script.</param>
    /// <returns><see cref="OperationStatus"/></returns>
    Task<OperationStatus> ExecuteSetNetworkMulticastDNSSettingsScript(ScriptOptions scriptOptions);

    /// <summary>
    /// Executes the script to set the hostname.
    /// </summary>
    /// <param name="hostname">The hostname.</param>
    /// <returns><see cref="OperationStatus"/></returns>
    Task<OperationStatus> ExecuteSetHostnameScript(string hostname);

    /// <summary>
    /// Executes the script to get NTP fallback information.
    /// </summary>
    /// <returns><see cref="OperationStatus"/> and a list of fallback NTP servers.</returns>
    Task<(OperationStatus, IEnumerable<string>)> ExecuteGetNetworkNTPFallbackInformationScript();

    /// <summary>
    /// Executes the script to get NTP settings.
    /// </summary>
    /// <returns><see cref="OperationStatus"/> and <see cref="NetworkNTPSettings"/>.</returns>
    Task<(OperationStatus, NetworkNTPSettings)> ExecuteGetNetworkNTPSettingsScript();

    /// <summary>
    /// Executes the script to set NTP settings.
    /// </summary>
    /// <param name="scriptOptions">The script options to execute the script.</param>
    /// <returns><see cref="OperationStatus"/></returns>
    Task<OperationStatus> ExecuteSetNetworkNTPSettingsScript(ScriptOptions scriptOptions);

    /// <summary>
    /// Executes the script to get proxy settings.
    /// </summary>
    /// <returns><see cref="OperationStatus"/> and <see cref="NetworkProxySettings"/>.</returns>
    Task<(OperationStatus, NetworkProxySettings)> ExecuteGetNetworkProxySettingsScript();

    /// <summary>
    /// Executes the script to set proxy settings.
    /// </summary>
    /// <param name="scriptOptions">The script options to execute the script.</param>
    /// <returns><see cref="OperationStatus"/></returns>
    Task<OperationStatus> ExecuteSetNetworkProxySettingsScript(ScriptOptions scriptOptions);

    /// <summary>
    /// Executes the script to get the current active state ("active", "inactive" etc.) of given Linux systemd unit.
    /// </summary>
    /// <param name="scriptOptions">The script options to execute the script.</param>
    /// <returns><see cref="OperationStatus"/> and the active state.</returns>
    Task<(OperationStatus, string)> ExecuteGetSystemdUnitActiveStateScript(ScriptOptions scriptOptions);

    /// <summary>
    /// Executes the script to get the current state (enabled or disabled) of given Linux systemd unit.
    /// </summary>
    /// <param name="scriptOptions">The script options to execute the script.</param>
    /// <returns><see cref="OperationStatus"/> and <see cref="ServiceDetail"/>.</returns>
    Task<(OperationStatus, ServiceDetail)> ExecuteGetSystemdUnitStateScript(ScriptOptions scriptOptions);

    /// <summary>
    /// Executes the script to enable, disable, start, stop or restart a Linux systemd unit.
    /// </summary>
    /// <param name="scriptOptions">The script options to execute the script.</param>
    /// <returns><see cref="OperationStatus"/></returns>
    Task<OperationStatus> ExecuteSetSystemdUnitStateScript(ScriptOptions scriptOptions);

    /// <summary>
    /// Executes a script to install a Debian package file.
    /// </summary>
    /// <param name="scriptOptions">The script options to execute the script.</param>
    /// <returns><see cref="OperationStatus"/></returns>
    Task<OperationStatus> ExecuteInstallDebianPackageScript(ScriptOptions scriptOptions);

    /// <summary>
    /// Executes the script to update the system.
    /// </summary>
    /// <param name="scriptOptions">The script options to execute the script.</param>
    /// <returns><see cref="OperationStatus"/></returns>
    Task<OperationStatus> ExecuteUpdateSystemScript(ScriptOptions scriptOptions);

    /// <summary>
    /// Executes the script to reset the system.
    /// </summary>
    /// <returns><see cref="OperationStatus"/></returns>
    Task<OperationStatus> ExecuteResetSystemScript();

    /// <summary>
    /// Executes the script to shutdown or restart the system.
    /// </summary>
    /// <param name="scriptOptions">The script options to execute the script.</param>
    /// <returns><see cref="OperationStatus"/></returns>
    Task<OperationStatus> ExecuteShutdownSystemScript(ScriptOptions scriptOptions);
}
