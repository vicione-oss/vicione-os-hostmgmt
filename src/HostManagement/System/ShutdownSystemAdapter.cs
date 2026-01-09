using HostManagement.Shared.Communication.Enums;

namespace HostManagement.System;

/// <summary>
/// Represents the shutdown system adapter.
/// </summary>
/// <param name="shutdownSystemScriptHandler">An <see cref="IShutdownSystemScriptHandler"/>
/// implementation.</param>
internal sealed class ShutdownSystemAdapter(IShutdownSystemScriptHandler shutdownSystemScriptHandler)
{
    private readonly IShutdownSystemScriptHandler _shutdownSystemScriptHandler = shutdownSystemScriptHandler;

    /// <summary>
    /// Shuts down the system.
    /// </summary>
    /// <returns><see cref="OperationStatus"/></returns>
    internal async Task<OperationStatus> ShutdownSystem()
        => await _shutdownSystemScriptHandler.ExecuteShutdownSystemScript();
}
