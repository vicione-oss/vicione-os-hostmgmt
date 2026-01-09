using HostManagement.Shared.Communication.Enums;

namespace HostManagement.System;

/// <summary>
/// Represents the update system adapter.
/// </summary>
/// <param name="updateSystemScriptHandler">An <see cref="IUpdateSystemScriptHandler"/>
/// implementation.</param>
internal sealed class UpdateSystemAdapter(IUpdateSystemScriptHandler updateSystemScriptHandler)
{
    private readonly IUpdateSystemScriptHandler _updateSystemScriptHandler = updateSystemScriptHandler;

    /// <summary>
    /// Updates the system.
    /// </summary>
    /// <param name="filePath">The file path to the update file.</param>
    /// <returns><see cref="OperationStatus"/></returns>
    internal async Task<OperationStatus> UpdateSystem(string filePath)
        => await _updateSystemScriptHandler.ExecuteUpdateSystemScript(filePath);
}
