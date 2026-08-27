using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Enums;
using Microsoft.Extensions.Logging;

namespace HostManagement.Services;

/// <summary>
/// Represents the NTP service.
/// </summary>
/// <param name="logger">An <see cref="ILogger"/> implementation.</param>
/// <param name="scriptHandler">An <see cref="IScriptHandler"/> implementation.</param>
internal sealed partial class NTPService(ILogger logger, IScriptHandler scriptHandler)
{
    private readonly ILogger _logger = logger;
    private readonly IScriptHandler _scriptHandler = scriptHandler;

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

            var (result, fallbackNTPServers) = await _scriptHandler.ExecuteGetNetworkNTPFallbackInformationScript()
                .ConfigureAwait(false);

            if (result.OperationStatus is OperationStatus.Error)
                return LogAndReturnGetNTPFallbackServersFailed(result.FormattedMessages);

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
            return LogAndReturnGetNTPFallbackServersFailed(exception: ex);
        }
    }

    private GetNTPFallbackInformationResult LogAndReturnGetNTPFallbackServersFailed(
        string? additionalMessage = null,
        Exception? exception = null)
    {
        var simpleMessage = ServicesResources.GetNTPFallbackServersFailed;

        var message =
            simpleMessage +
            (additionalMessage is not null
                ? Environment.NewLine + additionalMessage
                : string.Empty);

        GetNTPFallbackServersFailed(_logger, message, exception);

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
    private static partial void GetNTPFallbackServersFailed(ILogger logger, string? innerMessage, Exception? exception);
}
