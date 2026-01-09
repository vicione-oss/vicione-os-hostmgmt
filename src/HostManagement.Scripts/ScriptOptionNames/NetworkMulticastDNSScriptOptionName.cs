namespace HostManagement.Scripts.ScriptOptionNames;

/// <summary>
/// Provides all option names for the network multicast DNS script.
/// </summary>
public sealed class NetworkMulticastDNSScriptOptionName : ScriptOptionName
{
    private NetworkMulticastDNSScriptOptionName(string value) : base(value) { }

    /// <summary>
    /// The script argument for the multicast DNS (mDNS) enabled.
    /// </summary>
    public static NetworkMulticastDNSScriptOptionName MulticastDNSEnabled { get; } = new("--multicast-dns-enabled");
}
