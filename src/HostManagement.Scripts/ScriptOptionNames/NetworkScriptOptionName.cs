namespace HostManagement.Scripts.ScriptOptionNames;

/// <summary>
/// Provides all option names for the network script.
/// </summary>
public sealed class NetworkScriptOptionName : ScriptOptionName
{
    private NetworkScriptOptionName(string value) : base(value) { }

    /// <summary>
    /// The script argument for the VLAN ID.
    /// </summary>
    public static NetworkScriptOptionName VLANID { get; } = new("--vlan-id");

    /// <summary>
    /// The script argument for the interface name.
    /// </summary>
    public static NetworkScriptOptionName InterfaceName { get; } = new("--interface-name");

    /// <summary>
    /// The script argument for the interface enabled state.
    /// </summary>
    public static NetworkScriptOptionName InterfaceEnabled { get; } = new("--enabled");

    /// <summary>
    /// The script argument for the interface DHCP enabled state.
    /// </summary>
    public static NetworkScriptOptionName InterfaceDHCPEnabled { get; } = new("--dhcp-enabled");

    /// <summary>
    /// The script argument for an IP address in CIDR notation.
    /// </summary>
    public static NetworkScriptOptionName InterfaceIPAddress { get; } = new("--ip-address");

    /// <summary>
    /// The script argument for the gateway.
    /// </summary>
    public static NetworkScriptOptionName InterfaceGateway { get; } = new("--gateway");
}
