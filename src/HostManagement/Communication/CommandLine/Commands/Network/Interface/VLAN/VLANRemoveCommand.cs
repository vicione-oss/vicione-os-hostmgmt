using System.CommandLine;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Extensions;

namespace HostManagement.Communication.CommandLine.Commands.Network.Interface.VLAN;

/// <summary>
/// Represents the VLAN remove command.
/// </summary>
internal sealed class VLANRemoveCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="VLANRemoveCommand"/>.
    /// </summary>
    /// <param name="interfaceNameArgument">The interface name argument.</param>
    public VLANRemoveCommand(Argument<string> interfaceNameArgument) : base(
        CommandLineStrings_Common.RemoveCommandName,
        CommandLineStrings_NetworkInterfaces.VLANRemoveCommandDescription)
    {
        Add(interfaceNameArgument);

        SetAction((parseResult, cancellationToken)
            => RemoveHandler(parseResult.GetRequiredValue(interfaceNameArgument), cancellationToken));
    }

    private static async Task RemoveHandler(string interfaceName, CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (config.GetNetworkInterface(interfaceName) is not NetworkInterfaceDetail interfaceToConfigure)
        {
            SimpleOutput.WriteLineWarning(CommandLineStrings_NetworkInterfaces.InterfaceNotFound, interfaceName);
            return;
        }

        if (interfaceToConfigure.VLAN.ID.Equals(0))
        {
            SimpleOutput.WriteLineWarning(CommandLineStrings_NetworkInterfaces.VLANNotConfigured, interfaceName);
            return;
        }

        var vlanIdToRemove = interfaceToConfigure.VLAN.ID;
        var wasVlanEnabled = interfaceToConfigure.VLAN.Enabled;
        interfaceToConfigure.VLAN.ID = 0;
        interfaceToConfigure.VLAN.Enabled = false;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        if (wasVlanEnabled)
            SimpleOutput.WriteLineWarning(CommandLineStrings_NetworkInterfaces.VLANDisabled, interfaceName);

        SimpleOutput.WriteLine(CommandLineStrings_NetworkInterfaces.VLANIDRemoved, vlanIdToRemove, interfaceName);
    }
}
