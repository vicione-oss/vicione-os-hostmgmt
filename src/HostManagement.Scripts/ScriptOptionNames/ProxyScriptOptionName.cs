namespace HostManagement.Scripts.ScriptOptionNames;

/// <summary>
/// Provides all option names for the proxy script.
/// </summary>
public sealed class ProxyScriptOptionName : ScriptOptionName
{
    private ProxyScriptOptionName(string value) : base(value) { }

    /// <summary>
    /// The script argument for the proxy enabled state.
    /// </summary>
    public static ProxyScriptOptionName Enabled { get; } = new("--enabled");

    /// <summary>
    /// The script argument for the proxy server URL.
    /// </summary>
    public static ProxyScriptOptionName URL { get; } = new("--url");

    /// <summary>
    /// The script argument for the proxy port.
    /// </summary>
    public static ProxyScriptOptionName Port { get; } = new("--port");

    /// <summary>
    /// The script argument for the proxy username.
    /// </summary>
    public static ProxyScriptOptionName Username { get; } = new("--username");

    /// <summary>
    /// The script argument for the proxy password.
    /// </summary>
    public static ProxyScriptOptionName Password { get; } = new("--password");

    /// <summary>
    /// The script argument for the no proxy list.
    /// </summary>
    public static ProxyScriptOptionName NoProxyList { get; } = new("--no-proxy-list");

    /// <summary>
    /// The script argument for the no proxy list enabled.
    /// </summary>
    public static ProxyScriptOptionName NoProxyListEnabled { get; } = new("--no-proxy-list-enabled");
}
