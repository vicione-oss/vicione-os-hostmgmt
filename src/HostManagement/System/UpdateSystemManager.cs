using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Enums;
using Microsoft.Extensions.Logging;

namespace HostManagement.System;

/// <summary>
/// Represents the update system manager.
/// </summary>
/// <param name="logger">An <see cref="ILogger"/> implementation.</param>
internal sealed partial class UpdateSystemManager(ILogger logger)
{
    private readonly ILogger _logger = logger;

    /// <summary>
    /// Updates the system.
    /// </summary>
    /// <param name="filePath">The file path to the update file.</param>
    /// <returns><see cref="UpdateSystemResult"/></returns>
    public async Task<UpdateSystemResult> UpdateSystem(string filePath)
    {
        try
        {
            var result = new Result();
            var adapter = new UpdateSystemAdapter(new UpdateSystemScriptHandler(result));
            var status = await adapter.UpdateSystem(filePath);

            if (status == OperationStatus.Error)
            {
                var message =
                    SystemResources.UpdateSystemFailed +
                    Environment.NewLine +
                    result.Messages;

                LogError(_logger, message);

                return new UpdateSystemResult
                {
                    Status = OperationStatus.Error,
                    Message = message,
                };
            }

            return new UpdateSystemResult
            {
                Status = OperationStatus.Success,
                Message = string.Empty,
            };
        }
        catch (Exception ex)
        {
            var message =
                SystemResources.UpdateSystemFailed +
                Environment.NewLine +
                ex.Message;

            LogError(_logger, message);

            return new UpdateSystemResult
            {
                Status = OperationStatus.Error,
                Message = message,
            };
        }
    }

    [LoggerMessage(LogLevel.Error, "{InnerMessage}")]
    private static partial void LogError(ILogger logger, string? innerMessage);
}
