using System.Net.NetworkInformation;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Communication.NamedPipe.Client;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Contracts.NetworkStatus;
using HostManagement.Shared.Contracts.Service;
using HostManagement.Shared.Contracts.System;
using HostManagement.Shared.Enums;
using Microsoft.Extensions.Logging;

namespace HostManagement.Communication.CommandLine.Helper;

/// <summary>
/// Provides helper methods to send named pipe requests, e.g. to get or set a <see cref="SystemConfiguration"/>.
/// </summary>
internal static class NamedPipeRequestHelper
{
    private static readonly PipeClient s_pipeClient = CreatePipeClientAndConnectToServer();

    /// <summary>
    /// Sends a named pipe request to get the <see cref="SystemConfiguration"/> from the system.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="SystemConfiguration"/> or <see langword="null"/> if the configuration
    /// could not be loaded.</returns>
    internal static async Task<SystemConfiguration?> GetConfiguration(CancellationToken cancellationToken)
    {
        var topic = TopicContracts.GetSystemConfiguration;

        var getConfigResult = await s_pipeClient.SendOperationAsync(
            topic,
            cancellationToken: cancellationToken);

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

    /// <summary>
    /// Sends a named pipe request to set a <see cref="SystemConfiguration"/> to the system.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <param name="config">The <see cref="SystemConfiguration"/> to apply.</param>
    /// <returns><see langword="true"/> if successful, otherwise <see langword="false"/></returns>
    internal static async Task<bool> SetConfiguration(SystemConfiguration config, CancellationToken cancellationToken)
    {
        var topic = TopicContracts.SetSystemConfiguration;
        SetSystemConfigurationResult setConfigResult;

        try
        {
            setConfigResult = await s_pipeClient.SendOperationAsync(topic, config, cancellationToken: cancellationToken);
        }
        catch (NotSupportedException ex)
        {
            var message = $"Could not serialize system configuration for request with topic '{topic}'. {ex.Message}";

            SimpleOutput.WriteLineError(message);
            return false;
        }

        if (setConfigResult.Status == OperationStatus.Error)
        {
            OutputError(topic, setConfigResult);
            return false;
        }

        if (setConfigResult.Status == OperationStatus.Warning)
            OutputWarning(topic, setConfigResult);

        return true;
    }

    /// <summary>
    /// Sends a named pipe request to install the specified signed Debian package.
    /// </summary>
    /// <param name="signedDebianPackage">The package and signature file paths.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><see langword="true"/> if successful, otherwise <see langword="false"/></returns>
    internal static async Task<bool> InstallSignedDebianPackage(
        SignedDebianPackage signedDebianPackage,
        CancellationToken cancellationToken)
    {
        var topic = TopicContracts.InstallSignedDebianPackage;

        var installDebianPackageResult = await s_pipeClient.SendOperationAsync(
            topic,
            signedDebianPackage,
            cancellationToken: cancellationToken);

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

    /// <summary>
    /// Sends a named pipe request to get the NTP fallback information.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The list of fallback NTP servers or an empty list if the request failed.</returns>
    internal static async Task<List<string>> GetNTPFallbackInformation(CancellationToken cancellationToken)
    {
        var topic = TopicContracts.GetNTPFallbackInformation;

        var ntpFallbackInformationResult = await s_pipeClient.SendOperationAsync(
            topic,
            cancellationToken: cancellationToken);

        if (ntpFallbackInformationResult.Status == OperationStatus.Error)
        {
            OutputError(topic, ntpFallbackInformationResult);
            return [];
        }

        return ntpFallbackInformationResult.FallbackNTPServers;
    }

    internal static async Task<NTPSyncStatus?> GetNTPSyncStatus(CancellationToken cancellationToken)
    {
        var topic = TopicContracts.GetNTPSyncStatus;

        var ntpSyncStatusResult = await s_pipeClient.SendOperationAsync(
            topic,
            cancellationToken: cancellationToken);

        if (ntpSyncStatusResult.Status == OperationStatus.Error)
            OutputError(topic, ntpSyncStatusResult);

        return ntpSyncStatusResult.NTPSyncStatus;
    }

    /// <summary>
    /// Sends a named pipe request to get the original physical address.
    /// </summary>
    /// <param name="interfaceName">The interface name to get the original physical address for.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The original physical address or <see langword="null"/> if getting the original
    /// physical address failed.</returns>
    internal static async Task<PhysicalAddress?> GetOriginalPhysicalAddress(
        string interfaceName,
        CancellationToken cancellationToken)
    {
        var topic = TopicContracts.GetOriginalPhysicalAddress;

        var originalPhysicalAddressResult = await s_pipeClient.SendOperationAsync(
            topic,
            interfaceName,
            cancellationToken: cancellationToken);

        if (originalPhysicalAddressResult.Status == OperationStatus.Error ||
            originalPhysicalAddressResult.OriginalPhysicalAddress is null)
        {
            OutputError(topic, originalPhysicalAddressResult);
            return null;
        }

        return originalPhysicalAddressResult.OriginalPhysicalAddress;
    }

    /// <summary>
    /// Sends a named pipe request to get the network status information.
    /// </summary>
    /// <param name="interfaceName">The interface name to get network status information for.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The <see cref="NetworkStatusInformation"/> or <see langword="null"/> if getting
    /// the status information failed.</returns>
    internal static async Task<NetworkStatusInformation?> GetNetworkStatusInformation(
        string interfaceName,
        CancellationToken cancellationToken)
    {
        var topic = TopicContracts.GetNetworkStatusInformation;

        var networkStatusResult = await s_pipeClient.SendOperationAsync(
            topic,
            interfaceName,
            cancellationToken: cancellationToken);

        if (networkStatusResult.Status == OperationStatus.Error)
        {
            OutputError(topic, networkStatusResult);
            return null;
        }

        if (networkStatusResult.Status == OperationStatus.Warning)
            OutputWarning(topic, networkStatusResult);

        return networkStatusResult.NetworkStatusInformation;
    }

    /// <summary>
    /// Sends a named pipe request to get the DHCP lease information.
    /// </summary>
    /// <param name="interfaceName">
    /// The interface name for which the DHCP lease information shall be retrieved.
    /// </param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The <see cref="DHCPLease"/> or <see langword="null"/> if the request failed.</returns>
    internal static async Task<DHCPLease?> GetDHCPLeaseInformation(string interfaceName, CancellationToken cancellationToken)
        => await GetDHCPLease(TopicContracts.GetDHCPLeaseInformation, interfaceName, cancellationToken);

    /// <summary>
    /// Sends a named pipe request to renew the DHCP lease.
    /// </summary>
    /// <param name="interfaceName">The interface name for which the DHCP lease shall be renewed.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The <see cref="DHCPLease"/> or <see langword="null"/> if renewing the DHCP lease failed.</returns>
    internal static async Task<DHCPLease?> RenewDHCPLease(string interfaceName, CancellationToken cancellationToken)
        => await GetDHCPLease(TopicContracts.RenewDHCPLease, interfaceName, cancellationToken);

    /// <summary>
    /// Sends a named pipe request to reset the specified network interface to its default configuration.
    /// </summary>
    /// <param name="interfaceName">The interface name.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><see langword="true"/> if successful, otherwise <see langword="false"/></returns>
    internal static async Task<bool> ResetNetworkInterface(string interfaceName, CancellationToken cancellationToken)
        => await ControlSystem(TopicContracts.ResetNetworkInterface, interfaceName, cancellationToken);

    /// <summary>
    /// Sends a named pipe request to redeploy the application scripts.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><see langword="true"/> if successful, otherwise <see langword="false"/></returns>
    internal static async Task<bool> RedeployScripts(CancellationToken cancellationToken)
    {
        var topic = TopicContracts.RedeployScripts;

        var redeployScriptsResult = await s_pipeClient.SendOperationAsync(
            topic,
            cancellationToken: cancellationToken);

        if (redeployScriptsResult.Status == OperationStatus.Error)
        {
            OutputError(topic, redeployScriptsResult);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Sends a named pipe request to get the current state of a systemd unit.
    /// </summary>
    /// <param name="systemdUnitName">The systemd unit name.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The active state.</returns>
    internal static async Task<string?> GetSystemdUnitActiveState(
        string systemdUnitName,
        CancellationToken cancellationToken)
    {
        var topic = TopicContracts.GetServiceState;

        var getServiceStateResult = await s_pipeClient.SendOperationAsync(
            topic,
            systemdUnitName,
            cancellationToken: cancellationToken);

        if (getServiceStateResult.Status == OperationStatus.Error)
        {
            OutputError(topic, getServiceStateResult);
            return null;
        }

        return getServiceStateResult.ActiveState;
    }

    /// <summary>
    /// Sends a named pipe request to start, stop or restart a systemd unit.
    /// </summary>
    /// <param name="request">The name of the systemd unit and an optional delay.</param>
    /// <param name="controlAction">The control action.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><see langword="true"/> if successful, otherwise <see langword="false"/></returns>
    internal static async Task<bool> ControlSystemdUnit(
        ServiceControlRequest request,
        ServiceControlAction controlAction,
        CancellationToken cancellationToken)
    {
        var topic = controlAction switch
        {
            ServiceControlAction.Start => TopicContracts.StartService,
            ServiceControlAction.Stop => TopicContracts.StopService,
            ServiceControlAction.Restart => TopicContracts.RestartService,
            _ => throw new ArgumentOutOfRangeException(nameof(controlAction), controlAction, null)
        };

        var serviceControlResult = await s_pipeClient.SendOperationAsync(
            topic,
            request,
            cancellationToken: cancellationToken);

        if (serviceControlResult.Status == OperationStatus.Error)
        {
            OutputError(topic, serviceControlResult);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Sends a named pipe request to reset the system.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><see langword="true"/> if successful, otherwise <see langword="false"/></returns>
    internal static async Task<bool> ResetSystem(CancellationToken cancellationToken)
        => await ControlSystem(TopicContracts.ResetSystem, null, cancellationToken);

    /// <summary>
    /// Sends a named pipe request to restart the system.
    /// </summary>
    /// <param name="delayInSeconds">The delay in seconds.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An optional message.</returns>
    internal static async Task<string?> RestartSystem(int delayInSeconds, CancellationToken cancellationToken)
        => await ShutdownOrRestart(TopicContracts.RestartSystem, delayInSeconds, cancellationToken);

    /// <summary>
    /// Sends a named pipe request to shutdown the system.
    /// </summary>
    /// <param name="delayInSeconds">The delay in seconds.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An optional message.</returns>
    internal static async Task<string?> ShutdownSystem(int delayInSeconds, CancellationToken cancellationToken)
        => await ShutdownOrRestart(TopicContracts.ShutdownSystem, delayInSeconds, cancellationToken);

    /// <summary>
    /// Sends a named pipe request to update the system.
    /// </summary>
    /// <param name="filePath">The file path to the update file.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><see langword="true"/> if successful, otherwise <see langword="false"/></returns>
    internal static async Task<bool> UpdateSystem(string filePath, CancellationToken cancellationToken)
        => await ControlSystem(TopicContracts.UpdateSystem, filePath, cancellationToken);

    private static async Task<bool> ControlSystem(
        TopicContract<SystemControlResult> topic,
        string? content,
        CancellationToken cancellationToken)
    {
        var systemControlResult = await s_pipeClient.SendOperationAsync(
            topic,
            content ?? string.Empty,
            cancellationToken: cancellationToken);

        if (systemControlResult.Status == OperationStatus.Error)
        {
            OutputError(topic, systemControlResult);
            return false;
        }

        return true;
    }

    private static async Task<DHCPLease?> GetDHCPLease(
        TopicContract<GetDHCPLeaseInformationResult> topic,
        string interfaceName,
        CancellationToken cancellationToken)
    {
        var getDHCPLeaseInformationResult = await s_pipeClient.SendOperationAsync(
            topic,
            interfaceName,
            cancellationToken: cancellationToken);

        if (getDHCPLeaseInformationResult.Status == OperationStatus.Error)
        {
            OutputError(topic, getDHCPLeaseInformationResult);
            return null;
        }

        return getDHCPLeaseInformationResult.DHCPLease;
    }

    private static async Task<string?> ShutdownOrRestart(
        TopicContract<SystemControlResult> topic,
        int delayInSeconds,
        CancellationToken cancellationToken)
    {
        var systemControlResult = await s_pipeClient.SendOperationAsync(
            topic,
            delayInSeconds.ToString(),
            cancellationToken: cancellationToken);

        if (systemControlResult.Status == OperationStatus.Error)
        {
            OutputError(topic, systemControlResult);
            return null;
        }

        return systemControlResult.Message;
    }

    private static PipeClient CreatePipeClientAndConnectToServer()
    {
        // We have to use 'LogLevel' directly because CLI (non-daemon context) can't use environment variables.
        var pipeClient = new PipeClient("HostPipe", new CommandLineLogger(LogLevel.Warning).As<PipeClient>());

        _ = pipeClient.TryConnectAsync(5_000).Result;

        return pipeClient;
    }

    private static void OutputError(MessageContract topic, OperationResult operationResult)
    {
        var message = operationResult.Message
            ?? $"Named pipe request for topic '{topic}' returned an error result but the message was null.";

        SimpleOutput.WriteLineError(message);
    }

    private static void OutputWarning(MessageContract topic, OperationResult operationResult)
    {
        var message = operationResult.Message
            ?? $"Named pipe request for topic '{topic}' returned a warning result but the message was null.";

        SimpleOutput.WriteLineWarning(message);
    }
}
