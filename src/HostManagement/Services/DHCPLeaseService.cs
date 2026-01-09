using Microsoft.Extensions.Logging;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Contracts.Network;

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
            var result = new Result();

            var (status, dhcpLease) =
                await GetDHCPLease(new ScriptHandler(result), interfaceName, cancellationToken);

            if (status == OperationStatus.Error)
                LogAndReturnGetDHCPLeaseFailed(result.Messages);

            if (dhcpLease.LeaseObtained is null)
                LogAndReturnGetDHCPLeaseFailed(ServicesResources.DHCPLeaseEmpty);

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
    /// <returns><see cref="RenewDHCPLeaseResult"/></returns>
    internal async Task<RenewDHCPLeaseResult> RenewDHCPLease(string interfaceName, CancellationToken cancellationToken)
    {
        try
        {
            var result = new Result();

            var (status, dhcpLease) =
                await RenewDHCPLease(new ScriptHandler(result), interfaceName, cancellationToken);

            if (status == OperationStatus.Error)
                LogAndReturnRenewDHCPLeaseFailed(result.Messages);

            if (dhcpLease.LeaseObtained is null)
                LogAndReturnRenewDHCPLeaseFailed(ServicesResources.DHCPLeaseEmpty);

            return new RenewDHCPLeaseResult
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

            return new RenewDHCPLeaseResult
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

    private static async Task<(OperationStatus, DHCPLease)> RenewDHCPLease(
        ScriptHandler scriptHandler,
        string interfaceName,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var status = await scriptHandler.ExecuteRenewDHCPLeaseScript(interfaceName);

        if (status == OperationStatus.Error)
            return (OperationStatus.Error, DHCPLease.Empty);

        return await GetDHCPLeaseWithRetry(scriptHandler, interfaceName, cancellationToken);
    }

    private static async Task<(OperationStatus status, DHCPLease dhcpLease)> GetDHCPLeaseWithRetry(
        ScriptHandler scriptHandler,
        string interfaceName,
        CancellationToken cancellationToken)
    {
        var retryCount = 3;
        OperationStatus status;
        DHCPLease dhcpLease;

        do
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Delay(1000, cancellationToken);

            (status, dhcpLease) = await GetDHCPLease(scriptHandler, interfaceName, cancellationToken);
            retryCount--;
        }
        while (IsDHCPLeaseNull(status, dhcpLease) && retryCount > 0);

        return (status, dhcpLease);
    }

    private static async Task<(OperationStatus, DHCPLease)> GetDHCPLease(
        ScriptHandler scriptHandler,
        string interfaceName,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var (status, dhcpLease) = await scriptHandler.ExecuteGetInterfaceIPv4DHCPLeaseScript(interfaceName);

        if (status == OperationStatus.Error)
            return (OperationStatus.Error, DHCPLease.Empty);

        NetworkDNSSettings? networkDnsSettings = default;
        if (dhcpLease.NetworkDNSSettings is not null)
        {
            networkDnsSettings = new()
            {
                DNSSuffix = dhcpLease.NetworkDNSSettings.DNSSuffix ?? string.Empty,
                DNSSuffixEnabled = dhcpLease.NetworkDNSSettings.DNSSuffixEnabled ?? false,
                Hostname = dhcpLease.NetworkDNSSettings.Hostname ?? string.Empty,
                MulticastDNSEnabled = dhcpLease.NetworkDNSSettings.MulticastDNSEnabled ?? false,
                NameServers = dhcpLease.NetworkDNSSettings.NameServers ?? [],
                NameServersEnabled = dhcpLease.NetworkDNSSettings.NameServersEnabled ?? false,
                SearchDomains = dhcpLease.NetworkDNSSettings.SearchDomains ?? [],
                SearchDomainsEnabled = dhcpLease.NetworkDNSSettings.SearchDomainsEnabled ?? false,
                StaticHosts = dhcpLease.NetworkDNSSettings.StaticHosts?.Select(x => new StaticHostDetail()
                {
                    Hostname = x.Hostname,
                    IPAddress = x.IPAddress,
                }).ToList() ?? [],
                StaticHostsEnabled = dhcpLease.NetworkDNSSettings.StaticHostsEnabled ?? false,
            };
        }
        IPv4Detail? ipv4Detail = default;
        if (dhcpLease.IPv4Detail is not null)
        {
            ipv4Detail = new()
            {
                IPAddress = dhcpLease.IPv4Detail.IPAddress,
                Netmask = dhcpLease.IPv4Detail.Netmask,
            };
        }
        DHCPLease result = new()
        {
            LeaseObtained = dhcpLease.LeaseObtained,
            LeaseExpires = dhcpLease.LeaseExpires,
            Gateway = dhcpLease.Gateway,
            IPv4Detail = ipv4Detail,
            NetworkDNSSettings = networkDnsSettings,
        };
        return (OperationStatus.Success, result);
    }

    private static bool IsDHCPLeaseNull(OperationStatus status, DHCPLease dhcpLease)
        => status == OperationStatus.Success && dhcpLease.LeaseObtained is null;

    private GetDHCPLeaseInformationResult LogAndReturnGetDHCPLeaseFailed(string additionalMessage)
    {
        var simpleMessage = ServicesResources.GetDHCPLeaseFailed;
        var message = simpleMessage + Environment.NewLine + additionalMessage;

        GetDHCPLeaseFailed(_logger, message);

        return new GetDHCPLeaseInformationResult
        {
            Status = OperationStatus.Error,
            Message = simpleMessage,
            DHCPLease = null,
        };
    }

    private RenewDHCPLeaseResult LogAndReturnRenewDHCPLeaseFailed(string additionalMessage)
    {
        var simpleMessage = ServicesResources.RenewDHCPLeaseFailed;
        var message = simpleMessage + Environment.NewLine + additionalMessage;

        RenewDHCPLeaseFailed(_logger, message);

        return new RenewDHCPLeaseResult
        {
            Status = OperationStatus.Error,
            Message = simpleMessage,
            DHCPLease = null,
        };
    }

    [LoggerMessage(LogLevel.Warning, "{InnerMessage}")]
    private static partial void GetDHCPLeaseCanceled(ILogger logger, string? innerMessage);

    [LoggerMessage(LogLevel.Error, "{InnerMessage}")]
    private static partial void GetDHCPLeaseFailed(ILogger logger, string? innerMessage);

    [LoggerMessage(LogLevel.Warning, "{InnerMessage}")]
    private static partial void RenewDHCPLeaseCanceled(ILogger logger, string? innerMessage);

    [LoggerMessage(LogLevel.Error, "{InnerMessage}")]
    private static partial void RenewDHCPLeaseFailed(ILogger logger, string? innerMessage);
}
