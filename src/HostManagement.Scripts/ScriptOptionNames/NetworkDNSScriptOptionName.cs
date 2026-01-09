namespace HostManagement.Scripts.ScriptOptionNames;

/// <summary>
/// Provides all option names for the network DNS script.
/// </summary>
public sealed class NetworkDNSScriptOptionName : ScriptOptionName
{
    private NetworkDNSScriptOptionName(string value) : base(value) { }

    /// <summary>
    /// The script argument for the hostname.
    /// </summary>
    public static NetworkDNSScriptOptionName Hostname { get; } = new("--hostname");

    /// <summary>
    /// The script argument for the name servers.
    /// </summary>
    public static NetworkDNSScriptOptionName NameServers { get; } = new("--nameservers");

    /// <summary>
    /// The script argument for the DNS suffix.
    /// </summary>
    public static NetworkDNSScriptOptionName DNSSuffix { get; } = new("--dns-suffix");

    /// <summary>
    /// The script argument for the search domains.
    /// </summary>
    public static NetworkDNSScriptOptionName SearchDomains { get; } = new("--search-domains");
}
