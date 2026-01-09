using HostManagement.Scripts.ScriptOptionNames;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Contracts;

namespace HostManagement.SystemConfig.Service;

/// <summary>
/// Represents the service configuration adapter.
/// </summary>
internal sealed class ServiceConfigurationAdapter : ConfigurationAdapter<SystemConfigurationAdapter>
{
    private readonly IScriptHandler _scriptHandler;

    /// <summary>
    /// Initializes a new <see cref="ServiceConfigurationAdapter"/>.
    /// </summary>
    /// <param name="scriptHandler">An <see cref="IScriptHandler"/> implementation.</param>
    /// <param name="bypassHasChangesInternal">If set to <see langword="true"/> bypasses the
    /// internal check for changes and returns <see langword="true"/> when
    /// <see cref="ConfigurationAdapter.HasChanges(SystemConfiguration, SystemConfiguration)"/>
    /// is called.</param>
    public ServiceConfigurationAdapter(
        IScriptHandler scriptHandler,
        bool bypassHasChangesInternal = false)
        : base(bypassHasChangesInternal)
    {
        _scriptHandler = scriptHandler;
    }

    protected internal override async Task<OperationStatus> LoadConfiguration(
        SystemConfiguration config,
        CancellationToken cancellationToken)
    {
        var hasWarning = false;
        var hasError = false;

        foreach (var name in Configuration.EnvironmentVariables.ConfigurableServices)
        {
            var scriptOptions = new ScriptOptions();

            scriptOptions.Add(
                GetServiceOptionName.Name,
                name);

            var (status, serviceDetail) = await _scriptHandler.ExecuteGetServiceStateScript(scriptOptions);

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

    protected internal override async Task<OperationStatus> ApplyConfiguration(
        SystemConfiguration proposed,
        CancellationToken cancellationToken)
    {
        var hasWarning = false;
        var hasError = false;

        foreach (var service in proposed.Services)
        {
            var scriptOptions = new ScriptOptions();

            scriptOptions.Add(
                SetServiceOptionName.Service,
                service.Name);

            scriptOptions.Add(
                SetServiceOptionName.State,
                service.State.ToString());

            var status = await _scriptHandler.ExecuteSetServiceStateScript(scriptOptions);

            if (status == OperationStatus.Error)
            {
                hasError = true;
                continue;
            }

            if (status == OperationStatus.Warning)
                hasWarning = true;
        }

        if (hasError)
            return OperationStatus.Error;

        return hasWarning ? OperationStatus.Warning : OperationStatus.Success;
    }

    private protected override bool HasChangesInternal(
        SystemConfiguration currentConfig,
        SystemConfiguration proposedConfig)
    {
        return
            !currentConfig.Services.OrderBy(service => service.Name)
                .SequenceEqual(proposedConfig.Services.OrderBy(service => service.Name));
    }
}
