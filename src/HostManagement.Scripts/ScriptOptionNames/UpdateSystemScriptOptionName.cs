namespace HostManagement.Scripts.ScriptOptionNames;

/// <summary>
/// Provides all option names for the update system script.
/// </summary>
public sealed class UpdateSystemScriptOptionName : ScriptOptionName
{
    private UpdateSystemScriptOptionName(string value) : base(value) { }

    /// <summary>
    /// The script argument for the system file path.
    /// </summary>
    public static UpdateSystemScriptOptionName FilePath { get; } = new("--file");
}
