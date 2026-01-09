using HostManagement.Shared.Communication.Enums;

namespace HostManagement.ViciOneSuite;

/// <summary>
/// Represents a ViciOne Suite install script handler.
/// </summary>
internal interface IViciOneSuiteInstallScriptHandler
{
    /// <summary>
    /// Executes a script to get all ViciOne Suite versions.
    /// </summary>
    /// <returns><see cref="OperationStatus"/> and string of ViciOne Suite versions.</returns>
    Task<(OperationStatus, string)> ExecuteGetViciOneSuiteVersionsScript();

    /// <summary>
    /// Executes a script to get the version of the installed ViciOne Suite.
    /// </summary>
    /// <returns><see cref="OperationStatus"/> and string of installed ViciOne Suite version.</returns>
    Task<(OperationStatus, string)> ExecuteGetInstalledSuiteVersionScript();

    /// <summary>
    /// Executes a script to get a ViciOne Suite Debian file.
    /// </summary>
    /// <param name="suiteDebianFile">The ViciOne Suite Debian file.</param>
    /// <returns><see cref="OperationStatus"/></returns>
    Task<OperationStatus> ExecuteGetViciOneSuiteDebFileScript(string suiteDebianFile);

    /// <summary>
    /// Executes a script to install a ViciOne Suite Debian file.
    /// </summary>
    /// <param name="suiteDebianFile">The ViciOne Suite Debian file.</param>
    /// <returns><see cref="OperationStatus"/></returns>
    Task<OperationStatus> ExecuteViciOneSuiteInstallScript(string suiteDebianFile);
}
