using HostManagement.Shared.Communication.Enums;

namespace HostManagement.System;

/// <summary>
/// Represents a update system script handler.
/// </summary>
internal interface IUpdateSystemScriptHandler
{
    /// <summary>
    /// Executes the script to update the system.
    /// </summary>
    /// <param name="filePath">The file path to the update file.</param>
    /// <returns><see cref="OperationStatus"/></returns>
    Task<OperationStatus> ExecuteUpdateSystemScript(string filePath);
}
