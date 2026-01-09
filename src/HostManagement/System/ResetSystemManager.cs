using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Enums;
using Microsoft.Extensions.Logging;

namespace HostManagement.System;

/// <summary>
/// Represents the reset system manager.
/// </summary>
/// <param name="logger">An <see cref="ILogger"/> implementation.</param>
internal sealed partial class ResetSystemManager(ILogger logger)
{
    private readonly ILogger _logger = logger;

    /// <summary>
    /// Resets the system.
    /// </summary>
    /// <returns><see cref="ResetSystemResult"/></returns>
    public async Task<ResetSystemResult> ResetSystem()
    {
        try
        {
            var result = new Result();
            var adapter = new ResetSystemAdapter(new ResetSystemScriptHandler(result));
            var status = await adapter.ResetSystem();

            if (status == OperationStatus.Error)
            {
                var message =
                    SystemResources.ResetSystemFailed +
                    Environment.NewLine +
                    result.Messages;

                LogError(_logger, message);

                return new ResetSystemResult
                {
                    Status = OperationStatus.Error,
                    Message = message,
                };
            }

            return new ResetSystemResult
            {
                Status = OperationStatus.Success,
                Message = string.Empty,
            };
        }
        catch (Exception ex)
        {
            var message =
                SystemResources.ResetSystemFailed +
                Environment.NewLine +
                ex.Message;

            LogError(_logger, message);

            return new ResetSystemResult
            {
                Status = OperationStatus.Error,
                Message = message,
            };
        }
    }

    [LoggerMessage(LogLevel.Error, "{InnerMessage}")]
    private static partial void LogError(ILogger logger, string? innerMessage);
}
