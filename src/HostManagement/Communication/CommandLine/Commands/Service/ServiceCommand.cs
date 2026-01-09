using System.CommandLine;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Configuration;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Service;
using HostManagement.Shared.Enums;

namespace HostManagement.Communication.CommandLine.Commands.Service;

/// <summary>
/// Represents the service command.
/// </summary>
internal sealed class ServiceCommand : ConfigCommandBase
{
    private static readonly Option<string> s_nameOption = new(["-n", "--name"])
    {
        Description = "The name of the systemd unit.",
        IsRequired = true,
        Arity = ArgumentArity.ExactlyOne
    };

    /// <summary>
    /// Initializes a new <see cref="ServiceCommand"/>.
    /// </summary>
    public ServiceCommand() : base(
        CommandLineStrings_Systemd.ServiceCommandName,
        CommandLineStrings_Systemd.ServiceCommandDescription)
    {
        Add(
            CommandLineStrings_Common.EnableCommandName,
            CommandLineStrings_Systemd.ServiceEnableCommandDescription,
            s_nameOption,
            (systemdUnitName) => EnableDisableHandler(systemdUnitName, enable: true));

        Add(
            CommandLineStrings_Common.DisableCommandName,
            CommandLineStrings_Systemd.ServiceDisableCommandDescription,
            s_nameOption,
            (systemdUnitName) => EnableDisableHandler(systemdUnitName, enable: false));

        Add(
            CommandLineStrings_Common.ListCommandName,
            CommandLineStrings_Systemd.ServiceListCommandDescription,
            ListHandler);

        Add(
            CommandLineStrings_Systemd.ServiceStartCommandName,
            CommandLineStrings_Systemd.ServiceStartCommandDescription,
            s_nameOption,
            (systemdUnitName) => ControlSystemdUnitHandler(systemdUnitName, ServiceControlAction.Start));

        Add(
            CommandLineStrings_Systemd.ServiceStopCommandName,
            CommandLineStrings_Systemd.ServiceStopCommandDescription,
            s_nameOption,
            (systemdUnitName) => ControlSystemdUnitHandler(systemdUnitName, ServiceControlAction.Stop));

        Add(
            CommandLineStrings_Systemd.ServiceRestartCommandName,
            CommandLineStrings_Systemd.ServiceRestartCommandDescription,
            s_nameOption,
            (systemdUnitName) => ControlSystemdUnitHandler(systemdUnitName, ServiceControlAction.Restart));
    }

    private static async Task EnableDisableHandler(string systemdUnitName, bool enable)
    {
        if (await GetConfiguration() is not SystemConfiguration config)
            return;

        var serviceToConfigure = GetServiceDetailToConfigure(config, systemdUnitName);

        if (serviceToConfigure is null)
        {
            SimpleOutput.WriteLineError(CommandLineStrings_Systemd.UnitConfigurationNotAllowed, systemdUnitName);
            return;
        }

        var requestedState = enable ? ServiceState.Enabled : ServiceState.Disabled;

        if (serviceToConfigure.State == requestedState)
        {
            var warning = requestedState == ServiceState.Enabled
                ? CommandLineStrings_Systemd.UnitAlreadyEnabled
                : CommandLineStrings_Systemd.UnitAlreadyDisabled;

            SimpleOutput.WriteLineInformation(warning, systemdUnitName);
            return;
        }

        serviceToConfigure.State = requestedState;

        if (!await SetConfiguration(config))
            return;

        var enableMessage = enable
            ? CommandLineStrings_Systemd.UnitEnabled
            : CommandLineStrings_Systemd.UnitDisabled;

        SimpleOutput.WriteLine(enableMessage, systemdUnitName);
    }

    private static async Task ControlSystemdUnitHandler(
        string systemdUnitName,
        ServiceControlAction controlAction)
    {
        if (string.IsNullOrWhiteSpace(systemdUnitName))
        {
            SimpleOutput.WriteLineError(CommandLineStrings_Systemd.UnitEmpty, systemdUnitName);
            return;
        }

        var normalizedUnitName = GetSystemdUnitWithUnitType(systemdUnitName);

        if (!await NamedPipeRequestHelper.ControlSystemdUnit(normalizedUnitName, controlAction))
            return;

        var successMessage = controlAction switch
        {
            ServiceControlAction.Start => CommandLineStrings_Systemd.UnitStarted,
            ServiceControlAction.Stop => CommandLineStrings_Systemd.UnitStopped,
            ServiceControlAction.Restart => CommandLineStrings_Systemd.UnitRestarted,
            _ => throw new ArgumentOutOfRangeException(nameof(controlAction), controlAction, null)
        };

        SimpleOutput.WriteLine(successMessage, systemdUnitName);
    }

    private static async Task ListHandler()
    {
        if (await GetConfiguration() is not SystemConfiguration config)
            return;

        var output = new KeyValueOutput().WithTitle(CommandLineStrings_Systemd.Units);

        config.Services.ForEach(service => output.With(new(service.Name, service.State.ToString())));

        output.WriteLine();
    }

    private static ServiceDetail? GetServiceDetailToConfigure(
        SystemConfiguration config,
        string systemdUnitName)
    {
        var normalizedUnitName = GetSystemdUnitWithUnitType(systemdUnitName);

        return config.Services.FirstOrDefault(service => service.Name.Equals(normalizedUnitName));
    }

    private static string GetSystemdUnitWithUnitType(string systemdUnitName)
    {
        // Ensure the provided service name has a unit type
        return SystemdUnits.KnownUnitTypes.Any(systemdUnitName.EndsWith)
            ? systemdUnitName
            : systemdUnitName + ".service";
    }
}
