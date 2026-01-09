using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Enums;
using HostManagement.Configuration;

namespace HostManagement.Service;

/// <summary>
/// Represents the service control manager. It is responsible for managing service restart-requests.
/// </summary>
/// <param name="logger">An <see cref="ILogger"/> implementation.</param>
internal sealed partial class ServiceControlManager(ILogger logger)
{
    private const char WildcardChar = '*';

    private readonly ILogger _logger = logger;

    /// <summary>
    /// Starts the specified service.
    /// </summary>
    /// <param name="serviceToStart">The service to start.</param>
    /// <returns><see cref="ServiceControlResult"/></returns>
    public async Task<ServiceControlResult> StartService(string serviceToStart)
    {
        return await ControlService(
            serviceToStart,
            ServiceControlAction.Start,
            CompositeFormat.Parse(ServiceResources.StartServiceFailed));
    }

    /// <summary>
    /// Stops the specified service.
    /// </summary>
    /// <param name="serviceToStop">The service to stop.</param>
    /// <returns><see cref="ServiceControlResult"/></returns>
    public async Task<ServiceControlResult> StopService(string serviceToStop)
    {
        return await ControlService(
            serviceToStop,
            ServiceControlAction.Stop,
            CompositeFormat.Parse(ServiceResources.StopServiceFailed));
    }

    /// <summary>
    /// Restarts the specified service.
    /// </summary>
    /// <param name="serviceToRestart">The service to restart.</param>
    /// <returns><see cref="ServiceControlResult"/></returns>
    public async Task<ServiceControlResult> RestartService(string serviceToRestart)
    {
        return await ControlService(
            serviceToRestart,
            ServiceControlAction.Restart,
            CompositeFormat.Parse(ServiceResources.RestartServiceFailed));
    }

    private async Task<ServiceControlResult> ControlService(
        string serviceToControl,
        ServiceControlAction controlAction,
        CompositeFormat serviceControlFailedMessage)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(serviceToControl))
            {
                var message = string.Format(
                    CultureInfo.InvariantCulture,
                    CompositeFormat.Parse(ServiceResources.ServiceEmpty),
                    serviceToControl);

                LogError(_logger, message);

                return new ServiceControlResult
                {
                    Status = OperationStatus.Error,
                    Message = message,
                };
            }

            var isNotAllowedService = !EnvironmentVariables.ConfigurableServices
                .Any(configurableService => IsWildcardMatch(serviceToControl, configurableService));

            if (isNotAllowedService)
            {
                var message = string.Format(
                    CultureInfo.InvariantCulture,
                    CompositeFormat.Parse(ServiceResources.ServiceNotAllowed),
                    serviceToControl);

                LogError(_logger, message);

                return new ServiceControlResult
                {
                    Status = OperationStatus.Error,
                    Message = message,
                };
            }

            var result = new Result();
            var adapter = new ServiceControlAdapter(new ScriptHandler(result));
            var status = OperationStatus.Error;

            switch (controlAction)
            {
                case ServiceControlAction.Start: status = await adapter.StartService(serviceToControl); break;
                case ServiceControlAction.Stop: status = await adapter.StopService(serviceToControl); break;
                case ServiceControlAction.Restart: status = await adapter.RestartService(serviceToControl); break;
            }

            if (status == OperationStatus.Error)
            {
                var message =
                    string.Format(
                        CultureInfo.InvariantCulture,
                        serviceControlFailedMessage,
                        serviceToControl) +
                    Environment.NewLine +
                    result.Messages;

                LogError(_logger, message);

                return new ServiceControlResult
                {
                    Status = OperationStatus.Error,
                    Message = message,
                };
            }

            return new ServiceControlResult
            {
                Status = OperationStatus.Success,
                Message = string.Empty,
            };
        }
        catch (Exception ex)
        {
            var message =
                string.Format(
                    CultureInfo.InvariantCulture,
                    serviceControlFailedMessage,
                    serviceToControl) +
                Environment.NewLine +
                ex.Message;

            LogError(_logger, message);

            return new ServiceControlResult
            {
                Status = OperationStatus.Error,
                Message = message,
            };
        }
    }

    private static bool IsWildcardMatch(string serviceName, string configurableService)
    {
        var startIndex = configurableService.IndexOf(WildcardChar);

        // If no wildcard is present in the configurable service name, do a direct comparison.
        if (startIndex < 0)
            return string.Equals(serviceName, configurableService, StringComparison.Ordinal);

        var prefix = configurableService[..startIndex];
        var suffix = configurableService[(startIndex + 1)..];

        return
            serviceName.StartsWith(prefix, StringComparison.Ordinal) &&
            serviceName.EndsWith(suffix, StringComparison.Ordinal) &&
            serviceName.Length > prefix.Length + suffix.Length;
    }

    [LoggerMessage(LogLevel.Error, "{InnerMessage}")]
    private static partial void LogError(ILogger logger, string? innerMessage);
}
