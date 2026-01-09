using System.CommandLine;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Extensions;

namespace HostManagement.Communication.CommandLine.Commands.Network.Interface.IPv4;

/// <summary>
/// Represents the VLAN command.
/// </summary>
internal sealed class VLANCommand : ConfigCommandBase
{
    private static readonly Option<string> s_nameOption = CommonOptions.s_interfaceNameOption;

    private static readonly Option<int> s_vlanIdOption = new(["-i", "--vlan-id"])
    {
        Description = "The VLAN ID.",
        IsRequired = true,
        Arity = ArgumentArity.ExactlyOne
    };

    /// <summary>
    /// Initializes a new <see cref="VLANCommand"/>.
    /// </summary>
    public VLANCommand() : base(
        CommandLineStrings_NetworkInterfaces.VLANCommandName,
        CommandLineStrings_NetworkInterfaces.VLANCommandDescription)
    {
        Add(
            CommandLineStrings_Common.EnableCommandName,
            CommandLineStrings_NetworkInterfaces.VLANEnableCommandDescription,
            s_nameOption,
            (name) => EnableDisableHandler(interfaceName: name, enable: true));

        Add(
            CommandLineStrings_Common.DisableCommandName,
            CommandLineStrings_NetworkInterfaces.VLANDisableCommandDescription,
            s_nameOption,
            (name) => EnableDisableHandler(interfaceName: name, enable: false));

        Add(
            CommandLineStrings_Common.ListCommandName,
            CommandLineStrings_NetworkInterfaces.VLANListCommandDescription,
            ListHandler);

        Add(
            CommandLineStrings_Common.SetCommandName,
            CommandLineStrings_NetworkInterfaces.VLANSetCommandDescription,
            s_nameOption,
            s_vlanIdOption,
            SetHandler);

        Add(
            CommandLineStrings_Common.RemoveCommandName,
            CommandLineStrings_NetworkInterfaces.VLANRemoveCommandDescription,
            s_nameOption,
            RemoveHandler);
    }

    private static async Task EnableDisableHandler(string interfaceName, bool enable)
    {
        if (await GetConfiguration() is not SystemConfiguration config)
            return;

        if (config.GetNetworkInterface(interfaceName) is not NetworkInterfaceDetail interfaceToConfigure)
        {
            SimpleOutput.WriteLineWarning(CommandLineStrings_NetworkInterfaces.InterfaceNotFound, interfaceName);

            return;
        }

        if (interfaceToConfigure.IPv4.VLANEnabled.Equals(enable))
        {
            var info = enable
                ? CommandLineStrings_NetworkInterfaces.VLANAlreadyEnabled
                : CommandLineStrings_NetworkInterfaces.VLANAlreadyDisabled;

            SimpleOutput.WriteLineInformation(info, interfaceName);

            return;
        }

        interfaceToConfigure.IPv4.VLANEnabled = enable;

        if (!await SetConfiguration(config))
            return;

        var enableMessage = enable
            ? CommandLineStrings_NetworkInterfaces.VLANEnabled
            : CommandLineStrings_NetworkInterfaces.VLANDisabled;

        SimpleOutput.WriteLine(enableMessage, interfaceName);
    }

    private static async Task SetHandler(string interfaceName, int vlanId)
    {
        if (await GetConfiguration() is not SystemConfiguration config)
            return;

        if (config.GetNetworkInterface(interfaceName) is not NetworkInterfaceDetail interfaceToConfigure)
        {
            SimpleOutput.WriteLineWarning(CommandLineStrings_NetworkInterfaces.InterfaceNotFound, interfaceName);

            return;
        }

        if (!interfaceToConfigure.IPv4.VLANEnabled &&
            vlanId.Equals(0))
        {
            SimpleOutput.WriteLineInformation(CommandLineStrings_NetworkInterfaces.VLANAlreadyDisabled, interfaceName);

            return;
        }

        if (interfaceToConfigure.IPv4.VLANID.Equals(0))
        {
            interfaceToConfigure.IPv4.VLANID = vlanId;
            interfaceToConfigure.IPv4.VLANEnabled = true;

            if (!await SetConfiguration(config))
                return;

            SimpleOutput.WriteLine(
                CommandLineStrings_NetworkInterfaces.VLANIDAdded,
                vlanId,
                interfaceToConfigure.CommonInformation.Name);
        }
        else
        {
            var oldVlanId = interfaceToConfigure.IPv4.VLANID;
            interfaceToConfigure.IPv4.VLANID = vlanId;
            interfaceToConfigure.IPv4.VLANEnabled = true;

            if (!await SetConfiguration(config))
                return;

            SimpleOutput.WriteLine(
                CommandLineStrings_NetworkInterfaces.VLANIDSet,
                oldVlanId,
                vlanId,
                interfaceToConfigure.CommonInformation.Name);
        }
    }

    private static async Task RemoveHandler(string interfaceName)
    {
        if (await GetConfiguration() is not SystemConfiguration config)
            return;

        if (config.GetNetworkInterface(interfaceName) is not NetworkInterfaceDetail interfaceToConfigure)
        {
            SimpleOutput.WriteLineWarning(CommandLineStrings_NetworkInterfaces.InterfaceNotFound, interfaceName);

            return;
        }

        if (interfaceToConfigure.IPv4.VLANID.Equals(0))
        {
            SimpleOutput.WriteLineWarning(CommandLineStrings_NetworkInterfaces.VLANNotConfigured, interfaceName);

            return;
        }

        var vlanIdToRemove = interfaceToConfigure.IPv4.VLANID;
        var wasVlanEnabled = interfaceToConfigure.IPv4.VLANEnabled;
        interfaceToConfigure.IPv4.VLANID = 0;
        interfaceToConfigure.IPv4.VLANEnabled = false;

        if (!await SetConfiguration(config))
            return;

        if (wasVlanEnabled)
            SimpleOutput.WriteLineWarning(CommandLineStrings_NetworkInterfaces.VLANDisabled, interfaceName);

        SimpleOutput.WriteLine(CommandLineStrings_NetworkInterfaces.VLANIDRemoved, vlanIdToRemove, interfaceName);
    }

    private static async Task ListHandler()
    {
        if (await GetConfiguration() is not SystemConfiguration config)
            return;

        var output = new PropertyListOutput().WithTitle(CommandLineStrings_NetworkInterfaces.Interfaces);

        foreach (var @interface in config.NetworkInterfacesSettings.NetworkInterfaces)
        {
            var vlanId = @interface.IPv4.VLANID.Equals(0)
                ? CliOutputBase.NotSetValue
                : @interface.IPv4.VLANID.ToString();

            output.With(
                @interface.CommonInformation.Name,
                new(CommandLineStrings_NetworkInterfaces.VLANID, vlanId),
                @interface.IPv4.VLANEnabled);
        }

        output.WriteLine();
    }
}
