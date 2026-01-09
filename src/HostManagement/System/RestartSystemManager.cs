using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Enums;
using Microsoft.Extensions.Logging;

namespace HostManagement.System;

/// <summary>
/// Represents the restart system manager.
/// </summary>
/// <param name="logger">An <see cref="ILogger"/> implementation.</param>
internal sealed partial class RestartSystemManager(ILogger logger)
{
    private readonly ILogger _logger = logger;

    /// <summary>
    /// Restarts the system.
    /// </summary>
    /// <returns><see cref="RestartSystemResult"/></returns>
    public async Task<RestartSystemResult> RestartSystem()
    {
        try
        {
            var result = new Result();
            var adapter = new RestartSystemAdapter(new RestartSystemScriptHandler(result));
            var status = await adapter.RestartSystem();

            if (status == OperationStatus.Error)
            {
                var message =
                    SystemResources.RestartSystemFailed +
                    Environment.NewLine +
                    result.Messages;

                LogError(_logger, message);

                return new RestartSystemResult
                {
                    Status = OperationStatus.Error,
                    Message = message,
                };
            }

            return new RestartSystemResult
            {
                Status = OperationStatus.Success,
                Message = string.Empty,
            };
        }
        catch (Exception ex)
        {
            var message =
                SystemResources.RestartSystemFailed +
                Environment.NewLine +
                ex.Message;

            LogError(_logger, message);

            return new RestartSystemResult
            {
                Status = OperationStatus.Error,
                Message = message,
            };
        }
    }

    [LoggerMessage(LogLevel.Error, "{InnerMessage}")]
    private static partial void LogError(ILogger logger, string? innerMessage);
}
