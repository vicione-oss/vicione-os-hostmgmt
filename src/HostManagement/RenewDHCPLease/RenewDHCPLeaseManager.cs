using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Enums;
using Microsoft.Extensions.Logging;

namespace HostManagement.RenewDHCPLease;

/// <summary>
/// Represents the renew DHCP lease manager.
/// </summary>
internal sealed partial class RenewDHCPLeaseManager
{
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new <see cref="RenewDHCPLeaseManager"/>.
    /// </summary>
    /// <param name="logger">An <see cref="ILogger"/> implementation.</param>
    public RenewDHCPLeaseManager(ILogger logger)
    {
        _logger = logger;
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
            var adapter = new RenewDHCPLeaseAdapter(
                new ScriptHandler(result));

            var (status, dhcpLease) =
                await adapter.RenewDHCPLease(interfaceName, cancellationToken);

            if (status == OperationStatus.Error)
            {
                var message =
                    RenewDHCPLeaseResources.RenewDHCPLeaseFailed +
                    Environment.NewLine +
                    result.Messages;

                LogError(_logger, message);

                return new RenewDHCPLeaseResult
                {
                    Status = OperationStatus.Error,
                    Message = message,
                    DHCPLease = null,
                };
            }

            if (dhcpLease.LeaseObtained is null)
            {
                var message =
                    RenewDHCPLeaseResources.RenewDHCPLeaseFailed +
                    Environment.NewLine +
                    "DHCP lease was empty.";

                LogError(_logger, message);

                return new RenewDHCPLeaseResult
                {
                    Status = OperationStatus.Error,
                    Message = message,
                    DHCPLease = null,
                };
            }

            if (status == OperationStatus.Warning)
            {
                var message =
                    RenewDHCPLeaseResources.RenewDHCPLeaseCompletedWithWarnings +
                    Environment.NewLine +
                    result.Messages;

                LogWarning(_logger, message);

                return new RenewDHCPLeaseResult
                {
                    Status = OperationStatus.Warning,
                    Message = message,
                    DHCPLease = dhcpLease,
                };
            }

            return new RenewDHCPLeaseResult
            {
                Status = OperationStatus.Success,
                Message = string.Empty,
                DHCPLease = dhcpLease,
            };
        }
        catch (OperationCanceledException)
        {
            var message = RenewDHCPLeaseResources.RenewDHCPLeaseCanceled;

            LogError(_logger, message);

            return new RenewDHCPLeaseResult
            {
                Status = OperationStatus.Warning,
                Message = message,
                DHCPLease = null
            };
        }
        catch (Exception ex)
        {
            var message =
                RenewDHCPLeaseResources.RenewDHCPLeaseFailed +
                Environment.NewLine +
                ex.Message;

            LogError(_logger, message);

            return new RenewDHCPLeaseResult
            {
                Status = OperationStatus.Error,
                Message = message,
                DHCPLease = null
            };
        }
    }

    [LoggerMessage(LogLevel.Error, "{InnerMessage}")]
    private static partial void LogError(ILogger logger, string? innerMessage);

    [LoggerMessage(LogLevel.Warning, "{InnerMessage}")]
    private static partial void LogWarning(ILogger logger, string? innerMessage);
}
