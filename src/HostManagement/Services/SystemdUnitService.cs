using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Enums;
using HostManagement.Scripts.ScriptOptionNames;

namespace HostManagement.Services;

/// <summary>
/// Represents the systemd unit service. It is responsible for managing systemd units.
/// </summary>
/// <param name="logger">An <see cref="ILogger"/> implementation.</param>
/// <param name="scriptHandler">An <see cref="IScriptHandler"/> implementation.</param>
/// <param name="result">The result.</param>
/// <param name="configurableServices">The list of configurable services.</param>
internal sealed partial class SystemdUnitService(
    ILogger logger,
    IScriptHandler scriptHandler,
    Result result,
    List<string> configurableServices)
{
    private const char WildcardChar = '*';

    private readonly ILogger _logger = logger;
    private readonly IScriptHandler _scriptHandler = scriptHandler;
    private readonly Result _result = result;
    private readonly List<string> _configurableServices = configurableServices;

    /// <summary>
    /// Starts the specified systemd unit.
    /// </summary>
    /// <param name="systemdUnit">The systemd unit to start.</param>
    /// <returns><see cref="ServiceControlResult"/></returns>
    public async Task<ServiceControlResult> StartSystemdUnit(string systemdUnit)
    {
        return await ControlSystemdUnit(
            systemdUnit,
            ServiceControlAction.Start,
            CompositeFormat.Parse(ServicesResources.SystemdUnitStartFailed));
    }

    /// <summary>
    /// Stops the specified systemd unit.
    /// </summary>
    /// <param name="systemdUnit">The systemd unit to stop.</param>
    /// <returns><see cref="ServiceControlResult"/></returns>
    public async Task<ServiceControlResult> StopSystemdUnit(string systemdUnit)
    {
        return await ControlSystemdUnit(
            systemdUnit,
            ServiceControlAction.Stop,
            CompositeFormat.Parse(ServicesResources.SystemdUnitStopFailed));
    }

    /// <summary>
    /// Restarts the specified systemd unit.
    /// </summary>
    /// <param name="systemdUnit">The systemd unit to restart.</param>
    /// <returns><see cref="ServiceControlResult"/></returns>
    public async Task<ServiceControlResult> RestartSystemdUnit(string systemdUnit)
    {
        return await ControlSystemdUnit(
            systemdUnit,
            ServiceControlAction.Restart,
            CompositeFormat.Parse(ServicesResources.SystemdUnitRestartFailed));
    }

    private static bool IsWildcardMatch(string systemdUnitName, string configurableService)
    {
        var startIndex = configurableService.IndexOf(WildcardChar);

        // If no wildcard is present in the configurable service name, do a direct comparison.
        if (startIndex < 0)
            return string.Equals(systemdUnitName, configurableService, StringComparison.Ordinal);

        var prefix = configurableService[..startIndex];
        var suffix = configurableService[(startIndex + 1)..];

        return
            systemdUnitName.StartsWith(prefix, StringComparison.Ordinal) &&
            systemdUnitName.EndsWith(suffix, StringComparison.Ordinal) &&
            systemdUnitName.Length > prefix.Length + suffix.Length;
    }

    private static async Task<OperationStatus> SetSystemdUnitState(
        IScriptHandler scriptHandler,
        string name,
        ServiceControlAction controlAction)
    {
        var scriptOptions = new ScriptOptions();

        scriptOptions.Add(SetSystemdUnitStateScriptOptionName.Name, name);
        scriptOptions.Add(SetSystemdUnitStateScriptOptionName.State, controlAction.ToString());

        return await scriptHandler.ExecuteSetSystemdUnitStateScript(scriptOptions);
    }

    private async Task<ServiceControlResult> ControlSystemdUnit(
        string systemdUnit,
        ServiceControlAction controlAction,
        CompositeFormat controlFailedMessage)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(systemdUnit))
            {
                return LogErrorAndReturnServiceControlResult(
                    CompositeFormat.Parse(ServicesResources.SystemdUnitEmpty),
                    systemdUnit);
            }

            var isNotAllowedService = !_configurableServices
                .Any(configurableService => IsWildcardMatch(systemdUnit, configurableService));

            if (isNotAllowedService)
            {
                return LogErrorAndReturnServiceControlResult(
                    CompositeFormat.Parse(ServicesResources.SystemdUnitNotAllowed),
                    systemdUnit);
            }

            var status = OperationStatus.Error;

            switch (controlAction)
            {
                case ServiceControlAction.Start:
                    status = await SetSystemdUnitState(_scriptHandler, systemdUnit, ServiceControlAction.Start);
                    break;
                case ServiceControlAction.Stop:
                    status = await SetSystemdUnitState(_scriptHandler, systemdUnit, ServiceControlAction.Stop);
                    break;
                case ServiceControlAction.Restart:
                    status = await SetSystemdUnitState(_scriptHandler, systemdUnit, ServiceControlAction.Restart);
                    break;
            }

            if (status == OperationStatus.Error)
                return LogErrorAndReturnServiceControlResult(controlFailedMessage, systemdUnit, _result.Messages);

            return new ServiceControlResult
            {
                Status = OperationStatus.Success,
                Message = string.Empty,
            };
        }
        catch (Exception ex)
        {
            return LogErrorAndReturnServiceControlResult(controlFailedMessage, systemdUnit, null, ex);
        }
    }

    private ServiceControlResult LogErrorAndReturnServiceControlResult(
        CompositeFormat failedMessage,
        string systemdUnit,
        string? additionalMessage = null,
        Exception? additionalException = null)
    {
        var simpleMessage = string.Format(CultureInfo.InvariantCulture, failedMessage, systemdUnit);
        var message =
            simpleMessage +
            (additionalMessage is not null
                ? Environment.NewLine + additionalMessage
                : string.Empty);

        LogError(
            _logger,
            message,
            additionalException);

        return new ServiceControlResult
        {
            Status = OperationStatus.Error,
            Message = simpleMessage,
        };
    }

    [LoggerMessage(LogLevel.Error, "{InnerMessage}")]
    private static partial void LogError(ILogger logger, string? innerMessage, Exception? exception);
}
