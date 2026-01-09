using HostManagement.Shared.Communication.Enums;

namespace HostManagement;

/// <summary>
/// Represents a common script handler.
/// </summary>
internal interface ICommonScriptHandler
{
    /// <summary>
    /// Executes a script to get the system architecture (e.g. amd64, arm64).
    /// </summary>
    /// <returns><see cref="OperationStatus"/> and string of the architecture</returns>
    Task<(OperationStatus, string)> ExecuteGetArchitectureScript();
}
