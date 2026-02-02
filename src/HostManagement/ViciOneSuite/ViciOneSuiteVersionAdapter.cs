using System.IO.Abstractions;
using HostManagement.Configuration;
using HostManagement.Shared.Communication.Enums;

namespace HostManagement.ViciOneSuite;

/// <summary>
/// Represents the ViciOne Suite version adapter.
/// </summary>
/// <param name="suiteInstallScriptHandler">An <see cref="IViciOneSuiteInstallScriptHandler"/>
/// implementation.</param>
/// <param name="suiteVersionHelper">An <see cref="IViciOneSuiteVersionHelper"/> implementation.</param>
/// <param name="fileSystem">An <see cref="IFileSystem"/> implementation.</param>
internal sealed class ViciOneSuiteVersionAdapter(
    IViciOneSuiteInstallScriptHandler suiteInstallScriptHandler,
    IViciOneSuiteVersionHelper suiteVersionHelper,
    IFileSystem fileSystem)
{
    private readonly IViciOneSuiteInstallScriptHandler _suiteInstallScriptHandler = suiteInstallScriptHandler;
    private readonly IViciOneSuiteVersionHelper _suiteVersionHelper = suiteVersionHelper;
    private readonly IFileSystem _fileSystem = fileSystem;

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

            if (_fileSystem.File.Exists(suiteDebianFilePath))
            {
                try
                {
                    _fileSystem.File.Delete(suiteDebianFilePath);
                }
                catch { } // Swallow any exception to keep original exception
            }

            throw;
        }
    }
}
