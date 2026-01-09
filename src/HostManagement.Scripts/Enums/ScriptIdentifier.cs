namespace HostManagement.Scripts.Enums;

/// <summary>
/// Represents a script identifier.
/// </summary>
public enum ScriptIdentifier
{
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
    /// Script to get network interfaces.
    /// </summary>
    NetworkInterfacesGet,

    /// <summary>
    /// Script to get the installed version of the Host Management.
    /// </summary>
    HostManagementInstalledVersionGet,

    /// <summary>
    /// Script to get the installed version of the given package name.
    /// </summary>
    InstalledPackageVersionGet,

    /// <summary>
    /// Script to apply the netplan configuration.
    /// </summary>
    ApplyNetplanConfiguration,

    /// <summary>
    /// Script to get common information of all network interfaces.
    /// </summary>
    NetworkInterfacesCommonInformationGet,

    /// <summary>
    /// Script to get IPv4 DHCP lease information of a network interface.
    /// </summary>
    NetworkInterfaceIPv4DHCPLeaseGet,

    /// <summary>
    /// Script to get IPv4 settings of a network interface.
    /// </summary>
    NetworkInterfaceIPv4SettingsGet,

    /// <summary>
    /// Script to set IPv4 settings of a network interface.
    /// </summary>
    NetworkInterfaceIPv4SettingsSet,

    /// <summary>
    /// Script to get DNS settings.
    /// </summary>
    NetworkDNSSettingsGet,

    /// <summary>
    /// Script to set DNS settings.
    /// </summary>
    NetworkDNSSettingsSet,

    /// <summary>
    /// Script to set Multicast DNS settings.
    /// </summary>
    NetworkMulticastDNSSettingsSet,

    /// <summary>
    /// Script to get NTP settings.
    /// </summary>
    NetworkNTPSettingsGet,

    /// <summary>
    /// Script to set NTP settings.
    /// </summary>
    NetworkNTPSettingsSet,

    /// <summary>
    /// Script to get proxy settings.
    /// </summary>
    NetworkProxySettingsGet,

    /// <summary>
    /// Script to set proxy settings.
    /// </summary>
    NetworkProxySettingsSet,

    /// <summary>
    /// Script to set static hosts.
    /// </summary>
    NetworkStaticHostsSet,

    /// <summary>
    /// Script to get network status information of a network interface.
    /// </summary>
    NetworkStatusInformationGet,

    /// <summary>
    /// Script to renew DHCP lease of a network interface.
    /// </summary>
    RenewDHCPLease,

    /// <summary>
    /// Script to get the system architecture.
    /// </summary>
    CommonSystemArchitectureGet,

    /// <summary>
    /// Script to get the installed version of the ViciOne Suite.
    /// </summary>
    ViciOneSuiteInstalledVersionGet,

    /// <summary>
    /// Script to get the ViciOne deb-File for installation.
    /// </summary>
    ViciOneSuiteDebFileGet,

    /// <summary>
    /// Script to install the ViciOne deb-File.
    /// </summary>
    ViciOneSuiteInstallDebFile,

    /// <summary>
    /// Script to get the ViciOne version list.
    /// </summary>
    ViciOneSuiteVersionsGet,

    /// <summary>
    /// Script to get the current state of a Linux service.
    /// </summary>
    ServiceStateGet,

    /// <summary>
    /// Script to enable, disable or restart a Linux service.
    /// </summary>
    ServiceStateSet,

    /// <summary>
    /// Script to reset the system.
    /// </summary>
    ResetSystem,

    /// <summary>
    /// Script to restart the system.
    /// </summary>
    RestartSystem
}
