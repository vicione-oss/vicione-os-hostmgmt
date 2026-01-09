namespace HostManagement.Scripts.ScriptOptionNames;

/// <summary>
/// Provides all option names for the check group exists script.
/// </summary>
public sealed class CheckGroupExistsScriptOptionName : ScriptOptionName
{
    private CheckGroupExistsScriptOptionName(string value) : base(value) { }

    /// <summary>
    /// The script argument for the group.
    /// </summary>
    public static CheckGroupExistsScriptOptionName Group { get; } = new("--group");
}
