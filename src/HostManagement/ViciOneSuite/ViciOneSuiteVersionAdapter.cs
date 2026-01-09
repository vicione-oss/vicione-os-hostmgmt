using HostManagement.Common.Wrappers;
using HostManagement.Configuration;
using HostManagement.Shared.Communication.Enums;

namespace HostManagement.ViciOneSuite;

/// <summary>
/// Represents the ViciOne Suite version adapter.
/// </summary>
internal sealed class ViciOneSuiteVersionAdapter
{
    private readonly IViciOneSuiteInstallScriptHandler _suiteInstallScriptHandler;
    private readonly IViciOneSuiteVersionHelper _suiteVersionHelper;
    private readonly FileSystemWrapper _fileSystemWrapper;

    /// <summary>
    /// Initializes a new <see cref="ViciOneSuiteVersionAdapter"/>.
    /// </summary>
    /// <param name="suiteInstallScriptHandler">An <see cref="IViciOneSuiteInstallScriptHandler"/>
    /// implementation.</param>
    /// <param name="suiteVersionHelper">An <see cref="IViciOneSuiteVersionHelper"/> implementation.</param>
    /// <param name="fileSystemWrapper">A <see cref="FileSystemWrapper"/>.</param>
    public ViciOneSuiteVersionAdapter(
        IViciOneSuiteInstallScriptHandler suiteInstallScriptHandler,
        IViciOneSuiteVersionHelper suiteVersionHelper,
        FileSystemWrapper fileSystemWrapper)
    {
        _suiteInstallScriptHandler = suiteInstallScriptHandler;
        _suiteVersionHelper = suiteVersionHelper;
        _fileSystemWrapper = fileSystemWrapper;
    }

    /// <summary>
    /// Gets all non-pre-release ViciOne Suite versions.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see cref="OperationStatus"/> and the list of non-pre-release Suite versions.</returns>
    internal async Task<(OperationStatus, IEnumerable<string>)> GetNonPreReleaseSuiteVersions(
        CancellationToken cancellationToken)
    {
        var (status, suiteVersions) = await GetSuiteVersions(cancellationToken);

        if (status == OperationStatus.Warning)
            return (OperationStatus.Warning, Enumerable.Empty<string>());

        if (status == OperationStatus.Error)
            return (OperationStatus.Error, Enumerable.Empty<string>());

        (status, var filteredSuiteVersions) =
            await _suiteVersionHelper.GetFilteredSuiteVersions(
                suiteVersions,
                _suiteVersionHelper.NonPreReleaseVersionsFilter);

        if (status == OperationStatus.Error)
            return (OperationStatus.Error, Enumerable.Empty<string>());

        return (OperationStatus.Success, filteredSuiteVersions);
    }

    /// <summary>
    /// Gets all ViciOne Suite versions.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see cref="OperationStatus"/> and the list of all Suite versions.</returns>
    internal async Task<(OperationStatus, IEnumerable<string>)> GetSuiteVersions(
        CancellationToken cancellationToken)
    {
        var (status, versionsCsvString) =
            await _suiteInstallScriptHandler.ExecuteGetViciOneSuiteVersionsScript();

        if (status == OperationStatus.Error)
            return (OperationStatus.Error, Enumerable.Empty<string>());

        cancellationToken.ThrowIfCancellationRequested();

        (_, var installedSuiteVersionString) =
           await _suiteInstallScriptHandler.ExecuteGetInstalledSuiteVersionScript();

        cancellationToken.ThrowIfCancellationRequested();

        (status, var versionMapping) =
            await _suiteVersionHelper.GetVersionMapping(versionsCsvString);

        if (status == OperationStatus.Error)
            return (OperationStatus.Error, Enumerable.Empty<string>());

        (status, var suiteVersions) = await _suiteVersionHelper
            .GetCompatibleSuiteVersions(
                VersionInfo.Version.ToString(),
                installedSuiteVersionString,
                versionMapping);

        if (status == OperationStatus.Warning)
            return (OperationStatus.Warning, Enumerable.Empty<string>());

        if (status == OperationStatus.Error)
            return (OperationStatus.Error, Enumerable.Empty<string>());

        return (OperationStatus.Success, suiteVersions);
    }

    /// <summary>
    /// Installs the specified ViciOne Suite version.
    /// </summary>
    /// <param name="version">The ViciOne Suite version to install.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see cref="OperationStatus"/></returns>
    internal async Task<OperationStatus> InstallVersion(
        string version,
        CancellationToken cancellationToken)
    {
        var suiteDebianFileName = string.Empty;

        try
        {
            (var status, suiteDebianFileName) = await _suiteVersionHelper
                .GetSuiteVersionDebianFileName(version);

            if (status == OperationStatus.Error)
                return status;

            status = await _suiteInstallScriptHandler
                .ExecuteGetViciOneSuiteDebFileScript(suiteDebianFileName);

            if (status == OperationStatus.Error)
                return status;

            cancellationToken.ThrowIfCancellationRequested();

            status = await _suiteInstallScriptHandler
                .ExecuteViciOneSuiteInstallScript(suiteDebianFileName);

            return status;
        }
        catch (OperationCanceledException)
        {
            var suiteDebianFilePath = Path.Combine(
                EnvironmentVariables.SuiteDownloadDirectory,
                suiteDebianFileName);

            if (_fileSystemWrapper.ExistsFile(suiteDebianFilePath))
                _fileSystemWrapper.DeleteFile(suiteDebianFilePath);

            throw;
        }
    }
}
