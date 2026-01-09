namespace HostManagement.Scripts.ScriptOptionNames;

/// <summary>
/// Provides all option names for the network DNS script.
/// </summary>
public sealed class NetworkDNSStaticHostsScriptOptionName : ScriptOptionName
{
    private NetworkDNSStaticHostsScriptOptionName(string value) : base(value) { }

    /// <summary>
    /// The script argument for the IP address.
    /// </summary>
    public static NetworkDNSStaticHostsScriptOptionName IPAddress { get; } = new("--ip-address");

    /// <summary>
    /// The script argument for the hostname.
    /// </summary>
    public static NetworkDNSStaticHostsScriptOptionName Hostname { get; } = new("--hostname");
}
