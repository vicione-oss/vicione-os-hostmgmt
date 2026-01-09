using HostManagement.Scripts.ScriptOptionNames;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Enums;

namespace HostManagement.Service;

/// <summary>
/// Represents the service control adapter. It is responsible for calling the script to start, stop
/// or restart a service.
/// </summary>
/// <param name="scriptHandler">An <see cref="IScriptHandler"/> implementation.</param>
internal sealed class ServiceControlAdapter(IScriptHandler scriptHandler)
{
    private readonly IScriptHandler _scriptHandler = scriptHandler;

    /// <summary>
    /// Start a service.
    /// </summary>
    /// <param name="serviceName">The name of the service.</param>
    /// <returns><see cref="OperationStatus"/></returns>
    internal async Task<OperationStatus> StartService(string serviceName)
        => await ControlService(serviceName, ServiceControlAction.Start);

    /// <summary>
    /// Stops a service.
    /// </summary>
    /// <param name="serviceName">The name of the service.</param>
    /// <returns><see cref="OperationStatus"/></returns>
    internal async Task<OperationStatus> StopService(string serviceName)
        => await ControlService(serviceName, ServiceControlAction.Stop);

    /// <summary>
    /// Restarts a service.
    /// </summary>
    /// <param name="serviceName">The name of the service.</param>
    /// <returns><see cref="OperationStatus"/></returns>
    internal async Task<OperationStatus> RestartService(string serviceName)
        => await ControlService(serviceName, ServiceControlAction.Restart);

    private async Task<OperationStatus> ControlService(string serviceName, ServiceControlAction controlAction)
    {
        var scriptOptions = new ScriptOptions();

        scriptOptions.Add(
            SetServiceScriptOptionName.Name,
            serviceName);

        scriptOptions.Add(
            SetServiceScriptOptionName.State,
            controlAction.ToString());

        return await _scriptHandler.ExecuteSetServiceStateScript(scriptOptions);
    }
}
