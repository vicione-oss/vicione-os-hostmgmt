using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Contracts.Network;
using HostManagement.SystemConfig;
using Microsoft.Extensions.Logging;

namespace HostManagement.Services;

/// <summary>
/// Represents the DHCP lease service.
/// </summary>
/// <param name="logger">An <see cref="ILogger"/> implementation.</param>
internal sealed partial class DHCPLeaseService(ILogger logger)
{
    private readonly ILogger _logger = logger;

    /// <summary>
    /// Gets the DHCP lease information of the specified network interface.
    /// </summary>
    /// <param name="interfaceName">The interface name for which the DHCP lease shall be retrieved.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see cref="GetDHCPLeaseInformationResult"/></returns>
    internal async Task<GetDHCPLeaseInformationResult> GetDHCPLeaseInformation(
        string interfaceName,
        CancellationToken cancellationToken)
    {
        try
        {
            var (result, dhcpLease) = await GetDHCPLease(new ScriptHandler(), interfaceName, cancellationToken)
                .ConfigureAwait(false);

            if (result.OperationStatus is OperationStatus.Error)
                return LogAndReturnGetDHCPLeaseFailed(result.FormattedMessages);

            if (dhcpLease.LeaseObtained is null)
                return LogAndReturnGetDHCPLeaseFailed(ServicesResources.DHCPLeaseEmpty);

            return new GetDHCPLeaseInformationResult
            {
                Status = OperationStatus.Success,
                Message = string.Empty,
                DHCPLease = dhcpLease,
            };
        }
        catch (OperationCanceledException)
        {
            var message = ServicesResources.GetDHCPLeaseCanceled;

            GetDHCPLeaseCanceled(_logger, message);

            return new GetDHCPLeaseInformationResult
            {
                Status = OperationStatus.Warning,
                Message = message,
                DHCPLease = null
            };
        }
        catch (Exception ex)
        {
            return LogAndReturnGetDHCPLeaseFailed(ex.Message);
        }
    }

    /// <summary>
    /// Renews the DHCP lease of the specified network interface.
    /// </summary>
    /// <param name="interfaceName">The interface name for which the DHCP lease shall be renewed.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see cref="GetDHCPLeaseInformationResult"/></returns>
    internal async Task<GetDHCPLeaseInformationResult> RenewDHCPLease(
        string interfaceName,
        CancellationToken cancellationToken)
    {
        try
        {
            var (result, dhcpLease) = await RenewDHCPLease(new ScriptHandler(), interfaceName, cancellationToken);

            if (result.OperationStatus is OperationStatus.Error)
                return LogAndReturnRenewDHCPLeaseFailed(result.FormattedMessages);

            if (dhcpLease.LeaseObtained is null)
                return LogAndReturnRenewDHCPLeaseFailed(ServicesResources.DHCPLeaseEmpty);

            return new GetDHCPLeaseInformationResult
            {
                Status = OperationStatus.Success,
                Message = string.Empty,
                DHCPLease = dhcpLease,
            };
        }
        catch (OperationCanceledException)
        {
            var message = ServicesResources.RenewDHCPLeaseCanceled;

            RenewDHCPLeaseCanceled(_logger, message);

            return new GetDHCPLeaseInformationResult
            {
                Status = OperationStatus.Warning,
                Message = message,
                DHCPLease = null
            };
        }
        catch (Exception ex)
        {
            return LogAndReturnRenewDHCPLeaseFailed(ex.Message);
        }
    }

    private static async Task<(Result, DHCPLease)> RenewDHCPLease(
        ScriptHandler scriptHandler,
        string interfaceName,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var result = await scriptHandler.ExecuteIsNetworkInterfaceAvailableScript(interfaceName).ConfigureAwait(false);

        if (result.OperationStatus is OperationStatus.Warning or OperationStatus.Error)
            return (result, DHCPLease.Empty);

        result.AddResult(await scriptHandler.ExecuteRenewDHCPLeaseScript(interfaceName).ConfigureAwait(false));

        if (result.OperationStatus is OperationStatus.Error)
            return (result, DHCPLease.Empty);

        return await GetDHCPLeaseWithRetry(scriptHandler, interfaceName, cancellationToken);
    }

    private static async Task<(Result, DHCPLease)> GetDHCPLeaseWithRetry(
        ScriptHandler scriptHandler,
        string interfaceName,
        CancellationToken cancellationToken)
    {
        var retryCount = 3;
        Result result;
        DHCPLease dhcpLease;

        do
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Delay(1000, cancellationToken);

            (result, dhcpLease) = await GetDHCPLease(scriptHandler, interfaceName, cancellationToken);
            retryCount--;
        }
        while (IsDHCPLeaseNull(result.OperationStatus, dhcpLease) && retryCount > 0);

        return (result, dhcpLease);
    }

    private static async Task<(Result, DHCPLease)> GetDHCPLease(
        ScriptHandler scriptHandler,
        string interfaceName,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var (result, dhcpLease) = await scriptHandler
            .ExecuteGetInterfaceIPv4DHCPLeaseScript(interfaceName)
            .ConfigureAwait(false);

        if (result.OperationStatus is OperationStatus.Error)
            return (result, DHCPLease.Empty);

        return (result, dhcpLease.ToSharedContracts());
    }

    private static bool IsDHCPLeaseNull(OperationStatus status, DHCPLease dhcpLease)
        => status == OperationStatus.Success && dhcpLease.LeaseObtained is null;

    private GetDHCPLeaseInformationResult LogAndReturnGetDHCPLeaseFailed(
        string additionalMessage,
        bool wasRenewal = false)
    {
        var simpleMessage = wasRenewal ? ServicesResources.RenewDHCPLeaseFailed : ServicesResources.GetDHCPLeaseFailed;
        var message = simpleMessage + Environment.NewLine + additionalMessage;

        GetDHCPLeaseFailed(_logger, message);

        return new GetDHCPLeaseInformationResult
        {
            Status = OperationStatus.Error,
            Message = simpleMessage,
            DHCPLease = null,
        };
    }

    private GetDHCPLeaseInformationResult LogAndReturnRenewDHCPLeaseFailed(string additionalMessage)
        => LogAndReturnGetDHCPLeaseFailed(additionalMessage, true);

    [LoggerMessage(LogLevel.Warning, "{InnerMessage}")]
    private static partial void GetDHCPLeaseCanceled(ILogger logger, string? innerMessage);

    [LoggerMessage(LogLevel.Error, "{InnerMessage}")]
    private static partial void GetDHCPLeaseFailed(ILogger logger, string? innerMessage);

    [LoggerMessage(LogLevel.Warning, "{InnerMessage}")]
    private static partial void RenewDHCPLeaseCanceled(ILogger logger, string? innerMessage);

    [LoggerMessage(LogLevel.Error, "{InnerMessage}")]
    private static partial void RenewDHCPLeaseFailed(ILogger logger, string? innerMessage);
}
