using System.Globalization;
using System.Text;
using HostManagement.Scripts;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Contracts.System;
using Microsoft.Extensions.Logging;

namespace HostManagement.Services;

/// <summary>
/// Represents the system service that is responsible for managing all system commands.
/// </summary>
/// <param name="logger">An <see cref="ILogger"/> implementation.</param>
/// <param name="scriptHandler">An <see cref="IScriptHandler"/> implementation.</param>
/// <param name="result">The result.</param>
/// <param name="signatureValidator">An <see cref="ISignedDebianPackageSignatureValidator"/> implementation.</param>
internal sealed partial class SystemService(
    ILogger logger,
    IScriptHandler scriptHandler,
    Result result,
    ISignedDebianPackageSignatureValidator signatureValidator)
{
    private readonly ILogger _logger = logger;
    private readonly IScriptHandler _scriptHandler = scriptHandler;
    private readonly Result _result = result;
    private readonly ISignedDebianPackageSignatureValidator _signatureValidator = signatureValidator;

    /// <summary>
    /// Installs a signed Debian package using the specified public keys for signature verification.
    /// </summary>
    /// <param name="signedDebianPackage">The signed Debian package to install.</param>
    /// <param name="publicKeys">A list of public keys used to verify the package signature.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><see cref="SystemControlResult"/></returns>
    public async Task<SystemControlResult> InstallSignedDebianPackage(
        SignedDebianPackage signedDebianPackage,
        IReadOnlyCollection<string> publicKeys,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!_signatureValidator.IsSignatureValid(signedDebianPackage, publicKeys))
            {
                var message =
                    string.Format(
                        CultureInfo.InvariantCulture,
                        CompositeFormat.Parse(
                            ServicesResources.InstallingSignedPackageFailed),
                        signedDebianPackage.FilePath,
                        ServicesResources.PackageSignatureInvalid);

                LogError(_logger, message);

                return new SystemControlResult
                {
                    Status = OperationStatus.Error,
                    Message = message
                };
            }

            cancellationToken.ThrowIfCancellationRequested();

            var scriptOptions = new ScriptOptions();
            scriptOptions.Add(ScriptOptionNames.InstallDebianPackage.DebianPackage, signedDebianPackage.FilePath);

            var status = await _scriptHandler
                .ExecuteInstallDebianPackageScript(scriptOptions)
                .ConfigureAwait(false);

            if (status is OperationStatus.Warning or OperationStatus.Error)
            {
                var message =
                    string.Format(
                        CultureInfo.InvariantCulture,
                        CompositeFormat.Parse(
                            ServicesResources.InstallingSignedPackageFailed),
                        signedDebianPackage.FilePath,
                        _result.FormattedMessages);

                LogError(_logger, message);

                return new SystemControlResult
                {
                    Status = OperationStatus.Error,
                    Message = message
                };
            }

            InstallationSuccessful(_logger, signedDebianPackage.FilePath);

            return new SystemControlResult
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
                        ServicesResources.InstallingSignedPackageCanceled),
                    signedDebianPackage.FilePath);

            LogWarning(_logger, message);

            return new SystemControlResult
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
                        ServicesResources.InstallingSignedPackageFailed),
                    signedDebianPackage.FilePath,
                    ex.Message);

            LogException(_logger, message, ex);

            return new SystemControlResult
            {
                Status = OperationStatus.Error,
                Message = message
            };
        }
    }

    /// <summary>
    /// Updates the system.
    /// </summary>
    /// <param name="filePath">The file path to the update file.</param>
    /// <returns><see cref="SystemControlResult"/></returns>
    public async Task<SystemControlResult> UpdateSystem(string filePath)
    {
        try
        {
            var scriptOptions = new ScriptOptions();
            scriptOptions.Add(ScriptOptionNames.UpdateSystem.FilePath, filePath);

            var status = await _scriptHandler
                .ExecuteUpdateSystemScript(scriptOptions)
                .ConfigureAwait(false);

            if (status == OperationStatus.Error)
            {
                var message =
                    ServicesResources.UpdateSystemFailed +
                    Environment.NewLine +
                    _result.FormattedMessages;

                LogError(_logger, message);

                return new SystemControlResult
                {
                    Status = OperationStatus.Error,
                    Message = message,
                };
            }

            return await RestartSystem();
        }
        catch (Exception ex)
        {
            var message = ServicesResources.UpdateSystemFailed;

            LogException(_logger, message, ex);

            return new SystemControlResult
            {
                Status = OperationStatus.Error,
                Message = message,
            };
        }
    }

    /// <summary>
    /// Resets the system to system defaults.
    /// </summary>
    /// <returns><see cref="SystemControlResult"/></returns>
    public async Task<SystemControlResult> ResetSystem()
    {
        try
        {
            var status = await _scriptHandler
                .ExecuteResetSystemScript()
                .ConfigureAwait(false);

            if (status is OperationStatus.Error)
                return LogErrorAndReturnSystemControlResult();

            // Clear all proxy settings after successful reset
            var proxyScriptOptions = new ScriptOptions();
            proxyScriptOptions.Add(ScriptOptionNames.Proxy.Clear);

            status = await _scriptHandler
                .ExecuteSetNetworkProxySettingsScript(proxyScriptOptions)
                .ConfigureAwait(false);

            if (status is OperationStatus.Error)
                return LogErrorAndReturnSystemControlResult();

            return await RestartSystem();
        }
        catch (Exception ex)
        {
            var message = ServicesResources.ResetSystemFailed;

            LogException(_logger, message, ex);

            return new SystemControlResult
            {
                Status = OperationStatus.Error,
                Message = message,
            };
        }
    }

    private SystemControlResult LogErrorAndReturnSystemControlResult()
    {
        var message =
            ServicesResources.ResetSystemFailed +
            Environment.NewLine +
            _result.FormattedMessages;

        LogError(_logger, message);

        return new SystemControlResult
        {
            Status = OperationStatus.Error,
            Message = message,
        };
    }

    /// <summary>
    /// Restarts the system.
    /// </summary>
    /// <param name="messageContent">The message content.</param>
    /// <returns><see cref="SystemControlResult"/></returns>
    public async Task<SystemControlResult> RestartSystem(string messageContent = "")
    {
        try
        {
            var message = ServicesResources.SystemWillRestartNow;
            var delayInSeconds = 0;

            if (!string.IsNullOrWhiteSpace(messageContent))
            {
                if (!int.TryParse(messageContent, out delayInSeconds))
                {
                    message = ServicesResources.RestartSystemFailed + " " + ServicesResources.DelayInvalid;

                    LogError(_logger, message);

                    return new SystemControlResult
                    {
                        Status = OperationStatus.Error,
                        Message = message,
                    };
                }

                if (delayInSeconds < 0)
                    delayInSeconds = 0;

                if (delayInSeconds > 0)
                {
                    message = string.Format(
                        CultureInfo.InvariantCulture,
                        CompositeFormat.Parse(ServicesResources.SystemWillRestartDelayed),
                        delayInSeconds);
                }
            }

            var status = await ShutdownOrRestart(delayInSeconds, restart: true).ConfigureAwait(false);

            if (status == OperationStatus.Error)
            {
                message =
                    ServicesResources.RestartSystemFailed +
                    Environment.NewLine +
                    _result.FormattedMessages;

                LogError(_logger, message);

                return new SystemControlResult
                {
                    Status = OperationStatus.Error,
                    Message = message,
                };
            }

            return new SystemControlResult
            {
                Status = OperationStatus.Success,
                Message = message,
            };
        }
        catch (Exception ex)
        {
            var message = ServicesResources.RestartSystemFailed;

            LogException(_logger, message, ex);

            return new SystemControlResult
            {
                Status = OperationStatus.Error,
                Message = message,
            };
        }
    }

    /// <summary>
    /// Shuts down the system.
    /// </summary>
    /// <param name="messageContent">The message content.</param>
    /// <returns><see cref="SystemControlResult"/></returns>
    public async Task<SystemControlResult> ShutdownSystem(string messageContent = "")
    {
        try
        {
            var message = ServicesResources.SystemWillShutdownNow;
            var delayInSeconds = 0;

            if (!string.IsNullOrWhiteSpace(messageContent))
            {
                if (!int.TryParse(messageContent, out delayInSeconds))
                {
                    message = ServicesResources.ShutdownSystemFailed + " " + ServicesResources.DelayInvalid;

                    LogError(_logger, message);

                    return new SystemControlResult
                    {
                        Status = OperationStatus.Error,
                        Message = message,
                    };
                }

                if (delayInSeconds < 0)
                    delayInSeconds = 0;

                if (delayInSeconds > 0)
                {
                    message = string.Format(
                        CultureInfo.InvariantCulture,
                        CompositeFormat.Parse(ServicesResources.SystemWillShutdownDelayed),
                        delayInSeconds);
                }
            }

            var status = await ShutdownOrRestart(delayInSeconds, restart: false).ConfigureAwait(false);

            if (status == OperationStatus.Error)
            {
                message =
                    ServicesResources.ShutdownSystemFailed +
                    Environment.NewLine +
                    _result.FormattedMessages;

                LogError(_logger, message);

                return new SystemControlResult
                {
                    Status = OperationStatus.Error,
                    Message = message,
                };
            }

            return new SystemControlResult
            {
                Status = OperationStatus.Success,
                Message = message,
            };
        }
        catch (Exception ex)
        {
            var message = ServicesResources.ShutdownSystemFailed;

            LogException(_logger, message, ex);

            return new SystemControlResult
            {
                Status = OperationStatus.Error,
                Message = message,
            };
        }
    }

    private async Task<OperationStatus> ShutdownOrRestart(int delayInSeconds, bool restart)
    {
        var scriptOptions = new ScriptOptions();

        if (restart)
            scriptOptions.Add(ScriptOptionNames.ShutdownSystem.Restart);

        scriptOptions.Add(ScriptOptionNames.ShutdownSystem.Delay, delayInSeconds);

        return await _scriptHandler.ExecuteShutdownSystemScript(scriptOptions).ConfigureAwait(false);
    }

    [LoggerMessage(LogLevel.Information, "Installation of Debian package completed successfully. ('{FilePath}')")]
    private static partial void InstallationSuccessful(ILogger logger, string filePath);

    [LoggerMessage(LogLevel.Warning, "{Message}")]
    private static partial void LogWarning(ILogger logger, string message);

    [LoggerMessage(LogLevel.Error, "{Message}")]
    private static partial void LogError(ILogger logger, string message);

    [LoggerMessage(LogLevel.Error, "{Message}")]
    private static partial void LogException(ILogger logger, string message, Exception exception);
}
