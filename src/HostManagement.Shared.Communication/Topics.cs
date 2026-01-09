namespace HostManagement.Shared.Communication;

/// <summary>
/// Contains the topics used to identify a message handler.
/// </summary>
public static class Topics
{
    /// <summary>
    /// The "GetSystemConfiguration" topic.
    /// </summary>
    public const string GetSystemConfiguration = "GetSystemConfiguration";

    /// <summary>
    /// The "SetSystemConfiguration" topic.
    /// </summary>
    public const string SetSystemConfiguration = "SetSystemConfiguration";

    /// <summary>
    /// The "GetViciOneSuiteVersions" topic.
    /// </summary>
    public const string GetViciOneSuiteVersions = "GetViciOneSuiteVersions";

    /// <summary>
    /// The "InstallViciOneSuiteVersion" topic.
    /// </summary>
    public const string InstallViciOneSuiteVersion = "InstallViciOneSuiteVersion";

    /// <summary>
    /// The "RedeployScripts" topic.
    /// </summary>
    public const string RedeployScripts = "RedeployScripts";

    /// <summary>
    /// The "GetNetworkStatusInformation" topic.
    /// </summary>
    public const string GetNetworkStatusInformation = "GetNetworkStatusInformation";

    /// <summary>
    /// The "RenewDHCPLease" topic.
    /// </summary>
    public const string RenewDHCPLease = "RenewDHCPLease";

    /// <summary>
    /// The "StartService" topic.
    /// </summary>
    public const string StartService = "StartService";

    /// <summary>
    /// The "StopService" topic.
    /// </summary>
    public const string StopService = "StopService";

    /// <summary>
    /// The "RestartService" topic.
    /// </summary>
    public const string RestartService = "RestartService";

    /// <summary>
    /// The "ResetSystem" topic.
    /// </summary>
    public const string ResetSystem = "ResetSystem";

    /// <summary>
    /// The "RestartSystem" topic.
    /// </summary>
    public const string RestartSystem = "RestartSystem";

    /// <summary>
    /// The "echo" topic.
    /// </summary>
    public const string Echo = "echo";
}
