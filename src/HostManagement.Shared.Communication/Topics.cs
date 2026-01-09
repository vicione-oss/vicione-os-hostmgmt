namespace HostManagement.Shared.Communication;

/// <summary>
/// Contains the topics used to identify a message handler.
/// </summary>
public static class Topics
{
    /// <summary>
    /// The "LoadSystemConfiguration" topic.
    /// </summary>
    public const string LoadSystemConfiguration = "LoadSystemConfiguration";

    /// <summary>
    /// The "ApplySystemConfiguration" topic.
    /// </summary>
    public const string ApplySystemConfiguration = "ApplySystemConfiguration";

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
    /// The "echo" topic.
    /// </summary>
    public const string Echo = "echo";
}
