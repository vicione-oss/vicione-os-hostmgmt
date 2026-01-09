namespace HostManagement.Scripts.ScriptOptionNames;

/// <summary>
/// Provides all option names for the set execute permission script.
/// </summary>
public sealed class SetExecutePermissionScriptOptionName : ScriptOptionName
{
    private SetExecutePermissionScriptOptionName(string value) : base(value) { }

    /// <summary>
    /// The script argument for the permission (chmod).
    /// </summary>
    public static SetExecutePermissionScriptOptionName Permission { get; } = new("--permission");

    /// <summary>
    /// The script argument for the file.
    /// </summary>
    public static SetExecutePermissionScriptOptionName File { get; } = new("--file");
}
