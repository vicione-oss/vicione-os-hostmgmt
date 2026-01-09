using HostManagement.Shared.Communication.Enums;

namespace HostManagement.System;

/// <summary>
/// Represents a shutdown system script handler.
/// </summary>
internal interface IShutdownSystemScriptHandler
{
    /// <summary>
    /// Executes the script to shutdown the system.
    /// </summary>
    /// <returns><see cref="OperationStatus"/></returns>
    Task<OperationStatus> ExecuteShutdownSystemScript();
}
