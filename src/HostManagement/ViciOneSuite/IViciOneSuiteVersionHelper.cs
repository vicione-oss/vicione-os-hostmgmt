using HostManagement.Shared.Communication.Enums;

namespace HostManagement.ViciOneSuite;

/// <summary>
/// Represents a ViciOne Suite version helper
/// </summary>
internal interface IViciOneSuiteVersionHelper
{
    /// <summary>
    /// Gets the non-pre-release versions filter.
    /// </summary>
    Func<string, bool> NonPreReleaseVersionsFilter { get; }

    /// <summary>
    /// Gets the version mapping of ViciOne Suite and HostManagement.
    /// </summary>
    /// <param name="versionsCSVString">The CSV formatted versions string.</param>
    /// <returns><see cref="OperationStatus"/> and the version mapping</returns>
    Task<(OperationStatus, Dictionary<string, IList<string>>)> GetVersionMapping(string versionsCSVString);

    /// <summary>
    /// Gets all ViciOne Suite versions that are compatible with the current HostManagement version.
    /// </summary>
    /// <param name="hostManagementVersionString">The HostManagement version.</param>
    /// <param name="installedSuiteVersionString">The installed ViciOne Suite version.</param>
    /// <param name="versionMapping">The version mapping.</param>
    /// <returns><see cref="OperationStatus"/> and the list of compatible Suite versions.</returns>
    Task<(OperationStatus, IEnumerable<string>)> GetCompatibleSuiteVersions(
        string hostManagementVersionString,
        string installedSuiteVersionString,
        Dictionary<string, IList<string>> versionMapping);

    /// <summary>
    /// Filters the given ViciOne Suite versions based on a predicate.
    /// </summary>
    /// <param name="suiteVersions">The ViciOne Suite versions to filter.</param>
    /// <param name="predicate">The predicate.</param>
    /// <returns>Filtered ViciOne Suite versions</returns>
    Task<(OperationStatus, IEnumerable<string>)> GetFilteredSuiteVersions(
        IEnumerable<string> suiteVersions,
        Func<string, bool> predicate);

    /// <summary>
    /// Checks whether the given ViciOne Suite version is contained in the given version list.
    /// </summary>
    /// <param name="hostManagementVersionString">The HostManagement version.</param>
    /// <param name="suiteVersions">The ViciOne Suite versions to check.</param>
    /// <param name="suiteVersionString">The ViciOne Suite version to check for.</param>
    /// <returns><see cref="OperationStatus"/></returns>
    Task<OperationStatus> IsVersionContainedInVersionList(
        string hostManagementVersionString,
        IEnumerable<string> suiteVersions,
        string suiteVersionString);

    /// <summary>
    /// Gets the ViciOne Suite Debian file name including the extension for the specified version.
    /// </summary>
    /// <param name="suiteVersion">The ViciOne Suite version.</param>
    /// <returns><see cref="OperationStatus"/> and the ViciOne Suite Debian file name</returns>
    Task<(OperationStatus, string)> GetSuiteVersionDebianFileName(string suiteVersion);
}
