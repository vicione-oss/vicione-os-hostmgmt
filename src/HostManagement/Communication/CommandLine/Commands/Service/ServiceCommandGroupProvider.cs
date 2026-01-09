using System.CommandLine;
using HostManagement.Communication.CommandLine.Extensions;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Enums;

namespace HostManagement.Communication.CommandLine.Commands.Service;

/// <summary>
/// Represents the <see cref="ScriptServiceCommandGroupProvider"/>.
/// </summary>
internal static class ScriptServiceCommandGroupProvider
{
    private static readonly Option<string> s_nameOption = new(["-n", "--name"])
    {
        Description = "The name of the linux service.",
        IsRequired = true,
        Arity = ArgumentArity.ExactlyOne
    };

    private static readonly Command s_enableCommand = CommandHelper.ConstructCommand(
        "enable",
        "Enable the specified linux service.",
        s_nameOption,
        EnableHandler);

    private static readonly Command s_disableCommand = CommandHelper.ConstructCommand(
        "disable",
        "Disable the specified linux service.",
        s_nameOption,
        DisableHandler);

    private static readonly Command s_listCommand = CommandHelper.ConstructCommand(
        "list",
        "List all configurable linux services.",
        ListHandler);

    private static async Task EnableHandler(string serviceName)
        => await EnableDisableHandler(serviceName, enable: true);

    private static async Task DisableHandler(string serviceName)
        => await EnableDisableHandler(serviceName, enable: false);

    /// <summary>
    /// Adds the service command group.
    /// </summary>
    /// <param name="parentCommand">The parent command.</param>
    internal static void AddServiceCommandGroup(this Command parentCommand)
    {
        var serviceCommand = parentCommand.AddCommand(
            "service",
            "Configure linux system services.");

        serviceCommand.AddCommand(s_enableCommand);
        serviceCommand.AddCommand(s_disableCommand);
        serviceCommand.AddCommand(s_listCommand);
    }

    private static async Task EnableDisableHandler(string serviceName, bool enable)
    {
        if (await NamedPipeRequestHelper.LoadConfiguration() is not SystemConfiguration config)
            return;

        var serviceToConfigure = config.Services
            .FirstOrDefault(service => service.Name.Equals(serviceName));

        if (serviceToConfigure is null)
        {
            ConsoleEx.WriteLineError(CommandLineStrings.ServiceConfigurationNotAllowed, serviceName);
            return;
        }

        var requestedState = enable ? ServiceState.Enabled : ServiceState.Disabled;

        if (serviceToConfigure.State == requestedState)
        {
            ConsoleEx.WriteLineWarning(
                requestedState == ServiceState.Enabled
                    ? CommandLineStrings.ServiceAlreadyEnabled
                    : CommandLineStrings.ServiceAlreadyDisabled,
                serviceName);
            return;
        }

        serviceToConfigure.State = requestedState;

        if (!await NamedPipeRequestHelper.ApplyConfiguration(config))
            return;

        Console.WriteLine(
            enable
                ? CommandLineStrings.ServiceEnabled
                : CommandLineStrings.ServiceDisabled,
            serviceName);
    }

    private static async Task ListHandler()
    {
        if (await NamedPipeRequestHelper.LoadConfiguration() is not SystemConfiguration config)
            return;

        foreach (var service in config.Services)
            Console.WriteLine($"{service.Name}: {service.State}");
    }
}
