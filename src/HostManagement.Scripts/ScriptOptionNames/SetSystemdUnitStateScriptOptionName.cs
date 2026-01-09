namespace HostManagement.Scripts.ScriptOptionNames;

/// <summary>
/// Provides all option names for the set systemd unit state script.
/// </summary>
public sealed class SetSystemdUnitStateScriptOptionName : ScriptOptionName
{
    private SetSystemdUnitStateScriptOptionName(string value) : base(value) { }

    /// <summary>
    /// The script argument for the systemd unit name.
    /// </summary>
    public static SetSystemdUnitStateScriptOptionName Name { get; } = new("--name");

    /// <summary>
    /// The script argument for the systemd unit state.
    /// </summary>
    public static SetSystemdUnitStateScriptOptionName State { get; } = new("--state");
}
