using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Enums;
using Microsoft.Extensions.Logging;

namespace HostManagement.Services;

/// <summary>
/// Represents the physical address service.
/// </summary>
/// <param name="logger">The logger instance.</param>
/// <param name="scriptHandler">An <see cref="IScriptHandler"/> implementation.</param>
internal sealed partial class PhysicalAddressService(ILogger logger, IScriptHandler scriptHandler)
{
    private readonly ILogger _logger = logger;
    private readonly IScriptHandler _scriptHandler = scriptHandler;

    /// <summary>
    /// Gets the original physical address of the specified network interface.
    /// </summary>
    /// <param name="interfaceName">The interface name for which the original physical address
    /// shall be retrieved.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see cref="GetOriginalPhysicalAddressResult"/></returns>
    internal async Task<GetOriginalPhysicalAddressResult> GetOriginalPhysicalAddress(
        string interfaceName,
        CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var (result, physicalAddress) = await _scriptHandler
                .ExecuteGetOriginalPhysicalAddressScript(interfaceName)
                .ConfigureAwait(false);

            if (result.OperationStatus is OperationStatus.Error)
                return LogAndReturnGetOriginalPhysicalAddressFailedResult(result.FormattedMessages);

            return new GetOriginalPhysicalAddressResult
            {
                Status = OperationStatus.Success,
                Message = string.Empty,
                OriginalPhysicalAddress = physicalAddress
            };
        }
        catch (OperationCanceledException)
        {
            var message = ServicesResources.GetOriginalPhysicalAddressCanceled;

            GetOriginalPhysicalAddressCanceled(_logger, message);

            return new GetOriginalPhysicalAddressResult
            {
                Status = OperationStatus.Warning,
                Message = message,
                OriginalPhysicalAddress = null
            };
        }
        catch (Exception ex)
        {
            return LogAndReturnGetOriginalPhysicalAddressFailedResult(ex.Message);
        }
    }

    private GetOriginalPhysicalAddressResult LogAndReturnGetOriginalPhysicalAddressFailedResult(string additionalMessage)
    {
        var simpleMessage = ServicesResources.GetOriginalPhysicalAddressFailed;
        var message = simpleMessage + Environment.NewLine + additionalMessage;

        GetOriginalPhysicalAddressFailed(_logger, message);

        return new GetOriginalPhysicalAddressResult
        {
            Status = OperationStatus.Error,
            Message = simpleMessage,
            OriginalPhysicalAddress = null
        };
    }

    [LoggerMessage(LogLevel.Warning, "{InnerMessage}")]
    private static partial void GetOriginalPhysicalAddressCanceled(ILogger logger, string? innerMessage);

    [LoggerMessage(LogLevel.Error, "{InnerMessage}")]
    private static partial void GetOriginalPhysicalAddressFailed(ILogger logger, string? innerMessage);
}
