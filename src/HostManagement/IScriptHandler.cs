using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Contracts.Service;

namespace HostManagement;

/// <summary>
/// Represents the script handler.
/// </summary>
internal interface IScriptHandler
{
    /// <summary>
    /// Executes the script to apply the netplan configuration.
    /// </summary>
    /// <returns><see cref="OperationStatus"/></returns>
    Task<OperationStatus> ExecuteApplyNetplanConfigurationScript();

    /// <summary>
    /// Executes the script to get the common information of all network interfaces.
    /// </summary>
    /// <returns><see cref="OperationStatus"/> and a list of
    /// <see cref="NetworkInterfaceCommonInformation"/> of all network interfaces.</returns>
    Task<(OperationStatus, List<NetworkInterfaceCommonInformation>)> ExecuteGetNetworkInterfacesCommonInformationScript();

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
    /// Executes the script to get the current state (enabled or disabled) of given Linux systemd unit.
    /// </summary>
    /// <param name="scriptOptions">The script options to execute the script.</param>
    /// <returns><see cref="OperationStatus"/></returns>
    Task<(OperationStatus, ServiceDetail)> ExecuteGetSystemdUnitStateScript(ScriptOptions scriptOptions);

    /// <summary>
    /// Executes the script to enable, disable, start, stop or restart a Linux systemd unit.
    /// </summary>
    /// <param name="scriptOptions">The script options to execute the script.</param>
    /// <returns><see cref="OperationStatus"/></returns>
    Task<OperationStatus> ExecuteSetSystemdUnitStateScript(ScriptOptions scriptOptions);
}
