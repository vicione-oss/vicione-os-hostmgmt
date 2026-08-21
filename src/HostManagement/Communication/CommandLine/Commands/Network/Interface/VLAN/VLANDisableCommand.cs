using System.CommandLine;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Extensions;

namespace HostManagement.Communication.CommandLine.Commands.Network.Interface.VLAN;

/// <summary>
/// Represents the VLAN disable command.
/// </summary>
internal sealed class VLANDisableCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="VLANDisableCommand"/>.
    /// </summary>
    /// <param name="interfaceNameArgument">The interface name argument.</param>
    public VLANDisableCommand(Argument<string> interfaceNameArgument)
        : base(CliStrings.Common.DisableCommandName, CliStrings.Network.Interface.VLAN.DisableCommandDescription)
    {
        Add(interfaceNameArgument);

        SetAction((parseResult, cancellationToken)
            => DisableHandler(parseResult.GetRequiredValue(interfaceNameArgument), cancellationToken));
    }

    private static async Task DisableHandler(string interfaceName, CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (config.GetNetworkInterface(interfaceName) is not NetworkInterfaceDetail interfaceToConfigure)
        {
            SimpleOutput.WriteLineWarning(CliStrings.Network.Interface.NotFound, interfaceName);
            return;
        }

        if (!interfaceToConfigure.VLAN.Enabled)
        {
            SimpleOutput.WriteLineInformation(CliStrings.Network.Interface.VLAN.AlreadyDisabled, interfaceName);
            return;
        }

        interfaceToConfigure.VLAN.Enabled = false;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        SimpleOutput.WriteLine(CliStrings.Network.Interface.VLAN.Disabled, interfaceName);
    }
}
