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
using HostManagement.Shared.Serialization;
using Microsoft.Extensions.Logging;
using CommunicationJsonContext = HostManagement.Shared.Communication.Contracts.SourceGenerationContext;

namespace HostManagement.Communication.CommandLine.Helper;

/// <summary>
/// Provides helper methods to send named pipe requests, e.g. to get or set a
/// <see cref="SystemConfiguration"/>.
/// </summary>
internal static partial class NamedPipeRequestHelper
{
    private static readonly List<string> s_validTopics = [.. Topics.GetAllTopics()];
    private static readonly List<string> s_topicsWithRequiredContent = [.. Topics.GetTopicsWithRequiredContent()];

    private static readonly ILogger s_logger = new ConsoleLogger(
        EnvironmentVariables.LogLevel,
        useShortLogLevels: false,
        addNewLine: true);

    /// <summary>
    /// Sends a named pipe request to get the <see cref="SystemConfiguration"/> from the system.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="SystemConfiguration"/> or <see langword="null"/> if the configuration
    /// could not be loaded.</returns>
    internal static async Task<SystemConfiguration?> GetConfiguration(CancellationToken cancellationToken)
    {
        NamedPipeMessage? namedPipeMessage = null;

        try
        {
            namedPipeMessage = await SendNamedPipeRequest(Topics.GetSystemConfiguration, null, cancellationToken);

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
                LogGettingConfigurationReturnedNull(s_logger, getConfigResult.Message);
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

            LogGettingConfigurationFailed(s_logger, message);
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
        NamedPipeMessage? namedPipeMessage = null;

        try
        {
            var systemConfigurationJson = SystemConfigurationSerializer.Serialize(config);

            namedPipeMessage = await SendNamedPipeRequest(
                Topics.SetSystemConfiguration,
                systemConfigurationJson,
                cancellationToken);

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
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A list of compatible non-pre-release ViciOne Suite versions or an empty collection</returns>
    internal static async Task<List<string>> GetNonPreReleaseViciOneSuiteVersions(CancellationToken cancellationToken)
    {
        NamedPipeMessage? namedPipeMessage = null;

        try
        {
            namedPipeMessage = await SendNamedPipeRequest(Topics.GetViciOneSuiteVersions, null, cancellationToken);

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
        LogViciOneSuitePreReleaseRequestedFromCli(s_logger, cliMessageContent.MessageContent);

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
        NamedPipeMessage? namedPipeMessage = null;

        try
        {
            namedPipeMessage = await SendNamedPipeRequest(
                Topics.InstallViciOneSuiteVersion,
                messageContent,
                cancellationToken);

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
    /// Sends a named pipe request to install the specified signed Debian package.
    /// </summary>
    /// <param name="messageContent">The message content containing the package and signature file paths.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><see langword="true"/> if successful, otherwise <see langword="false"/></returns>
    internal static async Task<bool> InstallSignedDebianPackage(
        string messageContent,
        CancellationToken cancellationToken)
    {
        NamedPipeMessage? namedPipeMessage = null;

        try
        {
            namedPipeMessage = await SendNamedPipeRequest(
                Topics.InstallSignedDebianPackage,
                messageContent,
                cancellationToken);

            var installDebianPackageResult = SourceGeneratedTypeSerializer
                .Deserialize(
                    namedPipeMessage.Content,
                    CommunicationJsonContext.Default.SystemControlResult);

            if (installDebianPackageResult.Status == OperationStatus.Error)
            {
                LogResultReturnedError(s_logger, installDebianPackageResult.Message);
                return false;
            }

            if (installDebianPackageResult.Status == OperationStatus.Warning)
            {
                LogResultReturnedWarning(s_logger, installDebianPackageResult.Message);
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

            LogInstallDebianPackageFailed(s_logger, message);
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
        NamedPipeMessage? namedPipeMessage = null;

        try
        {
            namedPipeMessage = await SendNamedPipeRequest(Topics.GetNTPFallbackInformation, null);

            var ntpFallbackInformationResult = SourceGeneratedTypeSerializer
                .Deserialize(
                    namedPipeMessage.Content,
                    CommunicationJsonContext.Default.GetNTPFallbackInformationResult);

            if (ntpFallbackInformationResult.Status == OperationStatus.Error)
            {
                LogResultReturnedError(s_logger, ntpFallbackInformationResult.Message);
                return [];
            }

            return ntpFallbackInformationResult.FallbackNTPServers;
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

            LogGettingNTPFallbackInformationFailed(s_logger, message);
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
        NamedPipeMessage? namedPipeMessage = null;

        try
        {
            namedPipeMessage = await SendNamedPipeRequest(Topics.GetOriginalPhysicalAddress, interfaceName);

            var originalPhysicalAddressResult = SourceGeneratedTypeSerializer
                .Deserialize(
                    namedPipeMessage.Content,
                    CommunicationJsonContext.Default.GetOriginalPhysicalAddressResult);

            if (originalPhysicalAddressResult.Status == OperationStatus.Error)
            {
                LogResultReturnedError(s_logger, originalPhysicalAddressResult.Message);
                return null;
            }

            if (originalPhysicalAddressResult.OriginalPhysicalAddress is null)
            {
                GettingOriginalPhysicalAddressReturnedNull(s_logger, originalPhysicalAddressResult.Message);
                return null;
            }

            return originalPhysicalAddressResult.OriginalPhysicalAddress;
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

            GettingOriginalPhysicalAddressFailed(s_logger, message);
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
        NamedPipeMessage? namedPipeMessage = null;

        try
        {
            namedPipeMessage = await SendNamedPipeRequest(
                Topics.GetNetworkStatusInformation,
                interfaceName,
                cancellationToken);

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
                LogGettingNetworkStatusInformationReturnedNull(s_logger, networkStatusResult.Message);
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
    /// Sends a named pipe request to get the DHCP lease information.
    /// </summary>
    /// <param name="interfaceName">The interface name for which the DHCP lease information shall
    /// be retrieved.</param>
    /// <returns>The <see cref="DHCPLease"/> or <see langword="null"/> if the request failed.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal static async Task<DHCPLease?> GetDHCPLeaseInformation(string interfaceName)
    {
        NamedPipeMessage? namedPipeMessage = null;

        try
        {
            namedPipeMessage = await SendNamedPipeRequest(Topics.GetDHCPLeaseInformation, interfaceName);

            var getDhcpLeaseResult = SourceGeneratedTypeSerializer
                .Deserialize(
                    namedPipeMessage.Content,
                    CommunicationJsonContext.Default.GetDHCPLeaseInformationResult);

            return getDhcpLeaseResult.DHCPLease;
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

            LogError(s_logger, message);
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
        NamedPipeMessage? namedPipeMessage = null;

        try
        {
            namedPipeMessage = await SendNamedPipeRequest(Topics.RenewDHCPLease, interfaceName, cancellationToken);

            var renewDhcpLeaseResult = SourceGeneratedTypeSerializer
                .Deserialize(
                    namedPipeMessage.Content,
                    CommunicationJsonContext.Default.GetDHCPLeaseInformationResult);

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

            LogError(s_logger, message);
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
        NamedPipeMessage? namedPipeMessage = null;

        try
        {
            namedPipeMessage = await SendNamedPipeRequest(Topics.RedeployScripts, null, cancellationToken);

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
        NamedPipeMessage? namedPipeMessage = null;

        try
        {
            namedPipeMessage = await SendNamedPipeRequest(Topics.GetServiceState, systemdUnitName, cancellationToken);

            var getServiceStateResult = SourceGeneratedTypeSerializer
                .Deserialize(
                    namedPipeMessage.Content,
                    CommunicationJsonContext.Default.GetServiceStateResult);

            if (getServiceStateResult.Status == OperationStatus.Error)
            {
                LogResultReturnedError(s_logger, getServiceStateResult.Message);
                return null;
            }

            return getServiceStateResult.ActiveState;
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

            GetSystemdUnitStateFailed(s_logger, message);
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
                LogControlSystemdUnitFailed(s_logger, serviceControlResult.Message);
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

            LogControlSystemdUnitFailedWithException(s_logger, controlAction.ToString(), message);
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
        NamedPipeMessage? namedPipeMessage = null;

        try
        {
            namedPipeMessage = await SendNamedPipeRequest(Topics.ResetSystem, null, cancellationToken);

            var systemControlResult = SourceGeneratedTypeSerializer
                .Deserialize(
                    namedPipeMessage.Content,
                    CommunicationJsonContext.Default.SystemControlResult);

            if (systemControlResult.Status == OperationStatus.Error)
            {
                LogResetSystemFailed(s_logger, systemControlResult.Message);
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
    /// <param name="delayInSeconds">The delay in seconds.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An optional message.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal static async Task<string?> RestartSystem(int delayInSeconds, CancellationToken cancellationToken)
    {
        NamedPipeMessage? namedPipeMessage = null;

        try
        {
            namedPipeMessage = await SendNamedPipeRequest(
                Topics.RestartSystem,
                delayInSeconds.ToString(),
                cancellationToken);

            var systemControlResult = SourceGeneratedTypeSerializer
                .Deserialize(
                    namedPipeMessage.Content,
                    CommunicationJsonContext.Default.SystemControlResult);

            if (systemControlResult.Status == OperationStatus.Error)
            {
                LogRestartSystemFailed(s_logger, systemControlResult.Message);
                return null;
            }

            return systemControlResult.Message;
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
        NamedPipeMessage? namedPipeMessage = null;

        try
        {
            namedPipeMessage = await SendNamedPipeRequest(
                Topics.ShutdownSystem,
                delayInSeconds.ToString(),
                cancellationToken);

            var systemControlResult = SourceGeneratedTypeSerializer
                .Deserialize(
                    namedPipeMessage.Content,
                    CommunicationJsonContext.Default.SystemControlResult);

            if (systemControlResult.Status == OperationStatus.Error)
            {
                LogShutdownSystemFailed(s_logger, systemControlResult.Message);
                return null;
            }

            return systemControlResult.Message;
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

            LogShutdownSystemFailed(s_logger, message);
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
        NamedPipeMessage? namedPipeMessage = null;

        try
        {
            namedPipeMessage = await SendNamedPipeRequest(Topics.UpdateSystem, filePath, cancellationToken);

            var systemControlResult = SourceGeneratedTypeSerializer
                .Deserialize(
                    namedPipeMessage.Content,
                    CommunicationJsonContext.Default.SystemControlResult);

            if (systemControlResult.Status == OperationStatus.Error)
            {
                LogUpdateSystemFailed(s_logger, systemControlResult.Message);
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

            LogUpdateSystemFailed(s_logger, message);
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

    // General logs
    [LoggerMessage(LogLevel.Error, "{InnerMessage}")]
    private static partial void LogError(ILogger logger, string? innerMessage);

    [LoggerMessage(LogLevel.Error, "{InnerMessage}")]
    private static partial void LogResultReturnedError(ILogger logger, string? innerMessage);

    [LoggerMessage(LogLevel.Warning, "{InnerMessage}")]
    private static partial void LogResultReturnedWarning(ILogger logger, string? innerMessage);

    [LoggerMessage(LogLevel.Information, "{InnerMessage}")]
    private static partial void LogResultReturnedSuccess(ILogger logger, string? innerMessage);

    // Logs for getting or setting the system configuration
    [LoggerMessage(LogLevel.Error, "Getting configuration failed. {InnerMessage}")]
    private static partial void LogGettingConfigurationFailed(ILogger logger, string? innerMessage);

    [LoggerMessage(LogLevel.Error, "Getting configuration returned 'null'. {InnerMessage}")]
    private static partial void LogGettingConfigurationReturnedNull(ILogger logger, string? innerMessage);

    [LoggerMessage(LogLevel.Error, "Setting configuration failed. {InnerMessage}")]
    private static partial void LogSetConfigurationFailed(ILogger logger, string? innerMessage);

    // Logs related to ViciOne Suite
    [LoggerMessage(LogLevel.Warning, "Install of pre-release ViciOne Suite version '{SuiteVersion}' was requested from CLI.")]
    private static partial void LogViciOneSuitePreReleaseRequestedFromCli(ILogger logger, string? suiteVersion);

    [LoggerMessage(LogLevel.Error, "Get ViciOne Suite versions failed. {InnerMessage}")]
    private static partial void LogGetViciOneSuiteVersionsFailed(ILogger logger, string? innerMessage);

    [LoggerMessage(LogLevel.Error, "Install ViciOne Suite version failed. {InnerMessage}")]
    private static partial void LogInstallViciOneSuiteVersionFailed(ILogger logger, string? innerMessage);

    [LoggerMessage(LogLevel.Error, "Install Debian package failed. {InnerMessage}")]
    private static partial void LogInstallDebianPackageFailed(ILogger logger, string? innerMessage);

    // Logs for getting original physical address
    [LoggerMessage(LogLevel.Error, "Getting original physical address failed. {InnerMessage}")]
    private static partial void GettingOriginalPhysicalAddressFailed(ILogger logger, string? innerMessage);

    [LoggerMessage(LogLevel.Error, "Getting original physical address returned 'null'. {InnerMessage}")]
    private static partial void GettingOriginalPhysicalAddressReturnedNull(ILogger logger, string? innerMessage);

    // Logs for fallback ntp servers
    [LoggerMessage(LogLevel.Error, "Getting fallback NTP information failed. {InnerMessage}")]
    private static partial void LogGettingNTPFallbackInformationFailed(ILogger logger, string? innerMessage);

    // Logs for network test
    [LoggerMessage(LogLevel.Error, "Getting network status information failed. {InnerMessage}")]
    private static partial void LogGettingNetworkStatusInformationFailed(ILogger logger, string? innerMessage);

    [LoggerMessage(LogLevel.Error, "Getting network status information returned 'null'. {InnerMessage}")]
    private static partial void LogGettingNetworkStatusInformationReturnedNull(ILogger logger, string? innerMessage);

    // Logs for DHCP lease renew
    [LoggerMessage(LogLevel.Error, "Renew DHCP lease failed. {InnerMessage}")]
    private static partial void LogRenewDHCPLeaseFailed(ILogger logger, string? innerMessage);

    // Logs for script redeployment
    [LoggerMessage(LogLevel.Error, "Redeploy scripts failed. {InnerMessage}")]
    private static partial void LogRedeployScriptsFailed(ILogger logger, string? innerMessage);

    // Logs for systemd units
    [LoggerMessage(LogLevel.Error, "Getting systemd unit state failed. {ExceptionMessage}")]
    private static partial void GetSystemdUnitStateFailed(ILogger logger, string exceptionMessage);

    [LoggerMessage(LogLevel.Error, "{Message}")]
    private static partial void LogControlSystemdUnitFailed(ILogger logger, string? message);

    [LoggerMessage(LogLevel.Error, "{ControlAction} systemd unit failed. {ExceptionMessage}")]
    private static partial void LogControlSystemdUnitFailedWithException(ILogger logger, string controlAction, string exceptionMessage);

    // Logs for system reset
    [LoggerMessage(LogLevel.Error, "Reset system failed. {InnerMessage}")]
    private static partial void LogResetSystemFailed(ILogger logger, string? innerMessage);

    // Logs for system restart
    [LoggerMessage(LogLevel.Error, "Restart system failed. {InnerMessage}")]
    private static partial void LogRestartSystemFailed(ILogger logger, string? innerMessage);

    // Logs for system shutdown
    [LoggerMessage(LogLevel.Error, "Shutdown system failed. {InnerMessage}")]
    private static partial void LogShutdownSystemFailed(ILogger logger, string? innerMessage);

    // Logs for system update
    [LoggerMessage(LogLevel.Error, "Update system failed. {InnerMessage}")]
    private static partial void LogUpdateSystemFailed(ILogger logger, string? innerMessage);
}
