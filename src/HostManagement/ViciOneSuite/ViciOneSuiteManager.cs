using System.Globalization;
using System.IO.Abstractions;
using System.Text;
using HostManagement.Common.Wrappers;
using HostManagement.Configuration;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Enums;
using Microsoft.Extensions.Logging;

namespace HostManagement.ViciOneSuite;

/// <summary>
/// Represents the ViciOne Suite manager.
/// </summary>
internal sealed class ViciOneSuiteManager
{
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new <see cref="ViciOneSuiteManager"/>.
    /// </summary>
    /// <param name="logger">An <see cref="ILogger"/> implementation.</param>
    public ViciOneSuiteManager(ILogger logger)
    {
        _logger = logger;
    }

    private static readonly FileSystemWrapper s_fileSystemWrapper = new(
        new FileSystem(),
        new ConsoleLogger(EnvironmentVariables.LogLevel));

    /// <summary>
    /// Gets all non-pre-release ViciOne Suite versions.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see cref="GetViciOneSuiteVersionsResult"/></returns>
    public async Task<GetViciOneSuiteVersionsResult> GetNonPreReleaseSuiteVersions(
        CancellationToken cancellationToken)
    {
        return await GetViciOneSuiteVersionsInternal(true, cancellationToken);
    }

    /// <summary>
    /// Gets all ViciOne Suite versions including non-pre-release versions.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see cref="GetViciOneSuiteVersionsResult"/></returns>
    public async Task<GetViciOneSuiteVersionsResult> GetViciOneSuiteVersions(
        CancellationToken cancellationToken)
    {
        return await GetViciOneSuiteVersionsInternal(false, cancellationToken);
    }

    /// <summary>
    /// Installs the specified ViciOne Suite version.
    /// </summary>
    /// <param name="suiteVersionToInstall">The ViciOne Suite version string.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <param name="isCliCall">Determines whether the call came from the CLI.</param>
    /// <returns></returns>
    public async Task<InstallViciOneSuiteVersionResult> InstallViciOneSuiteVersion(
        string suiteVersionToInstall,
        CancellationToken cancellationToken,
        bool isCliCall = false)
    {
        try
        {
            var result = new Result();

            var suiteVersionHelper = new ViciOneSuiteVersionHelper(
                new CommonScriptHandler(result),
                new StringWrapper(),
                result);

            var adapter = new ViciOneSuiteVersionAdapter(
                new ViciOneSuiteInstallScriptHandler(result),
                suiteVersionHelper,
                s_fileSystemWrapper);

            OperationStatus status;
            IEnumerable<string> versions;

            if (isCliCall)
                (status, versions) = await adapter.GetSuiteVersions(cancellationToken);
            else
                (status, versions) = await adapter.GetNonPreReleaseSuiteVersions(cancellationToken);

            if (status == OperationStatus.Warning ||
                status == OperationStatus.Error)
            {
                var message =
                    string.Format(
                        CultureInfo.InvariantCulture,
                        CompositeFormat.Parse(
                            ViciOneSuiteResources.InstallingSuiteVersionFailed +
                            Environment.NewLine +
                            result.Messages),
                        suiteVersionToInstall);

                _logger.LogError(message);

                return new InstallViciOneSuiteVersionResult
                {
                    Status = OperationStatus.Error,
                    Message = message
                };
            }

            status = await suiteVersionHelper.IsVersionContainedInVersionList(
                VersionInfo.Version.ToString(),
                versions,
                suiteVersionToInstall);

            if (status == OperationStatus.Error)
            {
                var message =
                    string.Format(
                        CultureInfo.InvariantCulture,
                        CompositeFormat.Parse(
                            ViciOneSuiteResources.InstallingSuiteVersionFailed +
                            Environment.NewLine +
                            result.Messages),
                        suiteVersionToInstall);

                _logger.LogError(message);

                return new InstallViciOneSuiteVersionResult
                {
                    Status = OperationStatus.Error,
                    Message = message
                };
            }

            status = await adapter.InstallVersion(suiteVersionToInstall, cancellationToken);

            if (status == OperationStatus.Warning)
            {
                var message =
                    string.Format(
                        CultureInfo.InvariantCulture,
                        CompositeFormat.Parse(
                            ViciOneSuiteResources.InstallingSuiteVersionCompletedWithWarnings +
                            Environment.NewLine +
                            result.Messages),
                        suiteVersionToInstall);

                _logger.LogWarning(message);

                return new InstallViciOneSuiteVersionResult
                {
                    Status = OperationStatus.Warning,
                    Message = message
                };
            }

            if (status == OperationStatus.Error)
            {
                var message =
                    string.Format(
                        CultureInfo.InvariantCulture,
                        CompositeFormat.Parse(
                            ViciOneSuiteResources.InstallingSuiteVersionFailed +
                            Environment.NewLine +
                            result.Messages),
                        suiteVersionToInstall);

                _logger.LogError(message);

                return new InstallViciOneSuiteVersionResult
                {
                    Status = OperationStatus.Error,
                    Message = message
                };
            }

            _logger.LogInformation(
                ViciOneSuiteResources.InstallingSuiteVersionSuccessful,
                suiteVersionToInstall);

            return new InstallViciOneSuiteVersionResult
            {
                Status = OperationStatus.Success,
                Message = string.Empty
            };
        }
        catch (OperationCanceledException)
        {
            var message =
                string.Format(
                    CultureInfo.InvariantCulture,
                    CompositeFormat.Parse(
                        ViciOneSuiteResources.InstallingSuiteVersionCanceled),
                    suiteVersionToInstall);

            _logger.LogWarning(message);

            return new InstallViciOneSuiteVersionResult
            {
                Status = OperationStatus.Warning,
                Message = message
            };
        }
        catch (Exception ex)
        {
            var message =
                string.Format(
                    CultureInfo.InvariantCulture,
                    CompositeFormat.Parse(
                        ViciOneSuiteResources.InstallingSuiteVersionFailed +
                        Environment.NewLine +
                        ex.Message),
                    suiteVersionToInstall);

            _logger.LogError(ex, message);

            return new InstallViciOneSuiteVersionResult
            {
                Status = OperationStatus.Error,
                Message = message
            };
        }
    }

    private async Task<GetViciOneSuiteVersionsResult> GetViciOneSuiteVersionsInternal(
        bool excludePreReleaseVersions,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = new Result();

            var adapter = new ViciOneSuiteVersionAdapter(
                new ViciOneSuiteInstallScriptHandler(result),
                new ViciOneSuiteVersionHelper(
                    new CommonScriptHandler(result),
                    new StringWrapper(),
                    result),
                s_fileSystemWrapper);

            OperationStatus status;
            IEnumerable<string> versions;

            if (excludePreReleaseVersions)
                (status, versions) = await adapter.GetNonPreReleaseSuiteVersions(cancellationToken);
            else
                (status, versions) = await adapter.GetSuiteVersions(cancellationToken);

            if (status == OperationStatus.Warning)
            {
                var message =
                    ViciOneSuiteResources.GettingSuiteVersionsCompletedWithWarnings +
                    Environment.NewLine +
                    result.Messages;

                _logger.LogWarning(message);

                return new GetViciOneSuiteVersionsResult
                {
                    Status = OperationStatus.Warning,
                    Message = message,
                    SuiteVersions = []
                };
            }

            if (status == OperationStatus.Error)
            {
                var message =
                    ViciOneSuiteResources.GettingSuiteVersionsFailed +
                    Environment.NewLine +
                    result.Messages;

                _logger.LogError(message);

                return new GetViciOneSuiteVersionsResult
                {
                    Status = OperationStatus.Error,
                    Message = message,
                    SuiteVersions = []
                };
            }

            return new GetViciOneSuiteVersionsResult
            {
                Status = OperationStatus.Success,
                Message = string.Empty,
                SuiteVersions = versions.ToList()
            };
        }
        catch (OperationCanceledException)
        {
            var message = ViciOneSuiteResources.GettingSuiteVersionsCanceled;

            _logger.LogWarning(message);

            return new GetViciOneSuiteVersionsResult
            {
                Status = OperationStatus.Warning,
                Message = message,
                SuiteVersions = []
            };
        }
        catch (Exception ex)
        {
            var message =
                ViciOneSuiteResources.GettingSuiteVersionsFailed +
                Environment.NewLine +
                ex.Message;

            _logger.LogError(ex, message);

            return new GetViciOneSuiteVersionsResult
            {
                Status = OperationStatus.Error,
                Message = message,
                SuiteVersions = []
            };
        }
    }
}
