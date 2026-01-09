using HostManagement.Shared.Communication.Enums;

namespace HostManagement.System;

/// <summary>
/// Represents the restart system adapter.
/// </summary>
/// <param name="restartSystemScriptHandler">An <see cref="IRestartSystemScriptHandler"/>
/// implementation.</param>
internal sealed class RestartSystemAdapter(IRestartSystemScriptHandler restartSystemScriptHandler)
{
    private readonly IRestartSystemScriptHandler _restartSystemScriptHandler = restartSystemScriptHandler;

    /// <summary>
    /// Restarts the system.
    /// </summary>
    /// <returns><see cref="OperationStatus"/></returns>
    internal async Task<OperationStatus> RestartSystem()
        => await _restartSystemScriptHandler.ExecuteRestartSystemScript();
}
