using System.Globalization;
using System.Text;
using HostManagement.Scripts;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Enums;
using Microsoft.Extensions.Logging;

namespace HostManagement.Services;

/// <summary>
/// Represents the systemd unit service. It is responsible for managing systemd units.
/// </summary>
/// <param name="logger">An <see cref="ILogger"/> implementation.</param>
/// <param name="scriptHandler">An <see cref="IScriptHandler"/> implementation.</param>
/// <param name="configurableServices">The list of configurable services.</param>
internal sealed partial class SystemdUnitService(
    ILogger logger,
    IScriptHandler scriptHandler,
    IReadOnlyCollection<string> configurableServices)
{
    private const char WildcardChar = '*';

    private readonly ILogger _logger = logger;
    private readonly IScriptHandler _scriptHandler = scriptHandler;
    private readonly IReadOnlyCollection<string> _configurableServices = configurableServices;

    /// <summary>
    /// Gets the active state of the specified systemd unit.
    /// </summary>
    /// <param name="systemdUnit">The systemd unit.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see cref="GetServiceStateResult"/></returns>
    public async Task<GetServiceStateResult> GetSystemdUnitActiveState(
        string systemdUnit,
        CancellationToken cancellationToken)
    {
        var failedCompositeFormat = CompositeFormat.Parse(ServicesResources.GetSystemdUnitActiveStateFailed);

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var scriptOptions = new ScriptOptions();
            scriptOptions.Add(ScriptOptionNames.GetSystemdUnitActiveState.Name, systemdUnit);

            var (result, activeState) = await _scriptHandler.ExecuteGetSystemdUnitActiveStateScript(scriptOptions);

            if (result.OperationStatus is OperationStatus.Error)
                return LogErrorAndReturnGetServiceStateResult(failedCompositeFormat, systemdUnit, result.FormattedMessages);

            return new GetServiceStateResult
            {
                Status = OperationStatus.Success,
                Message = string.Empty,
                ActiveState = activeState
            };
        }
        catch (OperationCanceledException)
        {
            var message =
                string.Format(
                    CultureInfo.InvariantCulture,
                    CompositeFormat.Parse(
                        ServicesResources.GetSystemdUnitActiveStateCanceled),
                    systemdUnit);

            GetSystemdUnitActiveStateCanceled(_logger, message);

            return new GetServiceStateResult
            {
                Status = OperationStatus.Warning,
                Message = message,
                ActiveState = "unknown"
            };
        }
        catch (Exception ex)
        {
            return LogErrorAndReturnGetServiceStateResult(failedCompositeFormat, systemdUnit, null, ex);
        }
    }

    /// <summary>
    /// Starts the specified systemd unit.
    /// </summary>
    /// <param name="systemdUnit">The systemd unit to start.</param>
    /// <returns><see cref="ServiceControlResult"/></returns>
    public async Task<ServiceControlResult> StartSystemdUnit(string systemdUnit)
        => await ControlSystemdUnit(
            systemdUnit,
            ServiceControlAction.Start,
            CompositeFormat.Parse(ServicesResources.SystemdUnitStartFailed));

    /// <summary>
    /// Stops the specified systemd unit.
    /// </summary>
    /// <param name="systemdUnit">The systemd unit to stop.</param>
    /// <returns><see cref="ServiceControlResult"/></returns>
    public async Task<ServiceControlResult> StopSystemdUnit(string systemdUnit)
        => await ControlSystemdUnit(
            systemdUnit,
            ServiceControlAction.Stop,
            CompositeFormat.Parse(ServicesResources.SystemdUnitStopFailed));

    /// <summary>
    /// Restarts the specified systemd unit.
    /// </summary>
    /// <param name="systemdUnit">The systemd unit to restart.</param>
    /// <returns><see cref="ServiceControlResult"/></returns>
    public async Task<ServiceControlResult> RestartSystemdUnit(string systemdUnit)
        => await ControlSystemdUnit(
            systemdUnit,
            ServiceControlAction.Restart,
            CompositeFormat.Parse(ServicesResources.SystemdUnitRestartFailed));

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

    private async Task<ServiceControlResult> ControlSystemdUnit(
        string systemdUnit,
        ServiceControlAction controlAction,
        CompositeFormat controlFailedMessage)
    {
        try
        {
            var isNotAllowedService = !_configurableServices
                .Any(configurableService => IsWildcardMatch(systemdUnit, configurableService));

            if (isNotAllowedService)
            {
                return LogErrorAndReturnServiceControlResult(
                    CompositeFormat.Parse(ServicesResources.SystemdUnitNotAllowed),
                    systemdUnit);
            }

            var result = new Result();

            switch (controlAction)
            {
                case ServiceControlAction.Start:
                    result = await SetSystemdUnitState(_scriptHandler, systemdUnit, ServiceControlAction.Start);
                    break;
                case ServiceControlAction.Stop:
                    result = await SetSystemdUnitState(_scriptHandler, systemdUnit, ServiceControlAction.Stop);
                    break;
                case ServiceControlAction.Restart:
                    result = await SetSystemdUnitState(_scriptHandler, systemdUnit, ServiceControlAction.Restart);
                    break;
            }

            if (result.OperationStatus is OperationStatus.Error)
            {
                return LogErrorAndReturnServiceControlResult(
                    controlFailedMessage,
                    systemdUnit,
                    result.FormattedMessages);
            }

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

    private static async Task<Result> SetSystemdUnitState(
        IScriptHandler scriptHandler,
        string name,
        ServiceControlAction controlAction)
    {
        var scriptOptions = new ScriptOptions();

        scriptOptions.Add(ScriptOptionNames.SetSystemdUnitState.Name, name);
        scriptOptions.Add(ScriptOptionNames.SetSystemdUnitState.State, controlAction.ToString());

        return await scriptHandler.ExecuteSetSystemdUnitStateScript(scriptOptions);
    }

    private GetServiceStateResult LogErrorAndReturnGetServiceStateResult(
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

        return new GetServiceStateResult
        {
            Status = OperationStatus.Error,
            Message = simpleMessage,
            ActiveState = "unknown"
        };
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

    [LoggerMessage(LogLevel.Warning, "{InnerMessage}")]
    private static partial void GetSystemdUnitActiveStateCanceled(ILogger logger, string? innerMessage);

    [LoggerMessage(LogLevel.Error, "{InnerMessage}")]
    private static partial void LogError(ILogger logger, string? innerMessage, Exception? exception);
}
