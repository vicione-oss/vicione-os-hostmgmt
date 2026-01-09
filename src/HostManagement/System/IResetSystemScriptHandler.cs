using HostManagement.Shared.Communication.Enums;

namespace HostManagement.System;

/// <summary>
/// Represents a reset system script handler.
/// </summary>
internal interface IResetSystemScriptHandler
{
    /// <summary>
    /// Executes the script to reset the system.
    /// </summary>
    /// <returns><see cref="OperationStatus"/></returns>
    Task<OperationStatus> ExecuteResetSystemScript();
}
