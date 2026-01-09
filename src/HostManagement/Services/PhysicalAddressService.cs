using Microsoft.Extensions.Logging;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Enums;

namespace HostManagement.Services;

internal sealed partial class PhysicalAddressService(ILogger logger)
{
    private readonly ILogger _logger = logger;

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

            var result = new Result();

            var (status, physicalAddress) = await new ScriptHandler(result)
                .ExecuteGetOriginalPhysicalAddressScript(interfaceName)
                .ConfigureAwait(false);

            if (status == OperationStatus.Error)
                LogAndReturnGetOriginalPhysicalAddressFailedResult(result.Messages);

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
                Status = OperationStatus.Error,
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
