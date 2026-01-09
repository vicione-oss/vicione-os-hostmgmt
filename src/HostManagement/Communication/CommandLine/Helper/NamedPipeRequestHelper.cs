using System.Text.Json;
using Microsoft.Extensions.Logging;
using HostManagement.Communication.NamedPipes.Client;
using HostManagement.Configuration;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Communication.Serialization;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Contracts.NetworkStatus;
using HostManagement.Shared.Enums;
using SourceGenerationContext = HostManagement.Shared.Contracts.SourceGenerationContext;
using CommunicationJsonContext = HostManagement.Shared.Communication.Contracts.SourceGenerationContext;

namespace HostManagement.Communication.CommandLine.Helper;

/// <summary>
/// Provides helper methods to send named pipe requests, e.g. to get or set a
/// <see cref="SystemConfiguration"/>.
/// </summary>
internal static partial class NamedPipeRequestHelper
{
    private static readonly ILogger s_logger = new ConsoleLogger(
        EnvironmentVariables.LogLevel,
        useShortLogLevels: false,
        addNewLine: true);

    /// <summary>
    /// Sends a named pipe request to get the <see cref="SystemConfiguration"/> from the system.
    /// </summary>
    /// <returns>A <see cref="SystemConfiguration"/> or <see langword="null"/> if the configuration
    /// could not be loaded.</returns>
    internal static async Task<SystemConfiguration?> GetConfiguration()
    {
        NamedPipeMessage? namedPipeMessage = null;

        try
        {
            namedPipeMessage = await SendNamedPipeRequest(Topics.GetSystemConfiguration, null)
                ?? throw new InvalidOperationException(CommandLineStrings.NamedPipeMessageWasNull);

            var getConfigResult = SourceGeneratedTypeSerializer
                .Deserialize(
                    namedPipeMessage.Content,
                    CommunicationJsonContext.Default.GetSystemConfigurationResult);

            if (getConfigResult.Status == OperationStatus.Error)
            {
                LogResultReturnedError(s_logger, getConfigResult.Message);
                return null;
            }

            if (getConfigResult.Configuration is null)
            {
                LogGetConfigurationReturnedNull(s_logger, getConfigResult.Message);
                return null;
            }

            if (getConfigResult.Status == OperationStatus.Warning)
                LogResultReturnedWarning(s_logger, getConfigResult.Message);

            return getConfigResult.Configuration;
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

            LogGetConfigurationFailed(s_logger, message);
            return null;
        }
    }

    /// <summary>
    /// Sends a named pipe request to set a <see cref="SystemConfiguration"/> to the system.
    /// </summary>
    /// <param name="config">The <see cref="SystemConfiguration"/> to apply.</param>
    /// <returns><see langword="true"/> if successful, otherwise <see langword="false"/></returns>
    internal static async Task<bool> SetConfiguration(SystemConfiguration config)
    {
        NamedPipeMessage? namedPipeMessage = null;

        try
        {
            var systemConfigurationJson = JsonSerializer
                .Serialize(config, SourceGenerationContext.Default.SystemConfiguration);

            namedPipeMessage = await SendNamedPipeRequest(Topics.SetSystemConfiguration, systemConfigurationJson)
                ?? throw new InvalidOperationException(CommandLineStrings.NamedPipeMessageWasNull);

            var setConfigResult = SourceGeneratedTypeSerializer
                .Deserialize(
                    namedPipeMessage.Content,
                    CommunicationJsonContext.Default.SetSystemConfigurationResult);

            if (setConfigResult.Status == OperationStatus.Error)
            {
                LogResultReturnedError(s_logger, setConfigResult.Message);
                return false;
            }

            if (setConfigResult.Status == OperationStatus.Warning)
                LogResultReturnedWarning(s_logger, setConfigResult.Message);

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

            LogSetConfigurationFailed(s_logger, message);
            return false;
        }
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
    /// <param name="interfaceName">The interface name to get network status information for.</param>
    /// <returns>The <see cref="NetworkStatusInformation"/> or <see langword="null"/> if getting
    /// the status information failed.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal static async Task<NetworkStatusInformation?> GetNetworkStatusInformation(
        string interfaceName)
    {
        NamedPipeMessage? namedPipeMessage = null;

        try
        {
            namedPipeMessage = await SendNamedPipeRequest(Topics.GetNetworkStatusInformation, interfaceName)
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
                LogGetConfigurationReturnedNull(s_logger, networkStatusResult.Message);
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
    /// Sends a named pipe request to renew the DHCP lease.
    /// </summary>
    /// <param name="interfaceName">The interface name for which the DHCP lease shall be renewed.</param>
    /// <returns>The <see cref="DHCPLease"/> or <see langword="null"/> if renewing the DHCP lease failed.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal static async Task<DHCPLease?> RenewDHCPLease(
        string interfaceName)
    {
        NamedPipeMessage? namedPipeMessage = null;

        try
        {
            namedPipeMessage = await SendNamedPipeRequest(Topics.RenewDHCPLease, interfaceName)
                ?? throw new InvalidOperationException(CommandLineStrings.NamedPipeMessageWasNull);

            var renewDhcpLeaseResult = SourceGeneratedTypeSerializer
                .Deserialize(
                    namedPipeMessage.Content,
                    CommunicationJsonContext.Default.RenewDHCPLeaseResult);

            if (renewDhcpLeaseResult.Status == OperationStatus.Error)
            {
                LogResultReturnedError(s_logger, renewDhcpLeaseResult.Message);
                return null;
            }

            if (renewDhcpLeaseResult.Status == OperationStatus.Warning)
                LogResultReturnedWarning(s_logger, renewDhcpLeaseResult.Message);

            return renewDhcpLeaseResult.DHCPLease;
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

            LogRenewDHCPLeaseFailed(s_logger, message);
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

    /// <summary>
    /// Sends a named pipe request to start, stop or restart a service.
    /// </summary>
    /// <param name="serviceName">The service name.</param>
    /// <param name="controlAction">The control action.</param>
    /// <returns><see langword="true"/> if successful, otherwise <see langword="false"/></returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal static async Task<bool> ControlService(string serviceName, ServiceControlAction controlAction)
    {
        NamedPipeMessage? namedPipeMessage = null;

        var topic = controlAction switch
        {
            ServiceControlAction.Start => Topics.StartService,
            ServiceControlAction.Stop => Topics.StopService,
            ServiceControlAction.Restart => Topics.RestartService,
            _ => throw new ArgumentOutOfRangeException(nameof(controlAction), controlAction, null)
        };

        try
        {
            namedPipeMessage = await SendNamedPipeRequest(topic, serviceName)
                ?? throw new InvalidOperationException(CommandLineStrings.NamedPipeMessageWasNull);

            var serviceControlResult = SourceGeneratedTypeSerializer
                .Deserialize(
                    namedPipeMessage.Content,
                    CommunicationJsonContext.Default.ServiceControlResult);

            if (serviceControlResult.Status == OperationStatus.Error)
            {
                LogControlServiceFailed(s_logger, controlAction.ToString(), serviceControlResult.Message);
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

            LogControlServiceFailed(s_logger, controlAction.ToString(), message);
            return false;
        }
    }

    /// <summary>
    /// Sends a named pipe request to reset the system.
    /// </summary>
    /// <returns><see langword="true"/> if successful, otherwise <see langword="false"/></returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal static async Task<bool> ResetSystem()
    {
        NamedPipeMessage? namedPipeMessage = null;

        try
        {
            namedPipeMessage = await SendNamedPipeRequest(Topics.ResetSystem, null)
                ?? throw new InvalidOperationException(CommandLineStrings.NamedPipeMessageWasNull);

            var resetSystemResult = SourceGeneratedTypeSerializer
                .Deserialize(
                    namedPipeMessage.Content,
                    CommunicationJsonContext.Default.ResetSystemResult);

            if (resetSystemResult.Status == OperationStatus.Error)
            {
                LogResetSystemFailed(s_logger, resetSystemResult.Message);
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

            LogResetSystemFailed(s_logger, message);
            return false;
        }
    }

    /// <summary>
    /// Sends a named pipe request to restart the system.
    /// </summary>
    /// <returns><see langword="true"/> if successful, otherwise <see langword="false"/></returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal static async Task<bool> RestartSystem()
    {
        NamedPipeMessage? namedPipeMessage = null;

        try
        {
            namedPipeMessage = await SendNamedPipeRequest(Topics.RestartSystem, null)
                ?? throw new InvalidOperationException(CommandLineStrings.NamedPipeMessageWasNull);

            var restartSystemResult = SourceGeneratedTypeSerializer
                .Deserialize(
                    namedPipeMessage.Content,
                    CommunicationJsonContext.Default.RestartSystemResult);

            if (restartSystemResult.Status == OperationStatus.Error)
            {
                LogRestartSystemFailed(s_logger, restartSystemResult.Message);
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

            LogRestartSystemFailed(s_logger, message);
            return false;
        }
    }

    private static async Task<NamedPipeMessage?> SendNamedPipeRequest(string topic, string? content)
    {
        PipeClient? client = null;

        try
        {
            client = new PipeClient("HostPipe", s_logger);

            // todo: Find better implementation for Topic class to allow generic check for
            // valid topic here without manual editing.
            var validTopics = new HashSet<string>
            {
                Topics.GetSystemConfiguration,
                Topics.SetSystemConfiguration,
                Topics.GetNetworkStatusInformation,
                Topics.RenewDHCPLease,
                Topics.RedeployScripts,
                Topics.StartService,
                Topics.StopService,
                Topics.RestartService,
                Topics.ResetSystem,
                Topics.RestartSystem,
                Topics.GetViciOneSuiteVersions,
                Topics.InstallViciOneSuiteVersion
            };

            if (!validTopics.Contains(topic))
            {
                throw new InvalidOperationException($"Topic '{topic}' is not supported.");
            }

            var requiresContent = new HashSet<string>
            {
                Topics.SetSystemConfiguration,
                Topics.GetNetworkStatusInformation,
                Topics.RenewDHCPLease,
                Topics.StartService,
                Topics.StopService,
                Topics.RestartService,
                Topics.InstallViciOneSuiteVersion
            };

            if (requiresContent.Contains(topic) && content is null)
            {
                throw new InvalidOperationException($"Parameter '{nameof(content)}' cannot be null for topic '{topic}'.");
            }

            return content is null
                ? await client.SendRequest(topic)
                : await client.SendRequest(topic, content);
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

    // Logs for getting or setting the system configuration
    [LoggerMessage(LogLevel.Error, "Getting configuration returned 'null'. {InnerMessage}")]
    private static partial void LogGetConfigurationReturnedNull(ILogger logger, string? innerMessage);

    [LoggerMessage(LogLevel.Error, "Getting configuration failed. {InnerMessage}")]
    private static partial void LogGetConfigurationFailed(ILogger logger, string? innerMessage);

    [LoggerMessage(LogLevel.Error, "Setting configuration failed. {InnerMessage}")]
    private static partial void LogSetConfigurationFailed(ILogger logger, string? innerMessage);

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

    // Logs for DHCP lease renew
    [LoggerMessage(LogLevel.Error, "Renew DHCP lease failed. {InnerMessage}")]
    private static partial void LogRenewDHCPLeaseFailed(ILogger logger, string? innerMessage);

    // Logs for script redeployment
    [LoggerMessage(LogLevel.Error, "Redeploy scripts failed. {InnerMessage}")]
    private static partial void LogRedeployScriptsFailed(ILogger logger, string? innerMessage);

    // Logs for service start, stop, restart
    [LoggerMessage(LogLevel.Error, "{ControlAction} service failed. {InnerMessage}")]
    private static partial void LogControlServiceFailed(ILogger logger, string controlAction, string? innerMessage);

    // Logs for system reset
    [LoggerMessage(LogLevel.Error, "Reset system failed. {InnerMessage}")]
    private static partial void LogResetSystemFailed(ILogger logger, string? innerMessage);

    // Logs for system restart
    [LoggerMessage(LogLevel.Error, "Restart system failed. {InnerMessage}")]
    private static partial void LogRestartSystemFailed(ILogger logger, string? innerMessage);
}
