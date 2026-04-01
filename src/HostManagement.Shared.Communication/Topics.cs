using HostManagement.Shared.Contracts.System;

namespace HostManagement.Shared.Communication;

/// <summary>
/// Contains the topics used to identify a message handler.
/// </summary>
public static partial class Topics
{
    #region Topics to get or set the complete configuration.
    /// <summary>
    /// The topic to get the current system configuration.
    /// </summary>
    public const string GetSystemConfiguration = "GetSystemConfiguration";

    /// <summary>
    /// The topic to set the specified system configuration.
    /// </summary>
    [RequiresContent]
    public const string SetSystemConfiguration = "SetSystemConfiguration";
    #endregion

    #region Topics to retrieve a state. This is not part of the configuration.
    /// <summary>
    /// The topic to get the original physical address (initial factory setting).
    /// </summary>
    /// <remarks>
    /// The name of the network interface must be specified as the message content.
    /// </remarks>
    [RequiresContent]
    public const string GetOriginalPhysicalAddress = "GetOriginalPhysicalAddress";

    /// <summary>
    /// The topic to get network status information.
    /// </summary>
    /// <remarks>
    /// The name of the network interface must be specified as the message content.
    /// </remarks>
    [RequiresContent]
    public const string GetNetworkStatusInformation = "GetNetworkStatusInformation";

    /// <summary>
    /// The topic to get DHCP lease information.
    /// </summary>
    /// <remarks>
    /// The name of the network interface must be specified as the message content.
    /// </remarks>
    [RequiresContent]
    public const string GetDHCPLeaseInformation = "GetDHCPLeaseInformation";

    /// <summary>
    /// The topic to get the NTP fallback servers.
    /// </summary>
    public const string GetNTPFallbackInformation = "GetNTPFallbackInformation";

    /// <summary>
    /// The topic to get the state of a systemd unit, e.g. of type 'service'.
    /// </summary>
    /// <remarks>
    /// The name of the service must be specified as the message content.
    /// </remarks>
    [RequiresContent]
    public const string GetServiceState = "GetServiceState";
    #endregion

    #region Topics to request an action. This is not part of the configuration. The action may result in a new state and/or return data.
    /// <summary>
    /// The topic to install a signed Debian package.
    /// </summary>
    /// <remarks>
    /// The package and signature file paths must be specified with the <see cref="SignedDebianPackage"/>
    /// type as the message content.
    /// </remarks>
    [RequiresContent]
    public const string InstallSignedDebianPackage = "InstallSignedDebianPackage";

    /// <summary>
    /// The topic to redeploy the application scripts.
    /// </summary>
    public const string RedeployScripts = "RedeployScripts";

    /// <summary>
    /// The topic to request a renewal of the DHCP lease.
    /// </summary>
    [RequiresContent]
    public const string RenewDHCPLease = "RenewDHCPLease";

    /// <summary>
    /// The topic to start a systemd unit, e.g. of type 'service'.
    /// </summary>
    /// <remarks>
    /// The name of the service must be specified as the message content.
    /// </remarks>
    [RequiresContent]
    public const string StartService = "StartService";

    /// <summary>
    /// The topic to stop a systemd unit, e.g. of type 'service'.
    /// </summary>
    /// <remarks>
    /// The name of the service must be specified as the message content.
    /// </remarks>
    [RequiresContent]
    public const string StopService = "StopService";

    /// <summary>
    /// The topic to restart a systemd unit, e.g. of type 'service'.
    /// </summary>
    /// <remarks>
    /// The name of the service must be specified as the message content.
    /// </remarks>
    [RequiresContent]
    public const string RestartService = "RestartService";

    /// <summary>
    /// The topic to reset the system.
    /// </summary>
    public const string ResetSystem = "ResetSystem";

    /// <summary>
    /// The topic to restart the system.
    /// </summary>
    /// <remarks>
    /// A delay in seconds can be specified as the message content.
    /// </remarks>
    public const string RestartSystem = "RestartSystem";

    /// <summary>
    /// The topic to shutdown the system.
    /// </summary>
    /// <remarks>
    /// A delay in seconds can be specified as the message content.
    /// </remarks>
    public const string ShutdownSystem = "ShutdownSystem";

    /// <summary>
    /// The topic to update the system.
    /// </summary>
    [RequiresContent]
    public const string UpdateSystem = "UpdateSystem";
    #endregion

    #region Topics for testing. This is not part of the configuration.
    /// <summary>
    /// The "echo" topic.
    /// </summary>
    public const string Echo = "echo";
    #endregion
}
