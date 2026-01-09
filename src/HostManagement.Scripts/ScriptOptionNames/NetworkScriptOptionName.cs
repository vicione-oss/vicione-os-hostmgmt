namespace HostManagement.Scripts.ScriptOptionNames;

/// <summary>
/// Provides all option names for the network script.
/// </summary>
public sealed class NetworkScriptOptionName : ScriptOptionName
{
    private NetworkScriptOptionName(string value) : base(value) { }

    /// <summary>
    /// The script argument for the VLAN enabled state.
    /// </summary>
    public static NetworkScriptOptionName VLANEnabled { get; } = new("--vlan-enabled");

    /// <summary>
    /// The script argument for the VLAN ID.
    /// </summary>
    public static NetworkScriptOptionName VLANID { get; } = new("--vlan-id");

    /// <summary>
    /// The script argument for the interface name.
    /// </summary>
    public static NetworkScriptOptionName InterfaceName { get; } = new("--interface-name");

    /// <summary>
    /// The script argument for the MAC address.
    /// </summary>
    public static NetworkScriptOptionName MACAddress { get; } = new("--mac-address");

    /// <summary>
    /// The script argument for the interface enabled state.
    /// </summary>
    public static NetworkScriptOptionName Enabled { get; } = new("--enabled");

    /// <summary>
    /// The script argument for the interface DHCP enabled state.
    /// </summary>
    public static NetworkScriptOptionName DHCPEnabled { get; } = new("--dhcp-enabled");

    /// <summary>
    /// The script argument for an IP address in CIDR notation.
    /// </summary>
    public static NetworkScriptOptionName IPAddress { get; } = new("--ip-address");

    /// <summary>
    /// The script argument for the gateway.
    /// </summary>
    public static NetworkScriptOptionName Gateway { get; } = new("--gateway");
}
