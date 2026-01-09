using HostManagement.Configuration;
using HostManagement.Scripts.ScriptOptionNames;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Service;
using HostManagement.Shared.Enums;

namespace HostManagement.SystemConfig.Service;

/// <summary>
/// Represents the service configuration adapter.
/// </summary>
/// <param name="scriptHandler">An <see cref="IScriptHandler"/> implementation.</param>
/// <param name="bypassHasChanges">
/// <inheritdoc cref="ConfigurationAdapter(bool, bool)" path="/param[@name='bypassHasChanges']"/>
/// </param>
/// <param name="bypassApplyConfig">
/// <inheritdoc cref="ConfigurationAdapter(bool, bool)" path="/param[@name='bypassApplyConfig']"/>
/// </param>
internal sealed class ServiceConfigurationAdapter(
    IScriptHandler scriptHandler,
    bool bypassHasChanges,
    bool bypassApplyConfig)
        : ConfigurationAdapter<SystemConfigurationAdapter>(bypassHasChanges, bypassApplyConfig)
{
    private readonly IScriptHandler _scriptHandler = scriptHandler;

    protected internal override async Task<OperationStatus> GetConfiguration(
        SystemConfiguration config,
        CancellationToken cancellationToken = default)
    {
        const char WildcardChar = '*';

        var hasWarning = false;
        var hasError = false;

        var configurableServicesWithoutWildcard = EnvironmentVariables.ConfigurableServices
            .Where(service => !service.Contains(WildcardChar))
            .ToList();

        foreach (var name in configurableServicesWithoutWildcard)
        {
            var scriptOptions = new ScriptOptions();

            scriptOptions.Add(
                GetServiceScriptOptionName.Name,
                name);

            var (status, serviceDetail) = await _scriptHandler
                .ExecuteGetServiceStateScript(scriptOptions)
                .ConfigureAwait(false);

            if (status == OperationStatus.Error)
            {
                hasError = true;
                continue;
            }

            if (status == OperationStatus.Warning)
                hasWarning = true;

            config.Services.Add(serviceDetail);
        }

        if (hasError)
            return OperationStatus.Error;

        return hasWarning ? OperationStatus.Warning : OperationStatus.Success;
    }

    private protected override bool HasChangesInternal(
        SystemConfiguration currentConfig,
        SystemConfiguration proposedConfig)
    {
        return !currentConfig.Services.OrderBy(service => service.Name)
                .SequenceEqual(proposedConfig.Services.OrderBy(service => service.Name));
    }

    private protected override async Task<OperationStatus> ApplyConfiguration(
        SystemConfiguration config,
        CancellationToken cancellationToken = default)
    {
        return await ConfigureServices(config.Services).ConfigureAwait(false);
    }

    private async Task<OperationStatus> ConfigureServices(List<ServiceDetail> services)
    {
        var hasWarning = false;

        foreach (var service in services)
        {
            var status = await SetServiceState(service.Name, service.State).ConfigureAwait(false);

            if (status == OperationStatus.Error)
                return OperationStatus.Error;

            if (status == OperationStatus.Warning)
                hasWarning = true;
        }

        return hasWarning ? OperationStatus.Warning : OperationStatus.Success;
    }

    private async Task<OperationStatus> SetServiceState(string serviceName, ServiceState serviceState)
    {
        var scriptOptions = new ScriptOptions();

        scriptOptions.Add(
            SetServiceScriptOptionName.Name,
            serviceName);

        scriptOptions.Add(
            SetServiceScriptOptionName.State,
            serviceState.ToString());

        return await _scriptHandler.ExecuteSetServiceStateScript(scriptOptions).ConfigureAwait(false);
    }
}
