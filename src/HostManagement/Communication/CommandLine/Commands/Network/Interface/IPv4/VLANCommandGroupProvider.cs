using System.CommandLine;
using HostManagement.Communication.CommandLine.Extensions;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;

namespace HostManagement.Communication.CommandLine.Commands.Network.Interface.IPv4;

internal static class VLANCommandGroupProvider
{
    private static readonly Option<string> s_nameOption = CommonOptions.s_interfaceNameOption;

    private static readonly Option<int> s_vlanIdOption = new(["-i", "--vlan-id"])
    {
        Description = "The VLAN ID.",
        IsRequired = true,
        Arity = ArgumentArity.ExactlyOne
    };

    private static readonly Command s_enableCommand = CommandHelper.ConstructCommand(
        "enable",
        "Enable VLAN for the specified network interface (assuming a valid VLAN-ID is configured).",
        s_nameOption,
        EnableHandler);

    private static readonly Command s_disableCommand = CommandHelper.ConstructCommand(
        "disable",
        "Disable VLAN for the specified network interface.",
        s_nameOption,
        DisableHandler);

    private static readonly Command s_setCommand = CommandHelper.ConstructCommand(
        "set",
        "Remove the existing VLAN ID and set a new one to the specified network interface.",
        s_nameOption,
        s_vlanIdOption,
        SetHandler);

    private static readonly Command s_removeCommand = CommandHelper.ConstructCommand(
        "remove",
        "Remove the existing VLAN ID and disable VLAN of the specified network interface.",
        s_nameOption,
        RemoveHandler);

    private static readonly Command s_listCommand = CommandHelper.ConstructCommand(
        "list",
        "List the existing VLAN configuration of all available network interfaces.",
        ListHandler);

    /// <summary>
    /// Adds the command group to configure VLAN settings of a specified network interface.
    /// </summary>
    /// <param name="parentCommand">The parent command.</param>
    internal static void AddVLANCommandGroup(this Command parentCommand)
    {
        var vlanCommand = parentCommand.AddCommand(
            "vlan",
            "Configure VLAN settings of a specified network interface.");

        vlanCommand.AddCommand(s_enableCommand);
        vlanCommand.AddCommand(s_disableCommand);
        vlanCommand.AddCommand(s_setCommand);
        vlanCommand.AddCommand(s_removeCommand);
        vlanCommand.AddCommand(s_listCommand);
    }

    private static async Task EnableHandler(string name)
        => await EnableDisableHandler(name, enable: true);

    private static async Task DisableHandler(string name)
        => await EnableDisableHandler(name, enable: false);

    private static async Task EnableDisableHandler(string interfaceName, bool enable)
    {
        if (await NamedPipeRequestHelper.LoadConfiguration() is not SystemConfiguration config)
            return;

        var interfaceToConfigure = config.NetworkInterfacesSettings.NetworkInterfaces
            .FirstOrDefault(@interface => @interface.CommonInformation.Name.Equals(interfaceName));

        if (interfaceToConfigure is null)
        {
            ConsoleEx.WriteLineError(CommandLineStrings.NetworkInterfaceNotFound, interfaceName);
            return;
        }

        if (interfaceToConfigure.IPv4.VLANEnabled.Equals(enable))
        {
            Console.WriteLine(
                enable
                    ? CommandLineStrings.VLANAlreadyEnabled
                    : CommandLineStrings.VLANAlreadyDisabled,
                interfaceName);

            return;
        }

        interfaceToConfigure.IPv4.VLANEnabled = enable;

        if (!await NamedPipeRequestHelper.ApplyConfiguration(config))
            return;

        Console.WriteLine(
            enable
                ? CommandLineStrings.VLANEnabled
                : CommandLineStrings.VLANDisabled,
            interfaceName);
    }

    private static async Task SetHandler(string interfaceName, int vlanId)
    {
        if (await NamedPipeRequestHelper.LoadConfiguration() is not SystemConfiguration config)
            return;

        var interfaceToConfigure = config.NetworkInterfacesSettings.NetworkInterfaces
            .FirstOrDefault(@interface => @interface.CommonInformation.Name.Equals(interfaceName));

        if (interfaceToConfigure is null)
        {
            ConsoleEx.WriteLineError(CommandLineStrings.NetworkInterfaceNotFound, interfaceName);
            return;
        }

        if (!interfaceToConfigure.IPv4.VLANEnabled &&
            vlanId.Equals(0))
        {
            ConsoleEx.WriteLineWarning(CommandLineStrings.VLANAlreadyDisabled, interfaceName);
            return;
        }

        if (interfaceToConfigure.IPv4.VLANID.Equals(0))
            await AddVlanId(config, interfaceToConfigure, vlanId);
        else
            await SetVlanId(config, interfaceToConfigure, vlanId);

        if (!interfaceToConfigure.IPv4.VLANEnabled)
            ConsoleEx.WriteLineWarning(CommandLineStrings.VLANCurrentlyDisabled, interfaceName);
    }

    private static async Task AddVlanId(
        SystemConfiguration config,
        NetworkInterfaceDetail interfaceToConfigure,
        int vlanId)
    {
        interfaceToConfigure.IPv4.VLANID = vlanId;

        if (!await NamedPipeRequestHelper.ApplyConfiguration(config))
            return;

        Console.WriteLine(
            CommandLineStrings.VLANIDAdded,
            vlanId,
            interfaceToConfigure.CommonInformation.Name);
    }

    private static async Task SetVlanId(
        SystemConfiguration config,
        NetworkInterfaceDetail interfaceToConfigure,
        int vlanId)
    {
        var oldVlanId = interfaceToConfigure.IPv4.VLANID;
        interfaceToConfigure.IPv4.VLANID = vlanId;

        if (!await NamedPipeRequestHelper.ApplyConfiguration(config))
            return;

        Console.WriteLine(
            CommandLineStrings.VLANIDSet,
            oldVlanId,
            vlanId,
            interfaceToConfigure.CommonInformation.Name);
    }

    private static async Task RemoveHandler(string interfaceName)
    {
        if (await NamedPipeRequestHelper.LoadConfiguration() is not SystemConfiguration config)
            return;

        var interfaceToConfigure = config.NetworkInterfacesSettings.NetworkInterfaces
            .FirstOrDefault(@interface => @interface.CommonInformation.Name.Equals(interfaceName));

        if (interfaceToConfigure is null)
        {
            ConsoleEx.WriteLineError(CommandLineStrings.NetworkInterfaceNotFound, interfaceName);
            return;
        }

        if (interfaceToConfigure.IPv4.VLANID.Equals(0))
        {
            ConsoleEx.WriteLineError(CommandLineStrings.VLANNotConfigured, interfaceName);
            return;
        }

        var vlanIdToRemove = interfaceToConfigure.IPv4.VLANID;
        var wasVlanEnabled = interfaceToConfigure.IPv4.VLANEnabled;
        interfaceToConfigure.IPv4.VLANID = 0;
        interfaceToConfigure.IPv4.VLANEnabled = false;

        if (!await NamedPipeRequestHelper.ApplyConfiguration(config))
            return;

        Console.WriteLine(
            CommandLineStrings.VLANIDRemoved,
            vlanIdToRemove,
            interfaceToConfigure.CommonInformation.Name);

        if (wasVlanEnabled)
            ConsoleEx.WriteLineWarning(CommandLineStrings.VLANDisabled, interfaceName);
        else
            Console.WriteLine(CommandLineStrings.VLANCurrentlyDisabled, interfaceName);
    }

    private static async Task ListHandler()
    {
        if (await NamedPipeRequestHelper.LoadConfiguration() is not SystemConfiguration config)
            return;

        var builder = new CommandLineStringBuilder();

        foreach (var @interface in config.NetworkInterfacesSettings.NetworkInterfaces)
        {
            builder.AppendLine(@interface.CommonInformation.Name);

            var vlanEnabledString = @interface.IPv4.VLANEnabled
                ? CommandLineStrings.enabled
                : CommandLineStrings.disabled;

            var vlanId = @interface.IPv4.VLANID.Equals(0)
                ? CommandLineStrings.NotSet
                : @interface.IPv4.VLANID.ToString();

            builder.AppendLine(1, $"VLAN ID: {vlanId} ({vlanEnabledString})");
        }

        Console.WriteLine(builder.Output);
    }
}
