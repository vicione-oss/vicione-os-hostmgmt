namespace HostManagement.Scripts.ScriptOptionNames;

/// <summary>
/// Provides all option names for the network NTP script.
/// </summary>
public sealed class NetworkNTPScriptOptionName : ScriptOptionName
{
    private NetworkNTPScriptOptionName(string value) : base(value) { }

    /// <summary>
    /// The script argument for the NTP servers enabled state.
    /// </summary>
    public static NetworkNTPScriptOptionName NTPServersEnabled { get; } = new("--ntpservers-enabled");

    /// <summary>
    /// The script argument for the NTP servers.
    /// </summary>
    public static NetworkNTPScriptOptionName NTPServers { get; } = new("--ntpservers");
}
