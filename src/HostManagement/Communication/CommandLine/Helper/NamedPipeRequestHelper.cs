using System.Text.Json;
using Microsoft.Extensions.Logging;
using HostManagement.Communication.NamedPipes.Client;
using HostManagement.Configuration;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Communication.Serialization;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.NetworkStatus;
using HostManagement.SystemConfig;
using SourceGenerationContext = HostManagement.Shared.Contracts.SourceGenerationContext;
using CommunicationJsonContext = HostManagement.Shared.Communication.Contracts.SourceGenerationContext;

namespace HostManagement.Communication.CommandLine.Helper;

/// <summary>
/// Provides helper methods to send named pipe requests, e.g. to load or apply a
/// <see cref="SystemConfiguration"/>.
/// </summary>
internal static partial class NamedPipeRequestHelper
{
    private static readonly ILogger s_logger = new ConsoleLogger(EnvironmentVariables.LogLevel);

    /// <summary>
    /// Sends a named pipe request to load the <see cref="SystemConfiguration"/> from the system.
    /// </summary>
    /// <returns>A <see cref="SystemConfiguration"/> or <see langword="null"/> if the configuration
    /// could not be loaded.</returns>
    internal static async Task<SystemConfiguration?> LoadConfiguration()
    {
        NamedPipeMessage? namedPipeMessage = null;

        try
        {
            namedPipeMessage = await SendNamedPipeRequest(Topics.LoadSystemConfiguration, null)
                ?? throw new InvalidOperationException(CommandLineStrings.NamedPipeMessageWasNull);

            var loadConfigResult = SourceGeneratedTypeSerializer
                .Deserialize(
                    namedPipeMessage.Content,
                    CommunicationJsonContext.Default.LoadSystemConfigurationResult);

            if (loadConfigResult.Status == OperationStatus.Error)
            {
                LogResultReturnedError(s_logger, loadConfigResult.Message);
                return null;
            }

            if (loadConfigResult.Configuration is null)
            {
                LogLoadConfigurationReturnedNull(s_logger, loadConfigResult.Message);
                return null;
            }

            if (loadConfigResult.Status == OperationStatus.Warning)
                LogResultReturnedWarning(s_logger, loadConfigResult.Message);

            return loadConfigResult.Configuration;
        }
        catch (Exception ex)
        {
            var message = ex.Message;

            if (namedPipeMessage?.Content is not null)
            {
                message +=
                    Environment.NewLine +
                    string.Format(" Message was: {0}", namedPipeMessage.Content);
            }

            LogLoadConfigurationFailed(s_logger, message);
            return null;
        }
    }

    /// <summary>
    /// Sends a named pipe request to apply a <see cref="SystemConfiguration"/> to the system.
    /// </summary>
    /// <param name="config">The <see cref="SystemConfiguration"/> to apply.</param>
    /// <returns><see langword="true"/> if successful, otherwise <see langword="false"/></returns>
    internal static async Task<bool> ApplyConfiguration(SystemConfiguration config)
    {
        NamedPipeMessage? namedPipeMessage = null;

        try
        {
            var systemConfigurationJson = JsonSerializer
                .Serialize(config, SourceGenerationContext.Default.SystemConfiguration);

            namedPipeMessage = await SendNamedPipeRequest(Topics.ApplySystemConfiguration, systemConfigurationJson)
                ?? throw new InvalidOperationException(CommandLineStrings.NamedPipeMessageWasNull);

            var applyConfigResult = SourceGeneratedTypeSerializer
                .Deserialize(
                    namedPipeMessage.Content,
                    CommunicationJsonContext.Default.ApplySystemConfigurationResult);

            if (applyConfigResult.Status == OperationStatus.Error)
            {
                LogResultReturnedError(s_logger, applyConfigResult.Message);
                return false;
            }

            if (applyConfigResult.Status == OperationStatus.Warning)
                LogResultReturnedWarning(s_logger, applyConfigResult.Message);

            return true;
        }
        catch (Exception ex)
        {
            var message = ex.Message;

            if (namedPipeMessage?.Content is not null)
            {
                message +=
                    Environment.NewLine +
                    string.Format(" Message was: {0}", namedPipeMessage.Content);
            }

            LogApplyConfigurationFailed(s_logger, message);
            return false;
        }
    }

    /// <summary>
    /// Applies the last saved <see cref="SystemConfiguration"/> JSON file to the system if it
    /// exists. Otherwise a new JSON file is created from the current configuration of the system.
    /// </summary>
    /// <returns><see langword="true"/> if successful, otherwise <see langword="false"/></returns>
    internal static async Task<bool> ApplyLastSavedConfiguration()
    {
        var manager = new SystemConfigurationManager();
        var applyConfigResult = await manager.ApplyConfigurationOnStartup();

        if (applyConfigResult.Status == OperationStatus.Error)
        {
            LogResultReturnedError(s_logger, applyConfigResult.Message);
            return false;
        }

        if (applyConfigResult.Status == OperationStatus.Warning)
            LogResultReturnedWarning(s_logger, applyConfigResult.Message);

        if (applyConfigResult.Status == OperationStatus.Success)
            LogResultReturnedSuccess(s_logger, applyConfigResult.Message);

        return true;
    }

    /// <summary>
    /// Sends a named pipe request to get a list of all compatible non-pre-release ViciOne Suite versions.
    /// </summary>
    /// <returns>A list of compatible non-pre-release ViciOne Suite versions or an empty collection</returns>
    internal static async Task<List<string>> GetNonPreReleaseViciOneSuiteVersions()
    {
        NamedPipeMessage? namedPipeMessage = null;

        try
        {
            namedPipeMessage = await SendNamedPipeRequest(Topics.GetViciOneSuiteVersions, null)
                ?? throw new InvalidOperationException(CommandLineStrings.NamedPipeMessageWasNull);

            var getSuiteVersionsResult = SourceGeneratedTypeSerializer
                    .Deserialize(
                        namedPipeMessage.Content,
                        CommunicationJsonContext.Default.GetViciOneSuiteVersionsResult);

            if (getSuiteVersionsResult.Status == OperationStatus.Error)
            {
                LogResultReturnedError(s_logger, getSuiteVersionsResult.Message);
                return [];
            }

            if (getSuiteVersionsResult.Status == OperationStatus.Warning)
            {
                LogResultReturnedWarning(s_logger, getSuiteVersionsResult.Message);
                return [];
            }

            return getSuiteVersionsResult.SuiteVersions;
        }
        catch (Exception ex)
        {
            var message = ex.Message;

            if (namedPipeMessage?.Content is not null)
            {
                message +=
                    Environment.NewLine +
                    string.Format(" Message was: {0}", namedPipeMessage.Content);
            }

            LogGetViciOneSuiteVersionsFailed(s_logger, message);
            return [];
        }
    }

    /// <summary>
    /// Sends a named pipe request to install the specified ViciOne Suite version.
    /// </summary>
    /// <param name="version">The ViciOne Suite version to install.</param>
    /// <returns><see langword="true"/> if successful, otherwise <see langword="false"/></returns>
    internal static async Task<bool> InstallViciOneSuiteVersion(string version)
        => await InstallViciOneSuiteVersionInternal(version);

    /// <summary>
    /// Sends a named pipe request to install the specified pre-release ViciOne Suite version.
    /// </summary>
    /// <param name="cliMessageContent">The <see cref="CliMessageContent"/> containing the
    /// pre-release ViciOne Suite version to install.</param>
    /// <returns><see langword="true"/> if successful, otherwise <see langword="false"/></returns>
    internal static async Task<bool> InstallPreReleaseViciOneSuiteVersion(CliMessageContent cliMessageContent)
    {
        LogViciOneSuitePreReleaseRequestedFromCli(s_logger, cliMessageContent.MessageContent);

        var messageContent = SourceGeneratedTypeSerializer
            .Serialize(
                cliMessageContent,
                CliSourceGenerationContext.Default.CliMessageContent);

        return await InstallViciOneSuiteVersionInternal(messageContent);
    }

    private static async Task<bool> InstallViciOneSuiteVersionInternal(string messageContent)
    {
        NamedPipeMessage? namedPipeMessage = null;

        try
        {
            namedPipeMessage = await SendNamedPipeRequest(Topics.InstallViciOneSuiteVersion, messageContent)
                ?? throw new InvalidOperationException(CommandLineStrings.NamedPipeMessageWasNull);

            var installSuiteVersionResult = SourceGeneratedTypeSerializer
                    .Deserialize(
                        namedPipeMessage.Content,
                        CommunicationJsonContext.Default.InstallViciOneSuiteVersionResult);

            if (installSuiteVersionResult.Status == OperationStatus.Error)
            {
                LogResultReturnedError(s_logger, installSuiteVersionResult.Message);
                return false;
            }

            if (installSuiteVersionResult.Status == OperationStatus.Warning)
            {
                LogResultReturnedWarning(s_logger, installSuiteVersionResult.Message);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            var message = ex.Message;

            if (namedPipeMessage?.Content is not null)
            {
                message +=
                    Environment.NewLine +
                    string.Format(" Message was: {0}", namedPipeMessage.Content);
            }

            LogInstallViciOneSuiteVersionFailed(s_logger, message);
            return false;
        }
    }

    /// <summary>
    /// Sends a named pipe request to get the network status information.
    /// </summary>
    /// <returns>The <see cref="NetworkStatusInformation"/> or <see langword="null"/> if getting
    /// the status information failed.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal static async Task<NetworkStatusInformation?> GetNetworkStatusInformation()
    {
        NamedPipeMessage? namedPipeMessage = null;

        try
        {
            namedPipeMessage = await SendNamedPipeRequest(Topics.GetNetworkStatusInformation, null)
                ?? throw new InvalidOperationException(CommandLineStrings.NamedPipeMessageWasNull);

            var networkStatusResult = SourceGeneratedTypeSerializer
                .Deserialize(
                    namedPipeMessage.Content,
                    CommunicationJsonContext.Default.GetNetworkStatusInformationResult);

            if (networkStatusResult.Status == OperationStatus.Error)
            {
                LogResultReturnedError(s_logger, networkStatusResult.Message);
                return null;
            }

            if (networkStatusResult.NetworkStatusInformation is null)
            {
                LogLoadConfigurationReturnedNull(s_logger, networkStatusResult.Message);
                return null;
            }

            if (networkStatusResult.Status == OperationStatus.Warning)
                LogResultReturnedWarning(s_logger, networkStatusResult.Message);

            return networkStatusResult.NetworkStatusInformation;
        }
        catch (Exception ex)
        {
            var message = ex.Message;

            if (namedPipeMessage?.Content is not null)
            {
                message +=
                    Environment.NewLine +
                    string.Format(" Message was: {0}", namedPipeMessage.Content);
            }

            LogGettingNetworkStatusInformationFailed(s_logger, message);
            return null;
        }
    }

    /// <summary>
    /// Sends a named pipe request to redeploy the application scripts.
    /// </summary>
    /// <returns><see langword="true"/> if successful, otherwise <see langword="false"/></returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal static async Task<bool> RedeployScripts()
    {
        NamedPipeMessage? namedPipeMessage = null;

        try
        {
            namedPipeMessage = await SendNamedPipeRequest(Topics.RedeployScripts, null)
                ?? throw new InvalidOperationException(CommandLineStrings.NamedPipeMessageWasNull);

            var redeployScriptsResult = SourceGeneratedTypeSerializer
                .Deserialize(
                    namedPipeMessage.Content,
                    CommunicationJsonContext.Default.RedeployScriptsResult);

            if (redeployScriptsResult.Status == OperationStatus.Error)
            {
                LogResultReturnedError(s_logger, redeployScriptsResult.Message);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            var message = ex.Message;

            if (namedPipeMessage?.Content is not null)
            {
                message +=
                    Environment.NewLine +
                    string.Format(" Message was: {0}", namedPipeMessage.Content);
            }

            LogRedeployScriptsFailed(s_logger, message);
            return false;
        }
    }

    private static async Task<NamedPipeMessage?> SendNamedPipeRequest(string topic, string? content)
    {
        PipeClient? client = null;

        try
        {
            client = new PipeClient("HostPipe", s_logger);

            if (topic.Equals(Topics.LoadSystemConfiguration))
            {
                return await client.SendRequest(topic);
            }
            else if (topic.Equals(Topics.ApplySystemConfiguration))
            {
                if (content is null)
                    throw new InvalidOperationException($"'{nameof(content)}' can not be null.");

                return await client.SendRequest(topic, content);
            }
            else if (topic.Equals(Topics.GetNetworkStatusInformation))
            {
                return await client.SendRequest(topic);
            }
            else if (topic.Equals(Topics.RedeployScripts))
            {
                return await client.SendRequest(topic);
            }
            else if (topic.Equals(Topics.GetViciOneSuiteVersions))
            {
                return await client.SendRequest(topic);
            }
            else if (topic.Equals(Topics.InstallViciOneSuiteVersion))
            {
                if (content is null)
                    throw new InvalidOperationException($"{nameof(content)} can not be null.");

                return await client.SendRequest(topic, content);
            }
            else
            {
                throw new InvalidOperationException($"Topic {topic} is not supported.");
            }
        }
        finally
        {
            client?.Dispose();

            // TODO This is a workaround for the following problem:
            //
            // If a client is finished and closes the remote end of the pipe the server registers this
            // and begins to dispose itself and starts a new server instance.
            //
            // In this time (up to 40 ms, but depending on the actual system or available resources)
            // a new client can still connect to the server which results in either
            // "Already in a connected state", "Broken pipe" or a halt on the client side.
            //
            // As a workaround we give the pipe server some time to restart after a client finishes
            // and a new one tries to connect.
            await Task.Delay(250);
        }
    }


    // General logs
    [LoggerMessage(LogLevel.Error, "{InnerMessage}")]
    private static partial void LogResultReturnedError(ILogger logger, string? innerMessage);

    [LoggerMessage(LogLevel.Warning, "{InnerMessage}")]
    private static partial void LogResultReturnedWarning(ILogger logger, string? innerMessage);

    [LoggerMessage(LogLevel.Information, "{InnerMessage}")]
    private static partial void LogResultReturnedSuccess(ILogger logger, string? innerMessage);

    // Logs for loading or applying the system configuration
    [LoggerMessage(LogLevel.Error, "Load configuration returned 'null'. {InnerMessage}")]
    private static partial void LogLoadConfigurationReturnedNull(ILogger logger, string? innerMessage);

    [LoggerMessage(LogLevel.Error, "Load configuration failed. {InnerMessage}")]
    private static partial void LogLoadConfigurationFailed(ILogger logger, string? innerMessage);

    [LoggerMessage(LogLevel.Error, "Apply configuration failed. {InnerMessage}")]
    private static partial void LogApplyConfigurationFailed(ILogger logger, string? innerMessage);

    // Logs related to ViciOne Suite
    [LoggerMessage(LogLevel.Warning, "Install of pre-release ViciOne Suite version '{SuiteVersion}' was requested from CLI.")]
    private static partial void LogViciOneSuitePreReleaseRequestedFromCli(ILogger logger, string? suiteVersion);

    [LoggerMessage(LogLevel.Error, "Get ViciOne Suite versions failed. {InnerMessage}")]
    private static partial void LogGetViciOneSuiteVersionsFailed(ILogger logger, string? innerMessage);

    [LoggerMessage(LogLevel.Error, "Install ViciOne Suite version failed. {InnerMessage}")]
    private static partial void LogInstallViciOneSuiteVersionFailed(ILogger logger, string? innerMessage);

    // Logs for network test
    [LoggerMessage(LogLevel.Error, "Getting network status information failed. {InnerMessage}")]
    private static partial void LogGettingNetworkStatusInformationFailed(ILogger logger, string? innerMessage);

    // Logs for script redeployment
    [LoggerMessage(LogLevel.Error, "Redeploy scripts failed. {InnerMessage}")]
    private static partial void LogRedeployScriptsFailed(ILogger logger, string? innerMessage);
}
