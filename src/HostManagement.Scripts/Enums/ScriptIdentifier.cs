namespace HostManagement.Scripts.Enums;

/// <summary>
/// Represents a script identifier.
/// </summary>
public enum ScriptIdentifier
{
    #region System
    /// <summary>
    /// Script to set the execute permission of a file.
    /// </summary>
    SetExecutePermission,

    /// <summary>
    /// Script to check if a user exists.
    /// </summary>
    CheckUserExists,

    /// <summary>
    /// Script to check if a group exists.
    /// </summary>
    CheckGroupExists,

    /// <summary>
    /// Script to set the ownership of a file.
    /// </summary>
    SetOwnership,

    /// <summary>
    /// Script to get the system architecture.
    /// </summary>
    CommonSystemArchitectureGet,

    /// <summary>
    /// Script to install a Debian package.
    /// </summary>
    InstallDebianPackage,

    /// <summary>
    /// Script to reset the system.
    /// </summary>
    ResetSystem,

    /// <summary>
    /// Script to shutdown or restart the system.
    /// </summary>
    ShutdownSystem,

    /// <summary>
    /// Script to update the system.
    /// </summary>
    UpdateSystem,

    /// <summary>
    /// Script to get the current active state ("active", "inactive" etc.) of a Linux systemd unit.
    /// </summary>
    SystemdUnitActiveStateGet,

    /// <summary>
    /// Script to get the current state (enabled or disabled) of a Linux systemd unit.
    /// </summary>
    SystemdUnitStateGet,

    /// <summary>
    /// Script to enable, disable, start, stop or restart a Linux systemd unit.
    /// </summary>
    SystemdUnitStateSet,
    #endregion

    #region Network
    /// <summary>
    /// Script to apply the netplan configuration.
    /// </summary>
    ApplyNetplanConfiguration,

    /// <summary>
    /// Script to check if a network interface is available.
    /// </summary>
    IsNetworkInterfaceAvailable,

    /// <summary>
    /// Script to get network interfaces.
    /// </summary>
    NetworkInterfacesGet,

    /// <summary>
    /// Script to get the original physical address of a network interface.
    /// </summary>
    OriginalPhysicalAddressGet,

    /// <summary>
    /// Script to get common information of all network interfaces.
    /// </summary>
    NetworkInterfacesCommonInformationGet,

    /// <summary>
    /// Script to get IPv4 settings of a network interface.
    /// </summary>
    NetworkInterfaceIPv4SettingsGet,

    /// <summary>
    /// Script to set IPv4 settings of a network interface.
    /// </summary>
    NetworkInterfaceIPv4SettingsSet,

    /// <summary>
    /// Script to get IPv4 DHCP lease information of a network interface.
    /// </summary>
    NetworkInterfaceIPv4DHCPLeaseGet,

    /// <summary>
    /// Script to renew DHCP lease of a network interface.
    /// </summary>
    RenewDHCPLease,

    /// <summary>
    /// Script to get network status information of a network interface.
    /// </summary>
    NetworkStatusInformationGet,
    #endregion

    #region DNS
    /// <summary>
    /// Script to set the hostname.
    /// </summary>
    HostnameSet,

    /// <summary>
    /// Script to set static hosts.
    /// </summary>
    NetworkStaticHostsSet,

    /// <summary>
    /// Script to set Multicast DNS settings.
    /// </summary>
    NetworkMulticastDNSSettingsSet,

    /// <summary>
    /// Script to get DNS settings.
    /// </summary>
    NetworkDNSSettingsGet,

    /// <summary>
    /// Script to set DNS settings.
    /// </summary>
    NetworkDNSSettingsSet,
    #endregion

    #region Proxy
    /// <summary>
    /// Script to get proxy settings.
    /// </summary>
    NetworkProxySettingsGet,

    /// <summary>
    /// Script to set proxy settings.
    /// </summary>
    NetworkProxySettingsSet,
    #endregion

    #region NTP
    /// <summary>
    /// Script to get NTP fallback information.
    /// </summary>
    NetworkNTPFallbackInformationGet,

    /// <summary>
    /// Script to get NTP settings.
    /// </summary>
    NetworkNTPSettingsGet,

    /// <summary>
    /// Script to set NTP settings.
    /// </summary>
    NetworkNTPSettingsSet,
    #endregion

    #region Test
    /// <summary>
    /// For unit testing purposes only.
    /// </summary>
    UnitTesting
    #endregion
}
