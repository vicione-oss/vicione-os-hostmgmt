namespace HostManagement.Scripts.ScriptOptionNames;

/// <summary>
/// Provides all option names for the get systemd unit state script.
/// </summary>
public sealed class GetSystemdUnitStateScriptOptionName : ScriptOptionName
{
    private GetSystemdUnitStateScriptOptionName(string value) : base(value) { }

    /// <summary>
    /// The script argument for the systemd unit name whose status is to be determined.
    /// </summary>
    public static GetSystemdUnitStateScriptOptionName Name { get; } = new("--name");
}
