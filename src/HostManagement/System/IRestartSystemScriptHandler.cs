using HostManagement.Shared.Communication.Enums;

namespace HostManagement.System;

/// <summary>
/// Represents a restart system script handler.
/// </summary>
internal interface IRestartSystemScriptHandler
{
    /// <summary>
    /// Executes the script to restart the system.
    /// </summary>
    /// <returns><see cref="OperationStatus"/></returns>
    Task<OperationStatus> ExecuteRestartSystemScript();
}
