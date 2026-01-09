using HostManagement.Shared.Communication.Enums;

namespace HostManagement.System;

/// <summary>
/// Represents the reset system adapter.
/// </summary>
/// <param name="resetSystemScriptHandler">An <see cref="IResetSystemScriptHandler"/>
/// implementation.</param>
internal sealed class ResetSystemAdapter(IResetSystemScriptHandler resetSystemScriptHandler)
{
    private readonly IResetSystemScriptHandler _resetSystemScriptHandler = resetSystemScriptHandler;

    /// <summary>
    /// Resets the system.
    /// </summary>
    /// <returns><see cref="OperationStatus"/></returns>
    internal async Task<OperationStatus> ResetSystem()
        => await _resetSystemScriptHandler.ExecuteResetSystemScript();
}
