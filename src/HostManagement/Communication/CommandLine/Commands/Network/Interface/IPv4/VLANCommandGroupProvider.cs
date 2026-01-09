using System.CommandLine;
using HostManagement.Communication.CommandLine.Extensions;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Extensions;

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
        if (await NamedPipeRequestHelper.GetConfiguration() is not SystemConfiguration config)
            return;

        if (config.GetNetworkInterface(interfaceName) is not NetworkInterfaceDetail interfaceToConfigure)
        {
            SimpleOutput.WriteLineWarning(CommandLineStrings.NetworkInterfaceNotFound, interfaceName);

            return;
        }

        if (interfaceToConfigure.IPv4.VLANEnabled.Equals(enable))
        {
            var info = enable
                ? CommandLineStrings.VLANAlreadyEnabled
                : CommandLineStrings.VLANAlreadyDisabled;

            SimpleOutput.WriteLineInformation(info, interfaceName);

            return;
        }

        interfaceToConfigure.IPv4.VLANEnabled = enable;

        if (!await NamedPipeRequestHelper.SetConfiguration(config))
            return;

        var enableMessage = enable
            ? CommandLineStrings.VLANEnabled
            : CommandLineStrings.VLANDisabled;

        SimpleOutput.WriteLine(enableMessage, interfaceName);
    }

    private static async Task SetHandler(string interfaceName, int vlanId)
    {
        if (await NamedPipeRequestHelper.GetConfiguration() is not SystemConfiguration config)
            return;

        if (config.GetNetworkInterface(interfaceName) is not NetworkInterfaceDetail interfaceToConfigure)
        {
            SimpleOutput.WriteLineWarning(CommandLineStrings.NetworkInterfaceNotFound, interfaceName);

            return;
        }

        if (!interfaceToConfigure.IPv4.VLANEnabled &&
            vlanId.Equals(0))
        {
            SimpleOutput.WriteLineInformation(CommandLineStrings.VLANAlreadyDisabled, interfaceName);

            return;
        }

        if (!interfaceToConfigure.IPv4.VLANEnabled)
            SimpleOutput.WriteLineWarning(CommandLineStrings.VLANCurrentlyDisabled, interfaceName);

        if (interfaceToConfigure.IPv4.VLANID.Equals(0))
        {
            interfaceToConfigure.IPv4.VLANID = vlanId;

            if (!await NamedPipeRequestHelper.SetConfiguration(config))
                return;

            SimpleOutput.WriteLine(
                CommandLineStrings.VLANIDAdded,
                vlanId,
                interfaceToConfigure.CommonInformation.Name);
        }
        else
        {
            var oldVlanId = interfaceToConfigure.IPv4.VLANID;
            interfaceToConfigure.IPv4.VLANID = vlanId;

            if (!await NamedPipeRequestHelper.SetConfiguration(config))
                return;

            SimpleOutput.WriteLine(
                CommandLineStrings.VLANIDSet,
                oldVlanId,
                vlanId,
                interfaceToConfigure.CommonInformation.Name);
        }
    }

    private static async Task RemoveHandler(string interfaceName)
    {
        if (await NamedPipeRequestHelper.GetConfiguration() is not SystemConfiguration config)
            return;

        if (config.GetNetworkInterface(interfaceName) is not NetworkInterfaceDetail interfaceToConfigure)
        {
            SimpleOutput.WriteLineWarning(CommandLineStrings.NetworkInterfaceNotFound, interfaceName);

            return;
        }

        if (interfaceToConfigure.IPv4.VLANID.Equals(0))
        {
            SimpleOutput.WriteLineWarning(CommandLineStrings.VLANNotConfigured, interfaceName);

            return;
        }

        var vlanIdToRemove = interfaceToConfigure.IPv4.VLANID;
        var wasVlanEnabled = interfaceToConfigure.IPv4.VLANEnabled;
        interfaceToConfigure.IPv4.VLANID = 0;
        interfaceToConfigure.IPv4.VLANEnabled = false;

        if (!await NamedPipeRequestHelper.SetConfiguration(config))
            return;

        if (wasVlanEnabled)
            SimpleOutput.WriteLineWarning(CommandLineStrings.VLANDisabled, interfaceName);

        SimpleOutput.WriteLine(CommandLineStrings.VLANIDRemoved, vlanIdToRemove, interfaceName);
    }

    private static async Task ListHandler()
    {
        if (await NamedPipeRequestHelper.GetConfiguration() is not SystemConfiguration config)
            return;

        var output = new PropertyListOutput().WithTitle(CommandLineStrings.Interfaces);

        foreach (var @interface in config.NetworkInterfacesSettings.NetworkInterfaces)
        {
            var vlanId = @interface.IPv4.VLANID.Equals(0)
                ? CliOutputBase.NotSetValue
                : @interface.IPv4.VLANID.ToString();

            output.With(
                @interface.CommonInformation.Name,
                new(CommandLineStrings.VLANID, vlanId),
                @interface.IPv4.VLANEnabled);
        }

        output.WriteLine();
    }
}
