using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Contracts.NetworkStatus;
using Microsoft.Extensions.Logging;

namespace HostManagement.NetworkStatus;

/// <summary>
/// Represents the network status information manager.
/// </summary>
internal sealed partial class NetworkStatusInformationManager
{
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new <see cref="NetworkStatusInformationManager"/>.
    /// </summary>
    /// <param name="logger">An <see cref="ILogger"/> implementation.</param>
    public NetworkStatusInformationManager(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Gets the network status information.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see cref="GetNetworkStatusInformationResult"/></returns>
    internal async Task<GetNetworkStatusInformationResult> GetNetworkStatusInformation(
        CancellationToken cancellationToken)
    {
        try
        {
            var result = new Result();
            var adapter = new NetworkStatusInformationAdapter(
                new NetworkStatusInformationScriptHandler(result));

            var (status, networkStatusInformation) =
                await adapter.GetNetworkStatusInformation(cancellationToken);

            if (status == OperationStatus.Error)
            {
                var message =
                    NetworkStatusResources.GettingNetworkStatusInformationFailed +
                    Environment.NewLine +
                    result.Messages;

                LogError(_logger, message);

                return new GetNetworkStatusInformationResult
                {
                    Status = OperationStatus.Error,
                    Message = message,
                    NetworkStatusInformation = networkStatusInformation,
                };
            }

            if (status == OperationStatus.Warning)
            {
                var message =
                    NetworkStatusResources.GettingNetworkStatusInformationCompletedWithWarnings +
                    Environment.NewLine +
                    result.Messages;

                LogWarning(_logger, message);

                return new GetNetworkStatusInformationResult
                {
                    Status = OperationStatus.Warning,
                    Message = message,
                    NetworkStatusInformation = networkStatusInformation,
                };
            }

            return new GetNetworkStatusInformationResult
            {
                Status = OperationStatus.Success,
                Message = string.Empty,
                NetworkStatusInformation = networkStatusInformation,
            };
        }
        catch (OperationCanceledException)
        {
            var message = NetworkStatusResources.GettingNetworkStatusInformationCanceled;

            LogError(_logger, message);

            return new GetNetworkStatusInformationResult
            {
                Status = OperationStatus.Warning,
                Message = message,
                NetworkStatusInformation = NetworkStatusInformation.Empty
            };
        }
        catch (Exception ex)
        {
            var message =
                NetworkStatusResources.GettingNetworkStatusInformationFailed +
                Environment.NewLine +
                ex.Message;

            LogError(_logger, message);

            return new GetNetworkStatusInformationResult
            {
                Status = OperationStatus.Error,
                Message = message,
                NetworkStatusInformation = NetworkStatusInformation.Empty
            };
        }
    }

    [LoggerMessage(LogLevel.Error, "{InnerMessage}")]
    private static partial void LogError(ILogger logger, string? innerMessage);

    [LoggerMessage(LogLevel.Warning, "{InnerMessage}")]
    private static partial void LogWarning(ILogger logger, string? innerMessage);
}
