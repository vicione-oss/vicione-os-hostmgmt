namespace HostManagement.Scripts.ScriptOptionNames;

/// <summary>
/// Provides all option names for the get_vicione_suite_versions script.
/// </summary>
public sealed class GetViciOneSuiteVersionsScriptOptionName : ScriptOptionName
{
    private GetViciOneSuiteVersionsScriptOptionName(string value) : base(value) { }

    /// <summary>
    /// The script argument for the ViciOne Suite repository endpoint (URL).
    /// </summary>
    public static GetViciOneSuiteVersionsScriptOptionName SuiteRepositoryEndpoint { get; } = new("--url");

    /// <summary>
    /// The script argument for the ViciOne Suite repository username.
    /// </summary>
    public static GetViciOneSuiteVersionsScriptOptionName SuiteRepositoryUsername { get; } = new("--username");

    /// <summary>
    /// The script argument for the ViciOne Suite repository password.
    /// </summary>
    public static GetViciOneSuiteVersionsScriptOptionName SuiteRepositoryPassword { get; } = new("--password");
}
