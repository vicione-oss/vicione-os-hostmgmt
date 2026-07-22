using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Enums;
using Microsoft.Extensions.Logging;

namespace HostManagement.Services;

/// <summary>
/// Represents the NTP service.
/// </summary>
/// <param name="logger">An <see cref="ILogger"/> implementation.</param>
internal sealed partial class NTPService(ILogger logger)
{
    private readonly ILogger _logger = logger;

    /// <summary>
    /// Gets the NTP fallback servers.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see cref="GetNTPFallbackInformationResult"/></returns>
    internal async Task<GetNTPFallbackInformationResult> GetNTPFallbackInformation(CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var (result, fallbackNTPServers) = await new ScriptHandler()
                    .ExecuteGetNetworkNTPFallbackInformationScript()
                    .ConfigureAwait(false);

            if (result.OperationStatus is OperationStatus.Error)
                LogAndReturnGetNTPFallbackServersFailed(result.FormattedMessages);

            return new GetNTPFallbackInformationResult
            {
                Status = OperationStatus.Success,
                Message = string.Empty,
                FallbackNTPServers = [.. fallbackNTPServers]
            };
        }
        catch (OperationCanceledException)
        {
            var message = ServicesResources.GetNTPFallbackServersCanceled;

            GetNTPFallbackServersCanceled(_logger, message);

            return new GetNTPFallbackInformationResult
            {
                Status = OperationStatus.Warning,
                Message = message,
                FallbackNTPServers = []
            };
        }
        catch (Exception ex)
        {
            return LogAndReturnGetNTPFallbackServersFailed(ex.Message);
        }
    }

    private GetNTPFallbackInformationResult LogAndReturnGetNTPFallbackServersFailed(string additionalMessage)
    {
        var simpleMessage = ServicesResources.GetNTPFallbackServersFailed;
        var message = simpleMessage + Environment.NewLine + additionalMessage;

        GetNTPFallbackServersFailed(_logger, message);

        return new GetNTPFallbackInformationResult
        {
            Status = OperationStatus.Error,
            Message = simpleMessage,
            FallbackNTPServers = [],
        };
    }

    [LoggerMessage(LogLevel.Warning, "{InnerMessage}")]
    private static partial void GetNTPFallbackServersCanceled(ILogger logger, string? innerMessage);

    [LoggerMessage(LogLevel.Error, "{InnerMessage}")]
    private static partial void GetNTPFallbackServersFailed(ILogger logger, string? innerMessage);
}
