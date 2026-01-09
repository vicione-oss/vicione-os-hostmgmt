namespace HostManagement.Scripts.ScriptOptionNames;

/// <summary>
/// Provides all option names for the network DNS script.
/// </summary>
public sealed class NetworkDNSScriptOptionName : ScriptOptionName
{
    private NetworkDNSScriptOptionName(string value) : base(value) { }

    /// <summary>
    /// The script argument for the name servers enabled state.
    /// </summary>
    public static NetworkDNSScriptOptionName NameServersEnabled { get; } = new("--nameservers-enabled");

    /// <summary>
    /// The script argument for the name servers.
    /// </summary>
    public static NetworkDNSScriptOptionName NameServers { get; } = new("--nameservers");

    /// <summary>
    /// The script argument for the DNS suffix enabled state.
    /// </summary>
    public static NetworkDNSScriptOptionName DNSSuffixEnabled { get; } = new("--dns-suffix-enabled");

    /// <summary>
    /// The script argument for the DNS suffix.
    /// </summary>
    public static NetworkDNSScriptOptionName DNSSuffix { get; } = new("--dns-suffix");

    /// <summary>
    /// The script argument for the search domains enabled state.
    /// </summary>
    public static NetworkDNSScriptOptionName SearchDomainsEnabled { get; } = new("--search-domains-enabled");

    /// <summary>
    /// The script argument for the search domains.
    /// </summary>
    public static NetworkDNSScriptOptionName SearchDomains { get; } = new("--search-domains");
}
