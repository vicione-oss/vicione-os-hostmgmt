using HostManagement.Scripts;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Contracts.NetworkStatus;
using Microsoft.Extensions.Logging;

namespace HostManagement.Services;

/// <summary>
/// Represents the network status service.
/// </summary>
/// <param name="logger">An <see cref="ILogger"/> implementation.</param>
/// <param name="scriptHandler">An <see cref="IScriptHandler"/> implementation.</param>
/// <param name="result">The result.</param>
internal sealed partial class NetworkStatusService(ILogger logger, IScriptHandler scriptHandler, Result result)
{
    private readonly ILogger _logger = logger;
    private readonly IScriptHandler _scriptHandler = scriptHandler;
    private readonly Result _result = result;

    /// <summary>
    /// Gets the network status information.
    /// </summary>
    /// <param name="interfaceName">The interface name to get network status information for.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see cref="GetNetworkStatusInformationResult"/></returns>
    internal async Task<GetNetworkStatusInformationResult> GetNetworkStatusInformation(
        string interfaceName,
        CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var status = await _scriptHandler
                .ExecuteIsNetworkInterfaceAvailableScript(interfaceName)
                .ConfigureAwait(false);

            if (status is OperationStatus.Warning or OperationStatus.Error)
                return LogFailedAndReturnResult(_result.Messages);

            (status, var rawInterfacesString) = await _scriptHandler
                .ExecuteGetNetworkInterfacesScript()
                .ConfigureAwait(false);

            if (status is OperationStatus.Error)
                return LogFailedAndReturnResult(_result.Messages);

            var scriptOptions = new ScriptOptions();
            scriptOptions.Add(ScriptOptionNames.GetNetworkStatusInformation.InterfaceName, interfaceName);
            scriptOptions.Add(ScriptOptionNames.GetNetworkStatusInformation.Interfaces, rawInterfacesString);

            (status, var networkStatusInformation) = await _scriptHandler
                .ExecuteGetNetworkStatusInformationScript(scriptOptions)
                .ConfigureAwait(false);

            if (status == OperationStatus.Error)
                return LogFailedAndReturnResult(_result.Messages);

            return new GetNetworkStatusInformationResult
            {
                Status = OperationStatus.Success,
                Message = string.Empty,
                NetworkStatusInformation = networkStatusInformation,
            };
        }
        catch (OperationCanceledException)
        {
            return LogCanceledAndReturnResult();
        }
        catch (Exception ex)
        {
            return LogFailedAndReturnResult(null, ex);
        }
    }

    private GetNetworkStatusInformationResult LogCanceledAndReturnResult()
    {
        var message = ServicesResources.GetNetworkStatusInformationCanceled;

        LogWarning(_logger, message);

        return new GetNetworkStatusInformationResult
        {
            Status = OperationStatus.Warning,
            Message = message,
            NetworkStatusInformation = NetworkStatusInformation.Empty
        };
    }

    private GetNetworkStatusInformationResult LogFailedAndReturnResult(
        string? additionalMessage = null,
        Exception? additionalException = null)
    {
        var simpleMessage = ServicesResources.GetNetworkStatusInformationFailed;
        var message =
            simpleMessage +
            (additionalMessage is not null
                ? Environment.NewLine + additionalMessage
                : string.Empty);

        LogError(_logger, message, additionalException);

        return new GetNetworkStatusInformationResult
        {
            Status = OperationStatus.Error,
            Message = simpleMessage,
            NetworkStatusInformation = NetworkStatusInformation.Empty,
        };
    }

    [LoggerMessage(LogLevel.Error, "{InnerMessage}")]
    private static partial void LogError(ILogger logger, string? innerMessage, Exception? exception);

    [LoggerMessage(LogLevel.Warning, "{InnerMessage}")]
    private static partial void LogWarning(ILogger logger, string? innerMessage);
}
