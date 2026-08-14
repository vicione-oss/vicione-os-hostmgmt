using System.CommandLine;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
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
    public VLANRemoveCommand(Argument<string> interfaceNameArgument)
        : base(CliStrings.Common.RemoveCommandName, CliStrings.Network.Interface.VLAN.RemoveCommandDescription)
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
            SimpleOutput.WriteLineWarning(CliStrings.Network.Interface.NotFound, interfaceName);
            return;
        }

        if (interfaceToConfigure.VLAN.ID.Equals(0))
        {
            SimpleOutput.WriteLineWarning(CliStrings.Network.Interface.VLAN.NotConfigured, interfaceName);
            return;
        }

        var vlanIdToRemove = interfaceToConfigure.VLAN.ID;
        var wasVlanEnabled = interfaceToConfigure.VLAN.Enabled;
        interfaceToConfigure.VLAN.ID = 0;
        interfaceToConfigure.VLAN.Enabled = false;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        if (wasVlanEnabled)
            SimpleOutput.WriteLineWarning(CliStrings.Network.Interface.VLAN.Disabled, interfaceName);

        SimpleOutput.WriteLine(CliStrings.Network.Interface.VLAN.IDRemoved, vlanIdToRemove, interfaceName);
    }
}
