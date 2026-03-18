using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.NamedPipes.Client;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Communication.Serialization;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Contracts.NetworkStatus;
using HostManagement.Shared.Enums;
using HostManagement.Shared.Serialization;
using Microsoft.Extensions.Logging;
using CommunicationJsonContext = HostManagement.Shared.Communication.Contracts.SourceGenerationContext;

namespace HostManagement.Communication.CommandLine.Helper;

/// <summary>
/// Provides helper methods to send named pipe requests, e.g. to get or set a <see cref="SystemConfiguration"/>.
/// </summary>
internal static class NamedPipeRequestHelper
{
    private static readonly List<string> s_validTopics = [.. Topics.GetAllTopics()];
    private static readonly List<string> s_topicsWithRequiredContent = [.. Topics.GetTopicsWithRequiredContent()];

    /*
     * The only usage of the logger is the 'PipeClient'. Other console output is done via 'SimpleOutput' class.
     * We have to use 'LogLevel' directly because 'EnvironmentVariables.LogLevel' can't be used in a non-daemon context.
    */
    private static readonly ILogger s_logger = new ConsoleLogger(LogLevel.Warning, useShortLogLevels: false);

    /// <summary>
    /// Sends a named pipe request to get the <see cref="SystemConfiguration"/> from the system.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="SystemConfiguration"/> or <see langword="null"/> if the configuration
    /// could not be loaded.</returns>
    internal static async Task<SystemConfiguration?> GetConfiguration(CancellationToken cancellationToken)
    {
        var topic = Topics.GetSystemConfiguration;
        NamedPipeMessage? namedPipeMessage = null;

        try
        {
            namedPipeMessage = await SendNamedPipeRequest(topic, null, cancellationToken);

            var getConfigResult = SourceGeneratedTypeSerializer
                .Deserialize(
                    namedPipeMessage.Content,
                    CommunicationJsonContext.Default.GetSystemConfigurationResult);

            if (getConfigResult.Status == OperationStatus.Error ||
                getConfigResult.Configuration is null)
            {
                OutputError(topic, getConfigResult);
                return null;
            }

            if (getConfigResult.Status == OperationStatus.Warning)
                OutputWarning(topic, getConfigResult);

            return getConfigResult.Configuration;
        }
        catch (Exception ex)
        {
            OutputException(topic, namedPipeMessage, ex);
            return null;
        }
    }

    /// <summary>
    /// Sends a named pipe request to set a <see cref="SystemConfiguration"/> to the system.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <param name="config">The <see cref="SystemConfiguration"/> to apply.</param>
    /// <returns><see langword="true"/> if successful, otherwise <see langword="false"/></returns>
    internal static async Task<bool> SetConfiguration(SystemConfiguration config, CancellationToken cancellationToken)
    {
        var topic = Topics.SetSystemConfiguration;
        NamedPipeMessage? namedPipeMessage = null;

        try
        {
            var systemConfigurationJson = SystemConfigurationSerializer.Serialize(config);

            namedPipeMessage = await SendNamedPipeRequest(topic, systemConfigurationJson, cancellationToken);

            var setConfigResult = SourceGeneratedTypeSerializer
                .Deserialize(
                    namedPipeMessage.Content,
                    CommunicationJsonContext.Default.SetSystemConfigurationResult);

            if (setConfigResult.Status == OperationStatus.Error)
            {
                OutputError(topic, setConfigResult);
                return false;
            }

            if (setConfigResult.Status == OperationStatus.Warning)
                OutputWarning(topic, setConfigResult);

            return true;
        }
        catch (Exception ex)
        {
            OutputException(topic, namedPipeMessage, ex);
            return false;
        }
    }

    /// <summary>
    /// Sends a named pipe request to get a list of all compatible non-pre-release ViciOne Suite versions.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A list of compatible non-pre-release ViciOne Suite versions or an empty collection</returns>
    internal static async Task<List<string>> GetNonPreReleaseViciOneSuiteVersions(CancellationToken cancellationToken)
    {
        var topic = Topics.GetViciOneSuiteVersions;
        NamedPipeMessage? namedPipeMessage = null;

        try
        {
            namedPipeMessage = await SendNamedPipeRequest(topic, null, cancellationToken);

            var getSuiteVersionsResult = SourceGeneratedTypeSerializer
                .Deserialize(
                    namedPipeMessage.Content,
                    CommunicationJsonContext.Default.GetViciOneSuiteVersionsResult);

            if (getSuiteVersionsResult.Status == OperationStatus.Error)
            {
                OutputError(topic, getSuiteVersionsResult);
                return [];
            }

            if (getSuiteVersionsResult.Status == OperationStatus.Warning)
            {
                OutputWarning(topic, getSuiteVersionsResult);
                return [];
            }

            return getSuiteVersionsResult.SuiteVersions;
        }
        catch (Exception ex)
        {
            OutputException(topic, namedPipeMessage, ex);
            return [];
        }
    }

    /// <summary>
    /// Sends a named pipe request to install the specified ViciOne Suite version.
    /// </summary>
    /// <param name="version">The ViciOne Suite version to install.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><see langword="true"/> if successful, otherwise <see langword="false"/></returns>
    internal static async Task<bool> InstallViciOneSuiteVersion(string version, CancellationToken cancellationToken)
        => await InstallViciOneSuiteVersionInternal(version, cancellationToken);

    /// <summary>
    /// Sends a named pipe request to install the specified pre-release ViciOne Suite version.
    /// </summary>
    /// <param name="cliMessageContent">The <see cref="CliMessageContent"/> containing the
    /// pre-release ViciOne Suite version to install.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><see langword="true"/> if successful, otherwise <see langword="false"/></returns>
    internal static async Task<bool> InstallPreReleaseViciOneSuiteVersion(
        CliMessageContent cliMessageContent,
        CancellationToken cancellationToken)
    {
        SimpleOutput.WriteLineWarning(
            $"Install of pre-release ViciOne Suite version '{cliMessageContent.MessageContent}' was requested from CLI.");

        var messageContent = SourceGeneratedTypeSerializer
            .Serialize(
                cliMessageContent,
                CliSourceGenerationContext.Default.CliMessageContent);

        return await InstallViciOneSuiteVersionInternal(messageContent, cancellationToken);
    }

    private static async Task<bool> InstallViciOneSuiteVersionInternal(
        string messageContent,
        CancellationToken cancellationToken)
    {
        var topic = Topics.InstallViciOneSuiteVersion;
        NamedPipeMessage? namedPipeMessage = null;

        try
        {
            namedPipeMessage = await SendNamedPipeRequest(topic, messageContent, cancellationToken);

            var installSuiteVersionResult = SourceGeneratedTypeSerializer
                .Deserialize(
                    namedPipeMessage.Content,
                    CommunicationJsonContext.Default.InstallViciOneSuiteVersionResult);

            if (installSuiteVersionResult.Status == OperationStatus.Error)
            {
                OutputError(topic, installSuiteVersionResult);
                return false;
            }

            if (installSuiteVersionResult.Status == OperationStatus.Warning)
            {
                OutputWarning(topic, installSuiteVersionResult);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            OutputException(topic, namedPipeMessage, ex);
            return false;
        }
    }

    /// <summary>
    /// Sends a named pipe request to install the specified signed Debian package.
    /// </summary>
    /// <param name="messageContent">The message content containing the package and signature file paths.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><see langword="true"/> if successful, otherwise <see langword="false"/></returns>
    internal static async Task<bool> InstallSignedDebianPackage(
        string messageContent,
        CancellationToken cancellationToken)
    {
        var topic = Topics.InstallSignedDebianPackage;
        NamedPipeMessage? namedPipeMessage = null;

        try
        {
            namedPipeMessage = await SendNamedPipeRequest(topic, messageContent, cancellationToken);

            var installDebianPackageResult = SourceGeneratedTypeSerializer
                .Deserialize(
                    namedPipeMessage.Content,
                    CommunicationJsonContext.Default.SystemControlResult);

            if (installDebianPackageResult.Status == OperationStatus.Error)
            {
                OutputError(topic, installDebianPackageResult);
                return false;
            }

            if (installDebianPackageResult.Status == OperationStatus.Warning)
            {
                OutputWarning(topic, installDebianPackageResult);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            OutputException(topic, namedPipeMessage, ex);
            return false;
        }
    }

    /// <summary>
    /// Sends a named pipe request to get the NTP fallback information.
    /// </summary>
    /// <returns>The list of fallback NTP servers or an empty list if the request failed.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal static async Task<List<string>> GetNTPFallbackInformation()
    {
        var topic = Topics.GetNTPFallbackInformation;
        NamedPipeMessage? namedPipeMessage = null;

        try
        {
            namedPipeMessage = await SendNamedPipeRequest(topic, null);

            var ntpFallbackInformationResult = SourceGeneratedTypeSerializer
                .Deserialize(
                    namedPipeMessage.Content,
                    CommunicationJsonContext.Default.GetNTPFallbackInformationResult);

            if (ntpFallbackInformationResult.Status == OperationStatus.Error)
            {
                OutputError(topic, ntpFallbackInformationResult);
                return [];
            }

            return ntpFallbackInformationResult.FallbackNTPServers;
        }
        catch (Exception ex)
        {
            OutputException(topic, namedPipeMessage, ex);
            return [];
        }
    }

    /// <summary>
    /// Sends a named pipe request to get the original physical address.
    /// </summary>
    /// <param name="interfaceName">The interface name to get the original physical address for.</param>
    /// <returns>The original physical address or <see langword="null"/> if getting the original
    /// physical address failed.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal static async Task<string?> GetOriginalPhysicalAddress(string interfaceName)
    {
        var topic = Topics.GetOriginalPhysicalAddress;
        NamedPipeMessage? namedPipeMessage = null;

        try
        {
            namedPipeMessage = await SendNamedPipeRequest(topic, interfaceName);

            var originalPhysicalAddressResult = SourceGeneratedTypeSerializer
                .Deserialize(
                    namedPipeMessage.Content,
                    CommunicationJsonContext.Default.GetOriginalPhysicalAddressResult);

            if (originalPhysicalAddressResult.Status == OperationStatus.Error ||
                originalPhysicalAddressResult.OriginalPhysicalAddress is null)
            {
                OutputError(topic, originalPhysicalAddressResult);
                return null;
            }

            return originalPhysicalAddressResult.OriginalPhysicalAddress;
        }
        catch (Exception ex)
        {
            OutputException(topic, namedPipeMessage, ex);
            return null;
        }
    }

    /// <summary>
    /// Sends a named pipe request to get the network status information.
    /// </summary>
    /// <param name="interfaceName">The interface name to get network status information for.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The <see cref="NetworkStatusInformation"/> or <see langword="null"/> if getting
    /// the status information failed.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal static async Task<NetworkStatusInformation?> GetNetworkStatusInformation(
        string interfaceName,
        CancellationToken cancellationToken)
    {
        var topic = Topics.GetNetworkStatusInformation;
        NamedPipeMessage? namedPipeMessage = null;

        try
        {
            namedPipeMessage = await SendNamedPipeRequest(topic, interfaceName, cancellationToken);

            var networkStatusResult = SourceGeneratedTypeSerializer
                .Deserialize(
                    namedPipeMessage.Content,
                    CommunicationJsonContext.Default.GetNetworkStatusInformationResult);

            if (networkStatusResult.Status == OperationStatus.Error)
            {
                OutputError(topic, networkStatusResult);
                return null;
            }

            if (networkStatusResult.Status == OperationStatus.Warning)
                OutputWarning(topic, networkStatusResult);

            return networkStatusResult.NetworkStatusInformation;
        }
        catch (Exception ex)
        {
            OutputException(topic, namedPipeMessage, ex);
            return null;
        }
    }

    /// <summary>
    /// Sends a named pipe request to get the DHCP lease information.
    /// </summary>
    /// <param name="interfaceName">The interface name for which the DHCP lease information shall
    /// be retrieved.</param>
    /// <returns>The <see cref="DHCPLease"/> or <see langword="null"/> if the request failed.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal static async Task<DHCPLease?> GetDHCPLeaseInformation(string interfaceName)
    {
        var topic = Topics.GetDHCPLeaseInformation;
        NamedPipeMessage? namedPipeMessage = null;

        try
        {
            namedPipeMessage = await SendNamedPipeRequest(topic, interfaceName);

            var getDhcpLeaseResult = SourceGeneratedTypeSerializer
                .Deserialize(
                    namedPipeMessage.Content,
                    CommunicationJsonContext.Default.GetDHCPLeaseInformationResult);

            return getDhcpLeaseResult.DHCPLease;
        }
        catch (Exception ex)
        {
            OutputException(topic, namedPipeMessage, ex);
            return null;
        }
    }

    /// <summary>
    /// Sends a named pipe request to renew the DHCP lease.
    /// </summary>
    /// <param name="interfaceName">The interface name for which the DHCP lease shall be renewed.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The <see cref="DHCPLease"/> or <see langword="null"/> if renewing the DHCP lease failed.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal static async Task<DHCPLease?> RenewDHCPLease(string interfaceName, CancellationToken cancellationToken)
    {
        var topic = Topics.RenewDHCPLease;
        NamedPipeMessage? namedPipeMessage = null;

        try
        {
            namedPipeMessage = await SendNamedPipeRequest(topic, interfaceName, cancellationToken);

            var renewDhcpLeaseResult = SourceGeneratedTypeSerializer
                .Deserialize(
                    namedPipeMessage.Content,
                    CommunicationJsonContext.Default.GetDHCPLeaseInformationResult);

            return renewDhcpLeaseResult.DHCPLease;
        }
        catch (Exception ex)
        {
            OutputException(topic, namedPipeMessage, ex);
            return null;
        }
    }

    /// <summary>
    /// Sends a named pipe request to redeploy the application scripts.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><see langword="true"/> if successful, otherwise <see langword="false"/></returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal static async Task<bool> RedeployScripts(CancellationToken cancellationToken)
    {
        var topic = Topics.RedeployScripts;
        NamedPipeMessage? namedPipeMessage = null;

        try
        {
            namedPipeMessage = await SendNamedPipeRequest(topic, null, cancellationToken);

            var redeployScriptsResult = SourceGeneratedTypeSerializer
                .Deserialize(
                    namedPipeMessage.Content,
                    CommunicationJsonContext.Default.RedeployScriptsResult);

            if (redeployScriptsResult.Status == OperationStatus.Error)
            {
                OutputError(topic, redeployScriptsResult);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            OutputException(topic, namedPipeMessage, ex);
            return false;
        }
    }

    /// <summary>
    /// Sends a named pipe request to get the current state of a systemd unit.
    /// </summary>
    /// <param name="systemdUnitName">The systemd unit name.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The active state.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal static async Task<string?> GetSystemdUnitActiveState(
        string systemdUnitName,
        CancellationToken cancellationToken)
    {
        var topic = Topics.GetServiceState;
        NamedPipeMessage? namedPipeMessage = null;

        try
        {
            namedPipeMessage = await SendNamedPipeRequest(topic, systemdUnitName, cancellationToken);

            var getServiceStateResult = SourceGeneratedTypeSerializer
                .Deserialize(
                    namedPipeMessage.Content,
                    CommunicationJsonContext.Default.GetServiceStateResult);

            if (getServiceStateResult.Status == OperationStatus.Error)
            {
                OutputError(topic, getServiceStateResult);
                return null;
            }

            return getServiceStateResult.ActiveState;
        }
        catch (Exception ex)
        {
            OutputException(topic, namedPipeMessage, ex);
            return null;
        }
    }

    /// <summary>
    /// Sends a named pipe request to start, stop or restart a systemd unit.
    /// </summary>
    /// <param name="systemdUnitName">The systemd unit name.</param>
    /// <param name="controlAction">The control action.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><see langword="true"/> if successful, otherwise <see langword="false"/></returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal static async Task<bool> ControlSystemdUnit(
        string systemdUnitName,
        ServiceControlAction controlAction,
        CancellationToken cancellationToken)
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
            namedPipeMessage = await SendNamedPipeRequest(topic, systemdUnitName, cancellationToken);

            var serviceControlResult = SourceGeneratedTypeSerializer
                .Deserialize(
                    namedPipeMessage.Content,
                    CommunicationJsonContext.Default.ServiceControlResult);

            if (serviceControlResult.Status == OperationStatus.Error)
            {
                OutputError(topic, serviceControlResult);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            OutputException(topic, namedPipeMessage, ex);
            return false;
        }
    }

    /// <summary>
    /// Sends a named pipe request to reset the system.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><see langword="true"/> if successful, otherwise <see langword="false"/></returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal static async Task<bool> ResetSystem(CancellationToken cancellationToken)
    {
        var topic = Topics.ResetSystem;
        NamedPipeMessage? namedPipeMessage = null;

        try
        {
            namedPipeMessage = await SendNamedPipeRequest(topic, null, cancellationToken);

            var systemControlResult = SourceGeneratedTypeSerializer
                .Deserialize(
                    namedPipeMessage.Content,
                    CommunicationJsonContext.Default.SystemControlResult);

            if (systemControlResult.Status == OperationStatus.Error)
            {
                OutputError(topic, systemControlResult);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            OutputException(topic, namedPipeMessage, ex);
            return false;
        }
    }

    /// <summary>
    /// Sends a named pipe request to restart the system.
    /// </summary>
    /// <param name="delayInSeconds">The delay in seconds.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An optional message.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal static async Task<string?> RestartSystem(int delayInSeconds, CancellationToken cancellationToken)
    {
        var topic = Topics.RestartSystem;
        NamedPipeMessage? namedPipeMessage = null;

        try
        {
            namedPipeMessage = await SendNamedPipeRequest(topic, delayInSeconds.ToString(), cancellationToken);

            var systemControlResult = SourceGeneratedTypeSerializer
                .Deserialize(
                    namedPipeMessage.Content,
                    CommunicationJsonContext.Default.SystemControlResult);

            if (systemControlResult.Status == OperationStatus.Error)
            {
                OutputError(topic, systemControlResult);
                return null;
            }

            return systemControlResult.Message;
        }
        catch (Exception ex)
        {
            OutputException(topic, namedPipeMessage, ex);
            return null;
        }
    }

    /// <summary>
    /// Sends a named pipe request to shutdown the system.
    /// </summary>
    /// <param name="delayInSeconds">The delay in seconds.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An optional message.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal static async Task<string?> ShutdownSystem(int delayInSeconds, CancellationToken cancellationToken)
    {
        var topic = Topics.ShutdownSystem;
        NamedPipeMessage? namedPipeMessage = null;

        try
        {
            namedPipeMessage = await SendNamedPipeRequest(topic, delayInSeconds.ToString(), cancellationToken);

            var systemControlResult = SourceGeneratedTypeSerializer
                .Deserialize(
                    namedPipeMessage.Content,
                    CommunicationJsonContext.Default.SystemControlResult);

            if (systemControlResult.Status == OperationStatus.Error)
            {
                OutputError(topic, systemControlResult);
                return null;
            }

            return systemControlResult.Message;
        }
        catch (Exception ex)
        {
            OutputException(topic, namedPipeMessage, ex);
            return null;
        }
    }

    /// <summary>
    /// Sends a named pipe request to update the system.
    /// </summary>
    /// <param name="filePath">The file path to the update file.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><see langword="true"/> if successful, otherwise <see langword="false"/></returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal static async Task<bool> UpdateSystem(string filePath, CancellationToken cancellationToken)
    {
        var topic = Topics.UpdateSystem;
        NamedPipeMessage? namedPipeMessage = null;

        try
        {
            namedPipeMessage = await SendNamedPipeRequest(topic, filePath, cancellationToken);

            var systemControlResult = SourceGeneratedTypeSerializer
                .Deserialize(
                    namedPipeMessage.Content,
                    CommunicationJsonContext.Default.SystemControlResult);

            if (systemControlResult.Status == OperationStatus.Error)
            {
                OutputError(topic, systemControlResult);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            OutputException(topic, namedPipeMessage, ex);
            return false;
        }
    }

    private static async Task<NamedPipeMessage> SendNamedPipeRequest(
        string topic,
        string? content,
        CancellationToken cancellationToken = default)
    {
        PipeClient? client = null;

        try
        {
            client = new PipeClient("HostPipe", s_logger);

            if (!s_validTopics.Contains(topic))
                throw new InvalidOperationException($"Topic '{topic}' is not supported.");

            if (s_topicsWithRequiredContent.Contains(topic) && content is null)
                throw new InvalidOperationException($"Parameter '{nameof(content)}' cannot be null for topic '{topic}'.");

            return content is null
                ? await client.SendRequest(topic, cancellationToken: cancellationToken)
                : await client.SendRequest(topic, content, cancellationToken);
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
            await Task.Delay(250, cancellationToken);
        }
    }

    private static void OutputException(string topic, NamedPipeMessage? namedPipeMessage, Exception exception)
    {
        var message = $"Named pipe request for topic '{topic}' had an exception. {exception.Message}";

        if (namedPipeMessage?.Content is string content)
            message += $" Message content was: {content}";

        SimpleOutput.WriteLineError(message);
    }

    private static void OutputError(string topic, OperationResult operationResult)
    {
        var message = operationResult.Message
            ?? $"Named pipe request for topic '{topic}' returned an error result but the message was null.";

        SimpleOutput.WriteLineError(message);
    }

    private static void OutputWarning(string topic, OperationResult operationResult)
    {
        var message = operationResult.Message
            ?? $"Named pipe request for topic '{topic}' returned a warning result but the message was null.";

        SimpleOutput.WriteLineWarning(message);
    }
}
