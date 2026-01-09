namespace HostManagement.Scripts.ScriptOptionNames;

/// <summary>
/// Provides all option names for the get_vicione_suite_deb_file script.
/// </summary>
public sealed class GetViciOneSuiteDebFileScriptOptionName : ScriptOptionName
{
    private GetViciOneSuiteDebFileScriptOptionName(string value) : base(value) { }

    /// <summary>
    /// The script argument for the ViciOne Suite repository endpoint (URL).
    /// </summary>
    public static GetViciOneSuiteDebFileScriptOptionName SuiteRepositoryEndpoint { get; } = new("--url");

    /// <summary>
    /// The script argument for the ViciOne Suite Debian file (.deb file).
    /// </summary>
    public static GetViciOneSuiteDebFileScriptOptionName SuiteDebianFile { get; } = new("--deb-file");

    /// <summary>
    /// The script argument for the ViciOne Suite repository username.
    /// </summary>
    public static GetViciOneSuiteDebFileScriptOptionName SuiteRepositoryUsername { get; } = new("--username");

    /// <summary>
    /// The script argument for the ViciOne Suite repository password.
    /// </summary>
    public static GetViciOneSuiteDebFileScriptOptionName SuiteRepositoryPassword { get; } = new("--password");

    /// <summary>
    /// The script argument for the ViciOne Suite Debian file download directory.
    /// </summary>
    public static GetViciOneSuiteDebFileScriptOptionName SuiteDebianFileDownloadDirectory { get; } = new("--download-path");
}
