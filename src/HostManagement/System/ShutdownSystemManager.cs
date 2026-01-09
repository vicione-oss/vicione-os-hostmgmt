using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Enums;
using Microsoft.Extensions.Logging;

namespace HostManagement.System;

/// <summary>
/// Represents the shutdown system manager.
/// </summary>
/// <param name="logger">An <see cref="ILogger"/> implementation.</param>
internal sealed partial class ShutdownSystemManager(ILogger logger)
{
    private readonly ILogger _logger = logger;

    /// <summary>
    /// Shuts down the system.
    /// </summary>
    /// <returns><see cref="ShutdownSystemResult"/></returns>
    public async Task<ShutdownSystemResult> ShutdownSystem()
    {
        try
        {
            var result = new Result();
            var adapter = new ShutdownSystemAdapter(new ShutdownSystemScriptHandler(result));
            var status = await adapter.ShutdownSystem();

            if (status == OperationStatus.Error)
            {
                var message =
                    SystemResources.ShutdownSystemFailed +
                    Environment.NewLine +
                    result.Messages;

                LogError(_logger, message);

                return new ShutdownSystemResult
                {
                    Status = OperationStatus.Error,
                    Message = message,
                };
            }

            return new ShutdownSystemResult
            {
                Status = OperationStatus.Success,
                Message = string.Empty,
            };
        }
        catch (Exception ex)
        {
            var message =
                SystemResources.ShutdownSystemFailed +
                Environment.NewLine +
                ex.Message;

            LogError(_logger, message);

            return new ShutdownSystemResult
            {
                Status = OperationStatus.Error,
                Message = message,
            };
        }
    }

    [LoggerMessage(LogLevel.Error, "{InnerMessage}")]
    private static partial void LogError(ILogger logger, string? innerMessage);
}
