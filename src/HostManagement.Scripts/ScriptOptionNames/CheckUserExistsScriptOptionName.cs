namespace HostManagement.Scripts.ScriptOptionNames;

/// <summary>
/// Provides all option names for the check user exists script.
/// </summary>
public sealed class CheckUserExistsScriptOptionName : ScriptOptionName
{
    private CheckUserExistsScriptOptionName(string value) : base(value) { }

    /// <summary>
    /// The script argument for the user.
    /// </summary>
    public static CheckUserExistsScriptOptionName User { get; } = new("--user");
}
