namespace HostManagement.Scripts.ScriptOptionNames;

/// <summary>
/// Provides all option names for the set service state script.
/// </summary>
public sealed class SetServiceScriptOptionName : ScriptOptionName
{
    private SetServiceScriptOptionName(string value) : base(value) { }

    /// <summary>
    /// The script argument for the service name.
    /// </summary>
    public static SetServiceScriptOptionName Name { get; } = new("--name");

    /// <summary>
    /// The script argument for the service state.
    /// </summary>
    public static SetServiceScriptOptionName State { get; } = new("--state");
}
