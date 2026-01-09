namespace HostManagement.Scripts.ScriptOptionNames;

/// <summary>
/// Provides all option names for the install_deb_file script.
/// </summary>
public sealed class InstallViciOneSuiteDebFileScriptOptionName : ScriptOptionName
{
    private InstallViciOneSuiteDebFileScriptOptionName(string value) : base(value) { }

    /// <summary>
    /// The script argument for the ViciOne Suite Debian file (.deb file).
    /// </summary>
    public static InstallViciOneSuiteDebFileScriptOptionName SuiteDebianFile { get; } = new("--deb-file");

    /// <summary>
    /// The script argument for the ViciOne Suite Debian file download directory.
    /// </summary>
    public static InstallViciOneSuiteDebFileScriptOptionName SuiteDebianFileDownloadDirectory { get; } = new("--download-path");

    /// <summary>
    /// The script argument to force reinstall the current ViciOne Suite.
    /// </summary>
    public static InstallViciOneSuiteDebFileScriptOptionName ForceReinstall { get; } = new("--force-reinstall");
}
