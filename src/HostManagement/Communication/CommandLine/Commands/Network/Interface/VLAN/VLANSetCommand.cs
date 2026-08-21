using System.CommandLine;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Extensions;

namespace HostManagement.Communication.CommandLine.Commands.Network.Interface.VLAN;

/// <summary>
/// Represents the VLAN set command.
/// </summary>
internal sealed class VLANSetCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="VLANSetCommand"/>.
    /// </summary>
    /// <param name="interfaceNameOption">The interface name option.</param>
    /// <param name="vlanIdOption">The VLAN ID option.</param>
    public VLANSetCommand(Option<string> interfaceNameOption, Option<int> vlanIdOption)
        : base(CliStrings.Common.SetCommandName, CliStrings.Network.Interface.VLAN.SetCommandDescription)
    {
        Add(interfaceNameOption);
        Add(vlanIdOption);

        SetAction((parseResult, cancellationToken)
            => SetHandler(
                parseResult.GetRequiredValue(interfaceNameOption),
                parseResult.GetRequiredValue(vlanIdOption),
                cancellationToken));
    }

    private static async Task SetHandler(string interfaceName, int vlanId, CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (config.GetNetworkInterface(interfaceName) is not NetworkInterfaceDetail interfaceToConfigure)
        {
            SimpleOutput.WriteLineWarning(CliStrings.Network.Interface.NotFound, interfaceName);
            return;
        }

        if (!interfaceToConfigure.VLAN.Enabled &&
            vlanId.Equals(0))
        {
            SimpleOutput.WriteLineInformation(CliStrings.Network.Interface.VLAN.AlreadyDisabled, interfaceName);
            return;
        }

        if (interfaceToConfigure.VLAN.ID.Equals(0))
        {
            interfaceToConfigure.VLAN.ID = vlanId;
            interfaceToConfigure.VLAN.Enabled = true;

            if (!await SetConfiguration(config, cancellationToken))
                return;

            SimpleOutput.WriteLine(
                CliStrings.Network.Interface.VLAN.IDAdded,
                vlanId,
                interfaceToConfigure.CommonInformation.Name);
        }
        else
        {
            var oldVlanId = interfaceToConfigure.VLAN.ID;
            interfaceToConfigure.VLAN.ID = vlanId;
            interfaceToConfigure.VLAN.Enabled = true;

            if (!await SetConfiguration(config, cancellationToken))
                return;

            SimpleOutput.WriteLine(
                CliStrings.Network.Interface.VLAN.IDSet,
                oldVlanId,
                vlanId,
                interfaceToConfigure.CommonInformation.Name);
        }
    }
}
