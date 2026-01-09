using System.CommandLine;
using HostManagement.Communication.CommandLine.Extensions;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Configuration;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Service;
using HostManagement.Shared.Enums;

namespace HostManagement.Communication.CommandLine.Commands.Service;

/// <summary>
/// Represents the <see cref="ServiceCommandGroupProvider"/>.
/// </summary>
internal static class ServiceCommandGroupProvider
{
    private static readonly Option<string> s_nameOption = new(["-n", "--name"])
    {
        Description = "The name of the service.",
        IsRequired = true,
        Arity = ArgumentArity.ExactlyOne
    };

    private static readonly Command s_enableCommand = CommandHelper.ConstructCommand(
        "enable",
        "Enable the specified service.",
        s_nameOption,
        (serviceName) => EnableDisableHandler(serviceName, enable: true));

    private static readonly Command s_disableCommand = CommandHelper.ConstructCommand(
        "disable",
        "Disable the specified service.",
        s_nameOption,
        (serviceName) => EnableDisableHandler(serviceName, enable: false));

    private static readonly Command s_startCommand = CommandHelper.ConstructCommand(
        "start",
        "Start the specified service.",
        s_nameOption,
        (serviceName) => ControlServiceHandler(serviceName, ServiceControlAction.Start));

    private static readonly Command s_stopCommand = CommandHelper.ConstructCommand(
        "stop",
        "Stop the specified service.",
        s_nameOption,
        (serviceName) => ControlServiceHandler(serviceName, ServiceControlAction.Stop));

    private static readonly Command s_restartCommand = CommandHelper.ConstructCommand(
        "restart",
        "Restart the specified service.",
        s_nameOption,
        (serviceName) => ControlServiceHandler(serviceName, ServiceControlAction.Restart));

    private static readonly Command s_listCommand = CommandHelper.ConstructCommand(
        "list",
        "List all configurable services.",
        ListHandler);

    /// <summary>
    /// Adds the service command group.
    /// </summary>
    /// <param name="parentCommand">The parent command.</param>
    internal static void AddServiceCommandGroup(this Command parentCommand)
    {
        var serviceCommand = parentCommand.AddCommand(
            "service",
            "Configure system services.");

        serviceCommand.AddCommand(s_enableCommand);
        serviceCommand.AddCommand(s_disableCommand);
        serviceCommand.AddCommand(s_startCommand);
        serviceCommand.AddCommand(s_stopCommand);
        serviceCommand.AddCommand(s_restartCommand);
        serviceCommand.AddCommand(s_listCommand);
    }

    private static async Task EnableDisableHandler(string serviceName, bool enable)
    {
        if (await NamedPipeRequestHelper.GetConfiguration() is not SystemConfiguration config)
            return;

        var serviceToConfigure = GetServiceToConfigure(config, serviceName);

        if (serviceToConfigure is null)
        {
            SimpleOutput.WriteLineError(CommandLineStrings.ServiceConfigurationNotAllowed, serviceName);

            return;
        }

        var requestedState = enable ? ServiceState.Enabled : ServiceState.Disabled;

        if (serviceToConfigure.State == requestedState)
        {
            var warning = requestedState == ServiceState.Enabled
                ? CommandLineStrings.ServiceAlreadyEnabled
                : CommandLineStrings.ServiceAlreadyDisabled;

            SimpleOutput.WriteLineInformation(warning, serviceName);

            return;
        }

        serviceToConfigure.State = requestedState;

        if (!await NamedPipeRequestHelper.SetConfiguration(config))
            return;

        var enableMessage = enable
            ? CommandLineStrings.ServiceEnabled
            : CommandLineStrings.ServiceDisabled;

        SimpleOutput.WriteLine(enableMessage, serviceName);
    }

    private static async Task ControlServiceHandler(string serviceName, ServiceControlAction controlAction)
    {
        if (string.IsNullOrWhiteSpace(serviceName))
        {
            SimpleOutput.WriteLineError(CommandLineStrings.ServiceEmpty, serviceName);

            return;
        }

        var normalizedServiceName = EnsureServiceUnitType(serviceName);

        if (!await NamedPipeRequestHelper.ControlService(normalizedServiceName, controlAction))
            return;

        var successMessage = controlAction switch
        {
            ServiceControlAction.Start => CommandLineStrings.ServiceStarted,
            ServiceControlAction.Stop => CommandLineStrings.ServiceStopped,
            ServiceControlAction.Restart => CommandLineStrings.ServiceRestarted,
            _ => throw new ArgumentOutOfRangeException(nameof(controlAction), controlAction, null)
        };

        SimpleOutput.WriteLine(successMessage, serviceName);
    }

    private static async Task ListHandler()
    {
        if (await NamedPipeRequestHelper.GetConfiguration() is not SystemConfiguration config)
            return;

        var output = new KeyValueOutput().WithTitle(CommandLineStrings.Services);

        config.Services.ForEach(service => output.With(new(service.Name, service.State.ToString())));

        output.WriteLine();
    }

    private static ServiceDetail? GetServiceToConfigure(SystemConfiguration config, string serviceName)
    {
        var normalizedServiceName = EnsureServiceUnitType(serviceName);

        return config.Services
            .FirstOrDefault(service => service.Name.Equals(normalizedServiceName));
    }

    private static string EnsureServiceUnitType(string serviceName)
    {
        // Ensure the provided service name has a unit type
        return Services.KnownUnitTypes.Any(serviceName.EndsWith)
            ? serviceName
            : serviceName + ".service";
    }
}
