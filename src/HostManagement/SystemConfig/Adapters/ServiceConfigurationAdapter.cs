using HostManagement.Scripts;
using HostManagement.Shared.Communication.Enums;
using HostManagement.SystemConfig.Contracts;

namespace HostManagement.SystemConfig.Adapters;

/// <summary>
/// Represents the service configuration adapter.
/// </summary>
/// <param name="scriptHandler">An <see cref="IScriptHandler"/> implementation.</param>
/// <param name="configurableServices">The names of services allowed to be configured.</param>
/// <param name="bypassHasChanges">
/// <inheritdoc cref="ConfigurationAdapter(bool, bool)" path="/param[@name='bypassHasChanges']"/>
/// </param>
/// <param name="bypassApplyConfig">
/// <inheritdoc cref="ConfigurationAdapter(bool, bool)" path="/param[@name='bypassApplyConfig']"/>
/// </param>
internal sealed class ServiceConfigurationAdapter(
    IScriptHandler scriptHandler,
    IReadOnlyCollection<string> configurableServices,
    bool bypassHasChanges,
    bool bypassApplyConfig)
        : ConfigurationAdapter<SystemConfigurationAdapter>(bypassHasChanges, bypassApplyConfig)
{
    private readonly IScriptHandler _scriptHandler = scriptHandler;

    protected internal override async Task<Result> GetConfiguration(
        SystemConfiguration config,
        CancellationToken cancellationToken)
    {
        const char WildcardChar = '*';

        var configurableServicesWithoutWildcard = configurableServices
            .Where(service => !service.Contains(WildcardChar))
            .ToList();

        var combinedResult = Result.SuccessResult;

        foreach (var name in configurableServicesWithoutWildcard)
        {
            var scriptOptions = new ScriptOptions();
            scriptOptions.Add(ScriptOptionNames.GetSystemdUnitState.Name, name);

            var (result, serviceDetail) = await _scriptHandler
                .ExecuteGetSystemdUnitStateScript(scriptOptions)
                .ConfigureAwait(false);

            combinedResult.AddResult(result);

            if (result.OperationStatus is OperationStatus.Error)
                continue;

            config.Services ??= [];
            config.Services.Add(serviceDetail);
        }

        return combinedResult;
    }

    private protected override bool HasConfigurationChanges(
        SystemConfiguration currentConfig,
        SystemConfiguration proposedConfig)
    {
        return SystemConfigurationChangeDetector.HasSequenceChanges(
            currentConfig.Services,
            proposedConfig.Services,
            service => service.Name);
    }

    private protected override async Task<Result> ApplyConfiguration(
        SystemConfiguration config,
        CancellationToken cancellationToken)
    {
        if (config.Services is not { } services)
            return Result.SuccessResult;

        var result = Result.SuccessResult;

        foreach (var service in services)
        {
            result.AddResult(await SetServiceState(service.Name, service.State).ConfigureAwait(false));

            if (result.OperationStatus is OperationStatus.Error)
                return result;
        }

        return result;
    }

    private async Task<Result> SetServiceState(string serviceName, ServiceState serviceState)
    {
        var scriptOptions = new ScriptOptions();
        scriptOptions.Add(ScriptOptionNames.SetSystemdUnitState.Name, serviceName);
        scriptOptions.Add(ScriptOptionNames.SetSystemdUnitState.State, serviceState.ToString());

        return await _scriptHandler.ExecuteSetSystemdUnitStateScript(scriptOptions).ConfigureAwait(false);
    }
}
