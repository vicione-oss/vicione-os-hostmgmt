using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Contracts.System;

namespace HostManagement.Services;

/// <summary>
/// Represents the system service that is responsible for managing all system commands.
/// </summary>
/// <param name="logger">An <see cref="ILogger"/> implementation.</param>
/// <param name="scriptHandler">An <see cref="IScriptHandler"/> implementation.</param>
/// <param name="result">The result.</param>
internal sealed partial class SystemService(
    ILogger logger,
    IScriptHandler scriptHandler,
    Result result)
{
    private readonly ILogger _logger = logger;
    private readonly IScriptHandler _scriptHandler = scriptHandler;
    private readonly Result _result = result;

    /// <summary>
    /// Installs a signed Debian package using the specified public keys for signature verification.
    /// </summary>
    /// <param name="signedDebianPackage">The signed Debian package to install.</param>
    /// <param name="publicKeys">A list of public keys used to verify the package signature.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><see cref="SystemControlResult"/></returns>
    public async Task<SystemControlResult> InstallSignedDebianPackage(
        SignedDebianPackage signedDebianPackage,
        List<string> publicKeys,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!IsSignatureValidForAnyKey(signedDebianPackage, publicKeys))
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

            var status = await _scriptHandler
                .ExecuteInstallDebianPackageScript(signedDebianPackage.FilePath)
                .ConfigureAwait(false);

            if (status is OperationStatus.Warning or OperationStatus.Error)
            {
                var message =
                    string.Format(
                        CultureInfo.InvariantCulture,
                        CompositeFormat.Parse(
                            ServicesResources.InstallingSignedPackageFailed),
                        signedDebianPackage.FilePath,
                        _result.Messages);

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
            var status = await _scriptHandler
                .ExecuteUpdateSystemScript(filePath)
                .ConfigureAwait(false);

            if (status == OperationStatus.Error)
            {
                var message =
                    ServicesResources.UpdateSystemFailed +
                    Environment.NewLine +
                    _result.Messages;

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
                Message = string.Empty,
            };
        }
        catch (Exception ex)
        {
            var message =
                ServicesResources.UpdateSystemFailed +
                Environment.NewLine +
                ex.Message;

            LogError(_logger, message);

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

            if (status == OperationStatus.Error)
            {
                var message =
                    ServicesResources.ResetSystemFailed +
                    Environment.NewLine +
                    _result.Messages;

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
                Message = string.Empty,
            };
        }
        catch (Exception ex)
        {
            var message =
                ServicesResources.ResetSystemFailed +
                Environment.NewLine +
                ex.Message;

            LogError(_logger, message);

            return new SystemControlResult
            {
                Status = OperationStatus.Error,
                Message = message,
            };
        }
    }

    /// <summary>
    /// Restarts the system.
    /// </summary>
    /// <returns><see cref="SystemControlResult"/></returns>
    public async Task<SystemControlResult> RestartSystem()
    {
        try
        {
            var status = await _scriptHandler
                .ExecuteRestartSystemScript()
                .ConfigureAwait(false);

            if (status == OperationStatus.Error)
            {
                var message =
                    ServicesResources.RestartSystemFailed +
                    Environment.NewLine +
                    _result.Messages;

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
                Message = string.Empty,
            };
        }
        catch (Exception ex)
        {
            var message =
                ServicesResources.RestartSystemFailed +
                Environment.NewLine +
                ex.Message;

            LogError(_logger, message);

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
    /// <returns><see cref="SystemControlResult"/></returns>
    public async Task<SystemControlResult> ShutdownSystem()
    {
        try
        {
            var status = await _scriptHandler
                .ExecuteShutdownSystemScript()
                .ConfigureAwait(false);

            if (status == OperationStatus.Error)
            {
                var message =
                    ServicesResources.ShutdownSystemFailed +
                    Environment.NewLine +
                    _result.Messages;

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
                Message = string.Empty,
            };
        }
        catch (Exception ex)
        {
            var message =
                ServicesResources.ShutdownSystemFailed +
                Environment.NewLine +
                ex.Message;

            LogError(_logger, message);

            return new SystemControlResult
            {
                Status = OperationStatus.Error,
                Message = message,
            };
        }
    }

    private static bool IsSignatureValidForAnyKey(SignedDebianPackage signedDebianPackage, List<string> publicKeys)
    {
        var signature = Minisign.Core.LoadSignatureFromFile(signedDebianPackage.SignatureFilePath);
        var signatureIsValid = false;

        foreach (var key in publicKeys)
        {
            var publicKey = Minisign.Core.LoadPublicKeyFromString(key);

            if (Minisign.Core.ValidateSignature(signedDebianPackage.FilePath, signature, publicKey))
            {
                signatureIsValid = true;
                break;
            }
        }

        return signatureIsValid;
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
