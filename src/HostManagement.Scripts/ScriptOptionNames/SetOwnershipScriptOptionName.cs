namespace HostManagement.Scripts.ScriptOptionNames;

/// <summary>
/// Provides all option names for the set ownership script.
/// </summary>
public sealed class SetOwnershipScriptOptionName : ScriptOptionName
{
    private SetOwnershipScriptOptionName(string value) : base(value) { }

    /// <summary>
    /// The script argument for the user.
    /// </summary>
    public static SetOwnershipScriptOptionName User { get; } = new("--user");

    /// <summary>
    /// The script argument for the group.
    /// </summary>
    public static SetOwnershipScriptOptionName Group { get; } = new("--group");

    /// <summary>
    /// The script argument for the file.
    /// </summary>
    public static SetOwnershipScriptOptionName File { get; } = new("--file");
}
